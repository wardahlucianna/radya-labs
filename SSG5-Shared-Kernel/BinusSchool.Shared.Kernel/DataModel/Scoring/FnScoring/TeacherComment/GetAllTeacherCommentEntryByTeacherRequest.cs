using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.TeacherComment
{
    public class GetAllTeacherCommentEntryByTeacherRequest
    {
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
