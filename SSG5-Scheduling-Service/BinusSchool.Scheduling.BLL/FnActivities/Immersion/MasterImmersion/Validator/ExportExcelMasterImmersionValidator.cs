using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using FluentValidation;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion.Validator
{
    public class ExportExcelMasterImmersionValidator : AbstractValidator<ExportExcelMasterImmersionRequest>
    {
        public ExportExcelMasterImmersionValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
        }
    }
}
