using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator
{
    public class SaveExemplaryCharacterValidator : AbstractValidator<List<SaveExemplaryCharacterRequest>>
    {
        public SaveExemplaryCharacterValidator()
        {
            RuleFor(x => x)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    //data.RuleFor(x => x.IdExemplary).NotEmpty();
                    data.RuleFor(x => x.IdAcademicYear).NotEmpty();
                    //data.RuleFor(x => x.DatePosted).NotEmpty();
                    //data.RuleFor(x => x.ExemplaryDate).NotEmpty();
                    //data.RuleFor(x => x.Title).NotEmpty();
                    //data.RuleFor(x => x.Description).NotEmpty();
                    data.RuleFor(x => x.ValueList)
                            .NotEmpty()
                            .ForEach(modal => modal.ChildRules(student =>
                            {
                                student.RuleFor(x => x.IdLtExemplaryValue).NotEmpty();
                            }));
                    data.RuleFor(x => x.Student)
                            .NotEmpty()
                            .ForEach(modal => modal.ChildRules(student =>
                            {
                                student.RuleFor(x => x.IdHomeroom).NotEmpty();
                                student.RuleFor(x => x.IdStudent).NotEmpty();
                            }));
                    data.RuleFor(x => x.Attachment)
                            .NotEmpty()
                            .ForEach(modal => modal.ChildRules(attachment =>
                            {
                                attachment.RuleFor(x => x.Url).NotEmpty();
                                attachment.RuleFor(x => x.FileName).NotEmpty();
                                attachment.RuleFor(x => x.OriginalFileName).NotEmpty();
                                attachment.RuleFor(x => x.FileExtension).NotEmpty();
                                attachment.RuleFor(x => x.FileSize).NotNull();
                            }));
                }));
        }
    }
}
