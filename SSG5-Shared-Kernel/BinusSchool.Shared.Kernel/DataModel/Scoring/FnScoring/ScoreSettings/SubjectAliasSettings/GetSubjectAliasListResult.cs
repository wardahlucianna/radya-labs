using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings
{
    public class GetSubjectAliasListResult : CodeWithIdVm
    {
        public GetSubjectAliasListResult_Subject Subject { set; get; }
        public GetSubjectAliasListResult_SubjectAlias SubjectAlias { set; get; }
        public GetSubjectAliasListResult_Validation Validation { set; get; }
    }

    public class GetSubjectAliasListResult_Subject : ItemValueVm
    {
        public string SubjectID { set; get; }
    }

    public class GetSubjectAliasListResult_SubjectAlias
    {
        public string IdSubjectAlias { set; get; }
        public string SubjectAlias { set; get; }
        public string NationalReportAlias { set; get; }
    }

    public class GetSubjectAliasListResult_Validation
    {
        public bool CanUpdateAlias { set; get; }
        public bool CanUpdateNationalReportAlias { set; get; }
    }
}
