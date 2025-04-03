using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterSearching
{
    public class GetMasterSearchingDataTableResult
    {
        public List<string> Result_Head { set; get; }
        public List<GetMasterSearchingDataTableResult_BodyVm> Result_Body { set; get; }
    }

    public class GetMasterSearchingDataTableResult_BodyVm
    {
        public NameValueVm Student { set; get; }
        public List<string> FieldData { set; get; }
        public bool IsPhotoExist { set; get; }
        public string FilePathPhoto { set; get; }
    }

    public class GetMasterSearchingDataTableResult_Table
    {
        public string IdRegistrant { set; get; }
        public string IdStudent { set; get; }
        public string Name { set; get; }
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public Gender Gender { set; get; }
        public string NationalityName { set; get; }
        public string Religion { set; get; }
        public string ReligionSubject { set; get; }
        public string NISN { set; get; }
        public string NIK { set; get; }
        public string ResidenceAddress { set; get; }
        public string CityName { set; get; }
        public string ProvinceName { set; get; }
        public string CountryName { set; get; }
        public string PostalCode { set; get; }
        public string HouseNumber { set; get; }
        public DateTime? DOB { set; get; }
        public string POB { get; set; }
        public string MobilePhoneNumber1 { set; get; }
        public string BinusianEmailAddress { set; get; }
        public string PersonalEmailAddress { set; get; }
        public string ResidencePhoneNumber { set; get; }
    }

    public class GetMasterSearchingDataTableResult_Photo
    {
        public string IdBinusian { set; get; }
        public string Student { set; get; }
        public string StudentName { set; get; }
        public string AcademicYear { set; get; }
        public string IdPhoto { set; get; }
        public string FileName { set; get; }
        public string FilePath { set; get; }
    }
}
