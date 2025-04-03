using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Achievement.Validator
{
    public class AddAchievementValidator : AbstractValidator<AddAchievementRequest>
    {
        public AddAchievementValidator()
        {
            RuleFor(x => x.IdHomeroomStudent).NotEmpty().WithMessage("Id homeroom student cannot empty");
            RuleFor(x => x.AchievementName).NotEmpty().WithMessage("Achievement name empty");
            RuleFor(x => x.IdAchievementCategory).NotEmpty().WithMessage("Id achievement category empty");
            RuleFor(x => x.IdFocusArea).NotEmpty().WithMessage("Focus area cannot empty");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date cannot cannot empty");
            RuleFor(x => x.IdUserTecaher).NotEmpty().WithMessage("User tecaher cannot empty");
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id user cannot empty");
        }
    }
}
