using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SuperMarsh.Helper {
    public class MongoDBHelper {
        public MongoDBHelper() {
        }

        public String DBName = "TestLiveDB";

        private MongoClient client;
        public MongoClient Client {
            get {
                if (client == null) {
                    ConnectDB();
                }
                return client;
            }
        }

        private IMongoDatabase dataBase;
        public IMongoDatabase DataBase {
            get {
                if (dataBase == null) {
                    ConnectDB();
                }
                return dataBase;
            }
        }
        public String GiftRecordCoName = "giftrecord";
        public String GiftValueCoName = "giftvalue";
        public String MarshRecordCoName = "marshrecord";
        public String UserRecordCoName = "userrecord";

        private void ConnectDB() {
            String url = "mongodb://127.0.0.1:27017";
            var mongoUrl = new MongoUrl(url);
            var settings = MongoClientSettings.FromUrl(mongoUrl);
            client = new MongoClient();
            dataBase = Client.GetDatabase(DBName);
        }

        public bool Insert(String CollectionName,BsonDocument Content) {
            var Collection = DataBase.GetCollection<BsonDocument>(CollectionName);
            Collection.InsertOne(Content);
            return true;
        }

        public bool InsertMany(String CollectionName,List<BsonDocument> Contents) {
            var Collection = DataBase.GetCollection<BsonDocument>(CollectionName);
            Collection.InsertMany(Contents);
            return true;
        }

        //当数据存在时更新不存在时插入
        public bool Save(String CollectionName, BsonDocument Content,String Key) {
            var updateOptions = new UpdateOptions { IsUpsert = true };
            var Collection = DataBase.GetCollection<BsonDocument>(CollectionName);

            var filter = Builders<BsonDocument>.Filter.Eq(f => f[Key], Content[Key]);
            var update = Builders<BsonDocument>.Update.Combine(BuildUpdateDefinition(Content, null));
            Collection.UpdateOne(filter, update, updateOptions);
            return true;
        }

        public List<BsonDocument> FindAll(String CollectionName) {
            var Collection = DataBase.GetCollection<BsonDocument>(CollectionName);
            return Collection.Find(new BsonDocument()).ToList();
        }

        public List<BsonDocument> FindTopN(String CollectionName,int TopN,String Index)
        {
            var Collection = DataBase.GetCollection<BsonDocument>(CollectionName);
            return Collection.Find(new BsonDocument()).SortByDescending(x => x[Index]).Limit(TopN).ToList();
        }


        private List<UpdateDefinition<BsonDocument>> BuildUpdateDefinition(BsonDocument bc, string parent)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();
            foreach (var element in bc.Elements)
            {
                var key = parent == null ? element.Name : $"{parent}.{element.Name}";
                var subUpdates = new List<UpdateDefinition<BsonDocument>>();
                //子元素是对象              
                if (element.Value.IsBsonDocument)
                {
                    updates.AddRange(BuildUpdateDefinition(element.Value.ToBsonDocument(), key));
                }
                //子元素是对象数组               
                else if (element.Value.IsBsonArray)
                {
                    var arrayDocs = element.Value.AsBsonArray;
                    var i = 0;
                    foreach (var doc in arrayDocs)
                    {
                        if (doc.IsBsonDocument)
                        {
                            updates.AddRange(BuildUpdateDefinition(doc.ToBsonDocument(), key + $".{i}"));
                        }
                        else
                        {
                            updates.Add(Builders<BsonDocument>.Update.Set(f => f[key], element.Value));
                            continue;
                        }
                        i++;
                    }
                }
                //子元素是其他               
                else
                {
                    updates.Add(Builders<BsonDocument>.Update.Set(f => f[key], element.Value));
                }
            }
            return updates;
        }
              
    }
}
