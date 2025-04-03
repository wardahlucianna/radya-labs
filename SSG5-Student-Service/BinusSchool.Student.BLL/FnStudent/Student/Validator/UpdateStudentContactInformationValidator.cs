using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class UpdateStudentContactInformationValidator : AbstractValidator<UpdateStudentContactInformationRequest>
    {
        public UpdateStudentContactInformationValidator() 
        {
            //Include(new AddParentRequest());
            //Student
            /*RuleFor(x => x.ResidencePhoneNumber).NotEmpty().WithName("Residence Phone Number");
            RuleFor(x => x.MobilePhoneNumber1).NotEmpty().WithName("Mobile Phone Number 1");
            RuleFor(x => x.MobilePhoneNumber2).NotEmpty().WithName("Mobile Phone Number 2");
            //RuleFor(x => x.contactInfo.MobilePhoneNumber3).NotEmpty().WithName("Mobile Phone Number 3");
            RuleFor(x => x.BinusianEmailAddress).NotEmpty().WithName("Binusian Email Address");
            RuleFor(x => x.PersonalEmailAddress).NotEmpty().WithName("Personal Email Address");
            //Primary / Emergency Contact
            RuleFor(x => x.EmergencyContactRole).NotEmpty().WithName("Emergency Contact Role");
            */
        }
    }
}
