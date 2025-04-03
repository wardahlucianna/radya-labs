using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ServiceAsAction.Validator
{
    public class SaveServiceAsActionEvidenceValidator : AbstractValidator<SaveServiceAsActionEvidenceRequest>
    {
        public SaveServiceAsActionEvidenceValidator()
        {
            RuleFor(x => x.IdServiceAsActionForm).NotEmpty();
            RuleFor(x => x.EvidenceType).NotEmpty();
            RuleFor(x => x.IdLoMappings).NotEmpty();
        }
    }
}
