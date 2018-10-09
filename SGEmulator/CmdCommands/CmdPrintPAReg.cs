using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdPrintPAReg : CmdCommand
	{
		public override string Command => "printpareg";

		public override int minNumParams => 1;

		public override void InterpretCommand(List<string> parameters)
		{
			string size = parameters[0].ToLower();

			for (int i = 0; i < 8; i++)
			{
				Long68k address = Program.cpu.ARegisters[i];

				switch (size)
				{
					case "byte":
					case "b":
						Console.WriteLine(Program.cpu.GetByteAt(address));
						break;
					case "word":
					case "w":
						Console.WriteLine(Program.cpu.GetWordAt(address));
						break;
					case "long":
					case "l":
						Console.WriteLine(Program.cpu.GetLongAt(address));
						break;
				}
			}
		}
	}
}
