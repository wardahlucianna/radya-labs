using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Achievement.Validator
{
    public class UpdateAchievementValidator : AbstractValidator<UpdateAchievementRequest>
    {
        public UpdateAchievementValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cannot empty");
            RuleFor(x => x.IdHomeroomStudent).NotEmpty().WithMessage("Id homeroom student cannot empty");
            RuleFor(x => x.AchievementName).NotEmpty().WithMessage("Achievement name empty");
            RuleFor(x => x.IdAchievementCategory).NotEmpty().WithMessage("Id achievement category empty");
            RuleFor(x => x.IdFocusArea).NotEmpty().WithMessage("Focus area cannot empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot cannot empty");
            RuleFor(x => x.IdUserTecaher).NotEmpty().WithMessage("User tecaher cannot empty");
        }
    }
}
