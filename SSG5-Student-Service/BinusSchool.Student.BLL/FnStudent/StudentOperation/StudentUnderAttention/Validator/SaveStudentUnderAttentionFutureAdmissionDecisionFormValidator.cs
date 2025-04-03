using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.StudentOperation.StudentUnderAttention.Validator
{
    public class SaveStudentUnderAttentionFutureAdmissionDecisionFormValidator : AbstractValidator<SaveStudentUnderAttentionFutureAdmissionDecisionFormRequest>
    {
        public SaveStudentUnderAttentionFutureAdmissionDecisionFormValidator()
        {
            RuleFor(x => x.IdTrStudentStatus).NotEmpty();
        }
    }
}
