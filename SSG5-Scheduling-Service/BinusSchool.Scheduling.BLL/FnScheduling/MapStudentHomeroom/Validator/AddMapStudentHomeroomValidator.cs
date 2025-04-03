using BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.MapStudentHomeroom.Validator
{
    public class AddMapStudentHomeroomValidator : AbstractValidator<AddMapStudentHomeroomRequest>
    {
        public AddMapStudentHomeroomValidator()
        {
            RuleFor(x => x.IdHomeroom).NotEmpty();

            RuleFor(x => x.Students)
                .NotEmpty()
                .ForEach(students => students.ChildRules(student => 
                {
                    student.RuleFor(x => x.IdStudent).NotEmpty();

                    student.RuleFor(x => x.Gender).IsInEnum();

                    student.RuleFor(x => x.Religion).NotEmpty();
                }));
        }
    }
}
