/*
---------------------------------------------------------------------------------------------------------

                      `@@@.  @@@@. :@@@@@   #@@@;  @    @  '@@@+         `@@+          
                      @# +@ @@  '@`;@#  @@ #@` ,@; @    @ '@, .@#     `@ @@@@@.        
                      @@  ;`@    @@;@   '@ @`   ;@ @    @ @,   .@     @@ @@@@@@`       
                      `@@@ :@@@@@@@;@   .@ @@@@@@@ @    @ @@@@@@@    @@@ @@@@@@@       
                      + `@@,@      ;@   .@ @       @    @ @          @@@  @@@@@@       
                      @  `@ @'   @;;@   .@ @@   @@ @#  :@ @@   @@    @@@@  #;`@@       
                      @@@@@ .@@@@@ ;@   .@  @@@@@  ;@@@@#  @@@@@     @@@@`   @@@       
                                                     @@`             @@@@@`  @@@       
                                                     @@              `@@@@   #@,       
                                                                      +@@#@@@@#        
                                                     @@                ,@@@@@:         
              																		 
----------------------------------------------------------------------------------------------------------

 THE SAMPLE CODE IS PROVIDED “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 IN NO EVENT SHALL PAGERDUTY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS 
 OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) SUSTAINED BY YOU OR A THIRD PARTY,
 HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT ARISING 
 IN ANY WAY OUT OF THE USE OF THIS SAMPLE CODE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
 The Code is not covered by any Seneye Service Level Agreements.

----------------------------------------------------------------------------------------------------------
*/ 
namespace Seneye.SUD.ListeningDriver .Raw
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    public static class ByteExtensions
    {


        public static byte[] getBytes(this object x)
        {
            int size = Marshal.SizeOf(x);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(x, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

          
            return arr;
        }

        public static short getShort(this byte[] m, int index)
        {
            return (short)((m[index + 1] << 8) | m[index]);
        }
        public static ushort getUShort(this byte[] m, int index)
        {
            return (ushort)((m[index + 1] << 8) | m[index]);
        }

        public static short ToShort(this byte m, byte l)
        {
            short mt = m;
            return (short)((mt << 8) | l);
        }

        public static byte[] FromShort(this short m)
        {
            byte[] a = new byte[2];
            a[0] = (byte)(m >> 8);
            a[1] = (byte)(m & 0xFF);

            return a;
        }

        
       

        public static BitArray GetArray(this byte b)
        {
            BitArray c = new BitArray(b);
            return c;
        }
        public static bool GetBit(this byte b, short index)
        {
            int x = (int)Math.Pow(2, index);
            return ( ( b & x) == x );
        }
        public static byte SetBit(this byte b, short index, bool value)
        {
            byte x = (byte)Math.Pow(2, index);
            if (value)
            {
                b |= x; 
            }
            else
            {
                b &= (byte)(0xFF - x);
            }
            return b;
        } 
        public static short Get2bits(this byte b, short index)
        {
            short i = 0;
            if (GetBit(b,  index)) i += 1;
            if (GetBit(b, (short)(index + 1))) i += 2; 
            return i;
        }

        public static byte Set2bits(this byte b, short index, short value)
        {
           b =  SetBit(b, index, ((value & 0x01) == 1));
          b =   SetBit(b, (short)(index +1) , ((((int)value) & 0x02) == 2));
            return b;
        }
        public static byte GetLO(this byte b)
        {
            return (byte)(b & 0x0F);
        }
        public static byte GetHI(this byte b)
        {
            return (byte)((b & 0xF0) >> 0x04);
        }
        public static byte SetLO(this byte b, byte value)
        {
            b &= 0xF0;
            b |= (byte)((byte)value & 0x0F);
            return b;
        }
        public static byte SetHI(this byte b, byte value)
        {
            BitArray r = new BitArray(b);
            b &= 0x0F;
            b |= (byte)((byte)value << 0x04);
            return b;
        }

        public static void CalculateCRC(this List<byte> l)
        {
            byte crc = 0x0;
            for (int i = 0; i < l.Count; i++)
            {
                crc = (byte)(crc ^ l[i]);
            }
            l.Add((byte)~crc);  
        }
        public static bool VerifyCRC(this byte[] l)
        {
            if (l.Length < 2) return false;
            int lastbyte = l.Length - 2;
            int crcbyte = l.Length - 1;
            byte crc = 0x0;
            for (int i = 0; i <= (lastbyte); i++)
            {
                crc = (byte)(crc ^ l[i]);
            }

            return (l[crcbyte] ==(~crc & 0xFF));
        }
        public static byte[] Range(this byte[] l, int start, int length)
        {
            byte[] n = new byte[length];
            int nl = 0;
            for (int i =0 ; i < length; i++)
            {
                
                n[nl] = l[ start+ i];
                nl++;
            }
            return n;
        }
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string ToPrintString(this byte[] l)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in l)
            {
                sb.AppendFormat("{0:x2}  ", b);
            }
            return sb.ToString();
        }


        public static DateTime ToDate(byte lsb, byte nsb1, byte nsb2, byte msb)
        {
            uint seconds = (uint)(lsb | nsb1 << 8 | nsb2 << 16 | msb << 24);
            DateTime dateorg = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateorg  = dateorg.AddSeconds(seconds);
            return dateorg;
        }
        public static byte[] FromDate(DateTime date)
        {
             
            TimeSpan span = date.Subtract(new DateTime(1970,1, 1, 0, 0, 0, DateTimeKind.Utc));
            uint seconds = (uint)span.TotalSeconds;
            byte[] data = new byte[4];
            data[0] = (byte)(seconds  & 0x000000FF);
            data[1] = (byte)((seconds & 0x0000FF00) >> 8);
            data[2] = (byte)((seconds & 0x00FF0000) >> 16);
            data[3] = (byte)((seconds & 0xFF000000) >> 24);
            return data;
        }



        internal static byte ToBCD(int b)
        {
            return (byte)((b & 0x0F) + (0x10 * (b & 0xF0)));
        }

        internal static int FromBCD(byte b)
        {
            return b.GetHI() * 10 + b.GetLO();

        }
    }
}
