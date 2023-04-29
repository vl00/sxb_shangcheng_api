using iSchool.Domain.Repository.Interfaces.Organization;
using iSchool.Organization.Domain.Modles;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Infrastructure.Mongodb.Queries
{
    public class StatisticsQueries : IStatisticsQueries
    {
        private readonly Context _context;

        public StatisticsQueries(Context context)
        {
            _context = context;
        }

        /// <summary>
        ///统计微信渠道过来的pv与uv
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>

        public List<PVUV4Wechat> GetPvUvForWebChat(DateTime startTime, DateTime endTime)
        {
            var pipeline = new[]{new BsonDocument(){
                {"$match", new BsonDocument(){
                    {"$and", new BsonArray(){new BsonDocument(){
                        {"time", new BsonDocument(){
                            {"$gte", startTime},
                            {"$lte",endTime}
                        }}
                    },new BsonDocument(){
                        {"event", "onload"}
                    },new BsonDocument(){
                        {"params.eid", new BsonDocument(){
                            {"$ne", BsonNull.Value}
                        }}
                    },new BsonDocument(){
                        {"url", new BsonRegularExpression("^\\/pagesa\\/pages\\/course_detail\\/course_detail")}
                    }}}
                }}
            },new BsonDocument(){
                {"$project", new BsonDocument(){
                    {"url", 1},
                    {"userid", new BsonDocument(){
                        {"$cond", new BsonDocument(){
                            {"if", new BsonDocument(){
                                {"$or", new BsonArray(){new BsonDocument(){
                                    {"userid", BsonNull.Value}
                                },new BsonDocument(){
                                    {"userid", ""}
                                }}}
                            }},
                            {"then", "$deviceid"},
                            {"else", "$userid"}
                        }}
                    }},
                    {"event", 1},
                    {"fw", 1},
                    {"params", 1},
                    {"day", new BsonDocument(){
                        {"$substr", new BsonArray(){"$time",0,10}}
                    }}
                }}
            },new BsonDocument(){
                {"$group", new BsonDocument(){
                    {"_id", new BsonDocument(){
                        {"courseid", "$params.id"},
                        {"eid", "$params.eid"},
                        {"userid", "$userid"},
                        {"day", "$day"},
                        {"surl", "$params.surl"}
                    }},
                    {"count", new BsonDocument(){
                        {"$sum", 1}
                    }}
                }}
            },new BsonDocument(){
                {"$group", new BsonDocument(){
                    {"_id", new BsonDocument(){
                        {"courseid", "$_id.courseid"},
                        {"eid", "$_id.eid"},
                        {"day", "$_id.day"},
                        {"surl", "$_id.surl"}
                    }},
                    {"pv", new BsonDocument(){
                        {"$sum", "$count"}
                    }},
                    {"uv", new BsonDocument(){
                        {"$sum", 1}
                    }}
                }}
            }};
            var res = _context.Statistics.Aggregate<PVUV4Wechat>(pipeline).ToList();
            return res;
        }
    }

}
