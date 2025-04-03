using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator
{
    public class AddMasterExtracurricularValidator : AbstractValidator<AddMasterExtracurricularRequest>
    {
        public AddMasterExtracurricularValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotNull();
            RuleFor(x => x.ExtracurricularName).NotEmpty();
            RuleFor(x => x.IdExtracurricularGroup).NotEmpty();
            RuleFor(x => x.GradeList).NotEmpty();

            RuleFor(x => x.IsShowAttendanceReportCard).NotNull();
            RuleFor(x => x.IsShowScoreReportCard).NotNull();
            RuleFor(x => x.IsRegularSchedule).NotNull();
            //RuleFor(x => x.ElectivesStartDate).NotEmpty();
            //RuleFor(x => x.ElectivesEndDate).NotEmpty();
            RuleFor(x => x.AttendanceStartDate).NotEmpty();
            RuleFor(x => x.AttendanceEndDate).NotEmpty();
            //RuleFor(x => x.ExtracurricularCategory).NotEmpty();
            RuleFor(x => x.ParticipantMin).NotNull();
            RuleFor(x => x.ParticipantMax).NotNull();

            RuleFor(x => x.Price).NotNull();
            RuleFor(x => x.NeedObjective).NotNull();

            When(x => x.IsShowScoreReportCard, () =>
            {
                RuleFor(x => x.IdExtracurricularScoreCompCategory).NotNull();
                RuleFor(x => x.IdExtracurricularScoreLegendCategory).NotNull();
            });

            RuleFor(x => x.IdExtracurricularType).NotEmpty();
            //RuleFor(x => x.MasterExtracurricularData)
            //    .NotEmpty()
            //    .ForEach(data => data.ChildRules(data =>
            //    {
            //        data.RuleFor(x => x.IdExtracurricular).NotEmpty();
            //        data.RuleFor(x => x.IsTransferParticipant).NotEmpty();
            //    }));
        }
    }
}
