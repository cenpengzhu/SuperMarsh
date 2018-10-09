using BilibiliDM_PluginFramework;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using SuperMarsh.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Model {
    public class MarshRuler {
        public MarshRuler() {
        }
        //所有沼泽轮数记录
        private List<MarshModel> allMarshs;
        public List<MarshModel> AllMarshs {
            get {
                if (allMarshs == null) {
                    allMarshs = new List<MarshModel>();
                }
                return allMarshs;
            }
        }


        //正在进行的沼泽
        public MarshModel CurrentMarsh {
            get {
                if (AllMarshs.Count == 0) {
                    LoadFromDB();
                }
                MarshModel temp = AllMarshs.Find(x => x.MarshNo == AllMarshs.Max(y => y.MarshNo));
                if (temp.IsWin())
                {
                    temp.WinMarsh();
                    DumpToDB();
                    var tempNextMarsh = temp.NextMarsh();
                    AllMarshs.Add(tempNextMarsh);
                    return tempNextMarsh;
                }
                else {
                    return temp;
                }
            }
        }

        //从数据库中读取
        private bool LoadFromDB()
        {
            var tempGiftValueList = SingleInstanceHelper.Instance.DBHelper.FindAll(SingleInstanceHelper.Instance.DBHelper.GiftValueCoName);
            if (tempGiftValueList == null || tempGiftValueList.Count == 0)
            {

            }
            else {
                foreach (var eachGiftValueBson in tempGiftValueList) {
                    var temp = new GiftValueModel();
                    temp.FromBson(eachGiftValueBson);
                    GiftValueList.Add(temp);
                }
            }

            var tempMarshList = SingleInstanceHelper.Instance.DBHelper.FindAll(SingleInstanceHelper.Instance.DBHelper.MarshRecordCoName);
            if (tempMarshList == null || tempMarshList.Count == 0)
            {
                MarshModel temp = new MarshModel();
                temp.TotalTime = 3600;
                temp.StartTime = TimeHelper.CurrentUnixTime();
                temp.TotalWeight = 10;
                temp.CurrentRunner = "无";
                AllMarshs.Add(temp);
            }
            else {
                foreach (var eachMarsh in tempMarshList) {
                    var temp = new MarshModel();
                    temp.FromBson(eachMarsh);
                    AllMarshs.Add(temp);
                }
            }
            return true;
        }

        //保存到数据库
        private bool DumpToDB()
        {
            foreach (var eachGiftValue in GiftValueList) {
                var temp = BsonSerializer.Deserialize<BsonDocument>(JsonHelper.getJsonByObject(eachGiftValue));
                SingleInstanceHelper.Instance.DBHelper.Save(SingleInstanceHelper.Instance.DBHelper.GiftValueCoName, temp, "Giftid");
            }
            foreach (var eachMarsh in AllMarshs) {
                var temp = BsonSerializer.Deserialize<BsonDocument>(JsonHelper.getJsonByObject(eachMarsh));
                SingleInstanceHelper.Instance.DBHelper.Save(SingleInstanceHelper.Instance.DBHelper.MarshRecordCoName, temp, "MarshNo");
            }

            return true;
        }

        //礼物价值
        private List<GiftValueModel> giftValueList;
        public List<GiftValueModel> GiftValueList {
            get {
                if (giftValueList == null) {
                    giftValueList = new List<GiftValueModel>();
                }
                return giftValueList;
            }
        }

        public bool Stop;
        public bool StopMarsh() {
            Stop = true;
            DumpToDB();
            return true;
        }
        public List<UserRecordModel> TopTenUserRecord {
            get {
                return GetTopTenUserRecord();
            }
        }
        //获取用户排行榜
        public List<UserRecordModel> GetTopTenUserRecord() {
            var result = new List<UserRecordModel>();
            var tempUserRecordList = SingleInstanceHelper.Instance.DBHelper.FindTopN(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName,10, "UserGetWeight");
            foreach (var eachUserRecord in tempUserRecordList) {
                var temp = new UserRecordModel();
                temp.FromBson(eachUserRecord);
                result.Add(temp);
            }
            return result;            
        }
        //接受到弹幕
        public void ReceiveDanmu(object sender, ReceivedDanmakuArgs e) {
            if (Stop) {
                return;
            }
            if (e.Danmaku != null && (e.Danmaku.MsgType == MsgTypeEnum.GiftSend || e.Danmaku.MsgType == MsgTypeEnum.GuardBuy)) {
                if (e.Danmaku.GiftCount > 0) {
                    //记录礼物价值
                    var obj = JObject.Parse(e.Danmaku.RawData);
                    GiftValueModel tempGiftValue = null;
                    if (GiftValueList.Count > 0) {
                         tempGiftValue = GiftValueList.Find(r => r.GiftName.Equals(e.Danmaku.GiftName));
                    }
                    if (tempGiftValue == null) {
                        tempGiftValue = new GiftValueModel()
                        {
                            GiftName = e.Danmaku.GiftName,
                            Giftid = obj["data"]["giftId"].ToObject<int>(),
                            GiftPrice = obj["data"]["price"].ToObject<double>(),
                        };
                        GiftValueList.Add(tempGiftValue);
                    }
                    //记录到数据库
                    var temp = BsonSerializer.Deserialize<BsonDocument>(e.Danmaku.RawData);
                    SingleInstanceHelper.Instance.DBHelper.Insert(SingleInstanceHelper.Instance.DBHelper.GiftRecordCoName, temp);
                    double Power = tempGiftValue.GiftPrice * e.Danmaku.GiftCount;

                    //增加用户的power数据到数据库顺便更新用户状态
                    var Collection = SingleInstanceHelper.Instance.DBHelper.DataBase.GetCollection<BsonDocument>(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName);
                    FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq(x => x["UserId"], e.Danmaku.UserID);
                    List<BsonDocument> result = Collection.Find(filter).ToList();
                    var tempUserRecord = new UserRecordModel();
                    int addFollow = obj["data"]["addFollow"].ToObject<int>();
                    if (result != null && result.Count > 0)
                    {
                        tempUserRecord.FromBson(result[0]);
                        tempUserRecord.UserTotalPower += Power;
                        if (addFollow == 0)
                        {
                            tempUserRecord.IsFan = false;
                        }
                        else {
                            tempUserRecord.IsFan = true;
                        }
                    }
                    else {
                        tempUserRecord.UserId = e.Danmaku.UserID;
                        tempUserRecord.UserName = e.Danmaku.UserName;
                        tempUserRecord.UserTotalPower = Power;
                        tempUserRecord.UserGetWeight = 0;
                        if (addFollow == 0)
                        {
                            tempUserRecord.IsFan = false;
                        }
                        else {
                            tempUserRecord.IsFan = true;
                        }
                    }
                    var TempBson = BsonSerializer.Deserialize<BsonDocument>(JsonHelper.getJsonByObject(tempUserRecord));
                    SingleInstanceHelper.Instance.DBHelper.Save(SingleInstanceHelper.Instance.DBHelper.UserRecordCoName, TempBson, "UserId");

                    CurrentMarsh.GetOneRunner(e.Danmaku.UserName, Power,e.Danmaku.UserID,addFollow);
                }
            }
        }
    }
}
