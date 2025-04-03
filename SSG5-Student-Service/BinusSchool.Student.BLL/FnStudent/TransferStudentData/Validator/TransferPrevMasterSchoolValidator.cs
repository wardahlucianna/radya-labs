using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.TransferStudentData.Validator
{
    public class TransferPrevMasterSchoolValidator : AbstractValidator<TransferPrevMasterSchoolRequest>
    {
        public TransferPrevMasterSchoolValidator()
        {      
            RuleFor(x => x.PrevMasterSchoolList)
                .NotEmpty()
                .ForEach(std => std.ChildRules(std =>
                {
                    std.RuleFor(x => x.IdPrevMasterSchool).NotEmpty();    
                    std.RuleFor(x => x.TypeLevel).NotEmpty();     
                    std.RuleFor(x => x.SchoolName).NotEmpty();               
                    std.RuleFor(x => x.Address).NotEmpty();               
                    std.RuleFor(x => x.Country).NotEmpty();               
                    std.RuleFor(x => x.Province).NotEmpty();               
                    std.RuleFor(x => x.Kota_kab).NotEmpty();   
                }));
        }
    }
}
