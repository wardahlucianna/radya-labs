using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model; 
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListEvidencesResult : CodeWithIdVm
    {
        public string IdExperience { get; set; }
        public DateTime? DateIn { get; set; }
        public EvidencesType EvidencesType { get; set; }
        public string EvidencesText { get; set; }
        public string EvidencesLink { get; set; }
        public List<ListEvidencesAttachment> Attachments { get; set; }
        public List<CodeWithIdVm> LearningOutcomes { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class ListEvidencesAttachment 
    {
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }
    }
}
