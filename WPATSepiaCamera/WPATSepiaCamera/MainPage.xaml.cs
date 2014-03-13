using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Devices;
using Microsoft.Xna.Framework.Media;

namespace WPATSepiaCamera
{
    public partial class MainPage : PhoneApplicationPage
    {
        PhotoCamera cam;
        MediaLibrary library = new MediaLibrary();
        DispatcherTimer timer;
        Queue<String> messages;

        // コンストラクター
        public MainPage()
        {
            InitializeComponent();

            txtMessage.Text = "";
            messages = new Queue<string>();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += new EventHandler(timer_tick);
            timer.Start();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true)
            {
                cam = new PhotoCamera(CameraType.Primary);
                cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                if ((MessageBox.Show("A Camera is not available on this device.", "Quit", MessageBoxButton.OK) == MessageBoxResult.OK))
                {
                    timer.Stop();
                    App.Quit();
                }
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                timer.Stop();
                cam.Dispose();
            }
        }

        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                BitmapImage bi = new BitmapImage();
                bi.SetSource(e.ImageStream);
                WriteableBitmap wb = new WriteableBitmap(bi);

                uint filter = 0xFFC29670;
                byte fR = (byte)((filter & 0x00FF0000) >> 16);
                byte fG = (byte)((filter & 0x0000FF00) >> 8);
                byte fB = (byte)(filter & 0x000000FF);
                for (int pixel = 0; pixel < wb.Pixels.Length; pixel++)
                {
                    int color = wb.Pixels[pixel];
                    byte A = (byte)((color & 0xFF000000) >> 24);
                    byte R = (byte)((color & 0x00FF0000) >> 16);
                    R = (R > fR) ? fR : R;
                    byte G = (byte)((color & 0x0000FF00) >> 8);
                    G = (G > fG) ? fG : G;
                    byte B = (byte)(color & 0x000000FF);
                    B = (B > fB) ? fB : B;
                    color = (A << 24) | (R << 16) | (G << 8) | B;
                    wb.Pixels[pixel] = color;
                }

                String tempJPEG = "TempJPEG";
                var store = IsolatedStorageFile.GetUserStoreForApplication();
                if (store.FileExists(tempJPEG))
                {
                    store.DeleteFile(tempJPEG);
                }
                IsolatedStorageFileStream stream = store.CreateFile(tempJPEG);
                Extensions.SaveJpeg(wb, stream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                stream.Close();

                string fileName = String.Format("sepia_%s.jpg", new DateTime().ToString("yyyyMMddhhmmss"));
                stream = store.OpenFile(tempJPEG, FileMode.Open, FileAccess.Read);
                library.SavePictureToCameraRoll(fileName, stream);
                stream.Close();

                showNotice("Picture has been saved to camera roll.");
            });
        }

        private void Shot_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null)
            {
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        showNotice("Captured image available, saving picture.");
                    });
                    cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        showNotice(ex.Message);
                    });
                }
            }
        }

        private void showNotice(String message)
        {
            messages.Enqueue(message);
        }

        private void timer_tick(object Sender, EventArgs e)
        {
            if (messages.Count > 0)
            {
                txtMessage.Text = messages.Dequeue();
            }
            else
            {
                txtMessage.Text = "";
            }
        }
    }
}