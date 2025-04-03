using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Util.FnConverter.ServiceAsActionToPdf;
using FluentValidation;

namespace BinusSchool.Util.FnConverter.ServiceAsActionToPdf.Validator
{
    public class ConvertServiceAsActionToPdfValidator : AbstractValidator<ConvertServiceAsActionToPdfRequest>
    {
        public ConvertServiceAsActionToPdfValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
        }
    }
}
