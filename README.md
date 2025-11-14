# Dynamic Field Scaffolder

![Status](https://img.shields.io/badge/status-in_development-yellowgreen)
![License](https://img.shields.io/badge/license-MIT-blue)

A console tool to automate the creation of dynamic UI fields (components) and generate their corresponding database migrations (SharpMigrations) by reading C# enum attributes.

---

## 🧐 About The Project

In systems with dynamic screens, adding a new field (e.g., "Birth Date") is often a manual and error-prone process. A developer typically needs to:

1.  Add the field's metadata to the UI component table (e.g., `front.Componente`).
2.  Add the physical column to the main data table (e.g., `dbo.Movimento`).
3.  Keep both in sync.

This tool solves that. It reads a central `enum` where new fields are defined with `[Description]` and `[GetType]` attributes. When the console is run, it connects to the database, interactively asks where the new fields should be placed (in which UI "Tab" and "Group"), and then performs both tasks: it inserts the metadata and generates a `.cs` `SharpMigrations` file, ready to be executed.

## ✨ Key Features

* **Attribute-based Scaffolding:** Define new fields directly in a C# `enum` using `[Description]` and `[GetType]`.
* **Interactive UI:** A console UI prompts the user to select the "Tab" (Aba) and "Group" (Grupo) where the new fields will be inserted.
* **Safety Checks:** Verifies if the component or column already exists in the database before attempting to create them.
* **Metadata Management:** Automatically inserts records into the `front.Componente` table with correct IDs, names, and group associations.
* **Migration Generation:** Generates a **SharpMigrations** `.cs` file (with `Up()` and `Down()` methods) to add the columns to the `dbo.Movimento` table.
* **Specific Business Logic:** Includes rules, such as automatically placing `DateTime` fields into the "Datas" group when the "ANOTACAO" tab is selected.

## 🛠️ Tech Stack

* **.NET** (Hostbuilder & Dependency Injection)
* **C#** (with Reflection to read attributes)
* **Dapper** (For database access)
* **SharpMigrations** (As the format for the generated migration file)
* **Snowflake IDs** (For unique ID generation)
* **SQL Server**

## ⚙️ How it Works (The Flow)

1.  A **Developer** adds a new member to an `enum` (e.g., `AttributeTarget.cs`), decorating it with `[Description("...")]` and `[GetType(...)]`.
2.  The **Developer** runs this console project (`Dynamic.Scaffolder`).
3.  The **Console** prompts for the "Tab Key" (e.g., "ANOTACAO").
4.  The **Console** fetches groups for that tab and asks the dev to pick a "Default Group".
5.  The **Tool** uses Reflection to read the `enum`.
6.  For each new member (that doesn't already exist in `front.Componente` or `dbo.Movimento`):
    * Generates a new Snowflake `ID`.
    * Inserts the metadata into the `front.Componente` table (using `ComponentRepository`).
    * Adds the new column definition to a list.
7.  Finally, the `SharpMigrationGenerator` is called.
8.  A **`.cs` file is created** (e.g., `Migracao_202511141200_AlterTable_Movimento.cs`) in the configured output directory, containing the SharpMigrations code to add the new columns.

## 🚀 How To Use (Setup Guide)

Follow these steps to configure and run the tool.

### 1. Prerequisites

* [.NET SDK](https://dotnet.microsoft.com/download) (compatible with the project version).
* SQL Server database access (with Read/Write permissions).
* The main project that uses `SharpMigrations` must be accessible (to receive the generated migration file).

### 2. Critical Configuration (Hardcoded Values)

This tool requires you to change some hardcoded values in the source code to match your environment.

* **`MigrationService.cs`**
    * `enumType`: Change `typeof(AttributeTarget)` to the `enum` you want to use as the source.
        ```csharp
        // private async Task ProcessarComponentes(...)
        var enumType = typeof(YourAttributeEnum); // <-- CHANGE THIS
        ```

* **`SharpMigrationGenerator.cs`**
    * `OutputDirectory`: Set the **exact file path** to the migrations folder of your main project.
        ```csharp
        // private const string OutputDirectory = @"";
        private const string OutputDirectory = @"C:\Projects\MySystem\src\MySystem.Migrations"; // <-- CHANGE THIS
        ```
    * `namespace`: Adjust the namespace for the generated migration file.
        ```csharp
        // sb.AppendLine("namespace MPS.FolhaMais.Migracoes;");
        sb.AppendLine("namespace YourProject.Migrations;"); // <-- CHANGE THIS
        ```

* **`ComponentRepository.cs`**
    * `MovimentoWindowId`: If your system's "Window" ID is not `985704331035648`, change it here.
        ```csharp
        // private const long MovimentoWindowId = 985704331035648;
        private const long MovimentoWindowId = YOUR_WINDOW_ID_HERE; // <-- CHANGE THIS
        ```

### 3. Configuration (appsettings.json)

Configure your connection string and Snowflake ID generator settings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=your_user;Password=your_password;TrustServerCertificate=True"
  },
  "IdGenerators": {
    "Plataforma": {
      "DatacenterId": 1,
      "WorkerId": 1,
      "Epoch": 1609459200000
    }
  }
}
```

### 4. Defining Attributes (The Pattern)

This is the main day-to-day step. In the `enum` you configured in "Step 2" (e.g., `AttributeTarget.cs`), add your new fields:

> **Note:** You will need to have a `[GetTypeAttribute]` defined in your project.

```csharp
// Example: AttributeTarget.cs
using System.ComponentModel;
using Dynamic.Scaffolder.Enums;
using MPS.FolhaMais.Attributes; // Namespace for your [GetTypeAttribute]

public enum AttributeTarget
{
    // ... other existing fields ...

    [Description("Night Shift Bonus Value")]
    [GetType(ComponentType.Money)]
    ValorAdicionalNoturno,

    [Description("Next Vacation Date")]
    [GetType(ComponentType.DateTime)]
    DataProximasFerias,

    [Description("Manager's Observation")]
    [GetType(ComponentType.Text)]
    ObservacaoGestor
}
```

### 5. Running the Tool
Compile and run the Dynamic.Scaffolder project:

```bash

dotnet run
```
The console will appear:

First, it will ask for the `Tab Key`:

```
> Enter the Tab key (e.g., ANOTACAO):
```
Next, it will list the groups and ask you to choose one:

```
> Available groups for tab 'ANOTACAO':
> 1. Main Data
> 2. Documents
> 3. Dates
>
> Choose the default group for the new fields:
```
The tool will process the attributes from the enum. You will see logs in the console:

`[SKIPPED] Component 'FieldName' (ReferenceModel) already exists.`

`[PENDING] Component 'Night Shift Bonus Value' will be saved to group 'Main Data'.`

`[PENDING] Component 'Next Vacation Date' will be saved to group 'Dates'.` (Example of the special rule)

At the end, you will see the confirmation:

`[SUCCESS] Migration file generated at: C:\...\Migracao_..._AlterTable_Movimento.cs`

`[SUCCESS] Operation completed successfully.`

### 6. Applying the Migration
This tool only generates the migration file; it does not run it.

Go to your main project (where the file was generated) and run your normal SharpMigrations process (e.g., migrate.bat or similar) to apply the changes to the database.

### 🏛️ Database Structure
This tool primarily interacts with the following tables:

`front.Aba`: (Read) Used to select the Tab.

`front.GrupoCampos`: (Read) Used to select the Group.

`front.Componente`: (Write) Where the metadata for the new UI fields is stored.

`dbo.Movimento`: (Write via Migration) The main data table that is physically altered by the generated migration.

### 📄 License
This project is licensed under the MIT License.
