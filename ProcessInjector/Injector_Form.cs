﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using EasyHook;

namespace ProcessInjector
{
    public partial class Injector_Form : Form
    {
        private string Version = "";
        private string ProcessName = "";
        private string ProcessPath = "";
        private int ProcessID = -1;

        private ComputerInfo ci = new ComputerInfo();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        //窗体加载
        public Injector_Form()
        {
            InitializeComponent();
            this.Text = "进程注入器（x86, x64自适应）";

            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ShowLog("当前内核版本：" + Version);
        }

        //记录日志
        private void ShowLog(string ShowInfo)
        {
            this.rtbLog.AppendText(ShowInfo + "\n");
        }

        //判断是否为64位的进程
        private bool IsWin64Process(int ProcessID)
        {
            bool bReturn = false;

            Process pProcess = Process.GetProcessById(ProcessID);

            if (pProcess != null)
            {
                if ((Environment.OSVersion.Version.Major > 5) || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
                {
                    bool retVal;
                    IsWow64Process(pProcess.Handle, out retVal);
                    bReturn = !retVal;
                }
            }

            return bReturn;
        }

        //选择进程
        private void bSelectProcess_Click(object sender, EventArgs e)
        {
            ProcessList_Form plf = new ProcessList_Form();
            plf.ShowDialog();

            if (Program.PID != -1 && Program.PNAME != string.Empty)
            {
                tbProcessID.Text = Program.PNAME + " [" + Program.PID + "]";
            }
            else if (Program.PID == -1 && !string.IsNullOrEmpty(Program.PNAME) && !string.IsNullOrEmpty(Program.PATH))
            {
                tbProcessID.Text = Program.PNAME;
            }
        }

        //注入
        private void bInject_Click(object sender, EventArgs e)
        {
            this.rtbLog.Clear();

            ProcessID = Program.PID;
            ProcessPath = Program.PATH;
            ProcessName = Program.PNAME;
            string sDllName = "WPELibrary.dll";

            try
            {
                if (string.IsNullOrEmpty(ProcessPath) && string.IsNullOrEmpty(ProcessName))
                {
                    MessageBox.Show("请先选择要注入的进程！");
                }
                else
                {
                    string injectionLibrary_x86 = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), sDllName);
                    string injectionLibrary_x64 = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), sDllName);

                    ShowLog("开始注入目标进程 =>> " + ProcessName);

                    if (ProcessID > -1)
                    {
                        RemoteHooking.Inject(ProcessID, injectionLibrary_x86, injectionLibrary_x64, "");
                    }
                    else
                    {
                        RemoteHooking.CreateAndInject(ProcessPath, string.Empty, 0, injectionLibrary_x86, injectionLibrary_x64, out this.ProcessID, string.Empty);
                    }

                    int targetPlat = IsWin64Process(ProcessID) ? 64 : 32;
                    ShowLog(string.Format("目标进程是{0}位程序，已自动调用{0}位的注入模块!", targetPlat));
                    ShowLog(string.Format("已成功注入目标进程 =>> {0}[{1}]", ProcessName, ProcessID));
                    ShowLog("注入完成，可关闭当前注入器.");
                }
            }
            catch (Exception ex)
            {
                ShowLog("出现错误：" + ex.Message);
            }
        }
    }
}
