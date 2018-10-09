using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Model {
    public class GiftValueModel {
        public GiftValueModel() { }

        public void FromBson(BsonDocument Content) {
            GiftName = Content["GiftName"].ToString();
            Giftid = Content["Giftid"].ToInt32();
            GiftPrice = Content["GiftPrice"].ToDouble();
        }
        public String GiftName;
        public int Giftid;
        public double GiftPrice;
    }
}
