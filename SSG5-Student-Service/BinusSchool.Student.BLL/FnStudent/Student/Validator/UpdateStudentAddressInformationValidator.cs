using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class UpdateStudentAddressInformationValidator : AbstractValidator<UpdateStudentAddressInformationRequest>
    {
        public UpdateStudentAddressInformationValidator() 
        {
            /*RuleFor(x => x.ResidenceAddress).NotEmpty().WithName("Residence Address");
            RuleFor(x => x.HouseNumber).NotEmpty().WithName("House Number");
            RuleFor(x => x.RT).NotEmpty().WithName("RT");
            RuleFor(x => x.RW).NotEmpty().WithName("RW");
            RuleFor(x => x.IdStayingWith).NotEmpty().WithName("ID Staying With");
            RuleFor(x => x.VillageDistrict).NotEmpty().WithName("Village District");
            RuleFor(x => x.SubDistrict).NotEmpty().WithName("Sub District");
            RuleFor(x => x.IdAddressCity).NotEmpty().WithName("ID Address City");
            RuleFor(x => x.IdAddressStateProvince).NotEmpty().WithName("ID Address State Province");
            RuleFor(x => x.IdAddressCountry).NotEmpty().WithName("ID Address Country");
            RuleFor(x => x.PostalCode).NotEmpty().WithName("Postal Code");
            RuleFor(x => x.DistanceHomeToSchool).NotEmpty().WithName("Distance Home to School");*/
        }
    }
}
