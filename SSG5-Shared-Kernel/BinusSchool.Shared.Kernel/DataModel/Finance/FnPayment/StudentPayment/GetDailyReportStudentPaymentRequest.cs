using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Employee.FnStaff.Teacher;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetDailyReportStudentPaymentRequest
    {
        public string IdSchool { get; set; }
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }   
    }
}
