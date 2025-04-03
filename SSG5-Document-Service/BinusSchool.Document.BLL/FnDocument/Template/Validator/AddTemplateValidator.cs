using BinusSchool.Data.Model.Document.FnDocument.Template;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.Template
{
    public class AddTemplateValidator : AbstractValidator<AddTemplateRequest>
    {
        public AddTemplateValidator()
        {
            RuleFor(x => x.IdSchoolDocumentCategory).NotEmpty();

            RuleFor(x => x.JsonFormElement).NotEmpty();
            
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.AcademicYear)
                .NotEmpty()
                .ForEach(child => child.ChildRules(x => x.RuleFor(y => y).NotEmpty()));

            RuleFor(x => x.Semester)
                .NotEmpty();

            RuleFor(x => x.Term)
                .NotEmpty()
                .ForEach(child => child.ChildRules(x => x.RuleFor(y => y).NotEmpty()));

            RuleFor(x => x.Level)
                .NotEmpty()
                .ForEach(child => child.ChildRules(x => x.RuleFor(y => y).NotEmpty()));

            RuleFor(x => x.Grade)
                .NotEmpty()
                .ForEach(child => child.ChildRules(x => x.RuleFor(y => y).NotEmpty()));

            RuleFor(x => x.Subject)
                .NotEmpty()
                .ForEach(child => child.ChildRules(x => x.RuleFor(y => y).NotEmpty()));

            When(x => x.IsApprovalForm, () => RuleFor(x => x.IdApprovalType).NotEmpty());

            RuleFor(x => x.UserAndRole)
                .NotEmpty();
                //.ChildRules(child => 
                //{
                //    child.RuleFor(x => x.IdRole).NotEmpty();
                //});
        }
    }
}