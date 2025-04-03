using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailEntryMeritV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailEntryMeritV2Handler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailMeritTeacherV2Request>();

            string[] _columns = { "AcademicYear", "Semester", "Achievement", "DisciplineName", "VerifyingTeacher", "FocusArea", "Date", "Point", "CreateBy", "Merit/Achievement" };

            var PointStudent = await (from HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>()
                                      join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                      join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                                      join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                      join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                                      join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                                      join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                                      join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                                      join Point in _dbContext.Entity<TrStudentPoint>() on HomeroomStudent.Id equals Point.IdHomeroomStudent into JoinedPoint
                                      from Point in JoinedPoint.DefaultIfEmpty()
                                      join LevelInfraction in _dbContext.Entity<MsLevelOfInteraction>() on Point.IdLevelOfInteraction equals LevelInfraction.Id into JoinedLevel
                                      from LevelInfraction in JoinedLevel.DefaultIfEmpty()
                                      join ParentLevelInfraction in _dbContext.Entity<MsLevelOfInteraction>() on LevelInfraction.IdParentLevelOfInteraction equals ParentLevelInfraction.Id into JoinedParentLevelInfraction
                                      from ParentLevelInfraction in JoinedParentLevelInfraction.DefaultIfEmpty()
                                      join Sanction in _dbContext.Entity<MsSanctionMapping>() on Point.IdSanctionMapping equals Sanction.Id into JoinedSanction
                                      from Sanction in JoinedSanction.DefaultIfEmpty()
                                      where HomeroomStudent.Id == param.IdHomeroomStudent && Level.IdAcademicYear == param.IdAcademicYear
                                      select new
                                      {
                                          Student = Student.Id + " - " + (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                          Homeroom = Level.Code + " - " + Grade.Code + Classroom.Code,
                                          TotalDemerit = Point.DemeritPoint,
                                          TotalMerit = Point.MeritPoint,
                                          LevelOfInteraction = LevelInfraction == null
                                            ? ""
                                            : LevelInfraction.IdParentLevelOfInteraction == null
                                                ? LevelInfraction.NameLevelOfInteraction
                                                : ParentLevelInfraction.NameLevelOfInteraction + LevelInfraction.NameLevelOfInteraction,
                                          Sanction = Sanction.SanctionName,
                                      }
                                    ).SingleOrDefaultAsync(CancellationToken);

            if (PointStudent == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["StudentPoint"], "Id", param.IdHomeroomStudent));


            var IsHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                .Any(e => e.IdBinusian == param.IdUser && e.IdHomeroom == param.IdHomeroom && e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear &&
                e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id == param.IdLevel &&
                e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id == param.IdGrade);

            var predicate = PredicateBuilder.Create<TrEntryMeritStudent>(e => e.IdHomeroomStudent == param.IdHomeroomStudent
                                && ((e.RequestType == RequestType.Create && e.Status == "Approved") || (e.RequestType == RequestType.Delete && e.Status != "Decline")) && e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Type.ToString()))
                predicate = predicate.And(e => e.Type == param.Type);

            var query = _dbContext.Entity<TrEntryMeritStudent>()
                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                        .Include(e => e.MeritDemeritMapping)
                        .Include(e => e.UserCraete)
                        .Include(e => e.FocusArea)
                        .Include(e => e.EntryMeritStudentEvidances)
                        .Include(e => e.StudentMeritApproval).ThenInclude(e => e.User1)
                        .Where(predicate)
                        .Select(e => new WizardStudentDetailMeritV2
                        {
                            Id = e.Id,
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
                                        .OrderBy(f => f.DateIn)
                                        .Select(f => new AchievementEvidance
                                        {
                                            OriginalName = f.OriginalName,
                                            FileSize = f.FileSize,
                                            FileName = f.FileName,
                                            Url = f.Url,
                                            FileType = f.FileType
                                        }).LastOrDefault(),
                            IsDisabledDelete = IsHomeroomTeacher
                                ? e.RequestType == RequestType.Delete || e.IsDeleted ? true : false
                                : true,
                            IsDisabledEdit = e.Type == EntryMeritStudentType.Merit
                                                ? e.MeritUserCreate == param.IdUser ?
                                                    false : true
                                                : true,
                            DateUpdate = e.DateUp == null ? e.DateIn : e.DateUp,
                            ApprovalNote = e.StudentMeritApproval
                                            .OrderBy(f => f.DateIn)
                                            .Select(f => f.IdUserApproved3 == null
                                                                    ? f.IdUserApproved2 == null
                                                                        ? f.Note1
                                                                        : f.Note2
                                                                    : f.Note3)
                                            .LastOrDefault(),
                            Status = e.Status
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

            GetDetailMeritTeacherV2Result itemsMerit = new GetDetailMeritTeacherV2Result();
            List<WizardStudentDetailMeritV2> items = new List<WizardStudentDetailMeritV2>();
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                itemsMerit.Merit = result;
                itemsMerit.Student = PointStudent.Student;
                itemsMerit.Homeroom = PointStudent.Homeroom;
                itemsMerit.TotalDemerit = PointStudent.TotalDemerit;
                itemsMerit.TotalMerit = PointStudent.TotalMerit;
                itemsMerit.LevelOfInfraction = PointStudent.LevelOfInteraction;
                itemsMerit.Sanction = PointStudent.Sanction;
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                itemsMerit.Merit = result;
                itemsMerit.Student = PointStudent.Student;
                itemsMerit.Homeroom = PointStudent.Homeroom;
                itemsMerit.TotalDemerit = PointStudent.TotalDemerit;
                itemsMerit.TotalMerit = PointStudent.TotalMerit;
                itemsMerit.LevelOfInfraction = PointStudent.LevelOfInteraction;
                itemsMerit.Sanction = PointStudent.Sanction;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Date).CountAsync(CancellationToken);

            return Request.CreateApiResult2(itemsMerit as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));


        }
    }
}
