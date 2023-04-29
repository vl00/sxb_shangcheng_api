using iSchool.Organization.Domain;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable    

    /// <summary>
    /// 用户是否新用户（相对于course type来说）
    /// </summary>
    public class UserIsCourseTypeNewBuyerQryResult
    { 
        public bool IsNewBuyer { get; set; }
    }

#nullable disable
}
