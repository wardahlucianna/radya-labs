using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.School.FnBanner.Banner;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetSummaryCounselingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IRolePosition _serviceRolePosition;
        public GetSummaryCounselingHandler(IStudentDbContext studentDbContext, IRolePosition serviceRolePosition)
        {
            _dbContext = studentDbContext;
            _serviceRolePosition = serviceRolePosition;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSummaryCounselingRequest>() ;
            string[] _columns = { "AcademicYear", "StudentName", "BinusanID", "Level", "Grade", "Homeroom" };
            List<string> listIdHomeroomStudent = new List<string>();

            #region position allow akses
            List<string> listPosition = new List<string>()
            {
                PositionConstant.VicePrincipal,
                PositionConstant.Principal,
                PositionConstant.AffectiveCoordinator,
                PositionConstant.NonTeachingStaff, //HOGC
            };
            #endregion

            #region Position
            var listIdTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                        .Where(e=>listPosition.Contains(e.Position.Code))
                        .Select(e=>e.Id)
                        .ToListAsync(CancellationToken);

            var listHomeroomStudentEnroll = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                        .Where(e => e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear==param.IdAcademicYear)
                        .Select(e => new
                        {
                            IdGrade = e.HomeroomStudent.Homeroom.Grade.Id,
                            e.IdLesson,
                            e.IdHomeroomStudent
                        })
                        .Distinct()
                        .ToListAsync(CancellationToken);

            if (listIdTeacherPosition.Any())
            {
                var Request = new GetSubjectByUserRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    ListIdTeacherPositions = listIdTeacherPosition,
                    IdUser = param.IdUserCounselor,
                };

                var apiSubjectByUser = await _serviceRolePosition.GetSubjectByUser(Request);
                var getSubjectByUser = apiSubjectByUser.IsSuccess ?apiSubjectByUser.Payload:null;
                if (getSubjectByUser != null)
                {
                    var listIdLesson = getSubjectByUser.Select(e => e.Lesson.Id).ToList();
                    var listHomeroomStudentByPosition = listHomeroomStudentEnroll
                            .Where(e => listIdLesson.Contains(e.IdLesson))
                            .Select(e => e.IdHomeroomStudent)
                            .Distinct().ToList();
                    listIdHomeroomStudent.AddRange(listHomeroomStudentByPosition);
                }
            }
            #endregion

            #region Counsellor
            var listIdGradeByCounsellor = await _dbContext.Entity<MsCounselorGrade>()
                                    .Where(e => e.Counselor.IdUser == param.IdUserCounselor
                                                && e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                    .Select(e => e.IdGrade)
                                    .ToListAsync(CancellationToken);

            var listHomeroomStudentByCounsellor = listHomeroomStudentEnroll
                    .Where(e => listIdGradeByCounsellor.Contains(e.IdGrade))
                    .Select(e => e.IdHomeroomStudent)
                    .Distinct().ToList();

            listIdHomeroomStudent.AddRange(listHomeroomStudentByCounsellor);
            #endregion


            var query = _dbContext.Entity<MsHomeroomStudent>()
                    .Include(e => e.Student)
                    .Include(e => e.Homeroom)
                        .ThenInclude(e=>e.Grade)
                            .ThenInclude(e=>e.MsLevel)
                                .ThenInclude(e=>e.MsAcademicYear)
                    .Include(e => e.Homeroom)
                         .ThenInclude(e => e.MsGradePathwayClassroom)
                            .ThenInclude(e => e.Classroom)
                     .Where(e=>e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.Id==param.IdAcademicYear
                                && listIdHomeroomStudent.Contains(e.Id))
                     .Select(e => new
                     {
                         Id = e.Id,
                         IdAcademicYear = e.Homeroom.Grade.MsLevel.MsAcademicYear.Id,
                         AcademicYear = e.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                         StudentName = (e.Student.FirstName == null ? "" : e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
                         BinusanID = e.Student.IdBinusian,
                         IdStudent = e.Student.Id,
                         IdLevel = e.Homeroom.Grade.MsLevel.Id,
                         Level = e.Homeroom.Grade.MsLevel.Description,
                         Grade = e.Homeroom.Grade.Description,
                         IdGrade = e.Homeroom.Grade.Id,
                         Homeroom = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code + e.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                         IdHomeroom = e.Homeroom.Id,
                         Semester = e.Homeroom.Semester,
                     });

            //filter
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade==param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                query = query.Where(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.BinusanID.Contains(param.Search) || x.StudentName.Contains(param.Search));

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "BinusanID":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusanID)
                        : query.OrderBy(x => x.BinusanID);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level)
                        : query.OrderBy(x => x.Level);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade)
                        : query.OrderBy(x => x.Grade);
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
            };

            IReadOnlyList<object> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetSummaryCounselingResult
                {
                    IdStudent = x.IdStudent,
                    IdAcademicYear = x.IdAcademicYear,
                    AcademicYear = x.AcademicYear,
                    StudentName = x.StudentName,
                    BinusanID = x.BinusanID,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    Semester = x.Semester,
                    IsCounsellor = listHomeroomStudentByCounsellor.Where(e=>e==x.Id).Any()
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetSummaryCounselingResult
                {
                    IdStudent = x.IdStudent,
                    IdAcademicYear = x.IdAcademicYear,
                    AcademicYear = x.AcademicYear,
                    StudentName = x.StudentName,
                    BinusanID = x.BinusanID,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    Semester = x.Semester,
                    IsCounsellor = listHomeroomStudentByCounsellor.Where(e => e == x.Id).Any()
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.BinusanID).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
