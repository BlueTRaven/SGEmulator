using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SGEmulator.CmdCommands
{
	public static class CmdCommandHelper
	{
		private static readonly Regex binary = new Regex("^[01]{1,32}$", RegexOptions.Compiled);
		private static readonly Regex hex = new Regex("^[0123456789abcdef]{1,32}$", RegexOptions.Compiled);

		/// <summary>
		/// Converts a string parameter from literal binary/hex form to a word.
		/// prefix indicates type:
		/// 0b = binary
		/// 0x = hex
		/// </summary>
		public static Word68k ParamToWord(string param)
		{
			bool isbin = param.StartsWith("0b");
			bool ishex = param.StartsWith("0x");
			string sub = param.Substring(2);

			if (isbin && binary.IsMatch(sub))
			{
				return new Word68k(Convert.ToUInt16(sub, 2));
				//decoder.DecodeInstruction(binary, new Word68k(), new Word68k());
			}
			else if (ishex && hex.IsMatch(sub))
			{
				return new Word68k(Convert.ToUInt16(sub, 16));
				//decoder.DecodeInstruction(hex, new Word68k(), new Word68k());
			}
			else return new Word68k(Convert.ToUInt16(param, 10));

			return new Word68k();
		}
	}
}
