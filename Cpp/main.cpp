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
#include <stdio.h>
#include <wchar.h>
#include <string.h>
#include <stdlib.h>
#include "hidapi.h"
#include "sud_data.h"
#include <iostream>

#include <stdbool.h>

// Headers needed for sleeping.
#ifdef _WIN32
#include <windows.h>
#include "curses.h"
#else
#include <sys/ioctl.h>
#include <unistd.h> 
#include <curses.h>
#endif

SUDREADING curr_reading;
SUDLIGHTMETER curr_lm;
unsigned LEDStatus[5] = {0, 0, 0, 0, 0 };
unsigned short sud_off_x = 90;
unsigned short sud_off_y = 3;
unsigned short sud_x = 10;
bool hasColor;

void drawConsole();
void drawButton(unsigned short off_x, unsigned short off_y, unsigned short x, unsigned short button);
void drawButtonSmall(int off_x, int off_y, int x, int button);

int WriteHELLOSUD(hid_device *handle);
int WriteLED(hid_device *handle);
int WriteREADING(hid_device *handle);
int WriteBYESUD(hid_device *handle);

void ShowReading();
void ShowLmReading();

int main(int argc, char* argv[])
{

	unsigned char read_buf[65];
	int res;
	struct hid_device_info *devs, *cur_dev;

	hid_device_info *selected_sud = NULL;
	hid_device *handle;
	bool screen_err;
	int i;


#ifdef WIN32
	UNREFERENCED_PARAMETER(argc);
	UNREFERENCED_PARAMETER(argv);
#endif
	// Adjust Console 
	initscr();
#ifndef _WIN32  
	screen_err = true;
	while (screen_err)
	{
		struct winsize tw;
		ioctl(STDOUT_FILENO, TIOCGWINSZ, &tw);
		if (tw.ws_col >= 120 && tw.ws_row >= 33)
		{
			screen_err = false;
		}
		else
		{
			clear();
			printw("We need a bigger terminal (At lease 120x33 lines). This is %dx%d lines.", tw.ws_col, tw.ws_row);
			refresh();
			usleep(10000);
		}
	}
	 
#endif 
	if (resize_term(33, 120) == ERR)
		return -1;
	curs_set(0);
	hasColor = has_colors();
	if (hasColor)
	{
		start_color();
		init_pair(1, COLOR_WHITE, COLOR_BLACK);
		init_pair(2, COLOR_BLUE, COLOR_BLACK);
		init_pair(3, COLOR_RED, COLOR_BLACK);
		init_pair(4, COLOR_GREEN, COLOR_BLACK);
		init_color(COLOR_YELLOW, 255, 255, 0);
		init_pair(5, COLOR_YELLOW, COLOR_BLACK);
	}
	if (hid_init())
		return -1;

	clear();
	refresh();
	// Enumerate SUDs
	devs = hid_enumerate(9463, 8708);
	cur_dev = devs;
	i = 0;

	mvprintw(1, 2, "Select the device: (y to select)");
	refresh();
	while (cur_dev) {


		mvprintw(2, 2, "%d )%ls\n", ++i, cur_dev->serial_number);
		refresh();
		char input;
		std::cin >> input;
		if (input == 'y')
		{
			selected_sud = cur_dev;
			break;
		}
		cur_dev = cur_dev->next;
	}



	if (selected_sud == NULL) {
		mvprintw(2, 2,"No device selected\n"); move(32,2); refresh();
		return 1;
	}
	// 1) Open the device
	handle = hid_open_path(selected_sud->path);
	if (!handle) {
		mvprintw(2, 2, "unable to open device\n");  move(32,2); refresh();
		return 1;
	}
	hid_set_nonblocking(handle, 1);
	clear();

	// 2) Init HELOSUD
	res = WriteHELLOSUD(handle);
	if (res < 0)
	{
		mvprintw(2, 2, "Cannot send HELLOSUD\n");  move(32,2); refresh();
		return -1;
	}

	drawConsole();

	int exit = 0; 

	// Adjust timeout and not blocking calls 
	
	cbreak();
	noecho();
	nodelay(stdscr, TRUE);
	scrollok(stdscr, TRUE);
 
	while (exit == 0)
	{ 
		res = hid_read(handle, read_buf, 64);
		if (res == 64)
		{
			// Reading Answers from device
			if (read_buf[0] == 0x88)
			{
				switch (read_buf[1])
				{
				case 0x01: // HELLOSUD
				{
					const char* deviceType;
					unsigned short deviceVersion;
					switch (read_buf[3])
					{
					case 0:
						deviceType = "Home";
						break;
					case 1:
						deviceType = "Home";
						break;
					case 2:
						deviceType = "Pound";
						break;
					case 3:
						deviceType = "Reef";
						break;
					}
					deviceVersion = (read_buf[5] << 8) + read_buf[4];
					if (read_buf[2])
					{
						mvprintw(1, 2, "Device: %32ls v.%d.%d.%d  Type: %5s", cur_dev->serial_number, deviceVersion / 10000, (deviceVersion / 100) % 100, deviceVersion % 100, deviceType);
 
						attron(COLOR_PAIR(2) | A_BOLD);
						mvprintw(30, 2, "Press R for reading, 1-5 to change LED, Q to quit");
						attroff(COLOR_PAIR(2));
					}
					else
					{
						if (hasColor) attron(COLOR_PAIR(3));
						move(1, 2);
						printw("Device: %32ls v.%d.%d.%d  Type: %5s", cur_dev->serial_number, deviceVersion / 10000, (deviceVersion / 100) % 100, deviceVersion % 100, deviceType);
						move(2, 2);
						printw("This device need to be connected to SCA or SWS");
 
						if (hasColor) attroff(COLOR_PAIR(3));
					}

					move(40, 2);
					refresh();
					break;
				}
				case 0x02:  // READING
				{
					move(40, 2);
					if (read_buf[2])
					{
						if (hasColor) attron(COLOR_PAIR(5) | A_BOLD);
						mvprintw(31, 2, "%-120s", "Reading Request completed!"); 
						if (hasColor) attroff(COLOR_PAIR(5));
						refresh(); 
						break;
					}
					else
					{
						if (hasColor) attron(COLOR_PAIR(3) );
						mvprintw(31, 2, "%-120s", "Reading Request NOT completed."); refresh();
						if (hasColor) attroff(COLOR_PAIR(3));
						break;
					}
					refresh();
					break;
				}
				case 0x03:  // LED
				{
					move(40, 2);
					if (read_buf[2])
					{
						if (hasColor) attron(COLOR_PAIR(5) | A_BOLD );
						mvprintw(31, 2, "%-120s", "Request LED completed!");
						if (hasColor) attroff(COLOR_PAIR(5));
					}
					else
					{
						if (hasColor) attron(COLOR_PAIR(3));
						mvprintw(31, 2, "%-120s", "Request LED NOT completed."); refresh();
						if (hasColor) attroff(COLOR_PAIR(3)); 
						break;
					}
					for (int x = 0; x < 4; x++) drawButton(sud_off_x, sud_off_y, sud_x, x);
					drawButtonSmall(sud_off_x, sud_off_y, sud_x, 1);
					refresh();
					break;
				}
				}
			}
			if (read_buf[0] == 0x00)
			{
				switch (read_buf[1])
				{
				case 0x01: // Reading
				{ 
					memcpy(&curr_reading, &read_buf[2], sizeof(SUDREADING));
					ShowReading();
					break;
				}
				case 0x02: // Lightmeter reading
				{
					memcpy(&curr_lm, &read_buf[2], sizeof(SUDLIGHTMETER));
					ShowLmReading();
					break;
				}
				}
			}
		}
		char c;
		if ((c = getch()) != ERR)
		{
			switch (c)
			{
			case 'r':
			{
				if (hasColor) attron(COLOR_PAIR(5) | A_BOLD);
				mvprintw(31, 2, "%-120s", "Taking Reading...");
				if (hasColor) attroff(COLOR_PAIR(5));
				refresh();

				if (WriteREADING(handle) < 0)
				{
					if (hasColor) attron(COLOR_PAIR(3));
					mvprintw(31, 2, "%-120s", "Cannot request READING");
					if (hasColor) attroff(COLOR_PAIR(3));
					refresh();
					exit = -1;
				}
				break;
			}
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			{
				int ix = c-0x31;
				LEDStatus[ix] = 1 - LEDStatus[ix];
				if (WriteLED(handle) < 0)
				{
					if (hasColor) attron(COLOR_PAIR(3));
					mvprintw(31, 2, "%-120s", "Cannot send LED\n");
					if (hasColor) attroff(COLOR_PAIR(3));
					refresh();
					exit = -1;
				}
				break;
			}
			case 'q':
			{
				if (WriteBYESUD(handle) < 0)
				{
					if (hasColor) attron(COLOR_PAIR(3));
					mvprintw(31, 2, "%-120s", "Cannot send BYESUD\n");
					if (hasColor) attroff(COLOR_PAIR(3)); 
					refresh();
					exit = -1;
				}
				else {
					exit = 1;
				}
				break;
			}
			}
		}

	}

	if (exit == 1)
	{
		hid_close(handle);
	}
	free(devs);
	return exit;
}
void drawConsole()
{
	clear();

	mvprintw(5, 2, "Temperature (C)");
	mvprintw(6, 2, "pH");
	mvprintw(7, 2, "NH3 (ppm)");
	mvprintw(8, 2, "In Water");
	mvprintw(9, 2, "Slide NOT fitted");
	mvprintw(10, 2, "Slide Expired");

	mvvline(5, 20, ACS_VLINE, 6);

	mvprintw(5, 50, "Is Kelvin");
	mvprintw(6, 50, "Kelvin");
	mvprintw(7, 50, "PAR");
	mvprintw(8, 50, "LUX");
	mvprintw(9, 50, "PUR");
	mvvline(5, 70, ACS_VLINE, 6);


	// SUD 
	mvvline(sud_off_y + 3, sud_off_x, ACS_VLINE, sud_x * 2 - 5);
	mvvline(sud_off_y + 3, sud_off_x + sud_x + 2, ACS_VLINE, sud_x * 2 - 5);
	mvaddch(sud_off_y + 2, sud_off_x, ACS_ULCORNER);
	mvaddch(sud_off_y + 2, sud_off_x + sud_x + 2, ACS_URCORNER);

	mvhline(sud_off_y + 2, sud_off_x + 1, ACS_HLINE, (sud_x - 2) / 2);
	mvaddch(sud_off_y + 2, sud_off_x + 1 + (sud_x - 2) / 2, ACS_LRCORNER);
	mvaddch(sud_off_y + 2, sud_off_x + 1 + (sud_x - ((sud_x - 2) / 2)), ACS_LLCORNER);
	hline(ACS_HLINE, (sud_x - 2) / 2);

	mvvline(sud_off_y, sud_off_x + 1 + (sud_x - 2) / 2, ACS_VLINE, 2);
	mvvline(sud_off_y, sud_off_x + 1 + (sud_x - ((sud_x - 2) / 2)), ACS_VLINE, 2);


	mvaddch(sud_off_y + sud_x * 2 - 2, sud_off_x, ACS_LLCORNER);
	hline(ACS_HLINE, sud_x + 1);
	mvaddch(sud_off_y + sud_x * 2 - 2, sud_off_x + sud_x + 2, ACS_LRCORNER);

	for (int x = 0; x < 4; x++) drawButton(sud_off_x, sud_off_y, sud_x, x);
	for (int x = 0; x < 2; x++) drawButtonSmall(sud_off_x, sud_off_y, sud_x, x);

	int i = 12;
	mvprintw(i++, 0, "          00000");
	mvprintw(i++, 0, "          000");
	mvprintw(i++, 0, "           00");
	mvprintw(i++, 0, "           000");
	mvprintw(i++, 0, "           0000");
	mvprintw(i++, 0, "           00000");
	mvprintw(i++, 0, "            0000000");
	mvprintw(i++, 0, "            000000000       0");
	mvprintw(i++, 0, "             0000000000   00000");
	mvprintw(i++, 0, "              0000000000 00000");
	mvprintw(i++, 0, "               00000000000000");
	mvprintw(i++, 0, "                00000000000");
	mvprintw(i++, 0, "                 00000000000");
	mvprintw(i++, 0, "                   0000000000");
	mvprintw(i++, 0, "                 I00000000000");
	mvprintw(i++, 0, "                0000  00000000");
	mvprintw(i++, 0, "               0000     00000");
	move(32, 2); 
	refresh();
}
void drawButton(unsigned short off_x, unsigned short off_y, unsigned short x, unsigned short button)
{
	unsigned short base_x, base_y;
	off_y = off_y + 6;

	base_x = off_x + 2 + (x - 5) / 2;
	base_y = off_y + (3 * button);
	if (hasColor && LEDStatus[button])
	{
		attron(COLOR_PAIR(3) | A_BOLD);
	}

	mvaddch(base_y, base_x, ACS_ULCORNER);
	hline(ACS_HLINE, 3);
	mvaddch(base_y, base_x + 4, ACS_URCORNER);

	mvaddch(base_y + 2, base_x, ACS_LLCORNER);
	hline(ACS_HLINE, 3);
	mvaddch(base_y + 2, base_x + 4, ACS_LRCORNER);

	mvvline(base_y + 1, base_x, ACS_VLINE, 1);
	mvvline(base_y + 1, base_x + 4, ACS_VLINE, 1);

	if (!hasColor) mvprintw(base_y + 1, base_x + 2, "%1s", (LEDStatus[button]) ? "x" : " ");

	if (hasColor) attroff(COLOR_PAIR(3));
}
void drawButtonSmall(int off_x, int off_y, int x, int button)
{
	unsigned short base_x, base_y;
	base_x = off_x + 2 + (6 * button);
	base_y = off_y + 3;

	if (hasColor && button == 1 && LEDStatus[4])
	{
		attron(COLOR_PAIR(4) | A_BOLD);
	}
	mvaddch(base_y, base_x, ACS_ULCORNER);
	addch(ACS_HLINE);
	addch(ACS_URCORNER);

	mvaddch(base_y + 1, base_x, ACS_LLCORNER);
	addch(ACS_HLINE);
	addch(ACS_LRCORNER);

	if (hasColor)
	{
		attroff(COLOR_PAIR(4));
	}
}

// Commands
int WriteHELLOSUD(hid_device *handle)
{
	unsigned char buf[65];
	memset(&buf, 0x00, 65);
	buf[1] = 'H';
	buf[2] = 'E';
	buf[3] = 'L';
	buf[4] = 'L';
	buf[5] = 'O';
	buf[6] = 'S';
	buf[7] = 'U';
	buf[8] = 'D';
	return hid_write(handle, (const unsigned char*)&buf, 65);
}
int WriteLED(hid_device *handle)
{
	unsigned char buf[65];
	memset(&buf, 0x00, 65);

	buf[1] = 'L';
	buf[2] = 'E';
	buf[3] = 'D';
	for (int i = 0; i < 5; i++)
	{
		buf[i + 4] =  '0' +   LEDStatus[i];
	}

	return hid_write(handle, (const unsigned char*)&buf, 65);
}
int WriteREADING(hid_device *handle)
{
	unsigned char buf[65];
	memset(&buf, 0x00, 65);
	buf[1] = 'R';
	buf[2] = 'E';
	buf[3] = 'A';
	buf[4] = 'D';
	buf[5] = 'I';
	buf[6] = 'N';
	buf[7] = 'G';

	return hid_write(handle, (const unsigned char*)&buf, 65);
}
int WriteBYESUD(hid_device *handle)
{
	unsigned char buf[65];
	memset(&buf, 0x00, 65);
	buf[1] = 'B';
	buf[2] = 'Y';
	buf[3] = 'E';
	buf[4] = 'S';
	buf[5] = 'U';
	buf[6] = 'D';

	return hid_write(handle, (const unsigned char*)&buf, 65);
}

void ShowReading()
{
	 
	if (hasColor) attron(A_BOLD);
	mvprintw(5, 22, "%-10.3f", curr_reading.Reading.T / 1000.0);
	mvprintw(6, 22, "%-10.2f", curr_reading.Reading.pH / 100.0);
	mvprintw(7, 22, "%-10.2f", curr_reading.Reading.Nh3 / 1000.0);
	mvprintw(8, 22, "%-10s", curr_reading.Reading.Bits.InWater ? "True " : "False");
	mvprintw(9, 22, "%-10s", curr_reading.Reading.Bits.SlideNotFitted ? "True " : "False");
	mvprintw(10, 22, "%-10s", curr_reading.Reading.Bits.SlideExpired ? "True " : "False");
	move(32, 0);
	if (hasColor) attroff(A_BOLD);
	refresh();
}

void ShowLmReading()
{
	if (hasColor) attron(A_BOLD);
	mvprintw(5, 72, "%-10s", curr_lm.IsKelvin ? "True " : "False");
	mvprintw(6, 72, "%-10.0f", curr_lm.Data.Kelvin / 1000.0);
	mvprintw(7, 72, "%-10u", curr_lm.Data.Par);
	mvprintw(8, 72, "%-10u", curr_lm.Data.Lux);
	mvprintw(9, 72, "%-4i%%", curr_lm.Data.PUR);
	move(32, 0);
	if (hasColor) attroff(A_BOLD);
	refresh();
}