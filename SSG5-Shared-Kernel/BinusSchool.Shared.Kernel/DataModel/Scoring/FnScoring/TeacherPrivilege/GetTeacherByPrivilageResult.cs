using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetTeacherByPrivilageResult
    {
        public CodeWithIdVm Position { get; set; }
        public List<CALHTeacherList> TeacherList { get; set; }
    }
    public class CALHTeacherList
    {
        public NameValueVm Teacher { get; set; }
        public string BinusianEmailAddress { get; set; }
    }

    public class DetailCALHNonteacingLoad
    {
        public NameValueVm Teacher { get; set; }
        public string BinusianEmailAddress { get; set; }
        public CodeWithIdVm Position { get; set; }
        public string Data { get; set; }
        public ClassAdvisorData ClassAdvisorData { get; set; }
        public LevelHeadData LevelHeadData { get; set; }
    }

}
