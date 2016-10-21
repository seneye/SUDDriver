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
     
    public class SudLightMeter : BaseField
    {
        public SudLightMeter() : base(33)
        {
        } 

        public bool IsKelvin
        {
            get
            {
                return (this.buffer[0] > 0);
            }
        } 
        public int Kelvin
        {
            get
            {
                return (this.buffer[12] | this.buffer[13] << 8 | this.buffer[14] << 16 | this.buffer[15] << 24) / 1000;

            }
        }
        public float KelvinX
        {
            get
            {

                return (this.buffer[16] | this.buffer[17] << 8 | this.buffer[18] << 16 | this.buffer[19] << 24) / 10000F;

            }
        } 
        public float KelvinY
        {
            get
            {

                return (this.buffer[20] | this.buffer[21] << 8 | this.buffer[22] << 16 | this.buffer[23] << 24) / 10000F;

            }
        }
        public float Par
        {
            get
            {

                return this.buffer[24] | this.buffer[25] << 8 | this.buffer[26] << 16 | this.buffer[27] << 24;

            }
        } 
        public float Lux
        {
            get
            {

                return this.buffer[28] | this.buffer[29] << 8 | this.buffer[30] << 16 | this.buffer[31] << 24;

            }
        }
        public int PUR
        {
            get
            { 
                return  this.buffer[32];

            }
        }
    }
}
