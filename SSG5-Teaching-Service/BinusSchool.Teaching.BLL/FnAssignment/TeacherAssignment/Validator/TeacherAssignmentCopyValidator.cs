using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.Validator
{
    public class TeacherAssignmentCopyValidator : AbstractValidator<TeacherAssignmentCopyRequest>
    {
        public TeacherAssignmentCopyValidator()
        {
            
        }
    }
}
