using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Vsts.Cli
{
    public static class CommandOptionExtensions
    {
        private const string New = "new";
        private const string Active = "active";

        public static IEnumerable<string> AsStateDefault(this CommandOption commandOption)
        {
            if (!commandOption.Template.Equals(CommandOptionTemplates.StatesTemplate, StringComparison.OrdinalIgnoreCase))
                return Enumerable.Empty<string>();

            if (commandOption.HasValue())
                return commandOption.Values;

            return new[] { New, Active };
        }
    }
}