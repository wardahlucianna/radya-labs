using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Employee.FnStaff.StaffEducationInformation
{
    public class GetStaffEducationInformationResult
    {
        public string IdStaffEducation { get; set; }
        public string IdBinusian { get; set; }
        public ItemValueVm IdEducationLevel { get; set; }
        public string InstitutionName { get; set; }
        public string MajorName { get; set; }
        public string AttendingYear { get; set; }
        public string GraduateYear { get; set; }
        public decimal GPA { get; set; }
    }
}
