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
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.ServiceModel.Syndication;
using Microsoft.Phone.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace WPATFeedReader
{
    public partial class MainPage : PhoneApplicationPage
    {

        // コンストラクター
        public MainPage()
        {
            InitializeComponent();
        }

        // 画面がロードされてからの処理：RSSデータを非同期で読み込む
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            //////////////////////////////////////////////////////////////////////////////////////
            //// ① データ読み込み処理
            //////////////////////////////////////////////////////////////////////////////////////
            string URL = "http://www.bing.com/news/?format=RSS";
            WebClient cli = new WebClient();
            cli.DownloadStringCompleted += new DownloadStringCompletedEventHandler(cli_DownloadStringCompleted);
            cli.DownloadStringAsync(new Uri(URL));
        }

        void cli_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            ////////////////////////////////////////////////////////////////////////////////////
            // ② 読み込みデータのを、オブジェクトに変換しUIに渡す処理
            ////////////////////////////////////////////////////////////////////////////////////
            progressBar1.Visibility = Visibility.Collapsed;

            if (e.Error != null)
            {
                MessageBox.Show("Network Connection Error!!");
            }
            else
            {
                StringReader sr = new StringReader(e.Result);
                XmlReader xmldata = XmlReader.Create(sr);
                SyndicationFeed rssdata = SyndicationFeed.Load(xmldata);
                this.listResult.ItemsSource = rssdata.Items;
            }
        }

        // データをダブルタップしたらブラウザでデータを表示する
        private void ShowInBrowser(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SyndicationItem item = (SyndicationItem)((sender as ListBox).SelectedValue);
            if (item != null)
            {
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = item.Links[0].Uri;
                task.Show();
            }
        }

        private void abmAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Version 1.0");
        }

        private void listResult_DoubleTap(object sender, GestureEventArgs e)
        {
            this.ShowInBrowser(sender, e);
        }

    }

}