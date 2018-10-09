using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGEmulator
{
	#region structs
	public struct Byte68k
	{
		public byte b;

		public int this[int key]
		{
			get
			{
				if (key >= 8)
					throw new IndexOutOfRangeException();

				return b & (1 << key);
			}
			set
			{
				if (key >= 8)
					throw new IndexOutOfRangeException();

				b |= (byte)((value > 0 ? 1 : 0) << key);
			}
		}

		public Byte68k(byte b)
		{
			this.b = b;
		}

		public Byte68k(Byte68k b)
		{
			this.b = b.b;
		}
		
		#region bit operations
		public static Byte68k operator &(Byte68k b1, Byte68k b2)
		{
			return new Byte68k((byte)(b1.b & b2.b));
		}

		public static Byte68k operator |(Byte68k b1, Byte68k b2)
		{
			return new Byte68k((byte)(b1.b | b2.b));
		}

		public static Byte68k operator >>(Byte68k b, int shiftBits)
		{
			return new Byte68k((byte)(b.b >> shiftBits));
		}

		public static Byte68k operator <<(Byte68k b, int shiftBits)
		{
			return new Byte68k((byte)(b.b << shiftBits));
		}
		#endregion

		#region casts
		public static explicit operator byte(Byte68k b)
		{
			return b.b;
		}

		public static explicit operator Long68k(Byte68k b)
		{
			return new Long68k(b.b);
		}
		#endregion

		public static int GetSize()
		{
			return 8;
		}

		public override string ToString()
		{
			return b.ToString() + ", " + InstructionUtils.ToBin(b, GetSize());
		}
	}

	public struct Word68k
	{
		public ushort w;

		public int this[int key]
		{
			get
			{
				if (key >= 16)
					throw new IndexOutOfRangeException();

				return w & (1 << key);
			}
			set
			{
				if (key >= 16)
					throw new IndexOutOfRangeException();

				w |= (ushort)((value > 0 ? 1 : 0) << key);
			}
		}

		public Word68k(ushort w)
		{
			this.w = w;
		}

		public Word68k(Word68k w)
		{
			this.w = w.w;
		}

		public Long68k ToLong()
		{
			return new Long68k((uint)w);
		}

		public Long68k ToLong(Word68k other, bool direction = false)
		{
			byte[] bytes = new byte[4];

			if (direction)
			{
				byte[] b = other.GetBytes();
				bytes[0] = b[0];
				bytes[1] = b[1];

				b = GetBytes();
				bytes[2] = b[0];
				bytes[3] = b[1];
			}
			else
			{
				byte[] b = GetBytes();
				bytes[0] = b[0];
				bytes[1] = b[1];

				b = other.GetBytes();
				bytes[2] = b[0];
				bytes[3] = b[1];
			}

			return new Long68k(BitConverter.ToUInt32(bytes, 0));
		}

		public byte[] GetBytes()
		{
			return BitConverter.GetBytes(w);
		}

		#region bit operations
		public static Word68k operator &(Word68k w1, Word68k w2)
		{
			return new Word68k((ushort)(w1.w & w2.w));
		}

		public static Word68k operator |(Word68k w1, Word68k w2)
		{
			return new Word68k((ushort)(w1.w | w2.w));
		}

		public static Word68k operator >>(Word68k w, int shiftBits)
		{
			return new Word68k((ushort)(w.w >> shiftBits));
		}

		public static Word68k operator <<(Word68k w, int shiftBits)
		{
			return new Word68k((ushort)(w.w << shiftBits));
		}
		#endregion

		#region casts
		public static explicit operator Word68k(Opmode opm)
		{
			return new Word68k((ushort)opm);
		}

		public static implicit operator Byte68k(Word68k w)
		{
			//byte firstbytemask = 0b11111111;

			return new Byte68k((byte)w.w);
		}

		public static explicit operator Long68k(Word68k w)
		{
			return new Long68k(w.w);
		}

		public static explicit operator ushort(Word68k w)
		{
			return w.w;
		}
		#endregion

		public static int GetSize()
		{
			return 16;
		}

		public override string ToString()
		{
			return w.ToString() + ", " + InstructionUtils.ToBin(w, GetSize());
		}
	}

	public struct Long68k
	{
		public uint l;

		public int this[int key]
		{
			get
			{
				if (key >= 32)
					throw new IndexOutOfRangeException();

				return (int)(l & (1 << key));
			}
			set
			{
				if (key >= 32)
					throw new IndexOutOfRangeException();

				l |= (uint)((value > 0 ? 1 : 0) << key);
			}
		}

		public Long68k(uint l)
		{
			this.l = l;
		}

		public Long68k(Long68k l)
		{
			this.l = l.l;
		}

		public byte[] GetBytes()
		{
			return BitConverter.GetBytes(l);
		}
		
		#region bit operators
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
		#endregion

		#region cast
		public static implicit operator Byte68k(Long68k l)
		{
			byte firstbytemask = 0b11111111;

			return new Byte68k((byte)(l.l | firstbytemask));
		}

		public static explicit operator uint(Long68k l)
		{
			return l.l;
		}
		#endregion

		public static int GetSize()
		{
			return 32;
		}

		public override string ToString()
		{
			return l.ToString() + ", " + InstructionUtils.ToBin((int)l, GetSize());
		}
	}
	#endregion
}
