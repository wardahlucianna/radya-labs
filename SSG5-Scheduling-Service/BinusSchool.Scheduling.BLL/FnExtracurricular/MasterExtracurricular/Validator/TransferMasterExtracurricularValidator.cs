using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator
{
    public class TransferMasterExtracurricularValidator : AbstractValidator<TransferMasterExtracurricularRequest>
    {
        public TransferMasterExtracurricularValidator()
        {
            RuleFor(x => x.IdAcademicYearFrom).NotEmpty();
            RuleFor(x => x.IdAcademicYearDest).NotEmpty();
            RuleFor(x => x.SemesterFrom).NotNull();
            RuleFor(x => x.SemesterDest).NotNull();
            RuleFor(x => x.MasterExtracurricularData)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(x => x.IdExtracurricular).NotEmpty();
                    data.RuleFor(x => x.IsTransferParticipant).NotNull();
                }));
        }
    }
}
