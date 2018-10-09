using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdGetReg : CmdCommand
	{
		public override string Command => "getreg";

		public override int minNumParams => 1;

		public override void InterpretCommand(List<string> parameters)
		{
			int register = 0;
			int.TryParse(parameters[0], out register);

			if (register < 8 && register >= 0)
			{
				Long68k registerContents = Program.cpu.Registers[register];

				Console.WriteLine("Contents of register {0} are: {1}.", register, registerContents);
			}
			else Console.WriteLine("There is no register {0}. Use A register 0-7.", register);
		}
	}
}
