using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.StudentPrevSchoolInfo;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentPrevSchoolInfo.Validator
{
    public class UpdateStudentPrevSchoolInfoValidator : AbstractValidator<UpdateStudentPrevSchoolInfoRequest>
    {
        public UpdateStudentPrevSchoolInfoValidator()
        {
        }
    }
}
