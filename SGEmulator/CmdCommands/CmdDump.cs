using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SGEmulator.CmdCommands
{
	public class CmdDump : CmdCommand
	{
		public override string Command => "dump";

		public override int minNumParams => 0;
		public override int maxNumParams => 2;

		private static bool dumping;
		private static Thread thread;

		private int offset;
		private int count;

		private Stopwatch watch;

		public override void InterpretCommand(List<string> parameters)
		{
			if (dumping)
			{
				Console.WriteLine("Cannot start a dump while another is running.");
				return;
			}

			dumping = true;

			offset = 0;
			count = CPU.maxMemory;

			if (parameters.Count > 0)
				int.TryParse(parameters[0], out offset);

			if (parameters.Count > 1)
				int.TryParse(parameters[1], out count);

			thread = new Thread(StartDump);
			thread.Start();
				//fs.BeginWrite(Program.cpu.GetAllMemory(), 0, CPU.maxMemory, DumpFinished, Program.cpu);	
		}

		private void StartDump()
		{
			watch = Stopwatch.StartNew();

			if (!Directory.Exists(Directory.GetCurrentDirectory() + "/dumps/"))
				Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/dumps/");

			string filename = "bindump_" + Guid.NewGuid() + ".bin";
			Console.WriteLine("Dumping {0} bytes starting from offset {1} to file {2}.", count, offset, filename);

			using (FileStream fs = File.OpenWrite(Directory.GetCurrentDirectory() + "/dumps/" + filename))
			{
				DumpMemory(fs, offset, count);
			}
		}

		private void DumpMemory(FileStream fileStream, int offset, int count)
		{
			fileStream.Write(Program.cpu.GetAllMemory(), offset, count);
			DumpFinished();
		}

		private void DumpFinished()
		{
			Console.WriteLine("Dumping finished. Elapsed time: {0}", watch.Elapsed);
			dumping = false;
		}

		public override string GetHelp()
		{
			return "Dumps the entire contents of memory to a file located in the dumps folder. This operation is asynchronous.\n" +
				"Parameters:\n" +
				"1: (int) The memory offset to begin reading from. Default 0.\n" +
				"2: (int) The amount of memory to read. Default CPU.maxMemory.";
		}
	}
}
