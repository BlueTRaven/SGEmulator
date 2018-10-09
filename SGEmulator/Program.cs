using SGEmulator.CmdCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SGEmulator
{
	public enum ProgramState
	{
		CommandLine,	//command line emulation
		Emulate		//emulate cpu completely
	}

	public class Program
	{
		public static bool quit;
		public static Decoder decoder;
		public static CPU cpu;

		public static Dictionary<string, CmdCommand> commands;

		private static ProgramState currentState = ProgramState.CommandLine;
		public static int emulateTimes = -1;		//run in emulation state for this many instructions, then return to command line state

		static void Main(string[] args)
		{
			commands = new Dictionary<string, CmdCommand>();
			List<CmdCommand> cmds = GetAllCommands();
			cmds.ForEach(x => commands.Add(x.Command, x));

			cpu = new CPU(new Long68k(), true, true, 0);
			decoder = new Decoder();

			while (!quit)
			{
				if (currentState == ProgramState.CommandLine)
				{
					string input = Console.ReadLine();

					InterpretConsole(input);
				}
				else if (currentState == ProgramState.Emulate)
				{
					cpu.ReadNextInstruction();

					if (emulateTimes != -1)
					{
						emulateTimes--;
						if (emulateTimes == 0)
						{
							ChangeState(ProgramState.CommandLine);
							emulateTimes = -1;
						}
					}
				}
			}
		}

		public static void ChangeState(ProgramState state)
		{
			currentState = state;
			Console.WriteLine("Changing to " + state.ToString() + " state.");
		}

		private static void InterpretConsole(string consoleIn)
		{
			if (consoleIn != null)
			{
				string[] split = consoleIn.Split(' ');

				if (split != null && split[0] != null)
				{
					string command = split[0].ToLower();

					List<string> parameters = new List<string>();

					for (int i = 1; i < split.Length; i++)
						parameters.Add(split[i]);


					if (commands.ContainsKey(command))
					{
						CmdCommand cCommand = commands[command];

						if (parameters.Count < cCommand.minNumParams)
						{
							if (cCommand.maxNumParams != 0 && parameters.Count > cCommand.maxNumParams)
							{
								Console.WriteLine("Not enough, or too many, parameters. Num given: {0}, num required: {1} - {2}", parameters.Count, cCommand.minNumParams, cCommand.maxNumParams);
							}
							else Console.WriteLine("Not enough, or too many, parameters. Num given: {0}, num required: {1} - {1}", parameters.Count, cCommand.minNumParams);

							return;
						}

						cCommand.InterpretCommand(parameters);
					}
				}
			}
		}

		private static List<CmdCommand> GetAllCommands()
		{
			List<CmdCommand> commands = new List<CmdCommand>();

			List<Type> types = Assembly.GetExecutingAssembly().GetTypes().ToList();

			foreach (Type type in types)
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(CmdCommand)))
				{
					commands.Add(Activator.CreateInstance(type) as CmdCommand);
				}
			}

			return commands;
		}
		
		static void DoLoop()
		{
			EmMain main = new EmMain();

			Stopwatch watch = new Stopwatch();

			double time = 0;
			double timeStep = 1 / 60;

			double accumulator = 0;

			double currentTime = watch.ElapsedMilliseconds;

			while (!quit)
			{
				double nowTime = watch.ElapsedMilliseconds;
				double frameTime = nowTime - currentTime;

				if (frameTime > 0.25d)
					frameTime = 0.25d;

				currentTime = nowTime;

				accumulator += frameTime;

				while (accumulator >= timeStep)
				{
					float deltaTime = (float)Math.Min(frameTime, timeStep);

					main.Update(time, deltaTime);

					frameTime -= deltaTime;
					time += deltaTime;
				}
			}
		}
	}
}
