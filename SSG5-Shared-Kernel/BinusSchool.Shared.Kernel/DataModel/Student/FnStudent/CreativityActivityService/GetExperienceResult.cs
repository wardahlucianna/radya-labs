using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetExperienceResult
    {
        public string Id { get; set; }
        public string ExperienceName { get; set; }
        public string Student { get; set; }
        public string IdStudent { get; set; }
        public string ActivityDate { get; set; }
        public string Organizer { get; set; }
        public string Description { get; set; }
        public string Contribution { get; set; }
        public List<string> LearningOutcome { get; set; }
        public List<GetEvidance> Evidances { get; set; }
    }

    public class GetEvidance
    {
        public string Id { get; set; }
        public string Student { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Link { get; set; }
        public bool IsComment { get; set; }
        public List<string> Attachment { get; set; }
        public List<string> LearningOutcome { get; set; }
        public List<GetComment> Commens { get; set; }
    }

    public class GetComment
    {
        public string Id { get; set; }
        public string IdUserComment { get; set; }
        public string NameComment { get; set; }
        public string Date { get; set; }
        public string Comment { get; set; }
    }
}
