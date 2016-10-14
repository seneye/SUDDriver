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
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Diagnostics;
using System.ComponentModel.Composition;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;

namespace Seneye.SUD.ListeningDriver
{
    public interface ISUDDetector {
        IObservable<Unit> EnumerateDevices();
    }

    [Export(typeof(ISUDDetector)), PartCreationPolicy(CreationPolicy.Shared)]
	public class SUDDetector :  ISUDDetector, IDisposable
    {

        private const int WM_DEVICECHANGE = 0x0219;  
        private const int DBT_DEVICEARRIVAL = 0x8000;  
        private const int DBT_DEVICEQUERYREMOVE = 0x8001;  
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004; 
        private const int DBT_DEVNODES_CHANGED = 0x0007;  
        [Flags]
        private enum DEVICE_NOTIFY : uint
        {
            DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000,
            DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001,
            DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004
        }

        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        private const int DBT_DEVTYP_HANDLE = 0x00000006;
        private const int DBT_DEVTYP_OEM = 0x00000000;
        private const int DBT_DEVTYP_PORT = 0x00000003;
        private const int DBT_DEVTYP_VOLUME = 0x00000002;

        private bool disposed = false;
        private HwndSource _hwndSource;
        private static Guid _hidClassGuid;

        private Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");

        [ImportingConstructor]
        public SUDDetector( IMessageBus messageBus )
		{ 
            this.MessageBus = messageBus; 
            this._hwndSource = new HwndSource(0, 0, 0, 0, 0, "fake", IntPtr.Zero);
            ////this._EnumerateDevices();

            Register();

 

        } 
   
         
        private static Guid HidClassGuid
        {
            get
            {
                if (_hidClassGuid.Equals(Guid.Empty)) NativeMethods.HidD_GetHidGuid(ref _hidClassGuid);
                return _hidClassGuid;
            }
        } 
        protected IMessageBus MessageBus
        {
            get;
            private set;
        }
        private static NativeMethods.SP_DEVINFO_DATA CreateDeviceInfoData()
        {
            var deviceInfoData = new NativeMethods.SP_DEVINFO_DATA();

            deviceInfoData.cbSize = Marshal.SizeOf(deviceInfoData);
            deviceInfoData.DevInst = 0;
            deviceInfoData.ClassGuid = Guid.Empty;
            deviceInfoData.Reserved = IntPtr.Zero;

            return deviceInfoData;
        }
         
       public IObservable<Unit>  EnumerateDevices()
        {
                return Observable.Start(delegate
                {
                    this._EnumerateDevices();
                    return Unit.Default;
                }, RxApp.TaskpoolScheduler);
            }

        private void _EnumerateDevices()
        {
            List<string> paths = new List<string>(); 
            var hidClass = HidClassGuid;
            var deviceInfoSet = NativeMethods.SetupDiGetClassDevs(ref hidClass, null, 0, NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);

            if (deviceInfoSet.ToInt64() != NativeMethods.INVALID_HANDLE_VALUE)
            {
                var deviceInfoData = CreateDeviceInfoData();
                var deviceIndex = 0;

                while (NativeMethods.SetupDiEnumDeviceInfo(deviceInfoSet, deviceIndex, ref deviceInfoData))
                {
                    deviceIndex += 1;

                    var deviceInterfaceData = new NativeMethods.SP_DEVICE_INTERFACE_DATA();
                    deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
                    var deviceInterfaceIndex = 0;

                    while (NativeMethods.SetupDiEnumDeviceInterfaces(deviceInfoSet, ref deviceInfoData, ref hidClass, deviceInterfaceIndex, ref deviceInterfaceData))
                    {
                        deviceInterfaceIndex++;
                        var devicePath = GetDevicePath(deviceInfoSet, deviceInterfaceData);

                        if (!paths.Contains(devicePath)) paths.Add(devicePath); 
                    }
                }
                NativeMethods.SetupDiDestroyDeviceInfoList(deviceInfoSet);
            }
            paths.ForEach(x => {
                if (SUDDriver.DescribeSUD(x).ProductType == SeneyeProductType.SUDv2e)
                    this.MessageBus.SendMessage<string>(x, "SUDArrived"); 
                
                });
    
        }


        private  string GetDevicePath(IntPtr deviceInfoSet, NativeMethods.SP_DEVICE_INTERFACE_DATA deviceInterfaceData)
        {
            var bufferSize = 0;
            var interfaceDetail = new NativeMethods.SP_DEVICE_INTERFACE_DETAIL_DATA { Size = IntPtr.Size == 4 ? 4 + Marshal.SystemDefaultCharSize : 8 };

            NativeMethods.SetupDiGetDeviceInterfaceDetailBuffer(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref bufferSize, IntPtr.Zero);

            return NativeMethods.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, ref interfaceDetail, bufferSize, ref bufferSize, IntPtr.Zero) ? interfaceDetail.DevicePath : null;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcc_name;

        }
        
        public void Register()
        {
          
            this._hwndSource.AddHook(new HwndSourceHook(this.hwndSourceHook));

            DEV_BROADCAST_DEVICEINTERFACE notificationFilter = new DEV_BROADCAST_DEVICEINTERFACE();
            int size = Marshal.SizeOf(notificationFilter);
            notificationFilter.dbcc_size = size;
            notificationFilter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            notificationFilter.dbcc_reserved = 0;
            notificationFilter.dbcc_classguid = GUID_DEVINTERFACE_USB_DEVICE; // HidClassGuid;
            IntPtr buffer = IntPtr.Zero;
            buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(notificationFilter, buffer, true);
            IntPtr result = NativeMethods.RegisterDeviceNotification(this._hwndSource.Handle, buffer, (Int32)(DEVICE_NOTIFY.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES));
 
        }
         
        public void UnRegister()
        {
            if (this._hwndSource != null)
            {
                NativeMethods.UnregisterDeviceNotification(this._hwndSource.Handle);
                this._hwndSource.RemoveHook(this.hwndSourceHook);
            }
        }

        IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {             
            if (msg == WM_DEVICECHANGE)
            {
         
                if ((int)wParam == DBT_DEVICEARRIVAL)
                {                   
                    DEV_BROADCAST_DEVICEINTERFACE info = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));
                    if (info.dbcc_classguid == HidClassGuid)
                    {
                        if ( SUDDriver.DescribeSUD(info.dbcc_name).ProductType == SeneyeProductType.SUDv2e)
                            this.MessageBus.SendMessage<string>(info.dbcc_name, "SUDArrived"); 
                    }
                } 
                else if ((int)wParam == DBT_DEVICEREMOVECOMPLETE)
                {
                    DEV_BROADCAST_DEVICEINTERFACE info = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));
                     
                    if (info.dbcc_classguid == HidClassGuid)
                    {
                        this.MessageBus.SendMessage<string>(info.dbcc_name, "HIDDisconnected");
                    }
 
                } 
            } 
            return IntPtr.Zero; 
        }
 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
 
        private void Dispose(bool disposing)
        {
             if (!this.disposed)
            {
               
                if (disposing)
                { 
                    this._hwndSource.Dispose();
                }
                
            }
            disposed = true;
        }
         
        ~SUDDetector()      
        {
         
            Dispose(false);
        }

    }
}
