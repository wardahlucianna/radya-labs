using System.Collections;
using System.Collections.Generic;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom.Validator
{
    public class HomeroomTeacherValidator : AbstractValidator<IEnumerable<HomeroomTeacher>>
    {
        public HomeroomTeacherValidator()
        {
            RuleFor(x => x)
                .NotEmpty()
                .ForEach(teachers => teachers.ChildRules(teacher => 
                {
                    teacher.RuleFor(x => x.IdTeacher).NotEmpty();

                    teacher.RuleFor(x => x.IdPosition).NotEmpty();
                }));
        }
    }
}