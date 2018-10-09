using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdClear : CmdCommand
	{
		public override string Command => "clear";

		public override int minNumParams => 0;

		public override void InterpretCommand(List<string> parameters)
		{
			Console.Clear();
		}
	}
}
