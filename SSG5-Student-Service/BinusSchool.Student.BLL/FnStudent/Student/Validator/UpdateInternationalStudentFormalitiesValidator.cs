using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class UpdateInternationalStudentFormalitiesValidator : AbstractValidator<UpdateInternationalStudentFormalitiesRequest>
    {
        public UpdateInternationalStudentFormalitiesValidator()
        {
            /*RuleFor(x => x.KITASNumber).NotEmpty().MaximumLength(20).WithName("KITAS Number");
            RuleFor(x => x.KITASExpDate).NotEmpty().WithName("KITAS Exp Date");
            RuleFor(x => x.NSIBNumber).NotEmpty().MaximumLength(50).WithName("NSIB Number");
            RuleFor(x => x.NSIBExpDate).NotEmpty().WithName("NSIB Exp Date");*/
        }
    }
}
