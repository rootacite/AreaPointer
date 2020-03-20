using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AreaPointer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        static public MainWindow thisPtr;
        [DllImport("Kernel32.dll", EntryPoint = "TerminateProcess")]
        public static extern bool TerminateProcess(uint hThread, uint dwExitCode);
        [DllImport("Kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern uint OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        [DllImport("Kernel32.dll", EntryPoint = "GetCurrentProcess")]
        public static extern uint GetCurrentProcess();
        Area WinArea = new Area();
        List<int> hWndList = new List<int>();
        List<string> TitleList = new List<string>();
        public delegate bool CallBack(int hwnd, int y);
        [DllImport("user32.dll")]
        public static extern int EnumWindows(CallBack x, int y);
        [DllImport("user32")]
        public static extern int GetWindowText(int hwnd, StringBuilder lptrString, int nMaxCount);
        [DllImport("user32")]
        public static extern int GetParent(int hwnd);
        [DllImport("user32")]
        public static extern int IsWindowVisible(int hwnd);
        [DllImport("BuildIn.dll", EntryPoint = "ScreenShot")]
        public extern static uint ScreenShot(int opt_size);

        [DllImport("BuildIn.dll", EntryPoint = "getRate")]
        public extern static uint getRate(int hWnd);


        [DllImport("BuildIn.dll", EntryPoint = "FreeMemory")]
        public extern static void FreeMemory();
        Thread nRestore;
        public MainWindow()
        {
            InitializeComponent();
            thisPtr = this;
            //  MemoryStream stream = new MemoryStream(a);

             nRestore = new Thread(delegate() {
 
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    // this.MainGrid.Background = new ImageBrush(MainFarm);
                    Process[] ProcArray = Process.GetProcesses();
                    WndList.Items.Clear();
                    hWndList.Clear();
                    TitleList.Clear();
                    EnumWindows(this.Report, 0);
                }); 
            });
            nRestore.Start();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        public bool Report(int hwnd, int lParam)
        {
            int pHwnd;
            pHwnd = GetParent(hwnd);
            if (pHwnd == 0 && IsWindowVisible(hwnd) == 1)
            {
                StringBuilder sb = new StringBuilder(512);

                GetWindowText(hwnd, sb, sb.Capacity);
                if (sb.Length > 0)
                {
                    this.WndList.Items.Add(sb.ToString());
                    hWndList.Add(hwnd);
                    TitleList.Add(sb.ToString());
                }
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //  MessageBox.Show(WndList.Text);
            int ReshWnd = 0;
            for(int i = 0; i <= TitleList.Count; i++)
            {
                if (i == TitleList.Count)
                {
                    MessageBox.Show("错误", "error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }

                if(TitleList[i]== WndList.Text)
                {
                    ReshWnd = hWndList[i];
                    break;
                }
            }


            uint pPoint = getRate(ReshWnd);


            Thread waitForp = new Thread(delegate ()
              {
              int pW = DllTools.GetProcAddress(DllTools.GetModuleHandleA("BuildIn.dll"), "compis");
                  this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                  {
                      this.Hide();
                  });
                  unsafe
                  { 
                      while (!(*(bool*)pW))
                          Thread.Sleep(10);
                  }

                  this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                  {
                      WinArea.Show();
                   
                      unsafe
                      {
                       //   MessageBox.Show("l:" + (*(int*)pPoint).ToString()+"\n"+"t:" + (*(int*)(pPoint + 4)).ToString()+"\n"+"r:"+ (*(int*)(pPoint + 8)).ToString()+"\n"+"b:"+ (*(int*)(pPoint + 12)).ToString());

                          Area.mArea.X = *(int*)pPoint;
                          Area.mArea.Y = *(int*)(pPoint + 4);
                          Area.mArea.Width = (*(int*)(pPoint + 8))- *(int*)pPoint;
                          Area.mArea.Height =  (*(int*)(pPoint + 12))- *(int*)(pPoint + 4);

                       //   MessageBox.Show(Area.mArea.ToString());
                      }
                      Area.dispatcherTimer.Start();

                  });

              });

            waitForp.Start();
            //    MessageBox.Show(x.ToString() + ":" + y.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            TerminateProcess(GetCurrentProcess(), 1);
        }
    }
}
