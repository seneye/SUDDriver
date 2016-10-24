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


namespace Seneye.SUD.ListeningDriver
{
    using NLog;
    using ReactiveUI;
    using System;
    using System.ComponentModel;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Linq;
    using System.Reactive.Subjects;
    using Raw;

    public enum SeneyeProductType
    {
        Ignore,
        SUDv2e
    }
    public class SUDDescriptor
    {
        public string UUID { get; private set; }
        public ushort VID { get; private set; }
        public ushort PID { get; private set; }
        public SeneyeProductType ProductType { get; private set; }
        public string Path { get; private set; }

        public SUDDescriptor(string Path, ushort PID, ushort VID, string UUID, SeneyeProductType ProductType)
        {
            this.Path = Path;
            this.PID = PID;
            this.VID = VID;
            this.UUID = UUID;
            this.ProductType = ProductType;
        }
    }

    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public class SUDDriver : ReactiveObject
    {
        public const int DEFAULT_TIMEOUT = 2000;
        private static Logger log;

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private bool cancelInternalThread = false;

        private readonly string uuid;
        private readonly ushort VID;
        private readonly ushort PID;
        private readonly string devicePath;

        private IntPtr ReadHandle;
        private IntPtr WriteHandle;

        private int DriverFailure;

        public string DevicePath
        {
            get
            {
                return devicePath;
            }

        }


        private bool _IsWorking;
        public bool IsWorking
        {
            get
            {
                return _IsWorking;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this._IsWorking, value, "IsWorking");
            }
        }

        public string UUID
        {
            get
            {
                return this.uuid;
            }
        }

        private byte _DeviceType;
        public byte DeviceType
        {
            get
            {
                return _DeviceType;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this._DeviceType, value, "DeviceType");
            }
        }
        private Version _DeviceVersion;
        public Version DeviceVersion
        {
            get
            {
                return _DeviceVersion;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this._DeviceVersion, value, "DeviceVersion");
            }
        }


        public static SUDDescriptor DescribeSUD(string DevicePath)
        {

            var hidHandle = OpenDeviceIO(DevicePath, NativeMethods.ACCESS_NONE);
            var deviceAttributes = default(NativeMethods.HIDD_ATTRIBUTES);
            deviceAttributes.Size = Marshal.SizeOf(deviceAttributes);
            NativeMethods.HidD_GetAttributes(hidHandle, ref deviceAttributes);
            string uuid = GetDeviceSerialNumber(hidHandle);
            CloseDeviceIO(ref hidHandle);
            SeneyeProductType type = SeneyeProductType.Ignore;

            if (deviceAttributes.VendorID == 9463)
            {
                if (deviceAttributes.ProductID == 0x2203)
                {
                    type = SeneyeProductType.SUDv2e;
                }
                if (uuid == null || uuid.Length != 32)
                {
                    type = SeneyeProductType.Ignore;
                }
            }


            return new SUDDescriptor(DevicePath, deviceAttributes.ProductID, deviceAttributes.VendorID, uuid, type);
        }

        public SUDDriver(SUDDescriptor descriptor, string AppName)
        {
            this.PID = descriptor.PID;
            this.VID = descriptor.VID;
            this.uuid = descriptor.UUID;
            this.devicePath = descriptor.Path;

            // log source
            if (log == null)
            {
                log = NLog.LogManager.GetLogger(AppName + ".SUD." + descriptor.UUID);
            }



        }
        public SUDDriver(string DevicePath, string AppName)
            : this(DescribeSUD(DevicePath), AppName)
        {

        }



        #region IO
        internal static IntPtr OpenDeviceIO(string devicePath, uint deviceAccess)
        {

            var security = new NativeMethods.SECURITY_ATTRIBUTES();
            var flags = 0;

  

            security.lpSecurityDescriptor = IntPtr.Zero;
            security.bInheritHandle = true;
            security.nLength = Marshal.SizeOf(security);

            NativeMethods.COMMTIMEOUTS to = new NativeMethods.COMMTIMEOUTS();
            to.ReadIntervalTimeout = 2000;
            to.ReadTotalTimeoutConstant = 10000;
            to.ReadTotalTimeoutMultiplier = 2000;
            to.WriteTotalTimeoutConstant = 10000;
            to.WriteTotalTimeoutMultiplier = 2000;

            var Handle = NativeMethods.CreateFile(devicePath, deviceAccess, (int)(NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE), ref security, NativeMethods.OPEN_EXISTING, flags, 0);

            bool c = NativeMethods.SetCommTimeouts(Handle, ref to);
            if (!c) new Win32Exception(Marshal.GetLastWin32Error());

            return Handle;
           
        }

        public void OpenDevice(bool isV2)
        {

            if (IsOpen())
                return;

            this.ReadHandle = OpenDeviceIO(this.devicePath + ((isV2) ? "\\1" : ""), NativeMethods.GENERIC_READ);
            this.WriteHandle = OpenDeviceIO(this.devicePath + ((isV2) ? "\\2" : ""), NativeMethods.GENERIC_WRITE);
 

        }

        private bool IsOpen()
        {
            return (ReadHandle.ToInt32() != 0 & WriteHandle.ToInt32() != 0 & ReadHandle.ToInt32() != NativeMethods.INVALID_HANDLE_VALUE & WriteHandle.ToInt32() != NativeMethods.INVALID_HANDLE_VALUE);
        }

        private static void CloseDeviceIO(ref IntPtr handle)
        {

            if (handle.ToInt32() == 0 || handle.ToInt32() == NativeMethods.INVALID_HANDLE_VALUE)
                return;

            NativeMethods.CancelIoEx(handle, IntPtr.Zero);
            if (handle.ToInt32() == 0 || handle.ToInt32() == NativeMethods.INVALID_HANDLE_VALUE)
                return;

            NativeMethods.CloseHandle(handle);
            handle = IntPtr.Zero;
        }

        private void CloseDevice()
        {
            CloseDeviceIO(ref this.ReadHandle);
            CloseDeviceIO(ref this.WriteHandle);
        }


        #endregion

        #region Read/ Write Methods

        private byte[] ReadData(int timeout)
        {
 
            uint bytesRead = 0; 
            var buffer = new byte[65];   
      
            var overlapped = new NativeOverlapped(); 
            NativeMethods.ReadFile(ReadHandle, buffer, (uint)buffer.Length, out bytesRead, ref overlapped); 
            return buffer;  
        }



        private IObservable<bool> WriteData(byte[] data)
        {
            var o = Observable.Defer<bool>(() =>
            {
                bool ret = this.WriteDataRaw(data, 500);
                if (!ret)
                { 
                    if (!this.IsWorking)
                    {
                        log.Error("Abort Writing Offline");
                        return Observable.Throw<bool>(new SudDisconnectedException(this.UUID));
                    }
                    log.Error("Error on writing to SUD");
                    return Observable.Throw<bool>(new Win32Exception(Marshal.GetLastWin32Error()));
                }
                return Observable.Return<bool>(ret);
            });
            return o;
        }

        private bool WriteDataRaw(byte[] data, int timeout)
        {

            uint bytesWritten = 0;
            var overlapped = new NativeOverlapped();
            bool k = NativeMethods.WriteFile(WriteHandle, data, (uint)data.Length, out bytesWritten, ref overlapped);

            if (!k)
            {
                log.WarnException("SUD Driver", new Win32Exception(Marshal.GetLastWin32Error()));
            }
            return k; 
        }
         
        #endregion

        #region Utils
        private static string GetDeviceSerialNumber(IntPtr handle)
        {
            IntPtr buffer = Marshal.AllocHGlobal(126);
            NativeMethods.HidD_GetSerialNumberString(handle, buffer, 126);
            return Marshal.PtrToStringAuto(buffer);
        }



        public override string ToString()
        {
            return string.Format("VendorID={0}, ProductID={1}, Seriall={2}, DevicePath={3}",
                                this.VID,
                                this.PID,
                                this.uuid,
                                this.devicePath);
        }
        #endregion

        #region Handle Readings
        private Thread ReadThread;

        private void StartInternalLoop()
        {
            IsWorking = true;

            DriverFailure = 0;
            cancelInternalThread = false;
            OpenDevice(true);

            if (ReadThread != null && ReadThread.IsAlive)
            {
                return;
            }

            log.Info("Starting loop");
            ReadThread = new Thread(new ThreadStart(InternalReadLoop));
            ReadThread.IsBackground = true;
            ReadThread.Start();
        }

        public IObservable<bool> Start()
        {
            StartInternalLoop();

            return HelloSUD();
        }
         
        public void StopInternalLoop()
        { 
            cancelInternalThread = true;
        }

        public IObservable<byte[]> ReadCmd(Cmds cmd, short len)
        {
            return sudReadCmd
                .Where(r => r.Item1 == cmd)
                .Timeout(new TimeSpan(0, 0, 10))
                .Where(r => r != null)
                .Select(r =>
                    r.Item2.Take(len).ToArray()
                ).Catch<byte[], TimeoutException>(x => Observable.Empty<byte[]>());

        }

        internal IObservable<bool> ReadCmd(Cmds cmd)
        {
            return sudReadCmd
                .Where(r => r.Item1 == cmd)
                .Timeout(new TimeSpan(0, 0, 10))
                .Where(x => x != null && x.Item2 != null)
                .Select(r =>
                      (r.Item2[0] == 1)
                ).Catch<bool, TimeoutException>(x => Observable.Return<bool>(false));
        }



        private void InternalReadLoop()
        {

            while (true)
            {

                if (cancelInternalThread)
                {
                    CloseDevice();
                    log.Info("Internal loop closed");
                    this.IsWorking = false;
                    return;
                }

                byte[] data = this.ReadData(1000);
                UInt16 cmd = (UInt16)(data[1] << 0x08 | data[2]);
                
                
                if (data[1] == 0x88)
                {
                    Cmds c = (Cmds)((byte)(cmd));
                    sudReadCmd.OnNext(new Tuple<Cmds, byte[]>(c, data.Skip(3).ToArray()));
                }
                else
                {
                    if (cmd == 0x00)
                    {
                        CloseDevice();
                        this.IsWorking = false;
                        log.Info("Internal loop closed");
                        return;
                    }
                    switch (cmd)
                    {
                        case 0x01:
                            {

                                SudReading rawReading = new SudReading();
                                rawReading.SetContent(data.Skip(3).Take(rawReading.Length).ToArray());

                                sudReading.OnNext(rawReading);
                                break;
                            }
                        case 0x02:
                            {
                                SudLightMeter rawLightMeter = new SudLightMeter();
                                rawLightMeter.SetContent(data.Skip(3).Take(rawLightMeter.Length).ToArray());

                                lmReading.OnNext(rawLightMeter);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                } 
            }

        }
        #endregion

        #region events

        private Subject<SudLightMeter> lmReading = new Subject<SudLightMeter>();
        public Subject<SudLightMeter> LMReading
        {
            get { return lmReading; }
        }

        private Subject<SudReading> sudReading = new Subject<SudReading>();


        public Subject<SudReading> SUDReading
        {
            get { return this.sudReading; }
        }



        private Subject<Tuple<Cmds, byte[]>> sudReadCmd = new Subject<Tuple<Cmds, byte[]>>();

        #endregion
        internal IObservable<byte[]> RequestCmd(Cmds cmd, short len)
        {

            byte[] data = new byte[65];
            data[1] = (byte)cmd;
            return this.WriteData(data)
                      .Retry(10)
                      .Catch(delegate (SudDisconnectedException ex)
                      {
                          return Observable.Empty<bool>();
                      })
                      .Where(x => x)
                      .SelectMany(
                         this.ReadCmd(cmd, len)
                      )
                      .Timeout(TimeSpan.FromSeconds(10),
                         this.WriteData(data)
                         .Catch(delegate (SudDisconnectedException ex)
                         {
                             return Observable.Empty<bool>();
                         })
                         .Where(x => x)
                         .Do(x => log.Info("3 Timeout on cmd {0}", cmd))
                         .Retry(10).SelectMany(
                         this.ReadCmd(cmd, len)
                         ))
                       .Catch<byte[], Exception>(x =>
                       {
                           ;
                           return Observable.Empty<byte[]>();
                       })
                     .Take(1);
        }

        public IObservable<bool> HelloSUD()
        {

            byte[] data = new byte[65];
            data[1] = (byte)'H';
            data[2] = (byte)'E';
            data[3] = (byte)'L';
            data[4] = (byte)'L';
            data[5] = (byte)'O';
            data[6] = (byte)'S';
            data[7] = (byte)'U';
            data[8] = (byte)'D';

            return RequestCmd<SudVersion>(Cmds.CMD_HELLO, data)
                 .Do(x =>
                 {
                     this.DeviceType = x.Type;
                     this.DeviceVersion = x.ToVersion();

                 }).
                 Select(x => x.CanWork);

        }


        public IObservable<bool> ChangeLED(char[] state)
        {
            if (state.Length != 5)
                throw new ArgumentException("State is only 5 LED");

            byte[] data = new byte[65];
            data[1] = (byte)'L';
            data[2] = (byte)'E';
            data[3] = (byte)'D';

            for (var i = 0; i < 5; i++)
            {
                data[4 + i] = (byte)state[i];
            }


            return RequestCmd<SudResponse>(Cmds.CMD_CHANGELED, data)
                .SelectMany(x => Observable.Return<bool>(x.ACK));

        }
         
        public IObservable<bool> Slide(string code)
        { 
            if (code.StartsWith("\"") && code.EndsWith("\""))
            {
                code = code.Substring(1, code.Length - 2);
            }

            byte[] data = new byte[65];
            data[1] = (byte)'S';
            data[2] = (byte)'L';
            data[3] = (byte)'I';
            data[4] = (byte)'D';
            data[5] = (byte)'E';

            Byte[] c = System.Text.ASCIIEncoding.ASCII.GetBytes(code);

            Array.Copy(c, 0, data, 6, Math.Min(c.Length, 59)); 

            return RequestCmd<SudResponse>(Cmds.CMD_SLIDE, data)
                .SelectMany(x => Observable.Return<bool>(x.ACK));

        }

        public IObservable<bool> TakeReading()
        {
            byte[] data = new byte[65];
            data[1] = (byte)'R';
            data[2] = (byte)'E';
            data[3] = (byte)'A';
            data[4] = (byte)'D';
            data[5] = (byte)'I';
            data[6] = (byte)'N';
            data[7] = (byte)'G';
            return this.RequestCmd<SudResponse>(Cmds.CMD_TAKEREADING, data)
                .SelectMany(x => Observable.Return<bool>(x.ACK));
        }

        public IObservable<bool> ByeSUD()
        {
            byte[] data = new byte[65];
            data[1] = (byte)'B';
            data[2] = (byte)'Y';
            data[3] = (byte)'E';
            data[4] = (byte)'S';
            data[5] = (byte)'U';
            data[6] = (byte)'D';
            return this.RequestCmd<SudResponse>(Cmds.CMD_BYESUD, data)
                .SelectMany(x => Observable.Return<bool>(x.ACK));
        }
        internal IObservable<TReturn> RequestCmd<TReturn>(Cmds cmd, byte[] data) where TReturn : BaseField, new()
        {

            return this.WriteData(data)
                      .Catch(delegate (SudDisconnectedException ex)
                      {
                          return Observable.Empty<bool>();
                      })
                      .Retry(10)
                      .Where(x => x)
                      .SelectMany(x => this.ReadCmd<TReturn>(cmd))
                      .Timeout(TimeSpan.FromSeconds(10),
                         this.WriteData(data)
                             .Catch(delegate (SudDisconnectedException ex)
                             {
                                 return Observable.Empty<bool>();
                             })
                             .Do(x => log.Info("1 Timeout on cmd {0}", cmd))
                             .Retry(10)
                             .SelectMany(
                                 this.ReadCmd<TReturn>(cmd)
                             )
                      )
                     .Take(1);
        }
        internal IObservable<TResult> ReadCmd<TResult>(Cmds cmd) where TResult : BaseField, new()
        {
            return sudReadCmd
                .Where(r =>
                    r.Item1 == cmd
                    )
                 .Timeout(new TimeSpan(0, 0, 10))
                .Select(r =>
                {
                    TResult x = new TResult();
                    x.SetContent(r.Item2.Take(x.Length));
                    return x;
                }
              )
              .Catch<TResult, Exception>(x =>
              {
                  log.Warn("Driver Fail CMD");
                  DriverFailure++;
                  return Observable.Empty<TResult>();
              })
              .Take(1);
        }
    }
}
