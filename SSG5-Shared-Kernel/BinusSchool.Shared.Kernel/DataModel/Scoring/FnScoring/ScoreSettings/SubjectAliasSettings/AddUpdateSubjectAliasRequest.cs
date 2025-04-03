using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings
{
    public class AddUpdateSubjectAliasRequest
    {
        public string IdGrade { get; set; }
        public List<AddUpdateSubjectAliasRequest_Subject> Subjects { get; set; }
    }

    public class AddUpdateSubjectAliasRequest_Subject
    {
        public string IdSubject { get; set; }
        public string IdSubjectAlias { get; set; }
        public string SubjectAlias { get; set; }
        public string NationalReportAlias { get; set; }
    }
}
