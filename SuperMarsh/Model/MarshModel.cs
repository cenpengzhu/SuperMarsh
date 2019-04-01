using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SuperMarsh.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Model {
    public class MarshModel {
        public void FromBson(BsonDocument Content) {
            StartTime = TimeHelper.CurrentUnixTime();
            LeftTime = Content["LeftTime"].ToInt64();
            MarshNo = Content["MarshNo"].ToInt32();
            TotalWeight = Content["TotalWeight"].ToDouble();
            CurrentRunner = Content["CurrentRunner"].ToString();
            CurrentRunnerPower = Content["CurrentRunnerPower"].ToDouble();
            TotalPower = Content["TotalPower"].ToDouble();
            Winner = Content["Winner"].ToString();
            WinnerWeight = Content["WinnerWeight"].ToDouble();
            WinnerId = Content["WinnerId"].ToInt32();
            CurrentRunnerId = Content["CurrentRunnerId"].ToInt32();
            RunTimes = Content["RunTimes"].ToInt32();
            LastRunnerLeftTime = Content["LastRunnerLeftTime"].ToInt64();
        }
        //沼泽轮数
        private int marshNo;
        public int MarshNo {
            get {
                return marshNo;
            }
            set {
                if (value > marshNo) {
                    marshNo = value;
                }
            }
        }

        //沼泽开始时间
        private long startTime;
        public long StartTime {
            get {
                return startTime;
            }
            set {
                if (value > 0) {
                    startTime = value;
                }
            }
        }

        //沼泽当前时间
        public long CurrentTime {
            get {
                return TimeHelper.CurrentUnixTime();
            }
        }

        //沼泽剩余时间
        public long LeftTime {
            get {
                return TotalTime - CurrentTime + StartTime;
            }
            set {
                if (value > 1800 || value < 0)
                {
                    return;
                }
                else {
                    TotalTime = value + CurrentTime - StartTime;
                }
            }
        }

        //沼泽倒计时总时间
        private long totalTime;
        public long TotalTime {
            get {
                return totalTime;
            }
            set {
                if (value > totalTime) {
                    totalTime = value;
                }
            }
        }

        //沼泽尸体总重
        private double totalWeight;
        public double TotalWeight {
            get {
                return totalWeight;
            }
            set {
                if (value != totalWeight) {
                    totalWeight = value;
                }
            }
        }

        //沼泽当前正在逃离的人
        private String currentRunner;
        public String CurrentRunner {
            get {
                if (IsFan)
                {
                    return currentRunner;
                }
                else {
                    return currentRunner;
                }
            }
            set {
                currentRunner = value;
            }
        }

        //沼泽当前正在逃离的人的id
        private int currentRunnerId;
        public int CurrentRunnerId {
            get {
                return currentRunnerId;
            }
            set {
                currentRunnerId = value;
            }
        }

        //沼泽当前正在逃离的人是否关注
        private bool isFan;
        public bool IsFan {
            get {
                return isFan;
            }
            set {
                isFan = value;

            }
        }

        //沼泽当前逃离的人所使用的power
        private double currentRunnerPower;
        public double CurrentRunnerPower {
            get {
                return currentRunnerPower;
            }
            set {
                currentRunnerPower = value;
            }
        }

        //本轮沼泽的总power
        private double totalPower;
        public double TotalPower {
            get {
                return totalPower;
            }
            set {
                if (value > totalPower) {
                    totalPower = value;
                }
            }
        }

        //沼泽的胜利者
        private String winner;
        public String Winner {
            get {
                return winner;
            }
            set {
                if (value != "" && value != null)
                {
                    winner = value;
                }
                else {
                    winner = "无";
                }
            }
        }

        //沼泽胜利者的ID
        private int winnerId;
        public int WinnerId {
            get {
                return winnerId;
            }
            set {
                winnerId = value;
            }
        }


        //沼泽胜利者赢得的尸体
        private double winnerWeight;
        public double WinnerWeight {
            get {
                return winnerWeight;
            }
            set {
                if (value != winnerWeight) {
                    winnerWeight = value;
                }
            }
        }

        //沼泽沼底巨兽ID
        private int monsterId = 0;
        public int MonsterId {
            get {
                return monsterId;
            }
            set {
                if (monsterId != value) {
                    monsterId = value;
                }
            }
        }

        //沼泽沼底巨兽名字
        private String monsterName = "无";
        public String MonsterName {
            get {
                return monsterName;
            }
            set {
                if (monsterName != value) {
                    monsterName = value;
                }
            }
        }

        //总共挑战次数
        private int runTimes;
        public int RunTimes {
            get {
                return runTimes;
            }
            set {
                if (runTimes != value) {
                    runTimes = value;
                }
            }
        }


        //上次挑战者剩余时间
        private long lastRunnerLeftTime;
        public long LastRunnerLeftTime {
            get {
                return lastRunnerLeftTime;
            }
            set {
                if (lastRunnerLeftTime != value) {
                     lastRunnerLeftTime = value;
                }
            }
        }

        //沼气爆炸几率(百分之)
        public int BoomPercent {
            get {
                if ((LastRunnerLeftTime - LeftTime) > 20)
                {
                    int temp = (int)(LastRunnerLeftTime - LeftTime - 20) / 4;
                    if (temp > 100)
                    {
                        return 100;
                    }
                    else {
                        return temp;
                    }
                }
                else {
                    return 0;
                }
            }
        }

        //沼泽是否逃离成功
        public bool IsWin() {
            if (LeftTime > 0)
            {
                return false;
            }
            else {
                return true;
            }
        }

        //赢得沼泽
        public bool WinMarsh() {
            if (IsWin()) {
                //计算本局沼泽的信息
                Winner = CurrentRunner;
                WinnerId = CurrentRunnerId;
                WinnerWeight = TotalWeight * 0.5;
                //更新所有用户的尸体信息
                var TempUserRecordList = SingleInstanceHelper.Instance.DBHelper.FindAll(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName);
                List<UserRecordModel> UserRecordList = new List<UserRecordModel>();
                double AllUserTotalPower = 0.0;
                foreach (var eachUserRecord in TempUserRecordList) {
                    var tempUserRecord = new UserRecordModel();
                    tempUserRecord.FromBson(eachUserRecord);
                    AllUserTotalPower += tempUserRecord.UserTotalPower;
                    UserRecordList.Add(tempUserRecord);
                }
                foreach (var eachUserRecord in UserRecordList) {
                    eachUserRecord.UserGetWeight += (TotalPower / 10000 * 0.5) / AllUserTotalPower * eachUserRecord.UserTotalPower;
                    //记录尸体流水
                    UserGetWeightWaterRecordHelper.WaterRecord(eachUserRecord.UserName, eachUserRecord.UserId, (TotalPower / 10000 * 0.5) / AllUserTotalPower * eachUserRecord.UserTotalPower, "增加", "沼泽分红");
                    if (eachUserRecord.UserId == WinnerId) {
                        eachUserRecord.UserGetWeight += WinnerWeight;
                        UserGetWeightWaterRecordHelper.WaterRecord(eachUserRecord.UserName, eachUserRecord.UserId, WinnerWeight, "增加", "沼泽获胜");
                    }
                    var TempBson = BsonSerializer.Deserialize<BsonDocument>(JsonHelper.getJsonByObject(eachUserRecord));
                    SingleInstanceHelper.Instance.DBHelper.Save(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName, TempBson, "UserId");
                }
                return true;
            }
            return false;
        }

        //停止沼泽
        public bool StopMarsh() {
            return true;

        }
        //有新的挑战者 Power为瓜子数量
        public int GetOneRunner(String RunnerName, double Power, int UserId, int addFollow)
        {
            if (IsWin()) {
                return 0;
            }
            RunTimes += 1;

            //正常结束返回1
            int result = 1;

            if (BoomPercent > 0) {
                Random rd = new Random();
                int rdResult = rd.Next(0, 100);
                if(rdResult < BoomPercent) {
                    //沼气爆炸
                    result = 2;
                    double temp = TotalWeight * 0.1;
                    var Collection = SingleInstanceHelper.Instance.DBHelper.DataBase.GetCollection<BsonDocument>(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName);
                    FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq(x => x["UserId"], UserId);
                    List<BsonDocument> UserInDB = Collection.Find(filter).ToList();
                    if (UserInDB != null && UserInDB.Count > 0) {
                        var tempUserRecord = new UserRecordModel();
                        tempUserRecord.FromBson(UserInDB[0]);
                        tempUserRecord.UserGetWeight += temp;
                        var TempBson = BsonSerializer.Deserialize<BsonDocument>(JsonHelper.getJsonByObject(tempUserRecord));
                        SingleInstanceHelper.Instance.DBHelper.Save(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName, TempBson, "UserId");
                        //记录流水
                        UserGetWeightWaterRecordHelper.WaterRecord(tempUserRecord.UserName, tempUserRecord.UserId, temp, "增加", "沼气爆炸");
                    }
                    TotalWeight = TotalWeight - temp;
                }
                //否则无事发生
            }



            if (LeftTime + 10 > 1800)
            {
                LeftTime = 1800;
            }
            else {
                LeftTime += 10;
            }

            LastRunnerLeftTime = LeftTime;
            TotalWeight += Power / 10000 * 0.5;
            CurrentRunner = RunnerName;
            CurrentRunnerPower = Power;
            TotalPower += Power;
            CurrentRunnerId = UserId;

            if (addFollow == 0)
            {
                IsFan = false;
            }
            else {
                IsFan = true;
            }
            return result;
        }

        //生成下一轮沼泽
        public MarshModel NextMarsh() {
            if (IsWin())
            {
                var result = new MarshModel();
                result.MarshNo = MarshNo + 1;
                result.CurrentRunner = "无";
                result.CurrentRunnerPower = 0.0;
                result.StartTime = TimeHelper.CurrentUnixTime();
                result.LeftTime = 1800;
                result.TotalPower = 0.0;
                result.TotalWeight = TotalWeight - WinnerWeight + 10;
                result.WinnerWeight = 0.0;
                result.Winner = "无";
                result.RunTimes = 0;
                result.LastRunnerLeftTime = 1800;
                return result;
            }
            else {
                return null;
            }
        }
    }
}
