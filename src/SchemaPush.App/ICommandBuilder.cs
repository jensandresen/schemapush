using System.CommandLine;

namespace SchemaPush.App
{
    public interface ICommandBuilder
    {
        Command Build();
    }
}