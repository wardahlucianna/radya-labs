using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval
{
    public class GetTextbookPreparationApprovalRequest: CollectionSchoolRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string Status { get; set; }
    }
}
