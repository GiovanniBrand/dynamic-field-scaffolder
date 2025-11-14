using Dynamic.Scaffolder.DTOs;

namespace Dynamic.Scaffolder.UI;

public class MigrationConsoleUI
{
    public string TabKeyPrompt()
    {
        Console.Write("Digite a Chave da aba que você quer processar de movimento: ");
        string? tabKey = Console.ReadLine();

        while (string.IsNullOrWhiteSpace(tabKey))
        {
            Console.Write("Chave vazia ou nula, por favor digite-a novamente: ");
            tabKey = Console.ReadLine();
        }
        return tabKey;
    }

    public void TabNotFoundedDisplay(string chaveAba)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\nAviso: Aba com a chave '{chaveAba}' não foi encontrada. Tente novamente.");
        Console.ResetColor();
    }

    public FieldGroupDto? GroupChoosePrompt(TabDto aba, IEnumerable<FieldGroupDto> grupos)
    {
        Console.WriteLine($"\nQual dos grupos abaixo você quer usar para os novos campos na aba '{aba.Name}'?");

        int aux = 1;
        var dic = new Dictionary<int, FieldGroupDto>();
        foreach (var group in grupos)
        {
            Console.WriteLine($"  Para utilizar '{group.Name}', digite {aux}");
            dic.Add(aux, group);
            aux++;
        }

        Console.Write("Digite o número do grupo: ");
        string? groupNumber = Console.ReadLine();

        if (!int.TryParse(groupNumber, out int chosenNumber) || !dic.TryGetValue(chosenNumber, out var chosenGroup))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Erro: Número de grupo inválido. A operação será cancelada.");
            Console.ResetColor();
            return null;
        }

        return chosenGroup;
    }

    public void LogPendentSave(string description, string groupName)
    {
        Console.WriteLine($"Inserido (na transação): {description} no grupo {groupName}");
    }

    public void LogComponent(string description)
    {
        Console.WriteLine($"Já existe: {description}");
    }

    public void LogColumn(string description)
    {
        Console.WriteLine($"Coluna da tabela Movimento ja existente: {description}");
    }

    public void LogSucess()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nTransação concluída com sucesso! Todas as alterações foram salvas permanentemente no banco de dados.");
        Console.ResetColor();
    }

    public void LogError(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nOcorreu um erro e a transação foi revertida: {ex.Message}");
        Console.ResetColor();
    }

    public void LogGenerateMigrationFile()
    {
        Console.WriteLine($"\nGerando Arquivo de migração.");
    }

    public void LogGeneratedMigrationFile(string archive)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n Arquivo {archive} gerado com sucesso!");
        Console.ResetColor();
    }
    public void LogNoMigrationToPerform()
    {
        Console.WriteLine($"\n Nenhuma migração necssária para os dados!");
    }
}