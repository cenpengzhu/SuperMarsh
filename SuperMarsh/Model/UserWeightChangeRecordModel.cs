using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Model {
    public class UserWeightChangeRecordModel {
        public UserWeightChangeRecordModel() { }
        public void FromBson(BsonDocument Content)
        {
            UserName = Content["UserName"].ToString();
            UserId = Content["UserId"].ToInt64();
            ChangeValue = Content["ChangeValue"].ToDouble();
            ChangeType = Content["ChangeType"].ToString();
            ChangeMessage = Content["ChangeMessage"].ToString();
            TimeStamp = Content["TimeStamp"].ToInt64();
        }

        public String UserName;
        public long UserId;
        public double ChangeValue;
        public String ChangeType;
        public String ChangeMessage;
        public long TimeStamp;
    }
}
