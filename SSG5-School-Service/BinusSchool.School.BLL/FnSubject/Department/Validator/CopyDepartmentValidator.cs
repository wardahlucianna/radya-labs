using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSubject.Department;
using FluentValidation;

namespace BinusSchool.School.FnSubject.Department.Validator
{
    public class CopyDepartmentValidator : AbstractValidator<CopyDepartmentRequest>
    {
        public CopyDepartmentValidator()
        {
            RuleFor(x => x.IdAcadyearFrom).NotEmpty();
            RuleFor(x => x.IdAcadyearTo).NotEmpty();
        }
    }
}
