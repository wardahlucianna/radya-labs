using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterRule.Validator
{
    public class UpdateSupportingDocumentValidator : AbstractValidator<UpdateSupportingDocumentRequest>
    {
        public UpdateSupportingDocumentValidator()
        {
            When(x => x.ActionUpdateStatus == true, () =>
            {
                RuleFor(x => x.IdExtracurricularSupportDoc).NotEmpty();
            });

            When(x => x.ActionUpdateStatus == false, () =>
            {
                RuleFor(x => x.IdExtracurricularSupportDoc).NotEmpty();
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.FileName).NotEmpty();
                RuleFor(x => x.Grades).NotEmpty();
            });

        
        }
    }
}
