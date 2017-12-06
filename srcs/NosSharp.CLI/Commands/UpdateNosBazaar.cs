using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosSharp.CLI.Interfaces;

namespace NosSharp.CLI.Commands
{
    class UpdateNosBazaar : AbstractCliCommand
    {
        public UpdateNosBazaar() : base("UpdateNB")
        {
        }

        public override string GetContent()
        {
            return "";
        }
    }
}
