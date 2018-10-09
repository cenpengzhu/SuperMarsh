using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Model {
    public class UserRecordModel {
        public UserRecordModel() { }
        public void FromBson(BsonDocument Content) {
            UserName = Content["UserName"].ToString();
            UserId = Content["UserId"].ToInt64();
            UserGetWeight = Content["UserGetWeight"].ToDouble();
            UserTotalPower = Content["UserTotalPower"].ToDouble();
        }

        public override string ToString()
        {
            string str1 = String.Format("{0:F}", UserGetWeight);
            return UserName + "   " + str1.ToString()+"千克"; 
        }

        public string UserName;
        public long UserId;
        public double UserGetWeight;
        public double UserTotalPower;
        public bool IsFan;
     
    }
}
