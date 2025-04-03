using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class CreateSignatureReportResult
    {
        public string HtmlOutput { get; set; }
    }

    public class CreateSignatureReportResult_OtherPositions
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyList<OtherPositionByIdUserResult> Data { get; set; }
    }

    public class CreateSignatureReportResult_GetBinusianSignature
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public bool Crypted { get; set; }
    }
}
