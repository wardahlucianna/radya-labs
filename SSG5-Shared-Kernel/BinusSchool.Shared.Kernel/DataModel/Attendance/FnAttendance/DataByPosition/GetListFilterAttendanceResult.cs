using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition
{
    public class GetListFilterAttendanceResult
    {
        public List<GetListFilterAttendanceResult_Level> Level { get; set; }
        public List<GetListFilterAttendanceResult_Grade> Grade { get; set; }
        public List<GetListFilterAttendanceResult_Semester> Semester { get; set; }
        public List<GetListFilterAttendanceResult_Term> Term { get; set; }
    }

    public class GetListFilterAttendanceResult_Level : CodeWithIdVm
    {
        public int OrderNumber { get; set; }
    }

    public class GetListFilterAttendanceResult_Grade : CodeWithIdVm
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }
    }

    public class GetListFilterAttendanceResult_Semester : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public int OrderNumber { get; set; }
    }

    public class GetListFilterAttendanceResult_Term : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public int OrderNumber { get; set; }
    }
}
