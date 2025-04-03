using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Information;
using BinusSchool.Data.Model.Student.FnStudent.SiblingGroup;
using BinusSchool.Data.Model.Student.FnStudent.ParentRole;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentResult : ItemValueVm
    {
        
        public NameInfoVm nameInfo { get; set; }
        public PersonalStudentInfoVm personalInfoVm { get; set; }
        public IdInfoVm idInfo { get; set; }
        public BirthInfoVm birthInfo { get; set; }
        public ReligionInfoVm religionInfo { get; set; }
        public AddressInfoVm addressInfo { get; set; }
        public BankAndVAInfoVm BankAndVAInfo { get; set; }
        public PreviousSchoolInfoVm PreviousSchoolInfo { get; set; }
        public CardInfoVm cardInfo { get; set; }
        public ContactInfoVm contactInfo { get; set; }
        public MedicalInfoVm medicalInfo { get; set; }
        public OccupationInfoVm occupationInfo { get; set; }
        public AdditionalInfoVm additionalInfo { get; set; }
        public SpecialTreatmentInfoVm specialTreatmentInfo { get; set; }
        public string IdStudentEncrypted { get; set; }
        public string IdStudentEncryptedRjindael { get; set; }

    }
}
