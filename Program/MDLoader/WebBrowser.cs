using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MDLoader
{
    class SetWebbrowser
    {
        /// <summary>
        /// 修改Webbrowser控件模拟的IE版本
        /// </summary>
        /// <param name="ieMode">
        /// 7000: Pages containing standards-based <!DOCTYPE> directives aredisplayed in IE7 mode.
        /// 8000: Pages containing standards-based <!DOCTYPE> directives aredisplayed in IE8 mode
        /// 8888: Pages are always displayed in IE8mode, regardless of the <!DOCTYPE>directive. (This bypasses the exceptions listed earlier.)
        /// 9000: Use IE9 settings!
        /// 9999: Force IE9
        /// 10000: Use IE10 settings
        /// 11000: Use IE11 settings
        /// </param>
        public static void ChangeWebbrowserMode(int ieMode)
        {
            string appName = AppDomain.CurrentDomain.FriendlyName;
            string regPath = "";
            if (Is64BitOperatingSystem())
            {
                regPath = @"SOFTWARE\WOW6432Node\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION";
            }
            else
            {
                regPath = @"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION";
            }
            using (RegistryKey ieMainKey = Registry.LocalMachine.OpenSubKey(
                 regPath, true))
            {
                var orignalMode = ieMainKey.GetValue(appName);
                if (orignalMode == null || (int)orignalMode != ieMode)
                {
                    ieMainKey.SetValue(appName, ieMode, RegistryValueKind.DWord);
                }
                //
            }
        }


        /// <summary>
        /// The function determines whether the current operating system is a 
        /// 64-bit operating system.
        /// </summary>
        /// <returns>
        /// The function returns true if the operating system is 64-bit; 
        /// otherwise, it returns false.
        /// </returns>
        public static bool Is64BitOperatingSystem()
        {
            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                return true;
            }
            else  // 32-bit programs run on both 32-bit and 64-bit Windows
            {
                // Detect whether the current process is a 32-bit process 
                // running on a 64-bit system.
                bool flag;
                return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
                    IsWow64Process(GetCurrentProcess(), out flag)) && flag);
            }
        }

        /// <summary>
        /// The function determins whether a method exists in the export 
        /// table of a certain module.
        /// </summary>
        /// <param name="moduleName">The name of the module</param>
        /// <param name="methodName">The name of the method</param>
        /// <returns>
        /// The function returns true if the method specified by methodName 
        /// exists in the export table of the module specified by moduleName.
        /// </returns>
        static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule,
            [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);
    }


    public class WebBrowserEx :  WebBrowser

    {

        private const int WM_DROPFILES = 0x233;



        [DllImport("shell32.dll")]

        private static extern uint DragQueryFile(

            IntPtr hDrop,

            uint iFile,

            StringBuilder lpszFile,

            uint cch);



        [DllImport("shell32.dll")]

        private static extern void DragAcceptFiles(IntPtr hWnd, bool fAccept);



        public WebBrowserEx()

            : base()

        {

            base.AllowWebBrowserDrop = false;

        }



        public event DragFileEventHandler DragFile;



        protected override void OnHandleCreated(EventArgs e)

        {

            base.OnHandleCreated(e);

            if (!DesignMode)

            {

                DragAcceptFiles(Handle, true);

            }

        }



        protected override void WndProc(ref Message m)

        {

            StringBuilder sb = new StringBuilder(1024);

            if (m.Msg == WM_DROPFILES)

            {

                uint count = DragQueryFile(m.WParam, 0xffffffff, null, 0);

                string[] files = new string[count];

                for (uint i = 0; i < count; i++)

                {

                    DragQueryFile(m.WParam, i, sb, 1024);

                    files[i] = sb.ToString();

                }

                OnDragFile(new DragFileEventArgs(files));

                return;

            }



            base.WndProc(ref m);

        }



        protected virtual void OnDragFile(DragFileEventArgs e)

        {

            if (DragFile != null)

            {

                DragFile(this, e);

            }

        }

    }



    public delegate void DragFileEventHandler(

        object sender,

        DragFileEventArgs e);



    public class DragFileEventArgs : EventArgs

    {

        private string[] _files;



        public DragFileEventArgs(string[] files)

        {

            _files = files;

        }



        public string[] Files

        {

            get { return _files; }

        }

    }

}
