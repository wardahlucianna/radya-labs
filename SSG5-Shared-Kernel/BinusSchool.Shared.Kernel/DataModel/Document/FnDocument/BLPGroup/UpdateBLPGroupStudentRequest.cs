using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPGroup
{
    public class UpdateBLPGroupStudentRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdBLPGroupStudent { get; set; }
        public string IdStudent { get; set; }
        public string IdBLPStatus { get; set; }
        public string IdBLPGroup { get; set; }
        public DateTime? HardcopySubmissionDate { get; set; }
        public string IdHomeroom { get; set; }
    }
}
