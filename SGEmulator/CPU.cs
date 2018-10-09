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
		private Long68k[] Registers { get; set; }
		private Long68k[] ARegisters { get; set; }
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

			Program.decoder.DecodeInstruction(GetMemWord(ProgramCounter));

			Long68k instLength = (Long68k)Program.decoder.instructionLength;

			ProgramCounter = BitwiseAdd(ProgramCounter, instLength, false, false, false);
		}

        #region Get And Set Registers
        /// <summary>
        /// Gets the low order byte in the given register.
        /// </summary>
        /// <param name="addressReg">If true, indexes into an address register instead of a data register.
        /// Note that is the contents of the register, not the contents that the register points to.</param>
        public Byte68k GetRegByte(int register, bool addressReg)
        {
            Long68k mask = new Long68k(0xFF);

            if (addressReg)
                return new Byte68k(mask & ARegisters[register]);
            else return new Byte68k(mask & Registers[register]);
        }

        /// <summary>
        /// Gets the low order word in the given register.
        /// </summary>
        /// <param name="addressReg">If true, indexes into an address register instead of a data register.
        /// Note that is the contents of the register, not the contents that the register points to.</param>
        public Word68k GetRegWord(int register, bool addressReg)
        {
            Long68k mask = new Long68k(0xFFFF);

            if (addressReg)
                return new Word68k(mask & ARegisters[register]);
            else return new Word68k(mask & Registers[register]);
        }

        /// <summary>
        /// Gets the long in the given register.
        /// </summary>
        /// <param name="addressReg">If true, indexes into an address register instead of a data register.
        /// Note that is the contents of the register, not the contents that the register points to.</param>
        public Long68k GetRegLong(int register, bool addressReg)
        {
            if (addressReg)
                return ARegisters[register];
            else return Registers[register];
        }

        /// <summary>
        /// Sets a byte in the given register index.
        /// </summary>
        /// <param name="addressReg">If true, indexes into an address register instead of data register. 
        /// Note that this does not set the contents of memory that the register points to, just the pointer itself.</param>
        /// <param name="overwrite">should the byte overwrite the entire contents of the data register. If false only the low order 8 bits are set.</param>
        public void SetRegByte(Byte68k b, int register, bool addressReg, bool overwrite = false)
        {
            if (!overwrite)
            {
                Long68k content = new Long68k((Registers[register].l & 0xFF)) | (Long68k)b;

                if (addressReg)
                    ARegisters[register] = content;
                else Registers[register] = content;
            }
            else
            {
                if (addressReg)
                    ARegisters[register] = (Long68k)b;
                else Registers[register] = (Long68k)b;
            }
        }

        /// <summary>
        /// Sets a word in the given data register index.
        /// </summary>
        /// <param name="addressReg">If true, indexes into an address register instead of data register. 
        /// Note that this does not set the contents of memory that the register points to, just the pointer itself.</param>
        /// <param name="overwrite">should the byte overwrite the entire contents of the data register. If false only the low order 16 bits are set.</param>
        public void SetRegWord(Word68k w, int register, bool addressReg, bool overwrite = false)
        {
            if (!overwrite)
            {
                Long68k content = new Long68k((Registers[register].l & 0xFFFF)) | (Long68k)w;

                if (addressReg)
                    ARegisters[register] = content;
                else Registers[register] = content;
            }
            else
            {
                if (addressReg)
                    ARegisters[register] = (Long68k)w;
                else Registers[register] = (Long68k)w;
            }
        }

        /// <summary>
        /// Sets a long in the given data register index.
        /// Note that there is no overwrite parameter; data registers are a long in length, therefore
        /// we must overwrite the entire contents.
        /// <param name="addressReg">If true, indexes into an address register instead of data register. 
        /// Note that this does not set the contents of memory that the register points to, just the pointer itself.</param>
        /// </summary>
        public void SetRegLong(Long68k l, int register, bool addressReg)
        {
            if (addressReg)
                ARegisters[register] = l;
            else Registers[register] = l;
        }
        #endregion

        #region Get And Set Memory
        public Byte68k GetMemByte(Long68k address)
		{
			return new Byte68k(memory[address.l]);
		}

		public void SetMemByte(Long68k address, Byte68k value)
		{
			memory[address.l] = value.b;
		}

		public Word68k GetMemWord(Long68k address)
		{
			return new Word68k(BitConverter.ToUInt16(memory, (int)address.l));
		}

		public void SetMemWord(Long68k address, Word68k value)
		{
			byte[] bytes = new byte[2];

			bytes = BitConverter.GetBytes(value.w);

			SetMemBytes(address, bytes);
		}

		public Long68k GetMemLong(Long68k address)
		{
			return new Long68k(BitConverter.ToUInt32(memory, (int)address.l));
		}

		public void SetMemLong(Long68k address, Long68k value)
		{
			byte[] bytes = new byte[4];

			bytes = BitConverter.GetBytes(value.l);

			SetMemBytes(address, bytes);
		}

		public void SetMemBytes(Long68k address, byte[] bytes)
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
