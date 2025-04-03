using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class UpdateScoreContinuationSettingRequest
    {
        public string IdAcademicYear { get; set; }

        public List<ScoreContinuationSetting> ScoreContinuationSettings { get; set; }
    }

    public class ScoreContinuationSetting
    {
        public MeritDemeritCategory Category { get; set; }
        public string IdGrade { get; set; }
        public int Score { get; set; }
        public ScoreContinueOption Option { get; set; }
        public ScoreContinueEvery Every { get; set; }
        public OperationOption Operation { get; set; }
    }
}
