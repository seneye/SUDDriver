# SUD Driver

This sample can be compile crossplatform usign [Visual Code](https://code.visualstudio.com).

# Linux

ncurses is required to compile the sample. 
The sample might required root to run.

# OSX

You can compile ncurses from source as the follow.

```
$ curl -O ftp://ftp.gnu.org/gnu/ncurses/ncurses-6.0.tar.gz
$ tar -xzvf ncurses-6.0.tar.gz
$ cd ./ncurses-6.0
$ ./configure --prefix=/usr/local \
  --without-cxx --without-cxx-binding --without-ada --without-progs --without-curses-h \
  --with-shared --without-debug \
  --enable-widec --enable-const --enable-ext-colors --enable-sigwinch --enable-wgetch-events \
&& make
$ sudo make install

``` 
# Windows

MinGW is required to compile with Visual Code. 

PDCurses is required. 

Download the PDCurses file from Sourceforge.com and unzip it. 

Copy the extracted files to the following folders:

-   pdcurses.lib to MingW’s “/lib” folder
-    curses.h and panel.h to MingW’s “/include” folder
-   pdcures.dll to MingW’s “/bin” folder
