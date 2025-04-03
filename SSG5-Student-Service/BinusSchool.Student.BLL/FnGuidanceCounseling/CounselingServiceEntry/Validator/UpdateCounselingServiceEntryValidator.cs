using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry.Validator
{
    public class UpdateCounselingServiceEntryValidator : AbstractValidator<UpdateCounselingServiceEntryRequest>
    {
        public UpdateCounselingServiceEntryValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.IdCounselor).NotEmpty();

            RuleFor(x => x.IdStudent).NotEmpty();

            RuleFor(x => x.IdCounselingCategory).NotEmpty();

            RuleFor(x => x.Date).NotEmpty();

            RuleFor(x => x.Time).NotEmpty();
        }
    }
}
