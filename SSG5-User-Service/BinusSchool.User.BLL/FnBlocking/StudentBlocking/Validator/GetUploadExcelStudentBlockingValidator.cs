using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.StudentBlocking.Validator
{
    public class GetUploadExcelStudentBlockingValidator : AbstractValidator<UploadExcelContentDataStudentBlockingValidationRequest>
    {
        public GetUploadExcelStudentBlockingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

        }
    }
}
