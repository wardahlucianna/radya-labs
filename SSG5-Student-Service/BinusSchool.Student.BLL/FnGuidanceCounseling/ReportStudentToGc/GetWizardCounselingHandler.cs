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
    public class GetWizardCounselingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IRolePosition _serviceRolePosition;
        public GetWizardCounselingHandler(IStudentDbContext studentDbContext, IRolePosition serviceRolePosition)
        {
            _dbContext = studentDbContext;
            _serviceRolePosition = serviceRolePosition;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetWizardCounselingRequest>(nameof(GetWizardCounselingRequest.IdAcademicYear),
                                                                           nameof(GetWizardCounselingRequest.IdLevel),
                                                                           nameof(GetWizardCounselingRequest.IdGrade));
            List<GetWizardCounselingResult> GetWizardCounselingResult = new List<GetWizardCounselingResult>();
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
                       .Where(e => listPosition.Contains(e.Position.Code))
                       .Select(e => e.Id)
                       .ToListAsync(CancellationToken);

            var listHomeroomStudentEnroll = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                        .Where(e => e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
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
                    IdUser = param.IdUser,
                };

                var apiSubjectByUser = await _serviceRolePosition.GetSubjectByUser(Request);
                var getSubjectByUser = apiSubjectByUser.IsSuccess ? apiSubjectByUser.Payload : null;
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
                                    .Where(e => e.Counselor.IdUser == param.IdUser
                                                && e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                    .Select(e => e.IdGrade)
                                    .ToListAsync(CancellationToken);

            var listHomeroomStudentByCounsellor = listHomeroomStudentEnroll
                    .Where(e => listIdGradeByCounsellor.Contains(e.IdGrade))
                    .Select(e => e.IdHomeroomStudent)
                    .Distinct().ToList();

            listIdHomeroomStudent.AddRange(listHomeroomStudentByCounsellor);
            #endregion


            #region count-student
            var queryHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                    .Include(e => e.Student)
                    .Include(e => e.Homeroom)
                        .ThenInclude(e => e.MsGradePathwayClassroom)
                            .ThenInclude(e => e.GradePathway)
                                .ThenInclude(e => e.Grade)
                                    .ThenInclude(e => e.MsLevel)
                                        .ThenInclude(e => e.MsAcademicYear)
                    .Include(e => e.Homeroom)
                         .ThenInclude(e => e.MsGradePathwayClassroom)
                            .ThenInclude(e => e.Classroom)
                     .Where(e => e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear
                                    && listIdHomeroomStudent.Contains(e.Id))
                     .Select(e => new
                     {
                         Id = e.Id,
                         AcademicYear = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.Id,
                         IdStudent = e.IdStudent,
                         StudentName = (e.Student.FirstName == null ? "" : e.Student.FirstName) + (e.Student.MiddleName == null ? "" : " " + e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName),
                         BinusanID = e.Student.IdBinusian,
                         IdLevel = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id,
                         Level = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Description,
                         Grade = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Description,
                         IdGrade = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id,
                         Homeroom = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code + e.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                         IdHomeroom = e.Homeroom.Id,
                         Semester = e.Homeroom.Semester,
                     });

            //filter
            if (!string.IsNullOrEmpty(param.IdGrade))
                queryHomeroomStudent = queryHomeroomStudent.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdLevel))
                queryHomeroomStudent = queryHomeroomStudent.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                queryHomeroomStudent = queryHomeroomStudent.Where(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                queryHomeroomStudent = queryHomeroomStudent.Where(x => x.IdHomeroom == param.IdHomeroom);

            var listIdStudent = await queryHomeroomStudent
                .Select(e=>e.IdStudent)
                .ToListAsync(CancellationToken);

            GetWizardCounselingResult.Add(new GetWizardCounselingResult
            {
                NameWizard = "Total Student",
                Count = listIdStudent.Count(),
            });
            #endregion

            #region Total-session
            var GetCounsellingEntry = await _dbContext.Entity<TrCounselingServicesEntry>()
                    .Where(e => listIdStudent.Contains(e.IdStudent) && e.IdAcademicYear == param.IdAcademicYear)
                    .ToListAsync(CancellationToken);

            GetWizardCounselingResult.Add(new GetWizardCounselingResult
            {
                NameWizard = "Total Session",
                Count = GetCounsellingEntry.Count(),
            });
            #endregion

            #region counseling-category
            var idSchool = await _dbContext.Entity<MsAcademicYear>().Where(e=>e.Id==param.IdAcademicYear).Select(e=>e.IdSchool).FirstOrDefaultAsync(CancellationToken);
            var GetCounselingCategory = await _dbContext.Entity<MsCounselingCategory>().Where(e=>e.IdSchool== idSchool).ToListAsync(CancellationToken);

            foreach (var Item in GetCounselingCategory)
            {
                var countCounselingCategory = GetCounsellingEntry.Where(e => e.IdCounselingCategory == Item.Id).Count();
                GetWizardCounselingResult.Add(new GetWizardCounselingResult
                {
                    NameWizard = Item.CounselingCategoryName,
                    Count = countCounselingCategory,
                });
            }
            #endregion
            return Request.CreateApiResult2(GetWizardCounselingResult as object);
        }



    }
}
