using System.Data.Common;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateConnectionAsync();
}