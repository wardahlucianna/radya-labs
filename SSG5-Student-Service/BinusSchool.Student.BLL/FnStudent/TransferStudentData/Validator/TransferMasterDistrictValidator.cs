using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.TransferStudentData.Validator
{
    public class TransferMasterDistrictValidator : AbstractValidator<TransferMasterDistrictRequest>
    {
        public TransferMasterDistrictValidator()
        {    

            RuleFor(x => x.DistrictList)
                .NotEmpty()
                .ForEach(std => std.ChildRules(std =>
                {
                    std.RuleFor(x => x.IdDistrict).NotEmpty();    
                    std.RuleFor(x => x.DistrictName).NotEmpty();   
                }));    
       
        }
    }
}
