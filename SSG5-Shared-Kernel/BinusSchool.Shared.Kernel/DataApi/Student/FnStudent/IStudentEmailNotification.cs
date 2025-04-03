using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentEmailNotification : IFnStudent
    {
        [Post("/student/send-email/profile-update-to-staff")]
        Task<ApiErrorResult> SendEmailProfileUpdateToStaff([Body] SendEmailProfileUpdateToStaffRequest body);

        [Post("/student/send-email/approval-profile-update-to-parent")]
        Task<ApiErrorResult> SendEmailProfileApprovalUpdateToParent([Body] SendEmailProfileApprovalUpdateToParentRequest body);
    }
}
