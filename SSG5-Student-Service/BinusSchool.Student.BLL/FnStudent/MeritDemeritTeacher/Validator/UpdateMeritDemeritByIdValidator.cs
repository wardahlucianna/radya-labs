using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator
{
    public class UpdateMeritDemeritByIdValidator : AbstractValidator<UpdateMeritDemeritByIdRequest>
    {
        public UpdateMeritDemeritByIdValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot empty");
            RuleFor(x => x.IdMeritDemeritMapping).NotEmpty().WithMessage("Discipline name cannot empty");
        }
    }
}
