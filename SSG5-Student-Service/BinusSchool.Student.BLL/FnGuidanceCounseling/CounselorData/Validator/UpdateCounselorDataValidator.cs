using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselorData.Validator
{
    public class UpdateCounselorDataValidator : AbstractValidator<UpdateCounselorDataRequest>
    {
        public UpdateCounselorDataValidator()
        {
            Include(new AddCounselorDataValidator());

            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.IdRole).NotEmpty();
            RuleFor(x => x.IdPosition).NotEmpty();
        }
    }
}
