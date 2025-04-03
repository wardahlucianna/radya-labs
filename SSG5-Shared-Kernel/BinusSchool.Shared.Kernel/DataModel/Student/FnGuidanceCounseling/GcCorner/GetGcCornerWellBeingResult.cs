using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerWellBeingResult : ItemValueVm
    {
        public string ArticleName { get; set; }

        public string Link { get; set; }

        public List<AttachmentGcCornerPersonalWellBeing> Attachments { get; set; }
    }

    public class AttachmentGcCornerPersonalWellBeing
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
