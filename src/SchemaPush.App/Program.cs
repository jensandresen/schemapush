using System;
using System.CommandLine;
using System.Linq;

namespace SchemaPush.App
{
    class Program
    {
        static int Main(string[] args)
        {
            var root = new RootCommand();

            var builders = typeof(ICommandBuilder)
                .Assembly
                .GetTypes()
                .Where(x => x.IsClass)
                .Where(x => typeof(ICommandBuilder).IsAssignableFrom(x))
                .Select(Activator.CreateInstance)
                .Cast<ICommandBuilder>();
            
            foreach (var commandBuilder in builders)
            {
                var command = commandBuilder.Build();
                root.AddCommand(command);
            }
            
            return root.InvokeAsync(args).GetAwaiter().GetResult();
        }
    }
}
