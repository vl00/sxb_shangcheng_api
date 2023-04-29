using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.Mongodb.Entities
{
    /// <summary>
    /// mongodb 日志统计表
    /// </summary>
    [Serializable]
    public class Statistics
    {
        public ObjectId _id { get; set; }
        public string UserId { get; set; }
        public string Deviceid { get; set; }
        public string Url { get; set; }

        public int Adcode { get; set; }
        public string FW { get; set; }
        public string Appid { get; set; }
        public string Version { get; set; }
        public string Module { get; set; }
        public long Ip { get; set; }
        public int Platform { get; set; }
        public int System { get; set; }
        public int Client { get; set; }
        public string Sessionid { get; set; }
        public string UA { get; set; }
        public string Method { get; set; }
        public BsonDocument Params { get; set; }
        public string Event { get; set; }
        public string Referer { get; set; }
        public BsonDateTime Time { get; set; }
    }




}
