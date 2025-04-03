using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.HandbookManagement;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.HandbookManagement.Validator
{
    public class AddHandbookManagementValidator : AbstractValidator<AddHandbookManagementRequest>
    {
        public AddHandbookManagementValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.ViewFors).NotEmpty();

            RuleFor(x => x.Title).NotEmpty();

            When(x => !string.IsNullOrEmpty(x.Url), () => {
                RuleFor(x => x.Url).Must(StringUtil.IsValidUrl).WithMessage("Invalid url");
            });
        }
    }
}
