using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdPrintAReg : CmdCommand
	{
		public override string Command => "printareg";

		public override int minNumParams => 0;

		public override void InterpretCommand(List<string> parameters)
		{
			for (int i = 0; i < 8; i++)
			{
				if (i == 7)
					Console.WriteLine("Stack Pointer: " + Program.decoder.cpu.ARegisters[i]);
				else Console.WriteLine(Program.decoder.cpu.ARegisters[i]);
			}
		}

		public override string GetHelp()
		{
			return "Prints out the contents of all address registers.";
		}
	}
}
