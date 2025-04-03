using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollmentDetail;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Data.Model.Student.FnStudent.StudentApprovalSummary;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.StudentApprovalSummary
{
    public class GetStudentApprovalSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly int _newEntryApproval;
        private readonly IStudentEnrollmentDetail _studentEnrollmentService;

        public GetStudentApprovalSummaryHandler(IStudentDbContext schoolDbContext, IConfiguration configuration,
                                                IStudentEnrollmentDetail StudentEnrollmentService)
        {
            _dbContext = schoolDbContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
            _studentEnrollmentService = StudentEnrollmentService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            #region commented
            //var param = await Request.GetBody<GetStudentApprovalSummaryRequest>();
            //FillConfiguration();
            //_studentEnrollmentService.SetConfigurationFrom(ApiConfiguration);
            //_gradeService.SetConfigurationFrom(ApiConfiguration);


            //var grade = await _gradeService.Execute.GetGrades(
            //        new GetGradeRequest
            //        {
            //            IdSchool = param.IdSchool,
            //            IdAcadyear = param.AcademicYear,
            //            IdLevel = param.SchoolLevelId
            //        }
            //    );


            //var pathway = await _pathwayService.Execute.GetPathwaysByGradePathway(new GetPathwayGradeRequest
            //{
            //    IdGrade = grade.Payload.
            //});

            //var paramForStudentEnrollMent = new GetStudentEnrollmentRequest
            //{
            //    IdHomeroom = param.HomeroomID
            //};

            //var query = _dbContext.Entity<TrStudentInfoUpdate>()
            //            .Select(
            //                x => new GetStudentApprovalSummaryResult
            //                {
            //                    FromParentDesk = x.IsParentUpdate
            //                }
            //                    ).ToList();

            //#region nembak
            //List<GetStudentApprovalSummaryResult> data = new List<GetStudentApprovalSummaryResult>();

            //#region grade 1
            //data.Add(new GetStudentApprovalSummaryResult{ Grade = "1",Class ="1A",ClassID = "1A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "1", Class = "1B", ClassID = "1B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "1", Class = "1C", ClassID = "1C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "1", Class = "1D", ClassID = "1D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 2
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "2", Class = "2A", ClassID = "2A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "2", Class = "2B", ClassID = "2B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "2", Class = "2C", ClassID = "2C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "2", Class = "2D", ClassID = "2D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 3
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "3", Class = "3A", ClassID = "3A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "3", Class = "3B", ClassID = "3B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "3", Class = "3C", ClassID = "3C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "3", Class = "3D", ClassID = "3D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 4
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "4", Class = "4A", ClassID = "4A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "4", Class = "4B", ClassID = "4B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "4", Class = "4C", ClassID = "4C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "4", Class = "4D", ClassID = "4D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 5
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "5", Class = "5A", ClassID = "5A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "5", Class = "5B", ClassID = "5B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "5", Class = "5C", ClassID = "5C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "5", Class = "5D", ClassID = "5D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 6
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "6", Class = "6A", ClassID = "6A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "6", Class = "6B", ClassID = "6B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "6", Class = "6C", ClassID = "6C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "6", Class = "6D", ClassID = "6D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 7
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "7", Class = "7A", ClassID = "7A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "7", Class = "7B", ClassID = "7B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "7", Class = "7C", ClassID = "7C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "7", Class = "7D", ClassID = "7D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 8
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "8", Class = "8A", ClassID = "8A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "8", Class = "8B", ClassID = "8B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "8", Class = "8C", ClassID = "8C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "8", Class = "8D", ClassID = "8D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 9
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "9", Class = "9A", ClassID = "9A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "9", Class = "9B", ClassID = "9B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "9", Class = "9C", ClassID = "9C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "9", Class = "9D", ClassID = "9D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 10
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "10", Class = "10A", ClassID = "10A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "10", Class = "10B", ClassID = "10B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "10", Class = "10C", ClassID = "10C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "10", Class = "10D", ClassID = "10D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 11
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "11", Class = "11A", ClassID = "11A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "11", Class = "11B", ClassID = "11B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "11", Class = "11C", ClassID = "11C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "11", Class = "11D", ClassID = "11D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#region grade 12
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "12", Class = "12A", ClassID = "12A", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "12", Class = "12B", ClassID = "12B", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "12", Class = "12C", ClassID = "12C", FromParentDesk = 0, FromStaffDesk = 0 });
            //data.Add(new GetStudentApprovalSummaryResult { Grade = "12", Class = "12D", ClassID = "12D", FromParentDesk = 0, FromStaffDesk = 0 });
            //#endregion 

            //#endregion
            #endregion

            var param = await Request.GetBody<GetStudentApprovalSummaryRequest>();

            //var paramForStudentEnrollment = new GetStudentEnrollmentforStudentApprovalSummaryRequest
            //{
            //    AcademicYearId = param.AcademicYear,
            //    SchoolId = param.SchoolId,
            //    GradeId = param.GradeId,
            //    PathwayID = param.HomeroomId
            //};

            //var studentEnrollment = await _studentEnrollmentService.GetStudentEnrollmentForStudentApprovalSummary(paramForStudentEnrollment);

            var studentEnrollmentData = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(x => x.Homeroom)
                                        .ThenInclude(a => a.Grade)
                                        .ThenInclude(b => b.MsLevel)
                                        .ThenInclude(c => c.MsAcademicYear)
                                        .Include(x => x.Homeroom)
                                        .ThenInclude(x => x.MsGradePathwayClassroom)
                                        .ThenInclude(x => x.Classroom)
                                        .Include(x => x.Student)
                                        .Where(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == (string.IsNullOrEmpty(param.AcademicYear) ? x.Homeroom.Grade.MsLevel.IdAcademicYear : param.AcademicYear)
                                        && x.Homeroom.IdGrade == (string.IsNullOrEmpty(param.GradeId) ? x.Homeroom.IdGrade : param.GradeId)
                                        && x.IdHomeroom == (string.IsNullOrEmpty(param.HomeroomId) ? x.IdHomeroom : param.HomeroomId)
                                        && x.Semester == param.Semester
                                        && (string.IsNullOrEmpty(param.LevelId) ? true : x.Homeroom.Grade.IdLevel == param.LevelId)
                                        )
                                        .Select(x => new GetStudentEnrollmentforStudentApprovalSummaryResult
                                        {
                                            AcademicYearId = param.AcademicYear,
                                            GradeId = x.Homeroom.IdGrade,
                                            GradeName = x.Homeroom.Grade.Code,
                                            StudentId = x.IdStudent,
                                            HomeroomId = x.Homeroom.Id,
                                            HomeroomName = x.Homeroom.Grade.Description + x.Homeroom.MsGradePathwayClassroom.Classroom.Description
                                        }).ToListAsync();

            if (studentEnrollmentData != null)
            {
                //var result = studentEnrollment.Payload.ToList();
                if(studentEnrollmentData.Count < 1)
                {
                    var retVal = new List<GetStudentApprovalSummaryResult>();
                    return Request.CreateApiResult2(retVal as object);
                }
                else
                {
                    var studentEnrollmentDistinctGradeList = studentEnrollmentData
                                                                .Select(x => new GetStudentApprovalSummaryResult()
                                                                {
                                                                    GradeId = x.GradeId,
                                                                    Grade = x.GradeName,
                                                                    Class = x.HomeroomName,
                                                                    ClassID = x.HomeroomId
                                                                })
                                                                .GroupBy(x => new { x.GradeId, x.Grade, x.Class, x.ClassID, x.FromParentDesk, x.FromStaffDesk })
                                                                .ToList();

                    var retVal = new List<GetStudentApprovalSummaryResult>();

                    foreach (var studentEnrollmentDistinctGrade in studentEnrollmentDistinctGradeList)
                    {
                        var studentEnrollmentGrade = studentEnrollmentData
                                                        .Where(x => x.GradeId == studentEnrollmentDistinctGrade.Key.GradeId &&
                                                                    x.HomeroomId == studentEnrollmentDistinctGrade.Key.ClassID)
                                                        .GroupBy(x => x.StudentId)
                                                        .Select(x => x.First())
                                                        .ToList();

                        int fromParent = 0;
                        int fromStaff = 0;

                        foreach (GetStudentEnrollmentforStudentApprovalSummaryResult data in studentEnrollmentGrade)
                        {

                            //var getParentId = _dbContext.Entity<MsStudentParent>().Where(x => x.IdStudent == data.StudentId).Select(x => x.IdParent).ToList();

                            //Check from parent
                            var updateFromParent = _dbContext.Entity<TrStudentInfoUpdate>()
                                        //.Where(x => x.IdUser == data.StudentId || getParentId.Contains(x.IdUser))
                                        .Where(x => x.Constraint3Value == data.StudentId)
                                        .Where(x => x.IdApprovalStatus == _newEntryApproval)
                                        .Where(x => x.IsParentUpdate == 1)
                                        .FirstOrDefault();

                            //check from staff
                            var updateFromStaff = _dbContext.Entity<TrStudentInfoUpdate>()
                                        //.Where(x => x.IdUser == data.StudentId || getParentId.Contains(x.IdUser))
                                        .Where(x => x.Constraint3Value == data.StudentId)
                                        .Where(x => x.IdApprovalStatus == _newEntryApproval)
                                        .Where(x => x.IsParentUpdate == 0)
                                        .FirstOrDefault();

                            if (updateFromParent != null)
                            {
                                fromParent += 1;
                            }

                            if (updateFromStaff != null)
                            {
                                fromStaff += 1;
                            }
                        }

                        //studentEnrollmentDistinctGrade.Key.FromStaffDesk = fromStaff;
                        //studentEnrollmentDistinctGrade.Key.FromParentDesk = fromParent;

                        var result = new GetStudentApprovalSummaryResult()
                        {
                            GradeId = studentEnrollmentDistinctGrade.Key.GradeId,
                            Grade = studentEnrollmentDistinctGrade.Key.Grade,
                            Class = studentEnrollmentDistinctGrade.Key.Class,
                            ClassID = studentEnrollmentDistinctGrade.Key.ClassID,
                            FromParentDesk = fromParent,
                            FromStaffDesk = fromStaff
                        };
                        retVal.Add(result);
                    }

                    return Request.CreateApiResult2(retVal as object);
                }
            }
            else
            {
                var retVal = new List<GetStudentApprovalSummaryResult>();
                return Request.CreateApiResult2(retVal as object);
            }

        }
    }
}
