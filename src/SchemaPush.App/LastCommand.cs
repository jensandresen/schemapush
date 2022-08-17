using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace SchemaPush.App
{
    public class LastCommand : ICommandBuilder
    {
        public Command Build()
        {
            var lastCmd = new Command("last", "List the last X events.");

            var countOption = new Option<int>("--count", () => 5, "Number of items to return.");
            countOption.AddAlias("-n");
            lastCmd.AddOption(countOption);
            lastCmd.AddOption(new Option<string>(new[] { "--filename", "-f" }, "The filename of iCal file to read from."));

            lastCmd.Handler = CommandHandler.Create((int count, string filename) =>
            {
                var fullPath = Path.GetFullPath(filename);

                Console.WriteLine($"count: {count}");
                Console.WriteLine($"filename: {fullPath}");

                var content = File.ReadAllLines(fullPath);

                var events = EventHelper.Parse(content)
                    .OrderBy(x => x.Begin);

                foreach (var vEvent in events.TakeLast(count))
                {
                    Console.WriteLine($"{vEvent.Begin:t} > {vEvent.Description}");
                }
            });

            return lastCmd;
        }
    }

    public class TodayCommand : ICommandBuilder
    {
        public Command Build()
        {
            var links = new Dictionary<string, string>
            {
            };

            var command = new Command("list", "List events for a day.");
            command.AddOption(new Option<string>(new[] { "--name" }, "Who?"));
            command.AddOption(new Option<string>(new[] { "--day" }, "For what day?"));
            command.AddOption(new Option<bool>(new[] { "--send" }, "Send as push notification."));

            command.Handler = CommandHandler.Create(((string name, string day, bool send) =>
            {
                using var client = new HttpClient();

                var url = links[name];

                Console.WriteLine($"sending request for {name}...");

                var content = client
                    .GetStringAsync(url)
                    .GetAwaiter()
                    .GetResult();

                var lines = content
                    .Replace("\r", "")
                    .Split("\n");

                var dateOfEvents = day.ToLower() switch
                {
                    "today" => DateTime.Now.Date,
                    "tomorrow" => DateTime.Now.Date.AddDays(1),
                    _ => DateTime.Now.Date
                };

                var events = EventHelper
                    .Parse(lines)
                    .OrderBy(x => x.Begin)
                    .Where(x => x.Begin.Date == dateOfEvents)
                    .ToArray();

                if (events.Length == 0)
                {
                    Console.WriteLine($"No events available for {dateOfEvents}");
                    return;
                }

                var result = new List<string>();

                foreach (var vEvent in events)
                {
                    var text = vEvent.Summary;

                    var description = Regex.Replace(vEvent.Description, @"</\w+>", ", ");
                    description = Regex.Replace(description, @"<\w+>", "");
                    description = description.Replace(" ,", ",");
                    description = Regex.Replace(description, @",+", ",");
                    description = description.Replace(":,", ":");
                    description = Regex.Replace(description, @"\s*,\s*$", "");

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        text += ", " + description;
                    }

                    var begin = vEvent.Begin.ToLocalTime();
                    var end = vEvent.End.ToLocalTime();

                    Console.WriteLine($"{begin:t}-{end:t} > {text}");
                    result.Add($"{begin:t}-{end:t} > {text}");
                }

                if (send)
                {
                    var args = new Dictionary<string, string>
                    {
                        {"token", "..."},
                        {"user", "..."},
                        {"device", "..."},
                        {"monospace", "1"},
                        {"title", $"Skema for {name}"},
                        {"message", string.Join("\n", result)}
                    };

                    client
                        .PostAsync("https://api.pushover.net/1/messages.json", new FormUrlEncodedContent(args))
                        .GetAwaiter().GetResult();
                }
            }));

            return command;
        }
    }
}