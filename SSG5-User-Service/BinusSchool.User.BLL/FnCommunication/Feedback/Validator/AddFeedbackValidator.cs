using BinusSchool.Data.Model.User.FnCommunication.Feedback;
using FluentValidation;

namespace BinusSchool.User.FnCommunication.Feedback.Validator
{
    public class AddFeedbackValidator : AbstractValidator<AddFeedbackRequest>
    {
        public AddFeedbackValidator()
        {
            RuleFor(x => x.FeedbackType).NotEmpty();
            //RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
