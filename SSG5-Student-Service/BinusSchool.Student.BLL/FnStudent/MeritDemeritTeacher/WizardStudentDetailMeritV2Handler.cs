using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Exceptions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardStudentDetailMeritV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public WizardStudentDetailMeritV2Handler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<WizardDetailStudentRequest>();

            string[] _columns = { "AcademicYear", "Semester", "Achievement", "DisciplineName", "VerifyingTeacher", "FocusArea", "Date", "Point", "CreateBy", "Merit/Achievement" };

            var DataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                .Include(e => e.Student)
                                .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                .Where(e => e.IdStudent == param.IdStudent && e.Semester == param.Semester && e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                .Select(e => new
                                {
                                    Student = string.IsNullOrEmpty(e.Student.FirstName) != null ? $"{e.Student.Id} - {e.Student.FirstName.Trim()} {e.Student.LastName.Trim()}" : $"{e.Student.Id} - {e.Student.LastName.Trim()}",
                                    Homeroom = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Code + " - "
                                                + e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code
                                                + e.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            if (DataStudent == null)
                throw new BadRequestException(string.Format("Homeroom student is not found"));

            var PointStudent = await _dbContext.Entity<TrStudentPoint>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway)
                                    .ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                .Where(e => e.HomeroomStudent.IdStudent == param.IdStudent
                                    && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                    && e.HomeroomStudent.Homeroom.Semester == param.Semester)
                                .Select(e => new
                                {
                                    TotalDemerit = e.DemeritPoint,
                                    TotalMerit = e.MeritPoint,
                                    LevelOfInfraction = e.LevelOfInteraction.NameLevelOfInteraction,
                                    Sanction = e.SanctionMapping.SanctionName,
                                    IdAY = e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            var query = _dbContext.Entity<TrEntryMeritStudent>()
                        .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel).ThenInclude(e=>e.MsAcademicYear)
                        .Include(e=>e.MeritDemeritMapping)
                        .Include(e=>e.UserCraete)
                        .Include(e=>e.FocusArea)
                        .Include(e=>e.EntryMeritStudentEvidances)
                        .Include(e=>e.StudentMeritApproval).ThenInclude(e => e.User1)
                        .Where(e=>e.HomeroomStudent.IdStudent==param.IdStudent
                                && e.Status == "Approved"
                                && e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                && e.HomeroomStudent.Homeroom.Semester == param.Semester)
                        .Select(e => new WizardStudentDetailMeritV2
                        {
                            AcademicYear = e.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                            Semester = e.HomeroomStudent.Homeroom.Semester,
                            Achievement = e.Note,
                            DisciplineName = e.MeritDemeritMapping.DisciplineName,
                            VerifyingTeacher = e.Type == EntryMeritStudentType.Merit
                                                ? NameUtil.GenerateFullNameWithId(e.UserCraete.Id, e.UserCraete.DisplayName)
                                                : e.StudentMeritApproval
                                                    .OrderBy(f => f.DateIn)
                                                    .Select(f => NameUtil.GenerateFullNameWithId(f.User1.Id, f.User1.DisplayName))
                                                    .FirstOrDefault(),
                            FocusArea = e.FocusArea.Description,
                            Date = e.DateMerit,
                            Point = e.Point,
                            CreateBy = NameUtil.GenerateFullNameWithId(e.UserCraete.Id, e.UserCraete.DisplayName),
                            MeritAchievement = e.Type.GetDescription(),
                            Evidance = e.EntryMeritStudentEvidances
                                        .OrderBy(f=>f.DateIn)
                                        .Select(f => new AchievementEvidance
                                        {
                                            OriginalName = f.OriginalName,
                                            FileSize = f.FileSize,
                                            FileName = f.FileName,
                                            Url = f.Url,
                                            FileType = f.FileType
                                        }).LastOrDefault(),
                        });
                        
            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Date)
                        : query.OrderBy(x => x.Date);
                    break;
                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "Achievement":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Achievement)
                        : query.OrderBy(x => x.Achievement);
                    break;
                case "VerifyingTeacher":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.VerifyingTeacher)
                        : query.OrderBy(x => x.VerifyingTeacher);
                    break;
                case "FocusArea":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.FocusArea)
                        : query.OrderBy(x => x.FocusArea);
                    break;
                case "Date":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Date)
                        : query.OrderBy(x => x.Date);
                    break;
                case "Point":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Point)
                        : query.OrderBy(x => x.Point);
                    break;
                case "CreateBy":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CreateBy)
                        : query.OrderBy(x => x.CreateBy);
                    break;
                case "MeritAchievement":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.MeritAchievement)
                        : query.OrderBy(x => x.MeritAchievement);
                    break;
            };

            WizardStudentDetailMeritV2Result itemsMerit = new WizardStudentDetailMeritV2Result();
            List<WizardStudentDetailMeritV2> items = new List<WizardStudentDetailMeritV2>();
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                itemsMerit.Merit = result;
                itemsMerit.Student = DataStudent.Student;
                itemsMerit.Homeroom = DataStudent.Homeroom;
                itemsMerit.TotalDemerit = PointStudent == null ? 0 : PointStudent.TotalDemerit;
                itemsMerit.TotalMerit = PointStudent == null ? 0 : PointStudent.TotalMerit;
                itemsMerit.LevelOfInfraction = PointStudent == null ? null : PointStudent.LevelOfInfraction;
                itemsMerit.Sanction = PointStudent == null ? null : PointStudent.Sanction;
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                itemsMerit.Merit = result;
                itemsMerit.Student = DataStudent.Student;
                itemsMerit.Homeroom = DataStudent.Homeroom;
                itemsMerit.TotalDemerit = PointStudent == null ? 0 : PointStudent.TotalDemerit;
                itemsMerit.TotalMerit = PointStudent == null ? 0 : PointStudent.TotalMerit;
                itemsMerit.LevelOfInfraction = PointStudent == null ? null : PointStudent.LevelOfInfraction;
                itemsMerit.Sanction = PointStudent == null ? null : PointStudent.Sanction;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Date).CountAsync(CancellationToken);

            return Request.CreateApiResult2(itemsMerit as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
