using iSchool.Infrastructure.Dapper;
using iSchool.Organization.Appliaction.ResponseModels;
using MediatR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.RequestModels
{
    /// <summary>
    /// 
    /// </summary>
    public class SmLogUserOperation : INotification
    {
        public SmLogUserOperation()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; }
        public DateTime? Time { get; private set; }
        public Guid UserId { get; private set; }
        public string Desc { get; private set; }
        public object Params { get; private set; }
        public IDictionary<string, object> Oldata { get; private set; }
        public string Class { get; private set; }
        public string Method { get; private set; }

        public SmLogUserOperation SetTime(DateTime? time)
        {
            this.Time = time;
            return this;
        }

        public SmLogUserOperation SetUserId(Guid userId)
        {
            this.UserId = userId;
            return this;
        }

        public SmLogUserOperation SetDesc(string desc)
        {
            this.Desc = desc;
            return this;
        }

        public SmLogUserOperation SetClass(string c)
        {
            this.Class = c;
            return this;
        }

        public SmLogUserOperation SetMethod(string m)
        {
            this.Method = m;
            return this;
        }

        public SmLogUserOperation SetParams(string name, object parms)
        {
            if (!(this.Params is IDictionary<string, object> p)) this.Params = p = new Dictionary<string, object>();
            p[name] = parms;
            return this;
        }
        public SmLogUserOperation DelParams(string name)
        {
            if (this.Params is IDictionary<string, object> p)
                p.Remove(name);
            return this;
        }
        public SmLogUserOperation DelAllParams()
        {
            this.Params = null;
            return this;
        }

        /// <summary>
        /// 用于记录(表)旧数据
        /// </summary>
        /// <param name="name">一般为表名</param>
        /// <param name="parms">一条旧数据(一般为表一行)</param>
        /// <returns></returns>
        public SmLogUserOperation SetOldata(string name, object parms)
        {
            this.Oldata ??= new Dictionary<string, object>();
            this.Oldata[name] = parms;
            return this;
        }
        /// <summary>
        /// 用于记录(表)旧数据
        /// </summary>
        /// <param name="name">一般为表名</param>
        /// <param name="parms">多条旧数据(一般为表的行)</param>
        /// <returns></returns>
        public SmLogUserOperation SetOldata(string name, IEnumerable parms)
        {
            this.Oldata ??= new Dictionary<string, object>();
            this.Oldata[name] = parms;
            return this;
        }
        public SmLogUserOperation DelOldata(string name)
        {
            this.Oldata?.Remove(name);
            return this;
        }
        public SmLogUserOperation DelAllOldata()
        {
            this.Oldata = null;
            return this;
        }

    }
}
