using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Organization.Appliaction.ResponseModels.Evaluations
{
    public class EvaluationSimpleDto
    {
        public Guid Id { get; set; }

        public string ShortId { get; set; }
        public string Title { get; set; }

        public string Cover { get; set; }

        public byte Mode { get; set; }

        public bool Stick { get; set; }

        public Guid UserId { get; set; }

        public int Likes { get; set; }

        public byte Status { get; set; }

        public long No { get; set; }

    }
}
