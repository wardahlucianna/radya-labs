using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentPhoto.Validator
{
    public class DeleteStudentPhotoValidator : AbstractValidator<DeleteStudentPhotoRequest>
    {
        public DeleteStudentPhotoValidator()
        {
            RuleFor(x => x.IdStudentPhoto).NotEmpty();
        }
    }
}
