using FluentScheduler;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuestBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Fire everything up
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Loaded {Quest.Messages.Count} messages...");
            Bot.Initialize();
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Bot is initialized...");

            // Add scheduled messages to queue
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Config.Locale);
            var registry = new Registry();
            foreach (var message in Quest.Messages.Where(m => m.SendAt != null))
            {
                registry.Schedule(() => Bot.SendMessage(message))
                    .ToRunOnceAt(message.SendAt.Hours, message.SendAt.Minutes);
            }

            JobManager.Initialize(registry);
            Console.WriteLine(
                $"{DateTime.Now:HH:mm:ss} - Scheduled {Quest.Messages.Count(m => m.SendAt != null)} messages...");

            // Manual break
            Console.WriteLine("Press Ctrl+C to stop the bot.");
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Stopping the bot...");
                cancellationTokenSource.Cancel();
                eventArgs.Cancel = true;
            };

            await Task.Delay(-1, cancellationTokenSource.Token);
        }
    }
}