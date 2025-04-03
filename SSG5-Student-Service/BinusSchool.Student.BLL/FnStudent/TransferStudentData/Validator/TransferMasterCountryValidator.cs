using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.TransferStudentData.Validator
{
    public class TransferMasterCountryValidator : AbstractValidator<TransferMasterCountryRequest>
    {
        public TransferMasterCountryValidator()
        {               
            RuleFor(x => x.CountryList)
                .NotEmpty()
                .ForEach(std => std.ChildRules(std =>
                {
                    std.RuleFor(x => x.IdCountry).NotEmpty();    
                    std.RuleFor(x => x.CountryName).NotEmpty();   
                }));         
        }
    }
}
