using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model.Information;
using static BinusSchool.Data.Model.Employee.FnStaff.Teacher.StaffInformationResponseDTO;

namespace BinusSchool.Data.Model.Employee.FnStaff.Teacher
{
    public class GetTeacherDetailResult
    {
        public PersonalInfoDetailVM PersonalInfo { get; set; }
        //public List<FamilyMember> FamilyInfo { get; set; }
        public List<FamilyInfoDetailVm> FamilyInfo { get; set; }
        public ExpatriateInfoDetailVm ExpatriateInfo { get; set; }
        public AddressInfoDetailVm AddressInfo { get; set; }
        public ContactInfoDetailVm ContactInfo { get; set; }
        public JobInfoDetailVm JobInfo { get; set; }
    }
    public class PersonalInfoDetailVM
    {
        public string Photo { get; set; }
        public ItemValueVm IdDesignation { get; set; }
        public string IdEmployee { get; set; }
        public string IdBinusian { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string POB { get; set; }
        public DateTime DOB { get; set; }
        public Gender GenderName { get; set; }
        public string MotherMaidenName { get; set; } // temporarily unused
        public ItemValueVm IdNationality { get; set; }
        public ItemValueVm IdNationalityCountry { get; set; }
        public ItemValueVm IdReligion { get; set; }
        public string NIK { get; set; } //temporarily unused
        public string PassportNumber { get; set; }
        public DateTime? PassportExpDate { get; set; }
        public string MaritalStatus { get; set; }
        

    }
    public class FamilyInfoDetailVm
    {
        public string FamilyName { get; set; }
        public string RelationshipStatus { get; set; }
        //public string OccupationName { get; set; }
        //public DateTime DOB { get; set; }
        //public string MaritalStatus { get; set; }

        /*public string binusian_ID { get; set; }
        public string name { get; set; }
        public DateTime tgl_Lahir { get; set; }
        public string relation { get; set; }*/
    }
    public class ExpatriateInfoDetailVm
    {
        public string KITASNumber { get; set; }
        public string KITASSponsor { get; set; }
        public ItemValueVm IdKITASStatus { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public string IMTANumber { get; set; }
        public ItemValueVm IdIMTASchoolLevel { get; set; }
        public ItemValueVm IdIMTAMajorAssignPosition { get; set; }
        public DateTime? IMTAExpDate { get; set; }
    }
    public class AddressInfoDetailVm
    {
        public string ResisdenceAddress { get; set; }
        public string AddressCity { get; set; }
        public string PostalCode { get; set; }
    }
    public class ContactInfoDetailVm
    {
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
    }
    public class JobInfoDetailVm
    {
        public string IdBusinessUnit { get; set; }
        public string BusinessUnitName { get; set; }
        public string IdDepartment { get; set; }
        public string DepartmentName { get; set; }
        public string IdPosition { get; set; }
        public string PositionName { get; set; }
        public string SubjectSpecialization { get; set; }
        public int TeacherDurationWeek { get; set; }
        public string NUPTK { get; set; }
        public ItemValueVm IdEmployeeStatus { get; set; }
        public ItemValueVm IdPTKType { get; set; }
        public string NoSrtPengangkatan { get; set; }
        public DateTime? TglSrtPengangkatan { get; set; }
        public string NoSrtKontrak { get; set; }
        public DateTime? TglSrtKontrakKerja { get; set; }
        public string NoIndukGuruKontrak { get; set; }
        public bool IsPrincipalLicensed { get; set; }
        public ItemValueVm IdExpSpecialTreatments { get; set; }
        public ItemValueVm IdLabSkillsLevel { get; set; }
        public ItemValueVm IdIsyaratLevel { get; set; }
        public ItemValueVm IdBrailleExpLevel { get; set; }
        public string AdditionalTaskNotes { get; set; }
        public string IdBusinessUnitGroup { get; set; }
        public string BusinessUnitGroupName { get; set; }
        public string Division { get; set; }
    }
}
