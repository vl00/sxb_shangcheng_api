#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
    public class AddEvaluationToSpecialsCommandResult
    {
        /// <summary>表示绑定专题是否成功</summary>
        public bool Succeed { get; set; }
        /// <summary>(新)活动数据</summary>
        public EvaluationAddedResult_NewActivity? Activity { get; set; }
    }
}

#nullable disable