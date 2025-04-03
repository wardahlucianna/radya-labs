using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.TransferStudentData.Validator
{
    public class TransferOccupationTypeValidator : AbstractValidator<TransferOccupationTypeRequest>
    {
        public TransferOccupationTypeValidator()
        {      
              RuleFor(x => x.OccupationTypeList)
                .NotEmpty()
                .ForEach(std => std.ChildRules(std =>
                {
                    std.RuleFor(x => x.IdOccupationType).NotEmpty();    
                    std.RuleFor(x => x.OccupationTypeName).NotEmpty();               
                    std.RuleFor(x => x.OccupationTypeNameEng).NotEmpty();               
              
                }));
        }   
    }
}
