using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class UpdateEvidencesRequest
    {
        public string Id { get; set; }
        public string IdExperience { get; set; }
        public EvidencesType EvidencesType { get; set; }
        public string EvidencesText { get; set; }
        public string EvidencesLink { get; set; }
        public List<UpdateEvidencesAttachment> Attachments { get; set; }
        public List<string> IdLearningOutcomes { get; set; }
    }
    public class UpdateEvidencesAttachment 
    {
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }
    }
}
