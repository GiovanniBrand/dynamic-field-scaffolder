// Services/SharpMigrationGenerator.cs
using Dynamic.Scaffolder.Enums;
using Dynamic.Scaffolder.DTOs;
using Dynamic.Scaffolder.Services;
using System.Text;

public class SharpMigrationGenerator : ISharpMigrationGenerator
{
    private const string OutputDirectory = @""; // This the path to my migration project that will be runned your SharpMigration
    public async Task<string> GenerateAddColumnsMigrationAsync(string tableName, IEnumerable<ColumnDefinition> columns)
    {
        if (!columns.Any())
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
        string className = $"Migracao_{timestamp}_AlterTable_Movimento";

        sb.AppendLine("using SharpData.Filters;");
        sb.AppendLine("using SharpMigrations;");
        sb.AppendLine();
        sb.AppendLine("namespace MPS.FolhaMais.Migracoes;");
        sb.AppendLine();
        sb.AppendLine($"public class {className} : SchemaMigration");
        sb.AppendLine("{");

        // --- Up() Method ---
        sb.AppendLine("    public override void Up()");
        sb.AppendLine("    {");
        foreach (var column in columns)
        {
            string columnDefinition = MapComponentTypeToSqlType(column);
            sb.AppendLine($"        Add.Column({columnDefinition}).ToTable(\"{tableName}\");");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        // --- Down() Method ---
        sb.AppendLine("    public override void Down()");
        sb.AppendLine("    {");
        foreach (var column in columns)
        {
            sb.AppendLine($"        Remove.Column(\"{column.Name}\").FromTable(\"{tableName}\");");
        }
        sb.AppendLine("    }");

        sb.AppendLine("}");

        string fileName = $"{className}.cs";
        string filePath = Path.Combine(OutputDirectory, fileName);

        await File.WriteAllTextAsync(filePath, sb.ToString());

        return filePath;
    }

    private string MapComponentTypeToSqlType(ColumnDefinition column)
    {
        string sharpType;
        switch (column.ComponentType)
        {
            case ComponentType.Text: 
                sharpType = $"Column.AnsiString(\"{column.Name}\", 255)"; break;
            case ComponentType.DateTime: 
                sharpType = $"Column.DateTime(\"{column.Name}\")"; break;
            case ComponentType.Check: 
                sharpType = $"Column.Boolean(\"{column.Name}\")"; break;
            case ComponentType.Number: 
            case ComponentType.Money:
                sharpType = $"Column.Decimal(\"{column.Name}\")"; break;
            case ComponentType.Integer:
            case ComponentType.LookUp: 
                sharpType = $"Column.Int64(\"{column.Name}\")"; break;
            default: sharpType = $"Column.String(\"{column.Name}\", 255)"; break;
        }
        return sharpType;
    }
}