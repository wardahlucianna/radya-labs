using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Model.Information
{
    public class MedicalInfoVm
    {
        public Gender Gender { get; set;} 
        public ItemValueVm IdBloodType { get; set;}
        public int Height { get; set;}
        public int Weight { get; set;}
    }
}
