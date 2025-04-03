using BinusSchool.Common.Model.Information;
namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class AddStudentRequest
    {
        public NameInfoVm NameInfo { get; set; }
        public IdInfoVm IdInfo { get; set; }
        public BirthInfoVm BirthInfo { get; set; }
        public ReligionInfoVm ReligionInfo { get; set; }
        public AddressInfoVm AddressInfo { get; set; }
        public CardInfoVm CardInfo { get; set; }
        public ContactInfoVm ContactInfo { get; set; }
        public MedicalInfoVm MedicalInfo { get; set; }
        public OccupationInfoVm OccupationInfo { get; set; }
        public AdditionalInfoVm AdditionalInfo { get; set; }
        public SpecialTreatmentInfoVm SpecialTreatmentInfo { get; set; }
        public ChildInFamilyInfoVm ChildInFamilyInfo { get; set; }
    }
}
