using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetTeacherCommentResult
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Comment { get; set; }
        public int MaxCommentLength { get; set; }
        public string Score { get; set; }
        public int Semester { get; set; }
        public string IdSubject { get; set; }
    }
}