using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.TransferStudentData.Validator
{
    public class TransferMasterNationalityValidator: AbstractValidator<TransferMasterNationalityRequest>
    {
        public TransferMasterNationalityValidator()
        {        
            RuleFor(x => x.NationalityList)
                .NotEmpty()
                .ForEach(std => std.ChildRules(std =>
                {
                    std.RuleFor(x => x.IdNationality).NotEmpty();    
                    std.RuleFor(x => x.NationalityName).NotEmpty();   
                }));    
        }
    }
}
