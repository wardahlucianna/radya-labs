using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur
{
    public class CreateSignatureNatCurResult
    {
        public string HtmlOutput { get; set; }
    }

    public class CreateSignatureNatCurResult_OtherPositions
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyList<OtherPositionByIdUserResult> Data { get; set; }
    }
}
