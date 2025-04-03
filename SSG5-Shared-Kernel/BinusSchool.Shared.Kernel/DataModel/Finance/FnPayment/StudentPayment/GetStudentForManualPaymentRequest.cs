using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetStudentForManualPaymentRequest : CollectionRequest
    {
        public string IdEventPayment { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdSchoolLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdReligion { get; set; }

    }
}
