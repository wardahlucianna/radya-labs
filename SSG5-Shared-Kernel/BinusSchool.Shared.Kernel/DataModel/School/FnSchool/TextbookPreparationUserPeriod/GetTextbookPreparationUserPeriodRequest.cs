using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod
{
    public class GetTextbookPreparationUserPeriodRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string AssignAs { get; set; }
    }
}
