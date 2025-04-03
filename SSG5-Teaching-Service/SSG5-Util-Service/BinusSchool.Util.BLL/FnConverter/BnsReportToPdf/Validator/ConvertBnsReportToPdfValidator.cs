using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Util.FnConverter.BnsReportToPdf;
using FluentValidation;

namespace BinusSchool.Util.FnConverter.BnsReportToPdf.Validator
{
    public class ConvertBnsReportToPdfValidator : AbstractValidator<ConvertBnsReportToPdfRequest>
    {
        public ConvertBnsReportToPdfValidator()
        {
            //RuleFor(x => x.IdReportTemplate).NotEmpty();
            RuleFor(x => x.TemplateName).NotEmpty();

            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdAcadyear).NotEmpty();
            RuleFor(x => x.IdLevel).NotEmpty();
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.IdPeriod).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
        }
    }
}
