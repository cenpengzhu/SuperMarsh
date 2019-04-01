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
        private bool moveLock = false;
        private int originalHeight = 800;
        private int originalWidth = 800;
        private int changeHeight = 1000;
        private int changeWidth = 300;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MarchViewModel();
            this.DataContext = viewModel;
            this.Width = originalWidth;
            this.Height = originalHeight;
            extend = false;
            moveLock = false;
            extendButton.Content = ">";
            lockButton.Content = "lock";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!moveLock) {
                this.DragMove();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //设置定时器
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(10000000);   //时间间隔为一秒
            timer.Tick += new EventHandler((s,b)=> {
                viewModel.UpdateStatus(); });
                                
            //开启定时器
            timer.Start();

            //启动弹幕模块
            SingleInstanceHelper.Instance.DanmuLoader.ReceivedDanmaku += SingleInstanceHelper.Instance.MarshRuler.ReceiveDanmu;
            SingleInstanceHelper.Instance.DanmuLoader.ConnectAsync(1361615);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (extend)
            {
                this.Width = changeWidth;
                this.Height = changeHeight;
                extend = false;
                extendButton.Content = ">";
            }
            else {
                this.Width = originalWidth;
                this.Height = originalHeight;
                extend = true;
                extendButton.Content = "<";
            }
        }

        //结束沼泽
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SingleInstanceHelper.Instance.MarshRuler.StopMarsh();
            timer.Stop();
        }

        //锁定窗口
        private void lockButton_Click(object sender, RoutedEventArgs e)
        {
            if (moveLock)
            {
                moveLock = false;
                lockButton.Content = "lock";
            }
            else {
                moveLock = true;
                lockButton.Content = "unlock";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //如果没有或者竞猜已结束，则开始新的竞猜
            if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet == null || SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == Model.BetStatus.finished) {
                SingleInstanceHelper.Instance.MarshRuler.CurrentBet = new Model.BetModel();
                SingleInstanceHelper.Instance.MarshRuler.CurrentBet.BetMessage = BetMessage.Text;
                SingleInstanceHelper.Instance.MarshRuler.CurrentBet.BetSingularMessage = SingularMessage.Text;
                SingleInstanceHelper.Instance.MarshRuler.CurrentBet.BetDualMessage = DualMessage.Text;
                SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status = Model.BetStatus.betting;
            }
            else if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == Model.BetStatus.betting) {
                SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status = Model.BetStatus.betcomplete;
            }
            else if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == Model.BetStatus.betcomplete) {
                if (SingularWinCheck.IsChecked == true) {
                    SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status = Model.BetStatus.finished;
                    SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Settle(Model.BetType.singular);
                    //SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status = Model.BetStatus.finished;
                }
                else if (DualWinCheck.IsChecked == true) {
                    SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status = Model.BetStatus.finished;
                    SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Settle(Model.BetType.dual);
                   // SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status = Model.BetStatus.finished;
                }
                else {
                }               
            }
        }
    }
}
