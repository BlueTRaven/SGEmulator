﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdSetAReg : CmdCommand
	{
		public override string Command => "setareg";
		public override int minNumParams => 2;

		public override void InterpretCommand(List<string> parameters)
		{
			int register = 0;
			int.TryParse(parameters[0], out register);

			if (register < 8 && register >= 0)
			{
				string param2 = parameters[1].ToLower();

				Program.decoder.cpu.ARegisters[register] = (Long68k)CmdCommandHelper.ParamToWord(param2);

				Console.WriteLine("Set address register {0} to {1}.", register, param2);
			}
			else Console.WriteLine("There is no address register {0}. Use an address register 0-7.", register);
		}
	}
}
