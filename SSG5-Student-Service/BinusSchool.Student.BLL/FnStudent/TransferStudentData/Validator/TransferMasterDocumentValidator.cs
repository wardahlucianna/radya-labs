using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.TransferStudentData.Validator
{
    public class TransferMasterDocumentValidator  : AbstractValidator<TransferMasterDocumentRequest>
    {
        public TransferMasterDocumentValidator()
        {    
            RuleFor(x => x.IdDocument).NotEmpty();
            RuleFor(x => x.DocumentName).NotEmpty();
            RuleFor(x => x.IdDocumentType).NotEmpty();
        }
    }
}
