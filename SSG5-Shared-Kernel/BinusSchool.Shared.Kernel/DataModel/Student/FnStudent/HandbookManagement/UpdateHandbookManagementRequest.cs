using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.HandbookManagement
{
    public class UpdateHandbookManagementRequest
    {
        public string IdHandbookManagement { get; set; }
        public List<ViewForHandbookManagementRequest> ViewFors { get; set; }
        public List<LevelHandbookManagement> IdsLevel { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public List<AttachmentHandbookManagement> Attachments { get; set; }
    }
}
