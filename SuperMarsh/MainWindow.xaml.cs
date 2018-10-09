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
using SuperMarsh.ViewModel;
using System.Windows.Threading;
using SuperMarsh.Helper;

namespace SuperMarsh {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private DispatcherTimer timer;
        private MarchViewModel viewModel;
        private bool extend = false;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MarchViewModel();
            this.DataContext = viewModel;
            this.Width = 545;
            extend = false;
            extendButton.Content = ">";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //设置定时器
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(10000000);   //时间间隔为一秒
            timer.Tick += new EventHandler((s,b)=> {
                this.Topmost = true;
                viewModel.UpdateStatus(); });
                                
            //开启定时器
            timer.Start();

            //启动弹幕模块
            SingleInstanceHelper.Instance.DanmuLoader.ReceivedDanmaku += SingleInstanceHelper.Instance.MarshRuler.ReceiveDanmu;
            SingleInstanceHelper.Instance.DanmuLoader.ConnectAsync(1948661);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (extend)
            {
                this.Width = 545;
                extend = false;
                extendButton.Content = ">";
            }
            else {
                this.Width = 1000;
                extend = true;
                extendButton.Content = "<";
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SingleInstanceHelper.Instance.MarshRuler.StopMarsh();
            timer.Stop();
        }
    }
}
