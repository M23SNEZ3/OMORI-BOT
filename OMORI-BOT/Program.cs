using OMORI_BOT.M23.Commands;
using OMORI_BOT.M23.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
using Remora.Discord.Interactivity.Extensions;
using Remora.Discord.Pagination.Extensions;

namespace OMORI_BOT
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, args) =>
            {
                args.Cancel = true;
                cts.Cancel();
            };
            var host = CreateHostBuilder(args).UseConsoleLifetime().Build();
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                context.Database.EnsureCreated();
            } // Create DB
            var services = host.Services;
            var slashService = services.GetRequiredService<SlashService>();
            await slashService.UpdateSlashCommandsAsync();
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .AddDiscordService(
                    services =>
                    {
                        var configuration = services.GetRequiredService<IConfiguration>();

                        return configuration.GetValue<string?>("BOT_TOKEN")
                               ?? throw new InvalidOperationException(
                                   "No bot token has been provided. Set the "
                                   + "BOT_TOKEN environment variable to a valid token.");
                    }
                ).ConfigureServices(
                    (_, services) =>
                    {
                        services.Configure<DiscordGatewayClientOptions>(
                            options =>
                            {
                                options.Intents |= GatewayIntents.MessageContents
                                                   | GatewayIntents.GuildMessages;
                            });
                        services.AddTransient<IConfigurationBuilder, ConfigurationBuilder>()
                            .AddDbContext<ApplicationContext>() 
                            .AddTransient<DataBaseService>()
                            .AddTransient<WhiteListService>()
                            .AddDiscordCommands(true)
                            .AddPagination()
                            .AddInteractivity()
                            .AddTransient<AccessControlService>()
                            .AddCommandTree()
                            .WithCommandGroup<AboutCommand>()
                            .WithCommandGroup<WhiteListCommands>()
                            .WithCommandGroup<PurgeCommands>()
                            .WithCommandGroup<BirthdayCommands>()
                            .WithCommandGroup<GuildSettingCommands>();
                    }).ConfigureLogging(logging => { logging.AddConsole(); });
        }

    }

}
