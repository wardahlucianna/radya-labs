using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Employee.FnStaff.StaffJobInformation
{
    public class UpdateStaffJobInformationRequest
    {
        public string IdBinusian { get; set; }
        //public string IdBusinessUnit { get; set; }
        //public string BusinessUnitName { get; set; }
        //public string IdDepartment { get; set; }
        //public string DepartmentName { get; set; }
        //public string IdPosition { get; set; }
        //public string PositionName { get; set; }
        public string SubjectSpecialization { get; set; }
        public int TeacherDurationWeek { get; set; }
        public string NUPTK { get; set; }
        public string IdEmployeeStatus { get; set; }
        //public string IdEmployeeStatusDesc { get; set; }
        public string IdPTKType { get; set; }
        //public string IdPTKTypeDesc { get; set; }
        public string NoSrtPengangkatan { get; set; }
        public DateTime? TglSrtPengangkatan { get; set; }
        public string NoSrtKontrak { get; set; }
        public string NoIndukGuruKontrak { get; set; }
        public bool IsPrincipalLicensed { get; set; }
        public string IdLabSkillsLevel { get; set; }
        //public string IdLabSkillsLevelDesc { get; set; }
        public string IdExpSpecialTreatments { get; set; }
        //public string IdExpSpecialTreatmentsDesc { get; set; }
        public string IdBrailleExpLevel { get; set; }
        //public string IdBrailleExpLevelDesc { get; set; }
        public string IdIsyaratLevel { get; set; }
        //public string IdIsyaratLevelDesc { get; set; }
        public string AdditionalTaskNotes { get; set; }
        public DateTime? TglSrtKontrakKerja { get; set; }
    }
}
