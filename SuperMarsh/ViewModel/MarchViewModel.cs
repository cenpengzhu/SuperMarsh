using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMarsh.Helper;
using SuperMarsh.Model;
using System.IO;

namespace SuperMarsh.ViewModel {
    public class MarchViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(String propertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
                   
        public MarchViewModel() {     
            startTime = TimeHelper.CurrentUnixTime();
        }

        private long startTime;
        private static readonly object WriteFileLock = new object();

        //private String currentRunnner;
        public String CurrentRunner {
            get {
                return SingleInstanceHelper.Instance.MarshRuler.CurrentMarsh.CurrentRunner;
            }
        }

        //private double currentPool;
        public String CurrentPoolWeight {
            get {
                return SingleInstanceHelper.Instance.MarshRuler.CurrentMarsh.TotalWeight.ToString("f5") + "千克";
            }
        }

       // private long totalSeconds;


        public long TotalSeconds {
            get {
                return SingleInstanceHelper.Instance.MarshRuler.CurrentMarsh.TotalTime;
            }
        }

        private long LeftTime {
            get {
                return SingleInstanceHelper.Instance.MarshRuler.CurrentMarsh.LeftTime;
            }
        }

        public String LeftHours {
            get {
                if ((LeftTime) > 1)
                {
                    return String.Format("{0:D2}", ((LeftTime) / 3600));
                }
                else {
                    return "00";
                }
            }
        }

        public String LeftMins {
            get {
                if ((LeftTime) > 1)
                {
                    return String.Format("{0:D2}", ((LeftTime) % 3600) / 60);
                }
                else {
                    return "00";
                }
            }
        }

        public String LeftSeconds {
            get {
                if ((LeftTime) > 0)
                {
                    return String.Format("{0:D2}", (LeftTime) % 60);
                }
                else {
                    return "00";
                }
            }
        }

        public List<UserRecordModel> UserRecordList {
            get {
                return SingleInstanceHelper.Instance.MarshRuler.TopTenUserRecord;
            }
        }

        public String PekingTime {
            get {
                return System.DateTime.Now.ToString();
            }
        }

        public String MarshNo {
            get {
                return SingleInstanceHelper.Instance.MarshRuler.CurrentMarsh.MarshNo.ToString() + "轮";
            }
        }

        public String RunTimes {
            get {
                String temp = "正在第";
                temp += SingleInstanceHelper.Instance.MarshRuler.CurrentMarsh.RunTimes.ToString();
                temp += "次逃离:";
                return temp;
            }
        }

        public String BoomPercent {
            get {
                return SingleInstanceHelper.Instance.MarshRuler.CurrentMarsh.BoomPercent + "%";
            }
        }

        public String BetButtonText {
            get {
                if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet == null) {
                    return "开盘";
                }
                if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == BetStatus.betting) {
                    return "封盘";
                }
                if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == BetStatus.betcomplete)
                {
                    return "结算";
                }
                if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == BetStatus.finished)
                {
                    return "开盘";
                }
                return "";
            }
        }


        public void UpdateStatus() {
            OnPropertyChanged("LeftSeconds");
            OnPropertyChanged("LeftMins");
            OnPropertyChanged("LeftHours");
            OnPropertyChanged("CurrentPoolWeight");
            OnPropertyChanged("CurrentRunner");
            OnPropertyChanged("UserRecordList");
            OnPropertyChanged("PekingTime");
            OnPropertyChanged("MarshNo");
            OnPropertyChanged("RunTimes");
            OnPropertyChanged("BoomPercent");
            OnPropertyChanged("BetButtonText");

            String MarshStatusContent = "      沼泽游戏\n"+"        ";

            MarshStatusContent += (MarshNo + "\n");

            MarshStatusContent += "逃出倒计时:\n    ";
            MarshStatusContent += (LeftHours+":"+LeftMins+":"+LeftSeconds+"\n");

            MarshStatusContent += "尸体总重:\n    ";
            MarshStatusContent += (CurrentPoolWeight+ "\n");

            MarshStatusContent += (RunTimes+ "\n      ");
            MarshStatusContent += (CurrentRunner+ "\n");

            MarshStatusContent += "沼气爆炸几率:\n      ";
            MarshStatusContent += (BoomPercent + "\n");

            MarshStatusContent += "系统时间:\n  ";
            MarshStatusContent += (PekingTime+ "\n");


            WriteToMarshStatus(MarshStatusContent);

            String TopTenContent = "尸体榜前十:\n";
            foreach (var eachUser in UserRecordList) {
                TopTenContent += eachUser.ToString();
                TopTenContent += "\n";
            }
            WriteToTopTenUser(TopTenContent);

            String BetInfo = "";
            if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet == null)
            {
                BetInfo += "暂无竞猜！";
            }
            else {
                BetInfo += SingleInstanceHelper.Instance.MarshRuler.CurrentBet.BetMessage;
                BetInfo += "\n";
                BetInfo += "投喂奇数个礼物支持--";
                BetInfo += SingleInstanceHelper.Instance.MarshRuler.CurrentBet.BetSingularMessage;
                BetInfo += "--";
                BetInfo += SingleInstanceHelper.Instance.MarshRuler.CurrentBet.SingularTotalValue.ToString();
                BetInfo += "\n投喂偶数个礼物支持--";
                BetInfo += SingleInstanceHelper.Instance.MarshRuler.CurrentBet.BetDualMessage;
                BetInfo += "--";
                BetInfo += SingleInstanceHelper.Instance.MarshRuler.CurrentBet.DualTotalValue.ToString();
                if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == BetStatus.betting)
                {
                    BetInfo += "\n正在投注中！";
                }
                else if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == BetStatus.betcomplete)
                {
                    BetInfo += "\n投注已结束！";
                }
                else if (SingleInstanceHelper.Instance.MarshRuler.CurrentBet.Status == BetStatus.finished) {
                    BetInfo += "\n竞猜已结算！";
                }
            }
            WriteToBet(BetInfo);

        }

        public void WriteToMarshStatus(String content)
        {
            lock (WriteFileLock)
            {
                FileStream fs = new FileStream("F:\\MarshStatus.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(content);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
        }

        public void WriteToTopTenUser(String content)
        {
            lock (WriteFileLock)
            {
                FileStream fs = new FileStream("F:\\TopTenUser.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(content);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
        }

        public void WriteToBet(String content)
        {
            lock (WriteFileLock)
            {
                FileStream fs = new FileStream("F:\\Bet.txt", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(content);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
        }

    }
}
