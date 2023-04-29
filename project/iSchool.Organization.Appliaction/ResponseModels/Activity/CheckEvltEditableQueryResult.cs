using iSchool.Organization.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels
{
#nullable enable    

    public class CheckEvltEditableQueryResult
    {
        public bool Enable { get; set; }
        public TimeSpan? DisableTtl { get; set; }
        public Guid EvltId { get; set; }
        public ActivityEvaluationBind? Aeb { get; set; }
        public ActivityRule? Rule { get; set; }
    }

#nullable disable
}
