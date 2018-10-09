﻿using MongoDB.Bson;
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
                if (value > 3600 || value < 0)
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
                    eachUserRecord.UserGetWeight += (TotalPower / 100000 * 0.5) / AllUserTotalPower * eachUserRecord.UserTotalPower;
                    if (eachUserRecord.UserId == WinnerId) {
                        eachUserRecord.UserGetWeight += WinnerWeight;
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
        public bool GetOneRunner(String RunnerName,double Power,int UserId,int addFollow)
        {
            if (IsWin()) {
                return false;
            }
           if (LeftTime + 10 > 3600)
            {
                LeftTime = 3600;
            }
            else {
                LeftTime += 10;
            }
            TotalWeight += Power / 100000 * 0.5;
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
            return true;
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
                result.LeftTime = 3600;
                result.TotalPower = 0.0;
                result.TotalWeight = TotalWeight - WinnerWeight + 10;
                result.WinnerWeight = 0.0;
                result.Winner = "无";
                return result;
            }
            else {
                return null;
            }
        }
    }
}
