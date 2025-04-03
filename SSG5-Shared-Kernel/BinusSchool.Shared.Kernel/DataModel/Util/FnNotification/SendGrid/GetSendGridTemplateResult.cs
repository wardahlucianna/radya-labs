using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Util.FnNotification.SendGrid
{
    public class GetSendGridTemplateResult
    {
        public Metadata _Metadata { get; set; }
        public IEnumerable<Result> Result { get; set; }
    }
    
    public class Version
    {
        public string Id { get; set; }
        public string TemplateId { get; set; }
        public int Active { get; set; }
        public string Name { get; set; }
        public bool GeneratePlainContent { get; set; }
        public string Subject { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Editor { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class Result
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Generation { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IEnumerable<Version> Versions { get; set; }
    }

    public class Metadata
    {
        public string Prev { get; set; }
        public string Self { get; set; }
        public string Next { get; set; }
        public int Count { get; set; }
    }
}
