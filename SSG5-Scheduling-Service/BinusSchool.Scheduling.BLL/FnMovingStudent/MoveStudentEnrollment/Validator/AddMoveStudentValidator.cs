using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using FluentValidation;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment.Validator
{
    public class AddMoveStudentValidator : AbstractValidator<AddMoveStudentEnrollmentRequest>
    {
        public AddMoveStudentValidator()
        {

            RuleForEach(x => x.StudentEnrollment).ChildRules(studentEnrollment =>
            {
                studentEnrollment.RuleFor(x => x.IdLessonOld).NotEmpty().WithMessage("Id lesson old cannot empty");
                studentEnrollment.RuleFor(x => x.IdSubjectOld).NotEmpty().WithMessage("Id subject old cannot empty");
                //studentEnrollment.RuleFor(x => x.idSubjectLevelOld).NotEmpty().WithMessage("Id subject level old cannot empty");

                studentEnrollment.When(x => x.IsDelete == true, () =>
                {
                    studentEnrollment.RuleFor(x => x.IdHomeroomStudentEnrollment).NotEmpty().WithMessage("Id homeroom student enrollment cannot empty");
                    studentEnrollment.RuleFor(x => x.Note).NotEmpty().WithMessage("Note old cannot empty");
                    studentEnrollment.RuleFor(x => x.EffectiveDate).NotEmpty().WithMessage("Effective date cannot empty");
                });

                studentEnrollment.When(x => x.IsDelete == false, () =>
                {
                    studentEnrollment.RuleFor(x => x.IdHomeroomStudentEnrollment).NotEmpty().WithMessage("Id homeroom student enrollment cannot empty");
                    studentEnrollment.RuleFor(x => x.Note).NotEmpty().WithMessage("Note old cannot empty");
                    studentEnrollment.RuleFor(x => x.EffectiveDate).NotEmpty().WithMessage("Effective date cannot empty");
                    studentEnrollment.RuleFor(x => x.IdLessonNew).NotEmpty().WithMessage("Id lesson new cannot empty");
                    studentEnrollment.RuleFor(x => x.IdSubjectNew).NotEmpty().WithMessage("Id subject new cannot empty");
                    //studentEnrollment.RuleFor(x => x.idSubjectLevelNew).NotEmpty().WithMessage("Id subject level new cannot empty");
                });


            });

            
        }


    }
}
