using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator
{
	public enum Size
	{
		Byte = 0b00,
		Word = 0b01,
		Long = 0b10
	}

	public static class InstructionUtils
	{
		private static Word68k sizemask = new Word68k(0b00000000_11_000000);

		#region register
		private static Word68k sourceMask = new Word68k(0b111_000000000);
		private static Word68k destMask = new Word68k(0b111);

		public static Byte68k GetSourceReg(Word68k instruction)
		{
			return (instruction & sourceMask) >> 9;
		}

		public static Byte68k GetDestReg(Word68k instruction)
		{
			return instruction & destMask;
		}
		#endregion

		#region Opmode
		private static Word68k opmodeMask = new Word68k(0b111_000000);
		private static Word68k opmodeRMask = new Word68k(0b100);

		public static Opmode GetOpmodeFrom(Word68k b)
		{
			Word68k f = (b & opmodeMask) >> 6;

			return (Opmode)(int)f;
		}

		public static bool GetOpmodeReverse(Word68k opmode)
		{
			return (ushort)((opmode & opmodeRMask) >> 2) == 1;
		}
		#endregion

		/// <summary>
		/// Gets the size of a given instruction, assuming size is in bits 6&7.
		/// </summary>
		public static Size GetSize(Word68k instruction)
		{
			Word68k size = (instruction & sizemask) >> 6;

			return (Size)size.w;
		}

		public static string Reverse(string s)
		{
			char[] charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		public static string ToBin(int value, int len)
		{
			return (len > 1 ? ToBin(value >> 1, len - 1) : null) + "01"[value & 1];
		}
	}
}
