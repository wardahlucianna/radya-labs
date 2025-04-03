using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentForSalesForceResult
    {
        public string BinusianId { get; set; }
        public string StudentId { get; set; }
        public string Name { get; set; }
        public string Religion { get; set; }
        public string Gender { get; set; }
        public string Grade { get; set; }
        public string Level { get; set; }
        public string AcademicPeriod { get; set; }
        public string SchoolLocation { get; set; }
        public string StudentStatus { get; set; }
        public DateTime? JoinToSchoolDate { get; set; }
        public string NISN { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Nationality { get; set; }
        public string NationalIdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string AdmissionEmail { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpireDate { get; set; }
        public string DiseaseInformation { get; set; }
        public bool SpecialTreatment { get; set; }
        public string InstagramId { get; set; }
        public string LineId { get; set; }
        public List<GetStudentForSalesForceResult_ParentInformation> Parents { get; set; }
        public List<GetStudentForSalesForceResult_SiblingInformation> Sibings { get; set; }
    }

    public class GetStudentForSalesForceResult_ParentInformation
    {
        public string ParentType { get; set; }
        public string BinusianId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Nationality { get; set; }
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Status { get; set; }
        public string Education { get; set; }
        public string Address { get; set; }
    }

    public class GetStudentForSalesForceResult_SiblingInformation
    {
        public string BinusianId { get; set; }
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Status { get; set; }
        public string Grade { get; set; }
        public string SchoolLocation { get; set; }
    }
}
