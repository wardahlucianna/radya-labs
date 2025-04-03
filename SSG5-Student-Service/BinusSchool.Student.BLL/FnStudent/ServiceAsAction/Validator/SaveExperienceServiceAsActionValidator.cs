using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Validator
{
    public class SaveExperienceServiceAsActionValidator : AbstractValidator<SaveExperienceServiceAsActionRequest>
    {
        public SaveExperienceServiceAsActionValidator()
        {
            RuleFor(x => x.IdStudent).NotNull();
            RuleFor(x => x.ExperienceDetail).NotNull();
            RuleFor(x => x.ExperienceDetail.IdAcademicYear).NotEmpty();
            RuleFor(x => x.ExperienceDetail.IdServiceAsActionLocation).NotEmpty();
            RuleFor(x => x.ExperienceDetail.ExperienceName).NotEmpty();
            RuleFor(x => x.ExperienceDetail.IdServiceAsActionTypes).NotEmpty();

            RuleFor(x => x.OrganizationDetail).NotNull();
            RuleFor(x => x.OrganizationDetail.Organization).NotEmpty();
            RuleFor(x => x.OrganizationDetail.ContributionTMC).NotEmpty();
            RuleFor(x => x.OrganizationDetail.ActivityDescription).NotEmpty();

            RuleFor(x => x.IdLearningOutcomes).NotEmpty();
        }
    }
}
