﻿using System.Windows.Forms;

using EasyHook;

namespace WPELibrary.Lib
{
    public class MainClass : IEntryPoint
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
        public MainClass(RemoteHooking.IContext context, string channelName)
        {
            //
        }

        public void Run(RemoteHooking.IContext context, string channelName)
        {
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DLL_Form());
        }
    }
}
