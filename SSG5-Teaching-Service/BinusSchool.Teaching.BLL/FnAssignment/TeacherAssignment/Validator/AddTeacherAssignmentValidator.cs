using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.Validator
{
    public class AddTeacherAssignmentValidator : AbstractValidator<AddTeacherAssignmentRequest>
    {
        public AddTeacherAssignmentValidator()
        {
            //RuleFor(x => x.NonteacingLoadAcademic)
            //   .NotEmpty()
            //   .ForEach(d => d.ChildRules(data =>
            //   {
            //       data.RuleFor(x => x.IdSchoolUser)
            //           .NotEmpty();

            //       data.RuleFor(x => x.IdSchoolNonTeachingLoad)
            //           .NotEmpty();

            //       data.RuleFor(x => x.Data)
            //           .NotEmpty();
            //   }));
        }
    }
}
