using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentStatus.SendEmail
{
    public class SendEmailStudentStatusSpecialNeedFutureAdmissionRequest
    {
        public string IdSchool { get; set; }
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public int IdStudentStatusSpecial { get; set; }
    }
}
