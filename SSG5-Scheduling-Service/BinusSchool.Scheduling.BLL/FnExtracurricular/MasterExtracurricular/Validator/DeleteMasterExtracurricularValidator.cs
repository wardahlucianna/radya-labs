using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator
{
    public class DeleteMasterExtracurricularValidator : AbstractValidator<DeleteMasterExtracurricularRequest>
    {
        public DeleteMasterExtracurricularValidator()
        {
            RuleFor(x => x.IdExtracurricular).NotEmpty();
        }
    }
}
