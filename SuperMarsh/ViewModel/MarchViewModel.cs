using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperMarsh.Helper;
using SuperMarsh.Model;

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
                if ((LeftTime) > 1)
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

        public void UpdateStatus() {
            OnPropertyChanged("LeftSeconds");
            OnPropertyChanged("LeftMins");
            OnPropertyChanged("LeftHours");
            OnPropertyChanged("CurrentPoolWeight");
            OnPropertyChanged("CurrentRunner");
            OnPropertyChanged("UserRecordList");
            OnPropertyChanged("PekingTime");
        }

    }
}
