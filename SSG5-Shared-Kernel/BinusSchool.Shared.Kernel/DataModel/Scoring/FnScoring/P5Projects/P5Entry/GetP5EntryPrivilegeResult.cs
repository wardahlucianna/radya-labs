using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5Entry
{
    public class GetP5EntryPrivilegeResult
    {
        public GetP5EntryPrivilegeResult() 
        {
            Grade = new List<P5EntryPrivilege_GradeVm>();
            Homeroom = new List<P5EntryPrivilege_HomeroomVm>();
        }
        public List<P5EntryPrivilege_GradeVm> Grade {  get; set; }
        public List<P5EntryPrivilege_HomeroomVm> Homeroom { get; set; }
    }

    public class P5EntryPrivilege_GradeVm
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }
        public string Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class P5EntryPrivilege_HomeroomVm
    {
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public int OrderNumber { get; set; }
        public string Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }

}
