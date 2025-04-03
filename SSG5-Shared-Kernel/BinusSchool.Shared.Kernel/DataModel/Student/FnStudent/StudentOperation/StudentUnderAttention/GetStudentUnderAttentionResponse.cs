using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention
{
    public class GetStudentUnderAttentionResponse
    {
        public string IdTrStudentStatus { get; set; }
        public ItemValueVm School { get; set; }
        public ItemValueVm Student { get; set; }
        public ItemValueVm LastHomeroom { get; set; }
        public ItemValueVm LastAcademicYear { get; set; }
        public string Status { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime LastActiveDate { get; set; }
        public string Remarks { get; set; }
        public bool CanEdit { get; set; }
    }
}
