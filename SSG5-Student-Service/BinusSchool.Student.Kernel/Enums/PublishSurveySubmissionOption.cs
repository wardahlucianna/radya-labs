using System.ComponentModel;

namespace BinusSchool.Student.Kernel.Enums
{
    public enum PublishSurveySubmissionOption
    {
        [Description("Submit Review Per Child")]
        SubmitReviewPerChild,
        [Description("Submit Review Per Family")]
        SubmitReviewPerFamily,
        [Description("Submit 1 Review Per Child Or 1 Review Per Family")]
        Submit1ReviewPerChildOr1ReviewPerFamily,
    }
}
