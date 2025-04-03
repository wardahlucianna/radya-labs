using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator
{
    public class DeleteUngenerateScheduleStudentValidator : AbstractValidator<DeleteUngenerateScheduleStudentRequest>
    {
        public DeleteUngenerateScheduleStudentValidator()
        {
            RuleFor(x => x.IdAscTimetable).NotEmpty();

            RuleFor(x => x.Start)
                .NotEmpty()
                .LessThanOrEqualTo(x => x.End);
        
            RuleFor(x => x.End)
                .NotEmpty();
                // .GreaterThanOrEqualTo(x => x.Start);

            RuleFor(x => x.IdGrade).NotEmpty();

            RuleFor(x => x.Students)
                .NotEmpty()
                .ForEach(students => students.ChildRules(student =>
                {
                    student.RuleFor(x => x.IdStudent).NotEmpty();

                    student.RuleFor(x => x.ClassIds)
                        .NotEmpty()
                        .WithMessage("Can't be ungenerate because class id is empty, please map it")
                        .ForEach(classIds => classIds.ChildRules(classId =>
                            classId.RuleFor(x => x)
                                   .NotEmpty()
                                   .WithMessage("Can't be ungenerate because class id is empty, please map it")
                        ));
                }));
        }
    }
}
