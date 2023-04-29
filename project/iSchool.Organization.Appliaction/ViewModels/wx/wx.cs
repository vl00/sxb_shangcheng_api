using iSchool.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ViewModels.wx
{
    public class AccessTokenApiResult
    {
        public AccessTokenApiData Data { get; set; }
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Msg { get; set; }
    }

    public class AccessTokenApiData
    {
        public string AppID { get; set; }
        public string AppName { get; set; }
        public string Token { get; set; }
    }

    public class WxQRCodeCreateRequest
    {
        public static string New(string scene_str, int expsec = 30 * 24 * 60 * 60)
        {
            return new 
            {
                expire_seconds = expsec,
                action_name = "QR_STR_SCENE",
                action_info = new 
                {
                    scene = new 
                    {
                        scene_str = scene_str
                    }
                },
            }.ToJsonString(true);
        }
    }

    public class WxQRCodeUrlResultResponse
    {
        public string Ticket { get; set; }
        public int Expire_seconds { get; set; }
        public string Url { get; set; }
    }
}
