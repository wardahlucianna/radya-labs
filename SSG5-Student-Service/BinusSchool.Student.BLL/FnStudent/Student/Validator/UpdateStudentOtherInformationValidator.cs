using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class UpdateStudentOtherInformationValidator : AbstractValidator<UpdateStudentOtherInformationRequest>
    {
        public UpdateStudentOtherInformationValidator() 
        {
            /*RuleFor(x => x.FutureDream).NotEmpty().WithName("Future Dream");
            RuleFor(x => x.Hobby).NotEmpty().WithName("Hobby");*/
        }
    }
}
