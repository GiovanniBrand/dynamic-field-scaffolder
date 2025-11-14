using Dynamic.Scaffolder.DTOs;

namespace Dynamic.Scaffolder.Services
{
    public interface ISharpMigrationGenerator
    {
        Task<string> GenerateAddColumnsMigrationAsync(string tableName, IEnumerable<ColumnDefinition> columns);
    }
}
