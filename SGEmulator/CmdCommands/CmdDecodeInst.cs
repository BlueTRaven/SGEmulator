using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdDecodeInst : CmdCommand
	{
		public override string Command => "decodeinst";

		public override int minNumParams => 1;

		public override void InterpretCommand(List<string> parameters)
		{
			Word68k p1 = CmdCommandHelper.ParamToWord(parameters[0]);

			Program.decoder.DecodeInstruction(p1);
		}
	}
}
