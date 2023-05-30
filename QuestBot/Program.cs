using FluentScheduler;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace QuestBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initialize logs
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            // Fire everything up
            Log.Information("Loaded {Count} messages...", Quest.Messages.Count);
            Bot.Initialize();
            Log.Information("Bot is initialized...");

            // Add scheduled messages to queue
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Config.Locale);
            var registry = new Registry();
            foreach (var message in Quest.Messages.Where(m => m.SendAt != null))
            {
                registry.Schedule(() => Bot.SendMessage(message))
                    .ToRunOnceAt(message.SendAt.Hours, message.SendAt.Minutes);
            }

            JobManager.Initialize(registry);
            Log.Information("Scheduled {Count} messages...", Quest.Messages.Count(m => m.SendAt != null));

            // Manual break
            Console.WriteLine("Press Ctrl+C to stop the bot.");
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Log.Information("Stopping the bot...");
                cancellationTokenSource.Cancel();
                eventArgs.Cancel = true;
                JobManager.Stop();
            };

            await Task.Delay(-1, cancellationTokenSource.Token);
        }
    }
}