using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class CommRespon
    {
        public int code { get; set; }
        public string message { get; set; }
        public Object data { get; set; }

        public static CommRespon Success(string message, object data = null)
        {
            return new CommRespon
            {
                code = 200,
                message = message,
                data = data
            };
        }

        public static CommRespon Failure(string message, object data = null)
        {
            return new CommRespon
            {
                code = 400,
                message = message,
                data = data
            };
        }

    }
}
