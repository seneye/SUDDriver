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
namespace Seneye.SUD.ListeningDriver.Raw
{
    using System;
    using System.Reflection;
    using System.Text;
    public enum SudReadingType : byte
    {
        Fast = 0,
        Normal = 1,
        OffLine = 3,
        OnDemand = 4
    }
    public class SudReading : BaseField
    {
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            Type t = this.GetType();
            PropertyInfo[] ps = t.GetProperties();

            foreach (PropertyInfo p in ps)
            {
                var pt = p.PropertyType;
                if (pt.IsClass)
                {
                    s.AppendFormat("{0}: \r\n ------- {1}\r\n ------- \r\n", p.Name, p.GetValue(this).ToString());
                }
                else
                {
                    s.AppendFormat("{0}: {1}\r\n", p.Name, p.GetValue(this));
                }
            }
            return s.ToString();
        }
        public SudReading() : base(63)
        {


        }

        public UInt32 Ts
        {
            get
            {
                return System.BitConverter.ToUInt32(this.buffer, 0);
            }
        }
        public DateTime Date
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(this.Ts);
            }
        }
        public SudReadingType Type
        {
            get
            {
                return (SudReadingType)this.buffer[4].Get2bits(0);
            }
        }
        public bool InWater
        {
            get
            {
                return this.buffer[4].GetBit(2);
            }
        }
        public bool SlideNotFitted
        {
            get
            {
                return this.buffer[4].GetBit(3);
            }
        }
        public bool SlideExpired
        {
            get
            {
                return this.buffer[4].GetBit(4);
            }
        }
        public bool IsKelvin
        {
            get
            {
                return this.buffer[5].GetBit(4);
            }
        }
        public decimal Ph
        {
            get
            {
                return (decimal)(System.BitConverter.ToUInt16(this.buffer, 4 + 4) / 100.0);
            }
        }

        public decimal Nh3
        {
            get
            {
                return (decimal)(System.BitConverter.ToInt16(this.buffer, 4 + 4 + 2) / 1000.0);
            }
        }
        public decimal Temperature
        {
            get
            {
                return (decimal)(System.BitConverter.ToInt32(this.buffer, 4 + 4 + 2 + 2) / 1000.0);
            }
        }

        public float Kelvin
        {
            get
            {

                return (System.BitConverter.ToInt32(this.buffer, 0x28) / 1000F);
            }
        }
        public float KelvinX
        {
            get
            {

                return (System.BitConverter.ToInt32(this.buffer, 0x2C) / 10000F);
            }
        }

        public float KelvinY
        {
            get
            {

                return (System.BitConverter.ToInt32(this.buffer, 0x30) / 10000F);
            }
        }

        public float PAR
        {
            get
            {

                return (System.BitConverter.ToInt32(this.buffer, 0x34));
            }
        }
        public float Lux
        {
            get
            {
                return (System.BitConverter.ToInt32(this.buffer, 0x38));
            }
        }
    }
}
