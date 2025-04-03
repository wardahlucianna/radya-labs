using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention
{
    public class GetStudentUnderAttentionStudentStatusSpecialResponse
    {
        public ItemValueVm SpecialStudentStatus { get; set; }
        public string Remarks { get; set; }
    }
}
