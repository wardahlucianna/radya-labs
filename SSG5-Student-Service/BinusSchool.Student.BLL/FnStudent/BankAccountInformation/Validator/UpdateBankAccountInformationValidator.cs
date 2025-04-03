using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.Student.FnStudent.BankAccountInformation;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.BankAccountInformation.Validator
{
    public class UpdateBankAccountInformationValidator : AbstractValidator<UpdateBankAccountInformationRequest>
    {
        public UpdateBankAccountInformationValidator()
        {
            //RuleFor(x => x.AccountNumberCurrentValue).NotEmpty().WithName("Account Number Current Value");
            //RuleFor(x => x.AccountNameCurrentValue).NotEmpty().WithName("Account Name Current Value");
            //RuleFor(x => x.BankAccountNameCurrentValue).NotEmpty().WithName("Bank Account Name Current Value");
            //RuleFor(x => x.AccountNumberNewValue).NotEmpty().WithName("Account Number New Value");
            //RuleFor(x => x.AccountNameNewValue).NotEmpty().WithName("Account Name New Value");
            //RuleFor(x => x.BankAccountNameNewValue).NotEmpty().WithName("Bank Account Name New Value");
            /*RuleFor(x => x.RequestedDate).NotEmpty().WithName("Requested Date");
            RuleFor(x => x.ApprovalDate).NotEmpty().WithName("Approval Date");
            RuleFor(x => x.RejectDate).NotEmpty().WithName("Reject Date");
            RuleFor(x => x.Status).NotEmpty().WithName("Status");
            RuleFor(x => x.Notes).NotEmpty().WithName("Notes");*/
        }
    }
}
