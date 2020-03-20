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
        static public int loadH;
        static public int loadW;
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
            CroppedBitmap asBit;
            try
            {
                 asBit = new CroppedBitmap(MainFarm, mArea);
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    this.SW.Background = new ImageBrush(asBit);
                });
            }
            catch (Exception)
            {
                this.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
                {
                    this.SW.Background = new ImageBrush(MainFarm);
                });
            }

           
        //   MessageBox.Show("1");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.thisPtr.Close();
        }
        static double rate = 1.0;
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Delta < 0 && rate > 0.05) 
                {
                    if (Height * (rate - 0.05) < 50 || Width * (rate - 0.05) < 50)
                        return;
                    rate -= 0.05;
                    this.Height = loadH * rate;
                    this.Width = loadW * rate;
                }
                else
                {
                    if (Height * (rate + 0.05) > SystemParameters.WorkArea.Height || Width * (rate + 0.05) > SystemParameters.WorkArea.Width)
                        return;
                    rate += 0.05;
                    this.Height = loadH * rate;
                    this.Width = loadW * rate;
                }
            }
            else
            {
                if (e.Delta < 0)
                {
                    if (this.Opacity > 0.05)
                    {
                        this.Opacity -= 0.03;
                        SW.Background.Opacity -= 0.03;
                    }
                }
                else
                {
                    if (this.Opacity < 1)
                    {
                        this.Opacity += 0.03;
                        SW.Background.Opacity += 0.03;
                    }
                }
            }
           
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
