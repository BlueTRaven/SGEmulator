using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdQuit : CmdCommand
	{
		public override string Command { get => "quit"; }
		public override int minNumParams => 0;

		public override void InterpretCommand(List<string> parameters)
		{
			Program.quit = true;
		}
	}
}
