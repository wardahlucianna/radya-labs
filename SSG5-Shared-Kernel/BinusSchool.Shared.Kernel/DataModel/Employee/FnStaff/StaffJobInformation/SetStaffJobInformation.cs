using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Employee.FnStaff.StaffJobInformation
{
    public class SetStaffJobInformation
    {
        public string SubjectSpecialization { get; set; }
        public int TeacherDurationWeek { get; set; }
        public string NUPTK { get; set; }
        public ItemValueVm IdEmployeeStatus { get; set; }
        public ItemValueVm IdPTKType { get; set; }
        public string NoSrtPengangkatan { get; set; }
        public DateTime? TglSrtPengangkatan { get; set; }
        public string NoSrtKontrak { get; set; }
        //public DateTime? TglSrtKontrak { get; set; }
        public string NoIndukGuruKontrak { get; set; }
        public bool IsPrincipalLicensed { get; set; }
        public ItemValueVm IdExpSpecialTreatments { get; set; }
        public ItemValueVm IdLabSkillsLevel { get; set; }
        public ItemValueVm IdIsyaratLevel { get; set; }
        public ItemValueVm IdBrailleExpLevel { get; set; }
        public string AdditionalTaskNotes { get; set; }

    }
}
