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
            NewRunnerMessages = new List<string>();
            NewRunnerMessages.Add("挑战者表示不服，毅然踏进沼泽，是个恶人没错了！");
            NewRunnerMessages.Add("快来人啊，挑战者掉进沼泽里了！(假装呼救，其实希望挑战者淹死)");
            NewRunnerMessages.Add("挑战者踩着前人的头爬上来了，看来挑战者非常懂得沼泽逃生的技巧！");
            NewRunnerMessages.Add("哦，我的天呐，这黏糊糊的感觉如何？也许你们该采访一下挑战者！");
            NewRunnerMessages.Add("我希望挑战者在踏进沼泽之前吃了顿好的，因为...他可能再也吃不到了，哈哈哈哈！");
            NewRunnerMessages.Add("挑战者，你爸妈没有告诉过你要远离沼泽吗？");
            NewRunnerMessages.Add("生还是死？对于挑战者来说这恐怕不是一个选择，因为他已经死了。");
            NewRunnerMessages.Add("现在挑战者要给大家表演沼泽炖自己！快送个辣条支持一下8！");
            NewRunnerMessages.Add("哦，挑战者你落沼的姿势可不怎么样。");
            NewRunnerMessages.Add("还有谁？难道就这样眼睁睁地看着挑战者窃取你们的果实吗？");
            NewRunnerMessages.Add("挑战者，你往里挤一哈，后面还有人排队呢！");
            BoomMessage = "哦天呐！沼泽里的沼气爆炸了，那些黏糊糊的东西搞得挑战者浑身都是！";
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

        private static readonly object MarshLock = new object();
        private String Name = "沼泽管理员";
        private static readonly object WriteFileLock = new object();


        //当前竞猜
        public BetModel CurrentBet;


        //沼气爆炸消息
        public String BoomMessage;
        //沼泽消息
        public List<String> NewRunnerMessages;
        //正在进行的沼泽
        public MarshModel CurrentMarsh {
            get {
                lock (MarshLock) {
                    if (AllMarshs.Count == 0)
                    {
                        LoadFromDB();
                    }
                    MarshModel temp = AllMarshs.Find(x => x.MarshNo == AllMarshs.Max(y => y.MarshNo));
                    if (temp.IsWin())
                    {
                        temp.WinMarsh();
                        String WinnerMessage = "恭喜" + temp.Winner + "成功逃离沼泽！赢得了" + String.Format("{0:F}", temp.WinnerWeight) + "千克尸体！";
                        WriteToFile(WinnerMessage);
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
                        tempUserRecord.UserName = e.Danmaku.UserName;
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

                    if (Stop)
                    {
                        return;
                    }

                    //记录消息
                    Random rd = new Random();
                    int rdResult = rd.Next(1, 100);
                    if (rdResult < 50) {
                        String NewRunnerMessage = NewRunnerMessages[rdResult % 11].Replace("挑战者", e.Danmaku.UserName);
                        WriteToFile(NewRunnerMessage);
                    }
                    //沼泽处理
                    if (CurrentMarsh.GetOneRunner(e.Danmaku.UserName, Power, e.Danmaku.UserID, addFollow) == 2) {
                        String RealBoomMessage = BoomMessage.Replace("挑战者", e.Danmaku.UserName);
                        WriteToFile(RealBoomMessage);
                    }
                    //竞猜处理
                    if (CurrentBet != null) {
                        if (CurrentBet.Status == BetStatus.betting) {
                            CurrentBet.Bet(e.Danmaku.UserName, e.Danmaku.UserID, e.Danmaku.GiftCount, tempGiftValue.GiftPrice);
                        }
                    }
                }
            }
        }

        public void WriteToFile(String content) {
            lock(WriteFileLock) {
                FileStream fs = new FileStream("F:\\MarshMessage.txt", FileMode.Create);
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
