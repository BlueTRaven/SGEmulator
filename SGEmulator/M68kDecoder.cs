using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator
{
	public enum Size
	{
		Byte = 0,
		Word = 1,
		Long = 2
	}

	public enum Opmode
	{
		DByte = 0b000,
		DWord = 0b001,
		DLong = 0b010,
		RDByte = 0b100,	//reversed
		RDWord = 0b101,
		RDLong = 0b110,
		AWord = 0b011,
		ALong = 0b111,
	}

	public class M68kDecoder
	{
		Dictionary<string, OpCode> opCodes;

		private static Word68k reg1Mask = 0b111_000000000;
		private static Word68k reg2Mask = 0b111;

		private static Word68k opmodeMask = 0b111_000000;
		private static Word68k opmodeRMask = 0b100;

		private static Word68k instructionMask = 0b1111_000000000000;
		private ushort cregister;
		private Long68k[] registers;
		private Long68k[] aregisters;
		private Long68k stack;

		private byte[] memory;

		public M68kDecoder()
		{
			opCodes = new Dictionary<string, OpCode>();

			registers = new Long68k[8];
			aregisters = new Long68k[8];

			memory = new byte[72000];

			registers[0] = 5;
			registers[7] = 32;
			RegisterOpCodes();

			Word68k word = 0b1101_111_101_000_000;

			InterpretInstruction(word);
		}

		private void InterpretInstruction(Word68k instruction)
		{
			if ((instruction & instructionMask) == 0b1101_000000000000)    //add operator
			{
				InterpretADD(instruction);
			}
		}

		private void InterpretADD(Word68k instruction)
		{
			Opmode mode = GetOpmodeFrom(instruction);

			bool reverse = GetOpmodeReverse((Word68k)mode);    //do we output to the second register (true) or the first (false)

			Word68k reg1 = (instruction & reg1Mask) >> 9;
			Word68k reg2 = instruction & reg2Mask;

			if (mode == Opmode.ALong || mode == Opmode.AWord)
			{
				if (mode == Opmode.ALong)
				{
					Long68k a = GetLongAt(aregisters[reg1]);
					Long68k b = GetLongAt(aregisters[reg2]);

					SetLongAt(aregisters[reg1], a + b);
				}
				else if (mode == Opmode.AWord)
				{
					Word68k a = GetWordAt(aregisters[reg1]);
					Word68k b = GetWordAt(aregisters[reg2]);

					SetWordAt(aregisters[reg1], a + b);
				}
			}
			else
			{
				if (!reverse)
				{
					registers[reg1] = registers[reg2] + registers[reg1];
				}
				else
				{
					registers[reg2] = registers[reg1] + registers[reg2];
				}
			}
		}

		private Type GetOpmodeCast(Opmode mode)
		{
			switch (mode)
			{
				case Opmode.DByte:
				case Opmode.RDByte:
					return typeof(Byte68k);
				case Opmode.DLong:
				case Opmode.RDLong:
				case Opmode.ALong:
					return typeof(Long68k);
				case Opmode.DWord:
				case Opmode.RDWord:
				case Opmode.AWord:
					return typeof(Word68k);

				default: return null;
			}
		}

		private dynamic GetAndCast(dynamic t, Opmode mode)
		{
			switch (mode)
			{
				case Opmode.DByte:
				case Opmode.RDByte:
					return (Byte68k)t;
				case Opmode.DLong:
				case Opmode.RDLong:
					return (Long68k)t;
				case Opmode.DWord:
				case Opmode.RDWord:
					return (Word68k)t;
				case Opmode.ALong:	//TODO get value at address, cast and return it
				case Opmode.AWord:
					return 0;
				default: return 0;
			}
		}

		#region Get And Set Addresses
		private Byte68k GetByteAt(Long68k address)
		{
			return memory[address];
		}

		private void SetByteAt(Long68k address, Byte68k value)
		{
			memory[address] = value;
		}

		private Word68k GetWordAt(Long68k address)
		{
			byte b1 = memory[address];
			byte b2 = memory[address + 1];

			Word68k combined = b1 << 8 | b2;
			return combined;
		}

		private void SetWordAt(Long68k address, Word68k value)
		{
			byte[] bytes = new byte[2];

			bytes = BitConverter.GetBytes(value);

			SetMemory(address, bytes);
		}

		private Long68k GetLongAt(Long68k address)
		{
			byte[] bytes = new byte[4];

			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = memory[address + i];
			}

			Long68k combined = BitConverter.ToUInt32(bytes, 0);
			return combined;
		}

		private void SetLongAt(Long68k address, Long68k value)
		{
			byte[] bytes = new byte[4];

			bytes = BitConverter.GetBytes(value);

			SetMemory(address, bytes);
		}

		private void SetMemory(Long68k address, byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				memory[address + i] = bytes[i];
			}
		}
		#endregion

		private Opmode GetOpmodeFrom(Word68k b)
		{
			Word68k f = (b & opmodeMask) >> 6;

			return (Opmode)(int)f;
		}

		private bool GetOpmodeReverse(Word68k opmode)
		{
			return ((opmode & opmodeRMask) >> 2) == 1;
		}

		public void GetAtAddress()
		{

		}

		public void RegisterOpCodes()
		{
			opCodes.Add("add", new OpCode(0b1101_0000));
		}
	}

	#region structs
	public struct Byte68k
	{
		public byte b;

		public Byte68k(byte b)
		{
			this.b = b;
		}

		public Byte68k(Byte68k b)
		{
			this.b = b.b;
		}

		public static Byte68k operator &(Byte68k b1, Byte68k b2)
		{
			return new Byte68k(b1.b & b2.b);
		}

		public static Byte68k operator |(Byte68k b1, Byte68k b2)
		{
			return new Byte68k(b1.b | b2.b);
		}

		public static Byte68k operator >>(Byte68k b, int shiftBits)
		{
			return new Byte68k((byte)(b.b >> shiftBits));
		}

		public static Byte68k operator <<(Byte68k b, int shiftBits)
		{
			return new Byte68k((byte)(b.b << shiftBits));
		}

		public static implicit operator byte(Byte68k b)
		{
			return b.b;
		}

		public static implicit operator Byte68k(int b)
		{
			return new Byte68k(b);
		}

		public override string ToString()
		{
			return b.ToString();
		}
	}

	public struct Word68k
	{
		public ushort w;

		public Word68k(ushort w)
		{
			this.w = w;
		}

		public Word68k(Word68k w)
		{
			this.w = w.w;
		}

		public static Word68k operator &(Word68k w1, Word68k w2)
		{
			return new Word68k(w1.w & w2.w);
		}

		public static Word68k operator |(Word68k w1, Word68k w2)
		{
			return new Word68k(w1.w | w2.w);
		}

		public static Word68k operator >>(Word68k w, int shiftBits)
		{
			return new Word68k((ushort)(w.w >> shiftBits));
		}

		public static Word68k operator <<(Word68k w, int shiftBits)
		{
			return new Word68k((ushort)(w.w << shiftBits));
		}
		
		public static explicit operator Word68k(Opmode opm)
		{
			return new Word68k((ushort)opm);
		}

		public static implicit operator ushort(Word68k w)
		{
			return w.w;
		}

		public static implicit operator Word68k(int w)
		{
			return new Word68k((ushort)w);
		}

		public override string ToString()
		{
			return w.ToString();
		}
	}

	public struct Long68k
	{
		public uint l;

		public Long68k(uint l)
		{
			this.l = l;
		}

		public Long68k(Long68k l)
		{
			this.l = l.l;
		}

		public static Long68k operator &(Long68k l1, Long68k l2)
		{
			return new Long68k(l1.l & l2.l);
		}

		public static Long68k operator |(Long68k l1, Long68k l2)
		{
			return new Long68k(l1.l | l2.l);
		}

		public static Long68k operator >>(Long68k l, int shiftBits)
		{
			return new Long68k(l.l >> shiftBits);
		}

		public static Long68k operator <<(Long68k l, int shiftBits)
		{
			return new Long68k(l.l << shiftBits);
		}

		public static implicit operator uint(Long68k l)
		{
			return l.l;
		}

		public static implicit operator Long68k(uint l)
		{
			return new Long68k(l);
		}

		public override string ToString()
		{
			return l.ToString();
		}
	}
	#endregion

	public struct OpCode
	{
		private byte format;

		public OpCode(byte format)
		{
			this.format = format;
		}
	}
}
