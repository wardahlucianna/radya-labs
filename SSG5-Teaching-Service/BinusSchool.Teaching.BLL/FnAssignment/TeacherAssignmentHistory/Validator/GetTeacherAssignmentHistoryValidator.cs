using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignmentHistory;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignmentHistory.Validator
{
    public class GetTeacherAssignmentHistoryValidator : AbstractValidator<GetTeacherAssignmentHistoryRequest>
    {
        public GetTeacherAssignmentHistoryValidator()
        {
            
        }
    }
}
