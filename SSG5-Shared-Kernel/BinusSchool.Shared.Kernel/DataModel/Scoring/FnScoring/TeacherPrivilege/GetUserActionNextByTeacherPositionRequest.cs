using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetUserActionNextByTeacherPositionRequest
    {
        public string IdTeacherPosition { get; set; }
        public string IdTransactionScore { get; set; }
        public GetUserActionNextByTeacherPositionRequest_Score Score { get; set; }
        public GetUserActionNextByTeacherPositionRequest_ProgressStatus ProgressStatus { get; set; }
        public GetUserActionNextByTeacherPositionResult_ScoreData ScoreResult { get; set; }
    }

    public class GetUserActionNextByTeacherPositionRequest_Score
    {
        public string IdStudent { get; set; }
        public string IdScore { get; set; }
        public string IdSubComponentCounter { get; set; }
    }

    public class GetUserActionNextByTeacherPositionRequest_ProgressStatus
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string IdNationalStatus { get; set; }
        public string IdStudentStatus { get; set; }
    }
}
