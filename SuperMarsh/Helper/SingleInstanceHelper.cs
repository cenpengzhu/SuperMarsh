using BiliDMLib;
using SuperMarsh.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMarsh.Helper {
    public class SingleInstanceHelper {
        private static volatile SingleInstanceHelper instance;
        private static readonly object obj = new object();
        private SingleInstanceHelper() { }
        public static SingleInstanceHelper Instance {
            get {
                if (null == instance)
                {
                    lock (obj)
                    {
                        if (null == instance)
                        {
                            instance = new SingleInstanceHelper();
                        }
                    }

                }
                return instance;
            }
        }


        private DanmakuLoader danmuLoader;
        public DanmakuLoader DanmuLoader {
            get {
                if (danmuLoader == null)
                {
                    lock (obj) {
                        if (danmuLoader == null) {
                            danmuLoader = new DanmakuLoader();
                        }
                    }
                }
                return danmuLoader;
            }
        }

        private MarshRuler marshRuler;
        public MarshRuler MarshRuler {
            get {
                if (marshRuler == null) {
                    lock(obj) {
                        if (marshRuler == null) {
                            marshRuler = new MarshRuler();
                        }
                    }
                }
                return marshRuler;
            }
        }

        private MongoDBHelper dbHelper;
        public MongoDBHelper DBHelper {
            get {
                if (dbHelper == null)
                {
                    lock (obj)
                    {
                        if (dbHelper == null)
                        {
                            dbHelper = new MongoDBHelper();
                        }
                    }
                }
                return dbHelper;
            }
        }

    }
}
