using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdRunTimes : CmdCommand
	{
		public override string Command => "runtimes";

		public override int minNumParams => 1;

		public override void InterpretCommand(List<string> parameters)
		{
			int times = 0;
			int.TryParse(parameters[0], out times);

			if (times > 0)
			{
				Program.emulateTimes = times;
				Program.ChangeState(ProgramState.Emulate);
			}
			else Console.WriteLine("Cannot run negative times. Param 1 (int) times must be positive.");
		}
	}
}
