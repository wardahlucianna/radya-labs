﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetSectionSubjectTakenByStudentRequest
    {
        public string IdScoreSummaryTabSection { get; set; }
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { set; get; }
        public int Semester { set; get; }
        public string IdScoreSummaryTab { set; get; }
    }
}
