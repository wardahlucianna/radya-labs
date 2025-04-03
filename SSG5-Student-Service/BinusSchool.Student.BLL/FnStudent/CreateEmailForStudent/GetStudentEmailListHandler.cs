using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollmentDetail;
using BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreateEmailForStudent
{
    public class GetStudentEmailListHandler : FunctionsHttpSingleHandler
    {

        private readonly IStudentDbContext _dbContext;
        private readonly IStudentEnrollmentDetail _studentEnrollmentService;
        private readonly IStudentHomeroomDetail _studentHomeroomService;
        

        public GetStudentEmailListHandler(IStudentDbContext schoolDbContext,
                                            IStudentEnrollmentDetail studentEnrollmentService,
                                            IStudentHomeroomDetail studentHomeroomService)
        {
            _dbContext = schoolDbContext;
            _studentEnrollmentService = studentEnrollmentService;
            _studentHomeroomService = studentHomeroomService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = await Request.GetBody<GetStudentEmailListRequest>();

            var paramForStudentEnrollment = new GetStudentEnrollmentforStudentApprovalSummaryRequest
            {
                AcademicYearId = param.AcademicYearId,
                SchoolId = param.SchoolId,
                GradeId = param.GradeId,
                PathwayID = param.PathwayID
            };

            var studentEnrollment = await _studentEnrollmentService.GetStudentEnrollmentForStudentApprovalSummary(paramForStudentEnrollment);

            if (studentEnrollment.IsSuccess)
            {
                var studentEnrollmentResult = studentEnrollment.Payload.ToList();

                if (studentEnrollmentResult != null && studentEnrollmentResult.Count > 0)
                {
                    var StudentList = studentEnrollmentResult.Select(x => x.StudentId).ToList();

                    if (param.GetAll == false)
                    {
                        var predicate = PredicateBuilder.False<GetStudentEmailListResult>();

                        if (!string.IsNullOrEmpty(param.StudentID))
                        {
                            predicate = predicate.Or(s => s.StudentID.Contains(param.StudentID));
                        }

                        if(!string.IsNullOrEmpty(param.StudentName))
                        {
                            predicate = predicate.Or(s => s.StudentName.Contains(param.StudentName));
                        }

                        if (!string.IsNullOrEmpty(param.Email))
                        {
                            predicate = predicate.Or(s => s.StudentEmail.Contains(param.Email));
                        }

                        var query = _dbContext.Entity<MsStudent>()
                                    .Where(x => StudentList.Contains(x.Id))
                                    .Select
                                    (
                                        x => new GetStudentEmailListResult
                                        {
                                            StudentID = x.Id,
                                            StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
                                                        + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
                                            Gender = x.Gender.ToString(),
                                            StudentEmailStatus = x.EmailGenerate.IsSync == 1 ? "Active" : "Inactive",
                                            StudentEmail = x.BinusianEmailAddress
                                        }
                                    ).Where(predicate);

                        var items = await query.OrderByDynamic(param).SetPagination(param).ToListAsync(CancellationToken);

                        foreach (GetStudentEmailListResult data in items)
                        {
                            var classroomService = await _studentHomeroomService.GetGradeAndClassByStudents(new GetHomeroomByStudentRequest { Ids = new List<string> { data.StudentID }, IdGrades = new List<string> { param.GradeId } });

                            if (classroomService.IsSuccess)
                            {
                                var classroomServiceResult = classroomService.Payload.FirstOrDefault();

                                data.Classroom = classroomServiceResult.Classroom.Description;
                            }
                        }

                        var count = await query.CountAsync(CancellationToken);
                        return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));

                    }
                    else
                    {
                        param.GetAll = false;
                        var query = _dbContext.Entity<MsStudent>()
                                    .Where(x => StudentList.Contains(x.Id))
                                    .Select
                                    (
                                        x => new GetStudentEmailListResult
                                        {
                                            StudentID = x.Id,
                                            StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
                                                        + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
                                            Gender = x.Gender.ToString(),
                                            StudentEmailStatus = x.EmailGenerate.IsSync == 1 ? "Active" : "Inactive",
                                            StudentEmail = x.BinusianEmailAddress
                                        }
                                    );

                        var items = await query.OrderByDynamic(param).SetPagination(param).ToListAsync(CancellationToken);

                        foreach (GetStudentEmailListResult data in items)
                        {
                            var classroomService = await _studentHomeroomService.GetGradeAndClassByStudents(new GetHomeroomByStudentRequest { Ids = new List<string> { data.StudentID }, IdGrades = new List<string> { param.GradeId } });

                            if (classroomService.IsSuccess)
                            {
                                var classroomServiceResult = classroomService.Payload.FirstOrDefault();

                                data.Classroom = classroomServiceResult.Classroom.Description;
                            }
                        }

                        var count = await query.CountAsync(CancellationToken);
                        return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
                    }

                }
                else
                {
                    var item = new List<GetStudentEmailListResult>();

                    var count = item.Count;

                    return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
                }
            }
            else
            {
                var item = new List<GetStudentEmailListResult>();

                var count = item.Count;

                return Request.CreateApiResult2(item as object, param.CreatePaginationProperty(count));
            }

        }
    }
}
