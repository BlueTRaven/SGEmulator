using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdRunOnce : CmdCommand
	{
		public override string Command => "runonce";

		public override int minNumParams => 0;

		public override void InterpretCommand(List<string> parameters)
		{
			Program.emulateTimes = 1;
			Program.ChangeState(ProgramState.Emulate);
		}
	}
}
