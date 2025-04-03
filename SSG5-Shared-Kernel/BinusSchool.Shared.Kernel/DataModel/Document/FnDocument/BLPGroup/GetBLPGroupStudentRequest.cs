using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.BLPGroup
{
    public class GetBLPGroupStudentRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public BLPFinalStatus? BLPFinalStatus { get; set; }
    }
}
