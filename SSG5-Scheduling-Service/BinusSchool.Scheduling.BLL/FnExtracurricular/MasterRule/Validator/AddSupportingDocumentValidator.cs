using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterRule.Validator
{
    public class AddSupportingDocumentValidator : AbstractValidator<UpdateSupportingDocumentRequest>
    {
        public AddSupportingDocumentValidator()
        {            
            RuleFor(x => x.Name).NotEmpty();       
            RuleFor(x => x.FileName).NotEmpty();
            RuleFor(x => x.Grades).NotEmpty();
        }
    }
}
