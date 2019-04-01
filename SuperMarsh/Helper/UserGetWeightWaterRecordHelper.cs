using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SuperMarsh.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Helper {
    public class UserGetWeightWaterRecordHelper {
        public static bool WaterRecord(String username, long userid, double value, String type, String message)
        {
            if (value <= 0) {
                return false;
            }
            UserWeightChangeRecordModel WaterRecord = new UserWeightChangeRecordModel();
            WaterRecord.UserName = username;
            WaterRecord.UserId = userid;
            WaterRecord.ChangeValue = value;
            WaterRecord.ChangeType = type;
            WaterRecord.ChangeMessage = message;
            WaterRecord.TimeStamp = TimeHelper.CurrentUnixTime();
            var TempBson = BsonSerializer.Deserialize<BsonDocument>(JsonHelper.getJsonByObject(WaterRecord));
            SingleInstanceHelper.Instance.DBHelper.Insert(SingleInstanceHelper.Instance.DBHelper.UserWeightChangeRecordCoName, TempBson);
            return true;
        }
    }
}
