using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.User;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPositionInfo
{
    public class GetDetailTeacherPositionByUserIDResult
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public List<DetailUserRole> Roles { get; set; }
        public NameValueVm School { get; set; }
        public List<DetailNonteacingLoadAcademic> NonTeachingAssignmentAcademic { get; set; }
        public List<DetailNonteacingLoadNonAcademic> NonTeachingAssignmentNonAcademic { get; set; }
    }

    public class DetailUserRole
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public NameValueVm RoleGroup { get; set; }
    }
    public class DetailNonteacingLoadAcademic
    {
        public string Data { get; set; }
        public string PositionName { get; set; }
        public string PositionShortName { get; set; }
    }

    public class DetailNonteacingLoadNonAcademic
    {
        public string Data { get; set; }
        public string PositionName { get; set; }
        public string PositionShortName { get; set; }
    }
}
