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
		public override int maxNumParams => 3;

		public override void InterpretCommand(List<string> parameters)
		{
			Word68k p1 = CmdCommandHelper.ParamToWord(parameters[0]);
			Word68k p2 = new Word68k();
			if (parameters.Count > 1)
				p2 = CmdCommandHelper.ParamToWord(parameters[1]);

			Word68k p3 = new Word68k();
			if (parameters.Count > 2)
				p3 = CmdCommandHelper.ParamToWord(parameters[2]);

			Program.decoder.DecodeInstruction(p1, p2, p3);
		}
	}
}
