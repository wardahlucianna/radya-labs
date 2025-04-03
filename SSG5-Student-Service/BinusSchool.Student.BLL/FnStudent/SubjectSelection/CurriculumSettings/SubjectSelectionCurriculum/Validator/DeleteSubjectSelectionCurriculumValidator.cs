using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.CurriculumSettings.SubjectSelectionCurriculum.Validator
{
    public class DeleteSubjectSelectionCurriculumValidator : AbstractValidator<DeleteSubjectSelectionCurriculumRequest>
    {
        public DeleteSubjectSelectionCurriculumValidator()
        {
            RuleFor(x => x.IdSubjectSelectionCurriculum).NotEmpty();
        }
    }
}
