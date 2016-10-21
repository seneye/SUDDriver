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
#ifndef SUD_DATA_H
#define SUD_DATA_H

typedef struct {
	char reserved[8]; 
	int	 Kelvin;
	int  x;
	int	 y;
	unsigned int  Par;
	unsigned int  Lux;
	unsigned char  PUR;
} B_LIGHTMETER;


typedef struct {
	struct {
		unsigned : 2;
		unsigned InWater : 1;
		unsigned SlideNotFitted : 1;
		unsigned SlideExpired : 1;

		unsigned StateT : 2;
		unsigned StatePh : 2;
		unsigned StateNh3 : 2;

		unsigned Error : 1;

		unsigned IsKelvin : 1;

	} Bits;
	unsigned short pH;
	unsigned short Nh3;
	int  T;

	char reserved[16];
	B_LIGHTMETER Lm;

} SUDREADING_VALUES;


typedef struct {
	unsigned int IsKelvin;
	B_LIGHTMETER Data;
} SUDLIGHTMETER;


typedef struct {
	unsigned int Ts;
	SUDREADING_VALUES Reading;
} SUDREADING;

#endif