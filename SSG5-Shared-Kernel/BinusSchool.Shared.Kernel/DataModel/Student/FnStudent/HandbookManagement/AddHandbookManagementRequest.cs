using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.HandbookManagement
{
    public class AddHandbookManagementRequest
    {
        public string IdAcademicYear { get; set; }
        public List<ViewForHandbookManagementRequest> ViewFors { get; set; }
        public List<LevelHandbookManagement> IdsLevel { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public List<AttachmentHandbookManagement> Attachments { get; set; }
    }

    public class ViewForHandbookManagementRequest
    {
        public HandbookFor ViewFor { get; set; }
    }

    public class LevelHandbookManagement 
    {
        public string Id { get; set; }
    }

    public class AttachmentHandbookManagement
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
    }
}
