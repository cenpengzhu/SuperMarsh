using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SuperMarsh.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Model {
    //竞猜状态开盘等待投注，封盘结束投注，结算完毕
    public enum BetStatus {
        betting,
        betcomplete,
        finished,
    }
    //下注类型 单数投注 双数投注
    public enum BetType {
        singular,
        dual
    }
    public class BetModel {
        public BetModel() {
            userbetlist = new List<UserBetInfo>();
            betno = TimeHelper.CurrentUnixTime();
            singulartotalvalue = 0;
            dualtotalvalue = 0;
        }

        //竞猜编号
        private long betno;
        public long BetNo {
            get {
                return betno;
            }
        }

        //竞猜状态
        private BetStatus status;
        public BetStatus Status {
            get {
                return status;
            }
            set {
                if (status != value) {
                    status = value;
                }
            }
        }

        //竞猜信息
        private String betmessage;
        public String BetMessage {
            get {
                return betmessage;
            }
            set {
                betmessage = value;
            }
        }

        //单数信息
        private String betsingularmessage;
        public String BetSingularMessage {
            get {
                return betsingularmessage;
            }
            set {
                betsingularmessage = value;
            }
        }

        //双数信息
        private String betdualmessage;
        public String BetDualMessage {
            get {
                return betdualmessage;
            }
            set {
                betdualmessage = value;
            }
        }

        //获胜投注方向
        private BetType winnertype;
        public BetType WinnerType {
            get {
                return winnertype;
            }
            set {
                winnertype = value;
            }
        }
        //总投注瓜子数目
        public double TotalValue {
            get {
                return SingularTotalValue + DualTotalValue;
            }
        }

        //单数方总投注
        private double singulartotalvalue;
        public double SingularTotalValue {
            get {
                return singulartotalvalue;
            }
            set {
                if (singulartotalvalue < value)
                {
                    singulartotalvalue = value;
                }
            }
        }
        //双数方总投注
        private double dualtotalvalue;
        public double DualTotalValue {
            get {
                return dualtotalvalue;
            }
            set {
                if (dualtotalvalue < value)
                {
                    dualtotalvalue = value;
                }
            }
        }

        //投注列表
        private List<UserBetInfo> userbetlist;
        public List<UserBetInfo> UserBetList {
            get {
                return userbetlist;
            }
        }

        //下注
        public bool Bet(String userName,long userId,int giftCount,double giftPrice) {
            //如果当前竞猜可以投注
            if (Status == BetStatus.betting) {
                var tempUserBetInfo =  UserBetList.Find(p => p.UserId == userId && p.UserBetType ==(giftCount % 2 == 0 ? BetType.dual : BetType.singular));
                if (tempUserBetInfo == null)
                {
                    tempUserBetInfo = new UserBetInfo();
                    tempUserBetInfo.UserName = userName;
                    tempUserBetInfo.UserId = userId;
                    tempUserBetInfo.UserBetValue = giftCount * giftPrice;                    
                    tempUserBetInfo.UserBetType = giftCount % 2 == 0 ? BetType.dual : BetType.singular;
                    UserBetList.Add(tempUserBetInfo);
                }
                else {
                    tempUserBetInfo.UserBetValue += giftCount * giftPrice;
                }
                if (tempUserBetInfo.UserBetType == BetType.singular)
                {
                    SingularTotalValue += giftCount * giftPrice;
                }
                else if (tempUserBetInfo.UserBetType == BetType.dual)
                {
                    DualTotalValue += giftCount * giftPrice;
                }
                return true;
            }
            return false;
        }

        //结算
        public bool Settle(BetType winnerType) {
            WinnerType = winnerType;
            double WinnerTotalValue = winnerType == BetType.singular ? SingularTotalValue : DualTotalValue;
            foreach (var eachUserBetInfo in UserBetList) {
                if (eachUserBetInfo.UserBetType == winnerType) {
                    //赢得尸体计算
                    double temp = eachUserBetInfo.UserBetValue / WinnerTotalValue * TotalValue;
                    //10000瓜子折合1千克尸体
                    temp = temp / 10000;
                    var Collection = SingleInstanceHelper.Instance.DBHelper.DataBase.GetCollection<BsonDocument>(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName);
                    FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq(x => x["UserId"], eachUserBetInfo.UserId);
                    List<BsonDocument> UserInDB = Collection.Find(filter).ToList();
                    if (UserInDB != null && UserInDB.Count > 0)
                    {
                        var tempUserRecord = new UserRecordModel();
                        tempUserRecord.FromBson(UserInDB[0]);
                        tempUserRecord.UserGetWeight += temp;
                        var TempBson = BsonSerializer.Deserialize<BsonDocument>(JsonHelper.getJsonByObject(tempUserRecord));
                        SingleInstanceHelper.Instance.DBHelper.Save(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName, TempBson, "UserId");
                        //记录流水
                        UserGetWeightWaterRecordHelper.WaterRecord(tempUserRecord.UserName, tempUserRecord.UserId, temp, "增加", "竞猜获胜");
                    }
                }
            }
            DumpToDB();
            return true;
        }

        public bool DumpToDB() {
            if (Status == BetStatus.finished) {
                var temp = BsonSerializer.Deserialize<BsonDocument>(JsonHelper.getJsonByObject(this));
                SingleInstanceHelper.Instance.DBHelper.Insert(SingleInstanceHelper.Instance.DBHelper.BetRecordCoName, temp);
                return true;
            }
            return false;
        }
    }

    public class UserBetInfo {
        public String UserName;
        public long UserId;
        public BetType UserBetType;
        //用户下注数量=用户下注消耗的瓜子数目
        public double UserBetValue;
    }
}
