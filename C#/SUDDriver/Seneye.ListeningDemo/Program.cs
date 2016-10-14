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


namespace Seneye.ListeningDemo
{
    using ReactiveUI;
    using Seneye.SUD.ListeningDriver;
    using System;
    class Program
    {
        // 
        static SUDDetector detector;
        static MessageBus messageBus;
        // 
        static SUDDriver driver;
        static bool IsDriverAcceptingCommands;
        // SUD offset on console
        static int sud_off_x = 100;
        static int sud_off_y = 4;
        static int sud_x = 10;

        static char[] LEDStatus;

        static void PaintLine(int x, int y, int size, bool vertical)
        {
            if (vertical)
            {
                for (int i = 0; i < size; i++)
                {
                    Console.SetCursorPosition(x, y + i);
                    Console.Write("│");
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    Console.SetCursorPosition(x + i, y);
                    Console.Write("─");
                }
            }
        }
        [STAThread] 
        static void Main(string[] args)
        {


            messageBus = new MessageBus();
            detector = new SUD.ListeningDriver.SUDDetector(messageBus);

            messageBus.Listen<string>("SUDArrived").Subscribe(path =>
                FindAndStarSUD(path)
            );


            Console.SetCursorPosition(0, 1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Connect a SUD to this PC");
            Console.ForegroundColor = ConsoleColor.Black;

            detector.EnumerateDevices();



            bool loop = true;
            while (loop)
            {


                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Escape:
                        case ConsoleKey.Q:
                        case ConsoleKey.X:
                            {
                                if (driver != null && driver.IsWorking)
                                {
                                    driver.ByeSUD().Subscribe();
                                }
                                Console.SetCursorPosition(2, 40);
                                Console.WriteLine("Quit!");
                                return;
                            }
                        case ConsoleKey.R:
                            {
                                if (IsDriverAcceptingCommands && driver != null && driver.IsWorking)
                                {
                                    driver.TakeReading().Subscribe(a =>
                                    {
                                        if (a)
                                        {
                                            Console.SetCursorPosition(2, 40);
                                            Console.WriteLine("{0,30}", "Reading Request completed!");
                                        }
                                    });
                                }
                                Console.SetCursorPosition(2, 40);
                                break;
                            }
                        case ConsoleKey.D1:
                        case ConsoleKey.NumPad1:
                        case ConsoleKey.D2:
                        case ConsoleKey.NumPad2:
                        case ConsoleKey.D3:
                        case ConsoleKey.NumPad3:
                        case ConsoleKey.D4:
                        case ConsoleKey.NumPad4:
                        case ConsoleKey.D5:
                        case ConsoleKey.NumPad5:
                            {
                                if (IsDriverAcceptingCommands && driver != null && driver.IsWorking)
                                {
                                    int ix = (byte)key.KeyChar - 0x31;
                                    LEDStatus[ix] = (char)(0x61 - (byte)LEDStatus[ix]);
                                    driver.ChangeLED(LEDStatus).Subscribe(a =>
                                    {
                                        if (a)
                                        {
                                            for (var i = 0; i < 4; i++) drawButton(sud_off_x, sud_off_y, sud_x, i);
                                            drawButtonSmall(sud_off_x, sud_off_y, sud_x, 1);
                                            Console.SetCursorPosition(2, 40);
                                            Console.WriteLine("{0,30}", "Completed!");
                                        }
                                    });
                                }

                                break;
                            }
                        default:
                            break;
                    }
                }


            }

        }

        private static void drawButton(int off_x, int off_y, int x, int button)
        {
            off_y = off_y + 6;
            if (LEDStatus != null && LEDStatus[button] == '1')
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.SetCursorPosition(off_x + 2 + (x - 5) / 2, off_y + (3 * button));
            Console.Write("┌───┐");
            Console.SetCursorPosition(off_x + 2 + (x - 5) / 2, off_y + 1 + (3 * button));
            Console.Write("│   │");
            Console.SetCursorPosition(off_x + 2 + (x - 5) / 2, off_y + 2 + (3 * button));
            Console.Write("└───┘");

            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void drawButtonSmall(int off_x, int off_y, int x, int button)
        {

            if (button == 1 && LEDStatus != null && LEDStatus[4] == '1')
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.SetCursorPosition(off_x + 2 + (6 * button), off_y + 3);
            Console.Write("┌─┐");
            Console.SetCursorPosition(off_x + 2 + (6 * button), off_y + 1 + 3);
            Console.Write("└─┘");

            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void drawConsole()
        {
            Console.Clear();
            Console.SetWindowSize(120, 42);
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(2, 5);
            Console.Write("Temperature (C)");
            Console.SetCursorPosition(2, 6);
            Console.Write("pH");
            Console.SetCursorPosition(2, 7);
            Console.Write("NH3 (ppm)");
            Console.SetCursorPosition(2, 8);
            Console.Write("In Water");
            Console.SetCursorPosition(2, 9);
            Console.Write("Slide NOT fitted");
            Console.SetCursorPosition(2, 10);
            Console.Write("Slide Expired");

            PaintLine(20, 5, 6, true);

            Console.SetCursorPosition(50, 5);
            Console.Write("Is Kelvin");
            Console.SetCursorPosition(50, 6);
            Console.Write("Kelvin");
            Console.SetCursorPosition(50, 7);
            Console.Write("PAR");
            Console.SetCursorPosition(50, 8);
            Console.Write("LUX");

            PaintLine(70, 5, 6, true);


            PaintLine(sud_off_x, sud_off_y + 3, sud_x * 2 - 5, true);
            PaintLine(sud_off_x + sud_x + 2, sud_off_y + 3, sud_x * 2 - 5, true);

            Console.SetCursorPosition(sud_off_x, sud_off_y + 2);
            Console.Write("┌");
            Console.SetCursorPosition(sud_off_x + sud_x + 2, sud_off_y + 2);
            Console.Write("┐");

            Console.SetCursorPosition(sud_off_x + 1 + (sud_x - 2) / 2, sud_off_y + 2);
            Console.Write("┘");
            Console.SetCursorPosition(sud_off_x + 1 + (sud_x - ((sud_x - 2) / 2)), sud_off_y + 2);
            Console.Write("└");

            PaintLine(sud_off_x + 1 + (sud_x - 2) / 2, sud_off_y, 2, true);
            PaintLine(sud_off_x + 1 + (sud_x - ((sud_x - 2) / 2)), sud_off_y, 2, true);

            PaintLine(sud_off_x + 1, sud_off_y + 2, (sud_x - 2) / 2, false);
            PaintLine(sud_off_x + 2 + (sud_x - 2) / 2 + 2, sud_off_y + 2, (sud_x - 2) / 2, false);


            Console.SetCursorPosition(sud_off_x, sud_off_y + sud_x * 2 - 2);
            Console.Write("└");
            Console.SetCursorPosition(sud_off_x + sud_x + 2, sud_off_y + sud_x * 2 - 2);
            Console.Write("┘");

            PaintLine(sud_off_x + 1, sud_off_y + sud_x * 2 - 2, sud_x + 1, false);

            for (var i = 0; i < 4; i++)
            {
                drawButton(sud_off_x, sud_off_y, sud_x, i);
            }
            drawButtonSmall(sud_off_x, sud_off_y, sud_x, 0);
            drawButtonSmall(sud_off_x, sud_off_y, sud_x, 1);


            Console.SetCursorPosition(0, 15);

            Console.WriteLine("          ©©©©©                    ");
            Console.WriteLine("          ©©©                      ");
            Console.WriteLine("           ©©                      ");
            Console.WriteLine("           ©©©                     ");
            Console.WriteLine("           ©©©©                    ");
            Console.WriteLine("           ©©©©©:                  ");
            Console.WriteLine("            ©©©©©©©                ");
            Console.WriteLine("            ©©©©©©©©©        =     ");
            Console.WriteLine("             ©©©©©©©©©©   ©©©©©    ");
            Console.WriteLine("              ©©©©©©©©©© ©©©©©     ");
            Console.WriteLine("               ©©©©©©©©©©©©©©      ");
            Console.WriteLine("                ©©©©©©©©©©©        ");
            Console.WriteLine("                 ©©©©©©©©©©©       ");
            Console.WriteLine("                   ©©©©©©©©©©      ");
            Console.WriteLine("                 I©©©©©©©©©©©      ");
            Console.WriteLine("                ©©©©, ©©©©©©©©     ");
            Console.WriteLine("               ©©©©     ©©©©©      ");
            Console.WriteLine("                              	  ");

            Console.SetCursorPosition(2, 40);
        }

        private static void FindAndStarSUD(string path)
        {
            SUDDescriptor desc = SUDDriver.DescribeSUD(path);
            if (desc.ProductType != SeneyeProductType.SUDv2e)
            {
                return;
            }
            drawConsole();
            driver = new SUDDriver(desc.Path, desc.UUID);
            driver.SUDReading.Subscribe(x =>
            {
                Console.SetCursorPosition(22, 5);
                Console.Write("{0, -25}", x.Temperature);
                Console.SetCursorPosition(22, 6);
                Console.Write("{0, -25}", x.Ph);
                Console.SetCursorPosition(22, 7);
                Console.Write("{0, -25}", x.Nh3);
                Console.SetCursorPosition(22, 8);
                Console.Write("{0, -25}", x.InWater);
                Console.SetCursorPosition(22, 9);
                Console.Write("{0, -25}", x.SlideNotFitted);
                Console.SetCursorPosition(22, 10);
                Console.Write("{0, -25}", x.SlideExpired);

                Console.SetCursorPosition(2, 40);

            });
            driver.LMReading.Subscribe(x =>
            {
                Console.SetCursorPosition(72, 5);
                Console.Write("{0, -25}", x.IsKelvin);
                Console.SetCursorPosition(72, 6);
                Console.Write("{0, -25}", x.Kelvin);
                Console.SetCursorPosition(72, 7);
                Console.Write("{0, -25}", x.Par);
                Console.SetCursorPosition(72, 8);
                Console.Write("{0, -25}", x.Lux);

                Console.SetCursorPosition(2, 40);
            });
            // Start the driver
            driver.Start().Subscribe(ok =>
            {
                if (ok)
                {
                    Console.SetCursorPosition(2, 1);
                    Console.Write("Device: {0, -32} v.{1, -6} Type: {2, -5}", driver.UUID, driver.DeviceVersion, driver.DeviceType);
                    IsDriverAcceptingCommands = true;
                    LEDStatus = new char[5] { '0', '0', '0', '0', '0' };

                    Console.SetCursorPosition(2, 39);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("Press R for reading, 1-5 to change LED, Q to quit");

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition(2, 40);
                }
                else
                {
                    Console.SetCursorPosition(2, 1);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Device: {0, -32} v.{1, -6} Type: {2, -5}", driver.UUID, driver.DeviceVersion, driver.DeviceType);
                    Console.SetCursorPosition(2, 2);
                    Console.Write("This device need to be connected to SCA or SWS");
                    Console.ForegroundColor = ConsoleColor.White;
                    IsDriverAcceptingCommands = false;

                    Console.SetCursorPosition(2, 40);
                }
            }); 
        }
    }
}
