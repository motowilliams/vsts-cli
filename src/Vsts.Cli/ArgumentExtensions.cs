using System;
using System.Linq;
using Vsts.Cli;

namespace Vsts.Cli
{
    public static class ArgumentExtensions
    {
        public static string[] AsUpdatedArray(this string[] args)
        {
            //Empty or otherwise no intenet passed to the cli
            if (args == null || args.Length == 0 || (args.Length == 1 && args[0] == string.Empty))
            {
                Console.Logo();
                args = new[] { CommandName.DashH };
            }

            if (args.Any())
            {
                var strings = CommandName.HelpTemplate.Split('|').Select(x => x.Trim());
                if (!strings.Any(x => x.Equals(args[0], StringComparison.OrdinalIgnoreCase)))
                    args[0] = args[0].Normalize();
            }
            else
            {
                args = new[] { CommandName.DashH };
            }
            return args;
        }
    }
}