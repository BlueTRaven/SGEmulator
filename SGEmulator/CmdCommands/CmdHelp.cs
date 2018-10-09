using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdHelp : CmdCommand
	{
		public override string Command => "help";

		public override int minNumParams => 1;

		public override void InterpretCommand(List<string> parameters)
		{
			if (Program.commands.ContainsKey(parameters[0]))
			{
				Console.WriteLine(Program.commands[parameters[0]].GetHelp());
			}
			else Console.WriteLine("Could not find command {0}.", parameters[0]);
		}

		public override string GetHelp()
		{
			return "Gets help regarding a given command.";
		}
	}
}
