﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetSubjectMappingResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public ItemValueVm Semester { get; set; }
        public ItemValueVm Term { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        //public CodeWithIdVm Pathway { get; set; }
        public CodeWithIdVm TargetCurriculum { get; set; }
        public CodeWithIdVm TargetSubjectType { get; set; }
        public string Status { get; set; }
        public bool IsActionable { get; set; }
    }
}
