using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnSubject.Department;
using FluentValidation;

namespace BinusSchool.School.FnSubject.Department.Validator
{
    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentRequest>
    {
        public UpdateDepartmentValidator()
        {
            RuleFor(x => x.Id).NotEmpty();

            When(x => (DepartmentType)x.LevelType == DepartmentType.Level, () =>
            {
                RuleForEach(x => x.IdLevel).NotNull();
            });
        }
    }
}
