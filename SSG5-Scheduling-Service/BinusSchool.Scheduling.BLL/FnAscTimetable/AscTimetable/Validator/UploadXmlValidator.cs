
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using FluentValidation;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator
{
    public class UploadXmlValidator : AbstractValidator<AscTimeTableUploadXmlRequest>
    {
        public UploadXmlValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            
            RuleFor(x => x.IdSchoolAcademicyears).NotEmpty();

            RuleFor(x => x.Name).NotEmpty();

            //RuleFor(x => x.IdGradepathwayforCreateSession)
            //    .NotEmpty()
            //    .ForEach(grades => grades.ChildRules(grade => grade.RuleFor(x => x).NotEmpty()));

            When(x => x.AutomaticGenerateClassId, () =>
            {
                RuleFor(x => x.CodeGradeForAutomaticGenerateClassId)
               .NotEmpty()
               .WithMessage("Please select the class.")
               .ForEach(grades => grades.ChildRules(grade => grade.RuleFor(x => x).NotEmpty()));
            });

            When(x => x.IsCreateSessionSetFromXml, () =>
            {

                RuleFor(x => x.IdGradepathwayforCreateSession)
                .NotEmpty();

                RuleFor(x => x.SessionSetName)
                 .NotEmpty();

            });

            When(x => !x.IsCreateSessionSetFromXml , () =>
            {
                RuleFor(x => x.IdSessionSet).NotEmpty();
            });
        }
    }
}
