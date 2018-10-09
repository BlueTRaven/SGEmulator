using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdPrintReg : CmdCommand
	{
		public override string Command => "printreg";

		public override int minNumParams => 0;

		public override void InterpretCommand(List<string> parameters)
		{
			for (int i = 0; i < 8; i++)
			{
				Console.WriteLine(Program.decoder.cpu.Registers[i]);
			}
		}

		public override string GetHelp()
		{
			return "Prints out the contents of all data registers.";
		}
	}
}
