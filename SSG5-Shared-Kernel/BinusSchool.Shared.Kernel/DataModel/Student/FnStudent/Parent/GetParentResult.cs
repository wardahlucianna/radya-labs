using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Information;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class GetParentResult : ItemValueVm
    {
        public NameInfoVm nameInfo { get; set; }
        public PersonalParentInfoVm personalInfoVm { get; set; }
        public MedicalInfoVm medicalInfo { get; set; }
        public IdInfoVm idInfo { get; set; }
        public BirthInfoVm birthInfo { get; set; }
        public ReligionInfoVm religionInfo { get; set; }
        public AddressInfoVm addressInfo { get; set; }
        public CardInfoVm cardInfo { get; set; }
        public ContactInfoVm contactInfo { get; set; }
        public OccupationInfoVm occupationInfo { get; set; }
        public AuditableResult Audit { get; set; }
    }
}
