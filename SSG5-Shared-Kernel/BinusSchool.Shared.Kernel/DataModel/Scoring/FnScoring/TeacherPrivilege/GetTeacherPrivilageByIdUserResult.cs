using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetTeacherPrivilageByIdUserResult
    {
        public NameValueVm User { get; set; }
        public List<CodeWithIdVm> Roles { get; set; }
        public List<GetTeacherPrivilageByIdUserPosition_Result> Positions { get; set; }
    }

    public class GetTeacherPrivilageByIdUserPosition_Result : CodeWithIdVm
    {
        public string IdTeacherPosition { get; set; }
    }

}
