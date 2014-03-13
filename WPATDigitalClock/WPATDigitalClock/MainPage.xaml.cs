using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Threading;

namespace WPATDigitalClock
{
    public partial class MainPage : PhoneApplicationPage
    {
        // コンストラクター
        public MainPage()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime utc_now = DateTime.UtcNow;

            DateLocal.Text = now.ToShortDateString() + "(" + now.ToString("ddd") + ")";
            DateUTC.Text = utc_now.ToShortDateString() + "(" + utc_now.ToString("ddd") + ")";
            TimeLocal.Text = now.ToLongTimeString();
            TimeUTC.Text = utc_now.ToLongTimeString();
        }
    }
}