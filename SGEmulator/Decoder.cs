using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator
{
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

	public enum AddressingMode
	{	//s = one of b, w, l
		Dn				= 0b000,
		An				= 0b001,
		IndirAn			= 0b010,
		IndirAnPostInc	= 0b011,
		IndirAnPreDec	= 0b100,
		IndirAnDisp		= 0b101,	//An + disp (32 bit sign extand) = ea
		IndirAnDisp8b	= 0b110,	//An + (Xn + 8bdisp (32b sign extend)) = ea
		IndirAnDispBase = 0b110,	//An + ((Xn) + 32b disp) = ea
		IndirMemPostInd = 0b110,	//(An + bd) + Xn.s * scale + od
		IndirMemPreInd	= 0b110,	//(bd + An) + Xn.s * scale + od
		IndirPCDisp		= 0b111,	//PC + 16b (32b sign extend)
	}

	public class Decoder
	{
		private static Word68k maskABCD = new Word68k(0b1111_000_11111_0000);
		private static Word68k instABCD = new Word68k(0b1100_000_10000_0000);
		private static Word68k maskADD = new Word68k(0b1111_000000000000);
		private static Word68k instADD = new Word68k(0b1101_000000000000);
		private static Word68k maskADDI = new Word68k(0b11111111_00000000);
		private static Word68k instADDI = new Word68k(0b00000110_00000000);
		private static Word68k maskADDQ = new Word68k(0b1111_000_1_00_000000);
		private static Word68k instADDQ = new Word68k(0b0101_000_0_00_000000);
		private static Word68k maskADDX = new Word68k(0b1111_000_1_00_11_0000);
		private static Word68k instADDX = new Word68k(0b1101_000_1_00_00_0000);

		public string output;

		public Byte68k instructionLength;   //current instruction length in bytes
		public string instructionName;

		public Decoder()
		{
			Program.cpu.SetMemWord(new Long68k(0), new Word68k(0b00000110_01_000_111));      //addi add reg 7 to word-sized data in next 16 bits
			Program.cpu.SetMemWord(new Long68k(2), new Word68k(16));                         

			Program.cpu.SetMemWord(new Long68k(4), new Word68k(0b0101_111_0_10_000_111));    //addq add 7 to reg 7

			Program.cpu.Registers[7] = new Long68k(32);
		}

		public void DecodeInstruction(Word68k instruction)
		{
			instructionLength = new Byte68k(0);
			instructionName = "no instruction";
			output = "no output";

			if ((ushort)(instruction & maskABCD) == instABCD.w)
			{   //ABCD
				InstructionABCD(instruction);
			}
			else if ((ushort)(instruction & maskADD) == instADD.w)
			{	//ADD, ADDA
				InstructionADD(instruction);
			}
			else if ((ushort)(instruction & maskADDI) == instADDI.w)
			{
				InstructionADDI(instruction);
			}
			else if ((ushort)(instruction & maskADDQ) == instADDQ.w)
			{
				InstructionADDQ(instruction);
			}
			else if ((ushort)(instABCD & maskADDX) == instADDX.w)
			{
				InstructionADDX(instruction);
			}

            Console.WriteLine("Instruction: {0}, length: {1}", instructionName, instructionLength.b);

			Console.WriteLine("Output: {0}", output);
		}

		private void InstructionABCD(Word68k instruction)
		{
			instructionName = "ABCD";
			instructionLength = new Byte68k(16);
		}

		private void InstructionADD(Word68k instruction)
		{
			instructionName = "ADD";
			instructionLength = new Byte68k(16);
			Opmode mode = InstructionUtils.GetOpmodeFrom(instruction);

			bool reverse = InstructionUtils.GetOpmodeReverse((Word68k)mode);    
			//do we output to the second register (true) or the first (false)
			//only used for the ADD instruction, not ADDA

			Byte68k reg1 = InstructionUtils.GetSourceReg(instruction);
			Byte68k reg2 = InstructionUtils.GetDestReg(instruction);

			if (mode == Opmode.ALong || mode == Opmode.AWord)
			{   //ADDA instruction
				instructionName = "ADDA";

				if (mode == Opmode.ALong)
				{
					Long68k a = Program.cpu.GetMemLong(Program.cpu.ARegisters[reg1.b]);
					Long68k b = Program.cpu.GetMemLong(Program.cpu.ARegisters[reg2.b]);

					Program.cpu.SetMemLong(Program.cpu.ARegisters[reg1.b], CPU.BitwiseAdd(a, b, true, false));

					output = Program.cpu.ARegisters[reg1.b].ToString();
				}
				else if (mode == Opmode.AWord)
				{
					Word68k a = Program.cpu.GetMemWord(Program.cpu.ARegisters[reg1.b]);
					Word68k b = Program.cpu.GetMemWord(Program.cpu.ARegisters[reg2.b]);

					Program.cpu.SetMemWord(Program.cpu.ARegisters[reg1.b], CPU.BitwiseAdd(a, b, true, false));

					output = Program.cpu.ARegisters[reg1.b].ToString();
				}
			}
			else
			{	//ADD instruction
				if (!reverse)
				{
					Program.cpu.Registers[reg1.b] = CPU.BitwiseAdd(Program.cpu.Registers[reg2.b], Program.cpu.Registers[reg1.b], true, false);
					//cpu.Registers[reg1.b] = cpu.Registers[reg2.b] + cpu.Registers[reg1.b];

					output = Program.cpu.Registers[reg1.b].ToString();
				}
				else
				{
					Program.cpu.Registers[reg2.b] = CPU.BitwiseAdd(Program.cpu.Registers[reg1.b], Program.cpu.Registers[reg2.b], true, false);
					//cpu.Registers[reg2.b] = cpu.Registers[reg1.b] + cpu.Registers[reg2.b];

					output = Program.cpu.Registers[reg2.b].ToString();
				}
			}
		}

		private void InstructionADDI(Word68k instruction)
		{
			instructionName = "ADDI";
			Word68k a = Program.cpu.GetMemWord(CPU.BitwiseAdd(Program.cpu.ProgramCounter, new Long68k(2), false, false, false)); //get the next word after the instruction
			Word68k b = Program.cpu.GetMemWord(CPU.BitwiseAdd(Program.cpu.ProgramCounter, new Long68k(4), false, false, false)); //get the next word after that
			//note that if we're using a byte or word b will be junk data

			Word68k sizemask = new Word68k(0b00000000_11_000000);

			Word68k size = (instruction & sizemask) >> 6;

			Word68k byteMask = new Word68k(0b11111111);

			Long68k add = new Long68k();

			switch ((Size)size.w)
			{
				case Size.Byte:
					add = (Long68k)(a | byteMask);
					instructionLength = new Byte68k(4);    //we still take 32 bits even though we only use the lower 8 bits of the second word.
					break;
				case Size.Word:
					add = (Long68k)a;
					instructionLength = new Byte68k(4);
					break;
				case Size.Long:
					add = a.ToLong(b);
					instructionLength = new Byte68k(6);
					break;
			}

			Byte68k register = InstructionUtils.GetDestReg(instruction);

			Program.cpu.Registers[register.b] = CPU.BitwiseAdd(add, Program.cpu.Registers[register.b], true, false);

			output = Program.cpu.Registers[register.b].ToString();
		}

		private void InstructionADDQ(Word68k instruction)
		{   //get immediate data
			instructionName = "ADDQ";
			instructionLength = new Byte68k(2);

			Byte68k data = new Byte68k((instruction & new Word68k(0b0000_111_000000000)) >> 9);

			Word68k sizemask = new Word68k(0b00000000_11_000000);

			Word68k size = (instruction & sizemask) >> 6;

			bool isAR = false;

			Byte68k register = InstructionUtils.GetSourceReg(instruction);
			if (!isAR)
			{
				switch (InstructionUtils.GetSize(instruction))
				{
					case Size.Byte:
                        Program.cpu.SetRegByte(new Byte68k(CPU.BitwiseAdd(data, Program.cpu.GetRegByte(register.b, false), true, false)), register.b, false);
                        break;
					case Size.Word:
                        Program.cpu.SetRegWord(new Word68k(CPU.BitwiseAdd(new Word68k(data.b), Program.cpu.GetRegWord(register.b, false), true, false)), register.b, false);
                        break;
					case Size.Long:
						Program.cpu.SetRegLong(new Long68k(CPU.BitwiseAdd(new Long68k(data.b), Program.cpu.GetRegLong(register.b, false), true, false)), register.b, false);
						break;
				}
			}
			else
			{	//byte operations are not allowed with address register operations

			}

			output = Program.cpu.GetRegLong(register.b, false).ToString();
		}

		private void InstructionADDX(Word68k instruction)
		{
			Byte68k sourcereg = InstructionUtils.GetSourceReg(instruction);
			Byte68k destreg = InstructionUtils.GetDestReg(instruction);

			switch (InstructionUtils.GetSize(instruction))
			{
				case Size.Byte:
					Program.cpu.Registers[destreg.b] = new Long68k(CPU.BitwiseAdd(Program.cpu.Registers[sourcereg.b], (Byte68k)Program.cpu.Registers[destreg.b], true, true).b);
					break;
				case Size.Word:
					Program.cpu.Registers[destreg.b] = new Long68k(CPU.BitwiseAdd(Program.cpu.Registers[sourcereg.b], (Word68k)Program.cpu.Registers[destreg.b], true, true).w);
					break;
				case Size.Long:
					Program.cpu.Registers[destreg.b] = CPU.BitwiseAdd(Program.cpu.Registers[sourcereg.b], (Long68k)Program.cpu.Registers[destreg.b], true, true);
					break;
			}

			output = Program.cpu.Registers[destreg.b].ToString();
		}
	}
}
