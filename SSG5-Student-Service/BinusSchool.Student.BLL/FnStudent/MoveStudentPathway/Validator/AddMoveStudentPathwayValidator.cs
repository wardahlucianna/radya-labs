using BinusSchool.Data.Model.Student.FnStudent.MoveStudentPathway;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MoveStudentPahway.Validator
{
    public class AddMoveStudentPathwayValidator : AbstractValidator<AddMoveStudentPathwayRequest>
    {
        public AddMoveStudentPathwayValidator()
        {
            RuleFor(x => x.IdPathway).NotEmpty();

            RuleFor(x => x.IdStudents)
                .NotEmpty()
                .ForEach(ids => ids.ChildRules(id =>
                {
                    id.RuleFor(x => x).NotEmpty();
                }));
        }
    }
}