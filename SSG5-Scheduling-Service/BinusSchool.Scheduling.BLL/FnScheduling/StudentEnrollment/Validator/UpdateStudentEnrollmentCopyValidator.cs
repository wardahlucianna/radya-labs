using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment.Validator
{
    public class UpdateStudentEnrollmentCopyValidator : AbstractValidator<UpdateStudentEnrollmentCopyRequest>
    {
        public UpdateStudentEnrollmentCopyValidator(IServiceProvider provider)
        {
            var localizer = provider.GetService<IStringLocalizer>();
            
            RuleFor(x => x.IdAcadyearCopyTo).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.SemesterCopyTo).NotEmpty().WithMessage("Semester cannot empty");
            RuleFor(x => x.IdHomeroom).NotEmpty();
            RuleFor(x => x.IdStudents).NotEmpty().WithMessage("Id Lesson cannot empty");
        }
    }
}
