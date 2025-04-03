using BinusSchool.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention
{
    public class GetStudentUnderAttentionRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
