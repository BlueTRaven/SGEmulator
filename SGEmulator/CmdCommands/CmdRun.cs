using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public class CmdRun : CmdCommand
	{
		public override string Command => "run";

		public override int minNumParams => 0;

		public override void InterpretCommand(List<string> parameters)
		{
			Thread thread = new Thread(AwaitPause);
			Program.ChangeState(ProgramState.Emulate);
		}

		private void AwaitPause()
		{
			while (true)
			{
				ConsoleKeyInfo cki = Console.ReadKey(true);
				if (cki.Key == ConsoleKey.Escape)
				{
					Program.ChangeState(ProgramState.CommandLine);
					return;
				}
			}
		}

		public override string GetHelp()
		{
			return "Swaps the emulation state from command line to true emulation.\n" +
				"Pressing the esc key during this time will cause the emulator to return to the command line state.";
		}
	}
}
