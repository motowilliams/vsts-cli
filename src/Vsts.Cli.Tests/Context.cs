using System;
using System.IO;
using System.Text;
using FakeItEasy;

namespace Vsts.Cli.Tests
{
    public abstract class Context
    {
        protected IVsts vsts;
        protected IVstsApiHelper adapter;
        protected Cli cli;

        protected Context()
        {
            vsts = A.Fake<IVsts>();
            adapter = A.Fake<IVstsApiHelper>();


            var stringWriter = new StringWriter(new StringBuilder());

            cli = new Cli(vsts, adapter, stringWriter);
        }
    }
}