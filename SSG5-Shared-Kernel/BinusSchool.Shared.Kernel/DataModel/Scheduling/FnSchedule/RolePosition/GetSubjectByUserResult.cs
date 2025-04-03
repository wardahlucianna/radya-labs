using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition
{
    public class GetSubjectByUserResult
    {
        public GetSubjectLesson Lesson { get; set; }
        public GetSubjectUser Subject {  get; set; }
        public ItemValueVmWithOrderNumber Level {  get; set; }
        public ItemValueVmWithOrderNumber Grade {  get; set; }
        public GetSubjectHomeroom Homeroom {  get; set; }
    }

    public class GetSubjectUser : CodeWithIdVm
    {
        public string IdDepartment { get; set; }
    }
    public class GetSubjectLesson 
    {
        public string Id { get; set; }
        public string ClassId { get; set; }
    }
    public class GetSubjectHomeroom : ItemValueVm
    {
        public int Semester { get; set; }
        public string IdGradePathwayClassRoom { get; set; }
        public string IdGradePathway { get; set; }
    }

    public class ItemValueVmWithOrderNumber : CodeWithIdVm
    {
        public int OrderNumber { get; set; }
    }
}
