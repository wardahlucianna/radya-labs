using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class SaveCommentEvidencesRequest
    {
        public string IdCommentEvidences { get; set; }
        public string IdEvidences { get; set; }
        public string IdUserComment { get; set; }
        public string IdAcademicYear { get; set; }
        public string Comment { get; set; }
    }
}
