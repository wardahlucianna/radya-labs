using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTypeMapping
{
    public class GetListReportTypeMappingRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { set; get; }
        public string? IdLevel { set; get; }
        public string? IdGrade { set; get; }
        public string? IdReportType { set; get; }
    }
}
