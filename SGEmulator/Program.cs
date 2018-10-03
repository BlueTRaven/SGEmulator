using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator
{
	class Program
	{
		private static bool quit;

		static void Main(string[] args)
		{
			M68kDecoder decoder = new M68kDecoder();

			
			Console.ReadKey();
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
