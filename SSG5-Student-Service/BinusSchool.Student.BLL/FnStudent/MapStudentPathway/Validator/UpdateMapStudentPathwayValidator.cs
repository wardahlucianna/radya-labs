using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.MapStudentPathway.Validator
{
    public class UpdateMapStudentPathwayValidator : AbstractValidator<UpdateMapStudentPathwayRequest>
    {
        public UpdateMapStudentPathwayValidator()
        {
            RuleFor(x => x.IdGrade).NotEmpty();

            RuleFor(x => x.Mappeds)
                .NotEmpty()
                .ForEach(maps => maps.ChildRules(map =>
                {
                    map.RuleFor(x => x.IdStudent).NotEmpty();

                    map.RuleFor(x => x.IdPathway).NotEmpty();
                }));
        }
    }
}