using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public abstract class CmdCommand
	{
		public abstract string Command { get; }

		public abstract int minNumParams { get; }	//minimum number of parameters
		public virtual int maxNumParams { get; }

		public abstract void InterpretCommand(List<string> parameters);

		public virtual string GetHelp() { return ""; }
	}
}
