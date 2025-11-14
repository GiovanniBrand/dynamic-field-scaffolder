using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dynamic.Scaffolder.Data;
using Dynamic.Scaffolder.Services;
using Dynamic.Scaffolder.SnowFlakeConfig;
using Dynamic.Scaffolder.UI;

Console.WriteLine("Iniciando aplicação...");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString!));
        services.AddSingleton<ISnowflakeGenerator>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>()
                           .GetSection("IdGenerators:Plataforma")
                           .Get<GeneratorConfig>();
            if (config == null)
            {
                throw new InvalidOperationException("Configuração 'IdGenerators:Plataforma' não encontrada no appsettings.json.");
            }

            return new SnowflakeGenerator(config);
        });
        services.AddScoped<IComponentRepository, ComponentRepository>();
        services.AddScoped<ISharpMigrationGenerator, SharpMigrationGenerator>();
        services.AddScoped<MigrationConsoleUI>();
        services.AddScoped<MigrationService>();
    })
    .Build();
using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var migration = services.GetRequiredService<MigrationService>();
        await migration.RunAsync();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nOcorreu um erro fatal ao executar a migração:");
        Console.WriteLine(ex.ToString());
        Console.ResetColor();
    }
}



