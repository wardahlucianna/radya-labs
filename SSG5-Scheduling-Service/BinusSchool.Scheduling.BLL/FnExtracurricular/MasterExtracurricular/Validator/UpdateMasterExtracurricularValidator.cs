using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator
{
    public class UpdateMasterExtracurricularValidator : AbstractValidator<UpdateMasterExtracurricularRequest>
    {
        public UpdateMasterExtracurricularValidator()
        {
            When(x => x.UpdateAll, () =>
            {
                RuleFor(x => x.IdExtracurricular).NotEmpty();
                RuleFor(x => x.ExtracurricularName).NotEmpty();
                RuleFor(x => x.IdExtracurricularGroup).NotEmpty();
                RuleFor(x => x.IdExtracurricularType).NotEmpty();

                //RuleFor(x => x.IsShowAttendanceReportCard).NotNull();
                //RuleFor(x => x.IsShowScoreReportCard).NotNull();
                //RuleFor(x => x.IsRegularSchedule).NotNull();
                RuleFor(x => x.ElectivesStartDate).NotEmpty();
                RuleFor(x => x.ElectivesEndDate).NotEmpty();
                RuleFor(x => x.AttendanceStartDate).NotEmpty();
                RuleFor(x => x.AttendanceEndDate).NotEmpty();
                //RuleFor(x => x.ExtracurricularCategory).NotEmpty();
                RuleFor(x => x.ParticipantMin).NotNull();
                RuleFor(x => x.ParticipantMax).NotNull();

                //RuleFor(x => x.Price).NotNull();
                //RuleFor(x => x.NeedObjective).NotNull();
                //RuleFor(x => x.Status).NotNull();

                When(x => x.IsShowScoreReportCard, () =>
                {
                    RuleFor(x => x.IdExtracurricularScoreCompCategory).NotNull();
                    RuleFor(x => x.IdExtracurricularScoreLegendCategory).NotNull();
                });
            });

            When(x => x.UpdateAll == false, () =>
            {
                RuleFor(x => x.IdExtracurricular).NotEmpty();
                RuleFor(x => x.Status).NotNull();
            });
        }
    }
}
