using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration.Validator
{
    public class SaveStudentExtracurricularValidator : AbstractValidator<SaveStudentExtracurricularRequest>
    {
        public SaveStudentExtracurricularValidator()
        {
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdUserIn).NotEmpty();
            //RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.ExtracurricularList)
                .NotEmpty()
                .ForEach(data => data.ChildRules(child =>
                {
                    child.RuleFor(x => x.IdExtracurricular).NotEmpty();
                    child.RuleFor(x => x.IsChecked).NotNull();
                }));
        }
    }
}
