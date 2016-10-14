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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public class BaseField  
    {
        private int numByte;
        protected byte[] buffer;
 
        public BaseField(int bytenum)
        {
            this.buffer = new byte[bytenum];
            this.numByte = bytenum;
        }

        internal int Length
        {
            get
            {
                return this.numByte;
            }
        }
 
        public virtual byte[] GetContent()
        {
            return this.buffer.Clone() as byte[];
        }

        public virtual  void SetContent(byte[] data)
        {
            if (data.Length != this.numByte)
            {
                throw new ArgumentOutOfRangeException("data");
            }
            this.buffer = data.Clone() as byte[];
        }

        public virtual void SetContent(IEnumerable<byte> data)
        {
            var rdata = data.ToArray();
            if (rdata.Length != this.numByte)
            {
                throw new ArgumentOutOfRangeException("data");
            }
            this.buffer = rdata.Clone() as byte[];
        }

        public void SetContent(byte data)
        {
            if (1 != this.numByte)
            {
                throw new ArgumentOutOfRangeException("data");
            }
            this.buffer = new byte[] {data}  ;
        }
        public void SetContent(int i, byte data)
        {
            this.buffer[i] = data;
        }
        public virtual bool IsValid()
        {
            return true;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            Type t = this.GetType();
            PropertyInfo[] ps = t.GetProperties(System.Reflection.BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in ps)
            {
                s.AppendFormat("{0}: {1}\n\r", p.Name, p.GetValue(this));
            }
            return s.ToString();
        }

    }
}
