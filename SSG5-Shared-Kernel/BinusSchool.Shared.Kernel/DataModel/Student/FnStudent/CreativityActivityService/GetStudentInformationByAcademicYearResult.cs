using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetStudentInformationByAcademicYearResult
    {
        public List<ItemValueVm> AcademicYear { get; set; }
        public List<ItemValueVm> Level { get; set; }
        public List<ItemValueVm> Grade { get; set; }
        public List<int> Semester { get; set; }
        public List<ItemValueVm> Homeroom { get; set; }
        public string StudentName { get; set; }
        public string IdBinusian { get; set; }
        public StatusOverallExperienceStudent StatusOverall { get; set; }
    }
}
