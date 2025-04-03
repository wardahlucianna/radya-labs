using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularUnattendance.Validator
{
    public class AddExtracurricularUnattendanceValidator : AbstractValidator<AddExtracurricularUnattendanceRequest>
    {
        public AddExtracurricularUnattendanceValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.UnattendanceStartDate).NotEmpty();
            RuleFor(x => x.UnattendanceEndDate).NotEmpty();
            //RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.UnattendanceExtracurricularList)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(x => x.IdExtracurricular).NotEmpty();
                }));
        }
    }
}
