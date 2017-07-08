using System;
using System.Linq;
using FakeItEasy;
using Xunit;

namespace Vsts.Cli.Tests
{
    public class for_code_commands : Context
    {
        [Fact]
        public void no_args_should_launch_code_uri()
        {
            var execute = cli.Execute(CommandName.Code);

            Assert.Equal(0, execute);
            A.CallTo(() => vsts.CodeBranchUri).MustHaveHappened();
        }
    }
}