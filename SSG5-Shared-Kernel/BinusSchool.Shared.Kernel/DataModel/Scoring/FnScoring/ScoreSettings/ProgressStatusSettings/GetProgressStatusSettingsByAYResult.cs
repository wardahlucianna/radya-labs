using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings
{
    public class GetProgressStatusSettingsByAYResult
    {
        public CodeWithIdVm Grade { set; get; }
        public int OrderNumberGrade { set; get; }
        public GetProgressStatusSettingsByAYResult_StudentStatus StudentStatus { set; get; }
        public string StudentProgressText { set; get; }
        public string StudentProgressBahasaText { set; get; }
        public bool EnableAgreement { set; get; }
        public bool EnableHideReportCard { set; get; }
    }

    public class GetProgressStatusSettingsByAYResult_StudentStatus
    {
        public int IdStudentStatus { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
    }
}
