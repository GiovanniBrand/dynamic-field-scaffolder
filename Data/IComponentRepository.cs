using Dynamic.Scaffolder.DTOs;
using System.Data.Common;

namespace Dynamic.Scaffolder.Data;

public interface IComponentRepository
{
    Task<TabDto> GetTabPerKeyAsync(DbConnection connection, DbTransaction transaction, string chaveAba);
    Task<IEnumerable<FieldGroupDto>> GetGroupsPerTabAsync(DbConnection connection, DbTransaction transaction, long abaId);
    Task<bool> ExistsComponentAsync(DbConnection connection, DbTransaction transaction, string referenciaModel);
    Task<int> GetMaxSequenceGroupAsync(DbConnection connection, DbTransaction transaction, long grupoId);
    Task SaveComponentAsync(DbConnection connection, DbTransaction transaction, object parametros);
    Task<bool> ColumnExistsInTableAsync(DbConnection connection, DbTransaction transaction, string tableName, string schema, string columnName);
}