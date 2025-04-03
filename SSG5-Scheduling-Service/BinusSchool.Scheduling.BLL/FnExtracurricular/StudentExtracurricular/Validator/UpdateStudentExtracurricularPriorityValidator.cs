using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular.Validator
{
    public class UpdateStudentExtracurricularPriorityValidator : AbstractValidator<UpdateStudentExtracurricularPriorityRequest>
    {
        public UpdateStudentExtracurricularPriorityValidator()
        {
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.ExtracurricularPrimaryList)
                   .NotEmpty()
                   .ForEach(data => data.ChildRules(data =>
                   {
                       data.RuleFor(x => x.IdExtracurricular).NotEmpty();
                       data.RuleFor(x => x.IsPrimary).NotNull();
                   }));
        }
    }
}
