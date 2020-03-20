using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Windows.Threading;

namespace AreaPointer
{
    /// <summary>
    /// Area.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class Area : Window
    {
        static public Int32Rect mArea;
       static public MemoryStream mBit;
        static public byte[] Data;

        static  public BitmapImage MainFarm;
       public static  DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public Area()
        {
            InitializeComponent();
            dispatcherTimer.Tick += new EventHandler(TickMod);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(0.06);

        }

        public void TickMod(object a,EventArgs e)
        {
            int nSize;

            unsafe
            {
                uint pData = MainWindow.ScreenShot((int)&nSize);
                Data = new byte[nSize];
                for (int i = 0; i < nSize; i++)
                {
                    Data[i] = *(byte*)(pData + i);
                }
            }
            mBit = new MemoryStream(Data);
            MainWindow.FreeMemory();
            MainFarm = new BitmapImage();
            MainFarm.BeginInit();
            MainFarm.StreamSource = mBit;
            MainFarm.EndInit();
            //   MessageBox.Show("0");

            CroppedBitmap asBit = new CroppedBitmap(MainFarm, mArea);

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                this.SW.Background = new ImageBrush(asBit);
            });
        //   MessageBox.Show("1");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.thisPtr.Close();
        }
    }
}
