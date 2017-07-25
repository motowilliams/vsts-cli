using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace Vsts.Cli
{
    public static class CommandSets
    {
        public const string WorkItemIdentifier = "work item identifier";
        public const string WorkItemTypeDescription = "work item id or type, such as epic, user story, task or bug";
        public const string WorkItemStateDescription = "filter by states such as new, active, resolved, closed or removed";
        public const string WorkItemTagsDescription = "filter by any tag that assigned to work items";
        public const string WorkItemDescriptionValue = "include description";
        public const string WorkItemForMeDescription = "only return open work items assigned to me";
        public const string WorkItemBrowseDescription = "browse specific work item in VSTS";


        public static CommandArgument WorkItemType(string value = null)
        {
            if (value == null)
            {
                return new CommandArgument
                {
                    Name = WorkItemIdentifier,
                    Description = WorkItemTypeDescription,
                    ShowInHelpText = true
                };
            }

            return new CommandArgument
            {
                Name = WorkItemIdentifier,
                Description = WorkItemTypeDescription,
                ShowInHelpText = true,
                Values = { value }
            };
        }

        public static CommandOption WorkItemState(string value = null)
        {
            if (value == null)
            {
                return new CommandOption(CommandOptionTemplates.StatesTemplate, CommandOptionType.MultipleValue)
                {
                    Description = WorkItemStateDescription,
                    ShortName = CommandOptionTemplates.StatesTemplateShort
                };

            }

            return new CommandOption(CommandOptionTemplates.StatesTemplate, CommandOptionType.MultipleValue)
            {
                Description = WorkItemStateDescription,
                ShortName = CommandOptionTemplates.StatesTemplateShort,
                Values = { value }
            };
        }

        public static CommandOption WorkItemState(params string[] value)
        {
            if (value == null)
            {
                return new CommandOption(CommandOptionTemplates.StatesTemplate, CommandOptionType.MultipleValue)
                {
                    Description = WorkItemStateDescription,
                    ShortName = CommandOptionTemplates.StatesTemplateShort
                };

            }

            var state = new CommandOption(CommandOptionTemplates.StatesTemplate, CommandOptionType.MultipleValue)
            {
                Description = WorkItemStateDescription,
                ShortName = CommandOptionTemplates.StatesTemplateShort
            };
            state.Values.AddRange(value);
            return state;
        }

        public static CommandOption WorkItemTags(string value = null)
        {
            if (value == null)
            {
                return new CommandOption(CommandOptionTemplates.TagTemplate, CommandOptionType.MultipleValue)
                {
                    Description = WorkItemTagsDescription,
                    ShortName = CommandOptionTemplates.TagTemplateShort
                };
            }

            return new CommandOption(CommandOptionTemplates.TagTemplate, CommandOptionType.MultipleValue)
            {
                Description = WorkItemTagsDescription,
                ShortName = CommandOptionTemplates.TagTemplateShort,
                Values = { value }
            };
        }

        public static CommandOption WorkItemTags(params string[] value)
        {
            if (value == null)
            {
                return new CommandOption(CommandOptionTemplates.TagTemplate, CommandOptionType.MultipleValue)
                {
                    Description = WorkItemTagsDescription,
                    ShortName = CommandOptionTemplates.TagTemplateShort,
                };
            }

            var tag = new CommandOption(CommandOptionTemplates.TagTemplate, CommandOptionType.MultipleValue)
            {
                Description = WorkItemTagsDescription,
                ShortName = CommandOptionTemplates.TagTemplateShort
            };
            tag.Values.AddRange(value);

            return tag;
        }

        public static CommandOption WorkItemDescription(string value = null)
        {
            if (value == null)
            {
                return new CommandOption(CommandOptionTemplates.DescriptionTemplate, CommandOptionType.NoValue)
                {
                    Description = WorkItemDescriptionValue,
                    ShortName = CommandOptionTemplates.DescriptionTemplateShort
                };
            }

            return new CommandOption(CommandOptionTemplates.DescriptionTemplate, CommandOptionType.NoValue)
            {
                Description = WorkItemDescriptionValue,
                ShortName = CommandOptionTemplates.DescriptionTemplateShort,
                Values = { value }
            };
        }

        public static CommandOption WorkItemForMe(string value = null)
        {
            if (value == null)
            {
                return new CommandOption(CommandOptionTemplates.MyTemplate, CommandOptionType.NoValue)
                {
                    Description = WorkItemForMeDescription,
                    ShortName = CommandOptionTemplates.MyTemplateShort
                };
            }
            return new CommandOption(CommandOptionTemplates.MyTemplate, CommandOptionType.NoValue)
            {
                Description = WorkItemForMeDescription,
                ShortName = CommandOptionTemplates.MyTemplateShort,
                Values = { value }
            };
        }

        public static CommandOption WorkItemBrowse(string value = null)
        {
            if (value == null)
            {
                return new CommandOption(CommandOptionTemplates.BrowseTemplate, CommandOptionType.NoValue)
                {
                    Description = WorkItemBrowseDescription,
                    ShortName = CommandOptionTemplates.BrowseTemplateShort
                };
            }
            return new CommandOption(CommandOptionTemplates.BrowseTemplate, CommandOptionType.NoValue)
            {
                Description = WorkItemBrowseDescription,
                ShortName = CommandOptionTemplates.BrowseTemplateShort,
                Values = { value }
            };
        }
    }

    public static class CommandLineApplicationExtensions
    {
        public static CommandArgument AsWorkItemCommand(this CommandLineApplication source)
        {
            return source.Arguments.FirstOrDefault(x => x.Name.Equals(CommandSets.WorkItemIdentifier));
        }

        public static CommandOption AsWorkItemState(this CommandLineApplication source)
        {
            return source.Options.FirstOrDefault(x => x.Template.Equals(CommandOptionTemplates.StatesTemplate));
        }

        public static CommandOption AsWorkItemTags(this CommandLineApplication source)
        {
            return source.Options.FirstOrDefault(x => x.Template.Equals(CommandOptionTemplates.TagTemplate));
        }

        public static CommandOption AsWorkItemDescription(this CommandLineApplication source)
        {
            return source.Options.FirstOrDefault(x => x.Template.Equals(CommandOptionTemplates.DescriptionTemplate));
        }

        public static CommandOption AsWorkItemForMe(this CommandLineApplication source)
        {
            return source.Options.FirstOrDefault(x => x.Template.Equals(CommandOptionTemplates.MyTemplate));
        }

        public static CommandOption AsWorkItemBrowse(this CommandLineApplication source)
        {
            return source.Options.FirstOrDefault(x => x.Template.Equals(CommandOptionTemplates.BrowseTemplate));
        }
    }
}
