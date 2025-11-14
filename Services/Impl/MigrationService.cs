using Dynamic.Scaffolder;
using Dynamic.Scaffolder.Enums;
using Dynamic.Scaffolder.Helpers;
using MPS.FolhaMais.Attributes;
using Dynamic.Scaffolder.Data;
using Dynamic.Scaffolder.DTOs;
using Dynamic.Scaffolder.Services;
using Dynamic.Scaffolder.SnowFlakeConfig;
using Dynamic.Scaffolder.UI;
using System.ComponentModel;
using System.Data.Common;
using System.Reflection;

public class MigrationService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ISnowflakeGenerator _snowflakeGenerator;
    private readonly IComponentRepository _repository;
    private readonly ISharpMigrationGenerator sharpMigrationGenerator;
    private readonly MigrationConsoleUI _ui;

    public MigrationService(
        IDbConnectionFactory connectionFactory,
        ISnowflakeGenerator snowflakeGenerator,
        IComponentRepository repository,
        MigrationConsoleUI ui,
        ISharpMigrationGenerator sharpMigrationGenerator)
    {
        _connectionFactory = connectionFactory;
        _snowflakeGenerator = snowflakeGenerator;
        _repository = repository;
        _ui = ui;
        this.sharpMigrationGenerator = sharpMigrationGenerator;
    }

    public async Task RunAsync()
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            TabDto? aba = null;
            string chaveAba;
            do
            {
                chaveAba = _ui.TabKeyPrompt();
                aba = await _repository.GetTabPerKeyAsync(connection, transaction, chaveAba);
                if (aba == null)
                {
                    _ui.TabNotFoundedDisplay(chaveAba);
                }
            } while (aba == null);

            var grupos = await _repository.GetGroupsPerTabAsync(connection, transaction, aba.Id);
            var grupoEscolhido = _ui.GroupChoosePrompt(aba, grupos);

            if (grupoEscolhido == null)
            {
                await transaction.RollbackAsync();
                return;
            }

            await ProcessarComponentes(connection, transaction, aba, grupos, grupoEscolhido, chaveAba);

            await transaction.CommitAsync();
            _ui.LogSucess();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _ui.LogError(ex);
        }
    }

    private async Task ProcessarComponentes(
        DbConnection connection,
        DbTransaction transaction,
        TabDto tab,
        IEnumerable<FieldGroupDto> group,
        FieldGroupDto initialGroup,
        string tabKey)
    {
        var enumType = typeof(AttributeTarget); // This is your enumtype that will be scaffolded in the migration, i add some random class to exemplify that
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);

        var groupSequence = new Dictionary<long, int>();
        var newColumnsForMigration = new List<ColumnDefinition>();

        foreach (var field in fields)
        {
            var descAttr = field.GetCustomAttribute<DescriptionAttribute>(); 
            var typeAttr = field.GetCustomAttribute<GetTypeAttribute>();
            if (descAttr == null || typeAttr == null) continue;

            string description = descAttr.Description;
            string referenceModel = char.ToLowerInvariant(field.Name[0]) + field.Name.Substring(1);

            if (await _repository.ExistsComponentAsync(connection, transaction, referenceModel))
            {
                _ui.LogComponent(description);
                continue;
            }

            if (await _repository.ColumnExistsInTableAsync(connection, transaction, "Movimento", "dbo", field.Name))
            {
                _ui.LogColumn(field.Name);
                continue;
            }

            var targetGroup = GroupToBeInserted(initialGroup, group, typeAttr.Value, tabKey);
            newColumnsForMigration.Add(new ColumnDefinition(field.Name, typeAttr.Value));

            if (!groupSequence.ContainsKey(targetGroup.Id))
            {
                groupSequence[targetGroup.Id] = await _repository.GetMaxSequenceGroupAsync(connection, transaction, targetGroup.Id);
            }

            groupSequence[targetGroup.Id]++;
            int newSequence = groupSequence[targetGroup.Id];

            var parametros = new
            {
                Id = _snowflakeGenerator.NextId(),
                Chave = StringHelper.RandomString(),
                Nome = description,
                Descricao = description,
                Ordem = newSequence,
                ReferenciaModel = referenceModel,
                Aba = tab.Id,
                Grupo = targetGroup.Id,
                Aria = description,
                Help = description,
                Tipo = typeAttr.Value,
            };

            await _repository.SaveComponentAsync(connection, transaction, parametros);
            _ui.LogPendentSave(description, targetGroup.Name!);
        }

        if (newColumnsForMigration.Any())
        {
            _ui.LogGenerateMigrationFile();
            string generatedFilePath = await sharpMigrationGenerator.GenerateAddColumnsMigrationAsync(
                "dbo.Movimento",
                newColumnsForMigration);
            _ui.LogGeneratedMigrationFile(generatedFilePath);
        }
        else
        {
            _ui.LogNoMigrationToPerform();
        }
    }

    private FieldGroupDto GroupToBeInserted(FieldGroupDto inicialGroup, IEnumerable<FieldGroupDto> allGroups, ComponentType componentType, string tabKey)
    {

        if (string.Equals(tabKey, "ANOTACAO") && componentType == ComponentType.DateTime)
        {
            return allGroups.FirstOrDefault(g => g.Name == "Datas")
                ?? throw new InvalidOperationException("O grupo 'Datas' é necessário para a aba 'ANOTACAO', mas não foi encontrado.");
        }

        return inicialGroup;
    }
}