using Dapper;
using Dynamic.Scaffolder.DTOs;
using System.Data.Common;

namespace Dynamic.Scaffolder.Data;

public class ComponentRepository : IComponentRepository
{
    private const long MovimentoWindowId = 985704331035648;

    private const string GetTabSql = @"
        SELECT Id, Nome FROM front.Aba 
        WHERE Chave = @ChaveAba AND Janela = @WindowId";

    private const string GetGroupsSql = @"
        SELECT DISTINCT gc.Id, gc.Nome
        FROM front.GrupoCampos gc
        JOIN front.Componente c ON gc.Id = c.Grupo
        WHERE c.Aba = @AbaId";

    private const string ExistsComponentSql = @"
        SELECT 1 FROM front.Componente c
        JOIN front.Aba a ON a.Id = c.Aba
        WHERE a.Janela = @WindowId AND c.ReferenciaModel = @ReferenciaModel";

    private const string GetMaxSequenceSql = @"
        SELECT ISNULL(MAX(ordem), 0)
        FROM front.Componente WITH (UPDLOCK, HOLDLOCK)
        WHERE Grupo = @GrupoId";

    private const string SaveComponenteSql = @"
        INSERT INTO front.Componente (
            Id, Chave, Nome, Descricao, Ordem, ReferenciaModel, Aba, Grupo, Aria, Help, Tipo,
            Entidade, Visivel, Obrigatorio, SomenteLeitura, MembroBusca, ExibeTotalizador,
            ObrigatorioBusca, PermiteBuscaAvancada,
            CondicaoVisibilidade, CondicaoObrigatoriedade, CondicaoSomenteLeitura, Expressao,
            TipoMascara, Mascara, Tamanho, ValorMinimo, ValorMaximo, Filtro,
            OrigemCadastroModel, Icone, OrdenacaoInicial, PermiteZoom
        ) VALUES (
            @Id, @Chave, @Nome, @Descricao, @Ordem, @ReferenciaModel, @Aba, @Grupo, @Aria, @Help, @Tipo,
            1, 0, 0, 0, 0, 0, 0, 1, -- Valores padrão
            NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL -- Valores nulos
        )";


    public async Task<TabDto> GetTabPerKeyAsync(DbConnection connection, DbTransaction transaction, string tabKey)
    {
        return await connection.QueryFirstOrDefaultAsync<TabDto>(
            GetTabSql,
            new { ChaveAba = tabKey, WindowId = MovimentoWindowId },
            transaction: transaction);
    }

    public async Task<IEnumerable<FieldGroupDto>> GetGroupsPerTabAsync(DbConnection connection, DbTransaction transaction, long tabId)
    {
        return await connection.QueryAsync<FieldGroupDto>(
            GetGroupsSql,
            new { AbaId = tabId },
            transaction: transaction);
    }

    public async Task<bool> ExistsComponentAsync(DbConnection connection, DbTransaction transaction, string referenceModel)
    {
        var result = await connection.ExecuteScalarAsync<int?>(
            ExistsComponentSql,
            new { ReferenciaModel = referenceModel, WindowId = MovimentoWindowId },
            transaction: transaction);
        return result.HasValue && result.Value == 1;
    }

    public async Task<int> GetMaxSequenceGroupAsync(DbConnection connection, DbTransaction transaction, long groupId)
    {
        return await connection.ExecuteScalarAsync<int>(
            GetMaxSequenceSql,
            new { GrupoId = groupId },
            transaction: transaction);
    }

    public async Task SaveComponentAsync(DbConnection connection, DbTransaction transaction, object parameters)
    {
        await connection.ExecuteAsync(SaveComponenteSql, parameters, transaction: transaction);
    }

    public async Task<bool> ColumnExistsInTableAsync(DbConnection connection, DbTransaction transaction, string tableName, string schema, string columnName)
    {
        const string sql = @"
            SELECT 1
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @Schema
              AND TABLE_NAME = @TableName
              AND COLUMN_NAME = @ColumnName;
        ";

        var result = await connection.QueryFirstOrDefaultAsync<int?>(sql, new
        {
            Schema = schema,
            TableName = tableName,
            ColumnName = columnName
        }, transaction);

        return result.HasValue;
    }
}