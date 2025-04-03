using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class MoveSubjectEnrollRepairDataByHomeroomRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
    }
}
