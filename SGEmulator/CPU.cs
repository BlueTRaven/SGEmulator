using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator
{
	public class CPU
	{
		#region Condition Flags
		private Word68k extendMask = new Word68k(0b0000000000010000);
		private Word68k negativeMask = new Word68k(0b0000000000001000);
		private Word68k zeroMask = new Word68k(0b0000000000000100);
		private Word68k overflowFlag = new Word68k(0b0000000000000010);
		private Word68k carryFlag = new Word68k(0b0000000000000001);

		public bool ExtendFlag
		{
			get
			{
				return (StatusRegister & extendMask).w == 1;
			}
			set
			{
				if (value)
					StatusRegister |= extendMask;
				else StatusRegister &= new Word68k((ushort)~extendMask.w);
			}
		}

		public bool NegativeFlag
		{
			get
			{
				return (StatusRegister & negativeMask).w == 1;
			}
			set
			{
				if (value)
					StatusRegister |= negativeMask;
				else StatusRegister &= new Word68k((ushort)~negativeMask.w);
			}
		}

		public bool ZeroFlag
		{
			get
			{
				return (StatusRegister & zeroMask).w == 1;
			}
			set
			{
				if (value)
					StatusRegister |= zeroMask;
				else StatusRegister &= new Word68k((ushort)~zeroMask.w);
			}
		}

		public bool OverflowFlag
		{
			get
			{
				return (StatusRegister & overflowFlag).w == 1;
			}
			set
			{
				if (value)
					StatusRegister |= overflowFlag;
				else StatusRegister &= new Word68k((ushort)~overflowFlag.w);
			}
		}

		public bool CarryFlag
		{
			get
			{
				return (StatusRegister & carryFlag).w == 1;
			}
			set
			{
				if (value)
					StatusRegister |= carryFlag;
				else StatusRegister &= new Word68k((ushort)~carryFlag.w);
			}
		}
		#endregion

		public Word68k StatusRegister { get; private set; }
		public Long68k[] Registers { get; set; }
		public Long68k[] ARegisters { get; set; }
		public Long68k Stack { get { return ARegisters[7]; } set { ARegisters[7] = value; } }

		public Long68k ProgramCounter { get; private set; }

		private byte[] memory;
		public const int maxMemory = 72000;

		public CPU(Long68k startOffset, bool trace, bool supervisor, byte interruptPriority)
		{
			ProgramCounter = startOffset;

			if (trace) StatusRegister |= new Word68k(0b1000000000000000);
			if (supervisor) StatusRegister |= new Word68k(0b0010000000000000);

			StatusRegister |= new Word68k((ushort)((interruptPriority & 0b111) << 8));

			Registers = new Long68k[8];
			ARegisters = new Long68k[8];

			memory = new byte[maxMemory];
		}

		public void ReadNextInstruction()
		{
			Console.WriteLine("Running next instruction at offset " + ProgramCounter + ".");

			//PC = our current instruction offset.
			//Read instruction from offset. Store its length.
			//Increase program counter by instruction length.

			Program.decoder.DecodeInstruction(GetWordAt(ProgramCounter));

			Long68k instLength = (Long68k)Program.decoder.instructionLength;

			ProgramCounter = BitwiseAdd(ProgramCounter, instLength, false, false, false);
		}

		#region Get And Set Addresses
		public Byte68k GetByteAt(Long68k address)
		{
			return new Byte68k(memory[address.l]);
		}

		public void SetByteAt(Long68k address, Byte68k value)
		{
			memory[address.l] = value.b;
		}

		public Word68k GetWordAt(Long68k address)
		{
			return new Word68k(BitConverter.ToUInt16(memory, (int)address.l));
		}

		public void SetWordAt(Long68k address, Word68k value)
		{
			byte[] bytes = new byte[2];

			bytes = BitConverter.GetBytes(value.w);

			SetMemory(address, bytes);
		}

		public Long68k GetLongAt(Long68k address)
		{
			return new Long68k(BitConverter.ToUInt32(memory, (int)address.l));
		}

		public void SetLongAt(Long68k address, Long68k value)
		{
			byte[] bytes = new byte[4];

			bytes = BitConverter.GetBytes(value.l);

			SetMemory(address, bytes);
		}

		public void SetMemory(Long68k address, byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				memory[address.l + i] = bytes[i];
			}
		}
		#endregion

		#region Bitwise Math
		public static Byte68k BitwiseAdd(Byte68k a, Byte68k b, bool signed, bool addXbit, bool setCondCodes = true)
		{
			Byte68k output = new Byte68k();

			int carry = 0;

			int size = Byte68k.GetSize();

			for (int i = 0; i < size; i++)
			{
				int cnum = (a[i] >> i) + (b[i] >> i) + carry;

				carry = cnum / 2;
				cnum = cnum % 2;

				output[i] = cnum;
			}

			if (addXbit)
			{
				output = BitwiseAdd(output, new Byte68k(1), signed, false);
				Program.cpu.ExtendFlag = false;
			}

			if (carry == 1)                 //leftover carry
				Program.cpu.CarryFlag = true;
			else Program.cpu.CarryFlag = false;

			if (signed && carry == 1)       //leftover carry with signed unit = overflow
				Program.cpu.OverflowFlag = true;
			else Program.cpu.OverflowFlag = false;

			if (output.b == 0)              //output is zero
				Program.cpu.ZeroFlag = true;
			else Program.cpu.ZeroFlag = false;

			if (signed && output[size - 1] == 1)        //output is negative
				Program.cpu.NegativeFlag = true;
			else Program.cpu.NegativeFlag = false;

			Program.cpu.ExtendFlag = Program.cpu.CarryFlag;

			return output;
		}

		public static Word68k BitwiseAdd(Word68k a, Word68k b, bool signed, bool addXbit, bool setCondCodes = true)
		{
			Word68k output = new Word68k();

			int carry = 0;

			int size = Word68k.GetSize();

			for (int i = 0; i < size; i++)
			{
				int cnum = (a[i] >> i) + (b[i] >> i) + carry;

				carry = cnum / 2;
				cnum = cnum % 2;

				output[i] = cnum;
			}

			if (addXbit)
			{
				output = BitwiseAdd(output, new Word68k(1), signed, false);
				Program.cpu.ExtendFlag = false;

			}

			if (carry == 1)                 //leftover carry
				Program.cpu.CarryFlag = true;
			else Program.cpu.CarryFlag = false;

			if (signed && carry == 1)       //leftover carry with signed unit = overflow
				Program.cpu.OverflowFlag = true;
			else Program.cpu.OverflowFlag = false;

			if (output.w == 0)              //output is zero
				Program.cpu.ZeroFlag = true;
			else Program.cpu.ZeroFlag = false;

			if (signed && output[size - 1] == 1)        //output is negative
				Program.cpu.NegativeFlag = true;
			else Program.cpu.NegativeFlag = false;

			Program.cpu.ExtendFlag = Program.cpu.CarryFlag;

			return output;
		}

		public static Long68k BitwiseAdd(Long68k a, Long68k b, bool signed, bool addXbit, bool setCondCodes = true)
		{
			Long68k output = new Long68k();

			int carry = 0;

			int size = Long68k.GetSize();

			for (int i = 0; i < size; i++)
			{
				int cnum = (a[i] >> i) + (b[i] >> i) + carry;

				carry = cnum / 2;
				cnum = cnum % 2;

				output[i] = cnum;
			}

			if (addXbit)
			{
				output = BitwiseAdd(output, new Long68k(1), signed, false);
				Program.cpu.ExtendFlag = false;
			}

			if (carry == 1)                 //leftover carry
				Program.cpu.CarryFlag = true;
			else Program.cpu.CarryFlag = false;

			if (signed && carry == 1)       //leftover carry with signed unit = overflow
				Program.cpu.OverflowFlag = true;
			else Program.cpu.OverflowFlag = false;

			if (output.l == 0)              //output is zero
				Program.cpu.ZeroFlag = true;
			else Program.cpu.ZeroFlag = false;

			if (signed && output[size - 1] == 1)        //output is negative
				Program.cpu.NegativeFlag = true;
			else Program.cpu.NegativeFlag = false;

			Program.cpu.ExtendFlag = Program.cpu.CarryFlag;

			return output;
		}
		#endregion

		public byte[] GetAllMemory()
		{
			return memory;
		}
	}
}
