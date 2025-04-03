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
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class MeritDemeritTeacherOldHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;

        public MeritDemeritTeacherOldHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {


            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string IdStudent)
        {
            //var param = Request.ValidateParams<DetailMeritTeacherRequest>();

            //var query = _dbContext.Entity<MsHomeroomStudent>()
            //    .Include(e => e.Student).ThenInclude(e => e.StudentGrades).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
            //    .Include(e => e.Homeroom)
            //    .Where(e => e.Id == param.IdHomeroomStudent)
            //   .Select(x => new
            //   {
            //       IdStudent = x.Student.Id,
            //       NameStudent = x.Student.FirstName + x.Student.LastName,
            //       Homeroom = (x.Student.StudentGrades.Select(e => e.Grade.Code).SingleOrDefault()) + (x.Homeroom.MsGradePathwayClassroom.Classroom.Code),
            //       TotalMerit = _dbContext.Entity<TrEntryMeritStudent>()
            //            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student).ThenInclude(e => e.StudentGrades).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
            //            .Where(e => e.HomeroomStudent.Student.StudentGrades.Any(e => e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear) && e.HomeroomStudent.Id == param.IdHomeroomStudent && e.Status != "Decline")
            //            .Sum(e => e.Point),
            //       TotalDemerit = _dbContext.Entity<TrEntryDemeritStudent>()
            //            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student).ThenInclude(e => e.StudentGrades).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
            //           .Where(e => e.HomeroomStudent.Student.StudentGrades.Any(e => e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear) && e.HomeroomStudent.Id == param.IdHomeroomStudent && e.Status != "Decline")
            //            .Sum(e => e.Point),
            //       LevelOfInfraction = _dbContext.Entity<TrEntryDemeritStudent>()
            //            .Include(e => e.MeritDemeritMapping).ThenInclude(e => e.LevelOfInteraction)
            //             .Where(e => e.HomeroomStudent.Student.StudentGrades.Any(e => e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear) && e.HomeroomStudent.Id == param.IdHomeroomStudent && e.Status != "Decline")
            //              .Select(x => new { levelInfraction = x.MeritDemeritMapping.LevelOfInteraction.IdParentLevelOfInteraction == null ? x.MeritDemeritMapping.LevelOfInteraction.NameLevelOfInteraction : x.MeritDemeritMapping.LevelOfInteraction.Parent.NameLevelOfInteraction })
            //           .Max(e => e.levelInfraction),
            //       Merit = _dbContext.Entity<TrEntryMeritStudent>()
            //            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student).ThenInclude(e => e.StudentGrades).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
            //            .Where(e => e.HomeroomStudent.Student.StudentGrades.Any(e => e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear) && e.HomeroomStudent.Id == param.IdHomeroomStudent && e.Status != "Decline")
            //            .Select(e => new {
            //                DateIn = e.DateIn,
            //                DisciplineName = e.MeritDemeritMapping.DisciplineName,
            //                Point = e.Point,
            //                Note = e.Note,
            //                CreateBy = _dbContext.Entity<MsUser>().SingleOrDefault(y => y.Id == e.UserIn),
            //                UpdateBy = e.DateUp,
            //                Status = e.Status,
            //                IsDelete = false
            //            }),
            //       Demerit = _dbContext.Entity<TrEntryMeritStudent>()
            //            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student).ThenInclude(e => e.StudentGrades).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
            //            .Where(e => e.HomeroomStudent.Student.StudentGrades.Any(e => e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear) && e.HomeroomStudent.Id == param.IdHomeroomStudent && e.Status != "Decline")
            //            .Select(e => new {
            //                DateIn = e.DateIn,
            //                DisciplineName = e.MeritDemeritMapping.DisciplineName,
            //                Point = e.Point,
            //                Note = e.Note,
            //                CreateBy = _dbContext.Entity<MsUser>().SingleOrDefault(y => y.Id == e.UserIn),
            //                UpdateBy = e.DateUp,
            //                Status = e.Status,
            //                IsDelete = false
            //            })

            //   })
            //   .Select(x => new {
            //       IdStudent = x.IdStudent,
            //       NameStudent = x.NameStudent,
            //       Homeroom = x.Homeroom,
            //       TotalMerit = x.TotalMerit,
            //       TotalDemerit = x.TotalDemerit,
            //       LevelOfInfraction = x.LevelOfInfraction,
            //       Merit = x.Merit,
            //       Demerit = x.Demerit,
            //       Sunction = _dbContext.Entity<MsSanctionMapping>().SingleOrDefault(e => e.Min >= (x.TotalDemerit - x.TotalMerit) && e.Max <= (x.TotalDemerit - x.TotalMerit)).SanctionName,
            //   });

            //var result = await query
            //       .ToListAsync(CancellationToken);

            //List<DetailMeritTeacherResult> items;
            //items = result.Select(x => new DetailMeritTeacherResult
            //{
            //    Student = x.IdStudent + "-" + x.NameStudent,
            //    Homeroom = x.Homeroom,
            //    TotalMerit = x.TotalMerit,
            //    TotalDemerit = x.TotalDemerit,
            //    LevelOfInfraction = x.LevelOfInfraction,
            //    Sunction = x.Sunction,
            //    Merit = x.Merit.Select(e => new DetailMeritDemeritTeacher
            //    {
            //        Date = e.DateIn,
            //        NameDiscipline = e.DisciplineName,
            //        Note = e.Note,
            //        CreateBy = e.CreateBy.Id + " - " + e.CreateBy.DisplayName,
            //        UpdateDate = e.UpdateBy,
            //        Status = e.Status,
            //        IsDisabledDelete = e.Status == "Approval" || e.Status == null ? false : true,
            //    }).ToList(),
            //    Demerit = x.Merit.Select(e => new DetailMeritDemeritTeacher
            //    {
            //        Date = e.DateIn,
            //        NameDiscipline = e.DisciplineName,
            //        Note = e.Note,
            //        CreateBy = e.CreateBy.Id + " - " + e.CreateBy.DisplayName,
            //        UpdateDate = e.UpdateBy,
            //        Status = e.Status,
            //        IsDisabledDelete = e.Status == "Approval" || e.Status == null ? false : true,
            //    }).ToList(),
            //}).ToList();

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMeritDemeritTeacherRequest>();
            var predicateHomeroomStudent = PredicateBuilder.Create<MsHomeroomStudent>(x => x.Student.StudentGrades
                .Any(e => e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear) &&
                    x.Student.StudentGrades.Any(e => e.Grade.MsLevel.Id == param.IdLevel) &&
                    x.Student.StudentGrades.Any(e => e.Grade.Id == param.IdGrade) &&
                    x.Homeroom.Semester.ToString() == param.Semester.ToString()
                );

            string[] _columns = { "AcademicYear", "Semester", "Level", "Grade", "Homeroom", "IdStudent", "NameStudent", "Merit", "Demerit", "LevelOfInfraction", "Sunction", "LastUpdate" };




            var query = (from hs in _dbContext.Entity<MsHomeroomStudent>()
                         join s in _dbContext.Entity<MsStudent>() on hs.IdStudent equals s.IdBinusian
                         join sg in _dbContext.Entity<MsStudentGrade>() on s.IdBinusian equals sg.IdStudent
                         join g in _dbContext.Entity<MsGrade>() on sg.IdGrade equals g.Id
                         join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                         join sp in _dbContext.Entity<TrStudentPoint>() on hs.Id equals sp.IdHomeroomStudent into joinedPoint
                         from sp in joinedPoint.DefaultIfEmpty()
                         join h in _dbContext.Entity<MsHomeroom>() on hs.IdHomeroom equals h.Id
                         join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                         join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
                         join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                         join loi in _dbContext.Entity<MsLevelOfInteraction>() on sp.IdLevelOfInteraction equals loi.Id
                         join sc in _dbContext.Entity<MsSanctionMapping>() on sp.IdSanctionMapping equals sc.Id
                         where ay.Id == param.IdAcademicYear && g.Id == param.IdGrade && h.Semester == param.Semester
                         select new
                         {
                             IdHomeroom = h.Id,
                             IdHomeroomStudent = hs.Id,
                             AcademicYear = ay.Description,
                             Semester = h.Semester.ToString(),
                             Level = l.Description,
                             Grade = g.Description,
                             Homeroom = g.Code + c.Code,
                             IdStudent = s.IdBinusian,
                             NameStudent = (s.FirstName == null ? "" : s.FirstName) + (s.MiddleName == null ? "" : " " + s.MiddleName) + (s.LastName == null ? "" : " " + s.LastName),
                             Merit = sp == null ? 0 : sp.MeritPoint,
                             Demerit = sp == null ? 0 : sp.MeritPoint,
                             LevelOfInfraction = loi.IdParentLevelOfInteraction == null ? loi.NameLevelOfInteraction : loi.Parent.NameLevelOfInteraction,
                             LastUpdate = sp.DateUp,
                             Sunction = sc.SanctionName,
                         });

            if (!string.IsNullOrEmpty(param.IdHomeroom))
            {
                query = query.Where(e => e.IdHomeroom == param.IdHomeroom);
            }

            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
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
                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
                case "IdStudent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdStudent)
                        : query.OrderBy(x => x.IdStudent);
                    break;
                case "NameStudent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.NameStudent)
                        : query.OrderBy(x => x.NameStudent);
                    break;
                case "Merit":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Merit)
                        : query.OrderBy(x => x.Merit);
                    break;
                case "Demerit":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Demerit)
                        : query.OrderBy(x => x.Demerit);
                    break;
                case "LevelOfInfraction":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LevelOfInfraction)
                        : query.OrderBy(x => x.LevelOfInfraction);
                    break;
                case "Sunction":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Sunction)
                        : query.OrderBy(x => x.Sunction);
                    break;
                case "LastUpdate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LastUpdate)
                        : query.OrderBy(x => x.LastUpdate);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            var IsUsePointSystem = _dbContext.Entity<MsMeritDemeritComponentSetting>().SingleOrDefault(e => e.IdGrade == param.IdGrade).IsUsePointSystem;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                if (IsUsePointSystem)
                {
                    items = result.Select(x => new GetMeritDemeritTeacherResult
                    {
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        AcademicYear = x.AcademicYear,
                        Semester = x.Semester,
                        Level = x.Level,
                        Grade = x.Grade,
                        Homeroom = x.Homeroom,
                        IdStudent = x.IdStudent,
                        NameStudent = x.NameStudent,
                        Merit = x.Merit,
                        Demerit = x.Demerit,
                        LevelOfInfraction = x.LevelOfInfraction,
                        Sanction = x.Sunction,
                        LastUpdate = x.LastUpdate,

                    }).ToList();
                }
                else
                {
                    items = result.Select(x => new GetMeritDemeritTeacherResult
                    {
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        AcademicYear = x.AcademicYear,
                        Semester = x.Semester,
                        Level = x.Level,
                        Grade = x.Grade,
                        Homeroom = x.Homeroom,
                        IdStudent = x.IdStudent,
                        NameStudent = x.NameStudent,
                        LastUpdate = x.LastUpdate,
                    }).ToList();
                }
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                if (IsUsePointSystem)
                {
                    items = result.Select(x => new GetMeritDemeritTeacherResult
                    {
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        AcademicYear = x.AcademicYear,
                        Semester = x.Semester,
                        Level = x.Level,
                        Grade = x.Grade,
                        Homeroom = x.Homeroom,
                        IdStudent = x.IdStudent,
                        NameStudent = x.NameStudent,
                        Merit = x.Merit,
                        Demerit = x.Demerit,
                        LevelOfInfraction = x.LevelOfInfraction,
                        Sanction = x.Sunction,
                        LastUpdate = x.LastUpdate,

                    }).ToList();
                }
                else
                {
                    items = result.Select(x => new GetMeritDemeritTeacherResult
                    {
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        AcademicYear = x.AcademicYear,
                        Semester = x.Semester,
                        Level = x.Level,
                        Grade = x.Grade,
                        Homeroom = x.Homeroom,
                        IdStudent = x.IdStudent,
                        NameStudent = x.NameStudent,
                        LastUpdate = x.LastUpdate,
                    }).ToList();
                }
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdStudent).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMeritDemeritTeacherRequest, AddMeritDemeritTeacherValidator>();
            List<string> NoSaveSiswa = new List<string>();
            List<TrEntryMeritStudent> EntryMeritStudent = new List<TrEntryMeritStudent>();
            List<TrEntryDemeritStudent> EntryDemeritStudent = new List<TrEntryDemeritStudent>();
            List<TrStudentDemeritApprovalHs> HsDemeritStudent = new List<TrStudentDemeritApprovalHs>();
            List<TrStudentMeritApprovalHs> HsMeritStudent = new List<TrStudentMeritApprovalHs>();
            List<TrStudentPoint> StudentPoint = new List<TrStudentPoint>();
            List<AddMeritDemeritTeacherResult> MeritDemeritTeacherResult = new List<AddMeritDemeritTeacherResult>();

            var GetApproval = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
                  .Where(e => e.IdLevel == body.IdLevel)
                  .SingleOrDefaultAsync(CancellationToken);

            var UserApproval1 = await _dbContext.Entity<MsHomeroomTeacher>()
                .Where(e => e.IdTeacherPosition == GetApproval.Approval1)
                .SingleOrDefaultAsync(CancellationToken);

            var UserApproval2 = await _dbContext.Entity<MsHomeroomTeacher>()
                .Where(e => e.IdTeacherPosition == GetApproval.Approval2)
                .SingleOrDefaultAsync(CancellationToken);

            var UserApproval3 = await _dbContext.Entity<MsHomeroomTeacher>()
                .Where(e => e.IdTeacherPosition == GetApproval.Approval3)
                .SingleOrDefaultAsync(CancellationToken);

            var GetHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Where(e => body.MeritDemeritTeacher.Select(e => e.IdHomeroomStudent).Contains(e.Id))
                    .ToListAsync(CancellationToken);

            var GetLevelOfInteraction = _dbContext.Entity<MsLevelOfInteraction>()
                    .SingleOrDefault(e => e.Id == body.IdLevelInfraction).IdParentLevelOfInteraction == null
                    ? _dbContext.Entity<MsLevelOfInteraction>().Where(e => e.Id == body.IdLevelInfraction).Select(e => new { Id = e.Id, Name = e.NameLevelOfInteraction }).SingleOrDefault()
                    : _dbContext.Entity<MsLevelOfInteraction>().Where(e => e.Id == body.IdLevelInfraction).Select(e => new { Id = e.Id, Name = e.Parent.NameLevelOfInteraction }).SingleOrDefault();

            if (body.Category == 0) //Merit
            {
                #region Merit
                var GetScoreContinuationSetting = await _dbContext.Entity<MsScoreContinuationSetting>()
                     .Where(e => e.IdGrade == body.IdGrade && e.Category == (MeritDemeritCategory)0)
                     .SingleOrDefaultAsync(CancellationToken);

                var ResetMerit = GetScoreContinuationSetting == null
                       ? "Semester"
                       : GetScoreContinuationSetting.Category == (MeritDemeritCategory)0 && GetScoreContinuationSetting.ScoreContinueOption == (ScoreContinueOption)0
                       ? GetScoreContinuationSetting.ScoreContinueEvery.ToString() : "NoReset";

                var GetPoint = await _dbContext.Entity<TrStudentPoint>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                    .Where(e => body.MeritDemeritTeacher.Select(e => e.IdHomeroomStudent).Contains(e.Id))
                    .Select(e => new
                    {
                        Merit = e,
                        MeritPoint = ResetMerit == "Semester"
                            ? e.MeritPoint
                            : ResetMerit == "Academicyear"
                                ? e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear
                                    ? e.MeritPoint : e.MeritPoint - GetScoreContinuationSetting.Score
                                : e.MeritPoint - GetScoreContinuationSetting.Score,
                        IdStudent = e.IdHomeroomStudent,
                        LevelOfInfractin = e.LevelOfInteraction.IdParentLevelOfInteraction == null
                            ? e.LevelOfInteraction
                            : e.LevelOfInteraction.Parent,
                    })
                    .ToListAsync(CancellationToken);

                foreach (var ItemBodyMeritDemeritTeacher in body.MeritDemeritTeacher)
                {
                    var IdEntryMeritStudent = Guid.NewGuid().ToString();
                    var NewEntryMeritStudent = new TrEntryMeritStudent
                    {
                        Id = IdEntryMeritStudent,
                        IdHomeroomStudent = ItemBodyMeritDemeritTeacher.IdHomeroomStudent,
                        IdMeritDemeritMapping = body.IdMeritDemeritMapping,
                        Point = body.Point,
                        Note = ItemBodyMeritDemeritTeacher.Note,
                        RequestType = 0,
                        Status = "Waiting Approval (1)",
                    };
                    _dbContext.Entity<TrEntryMeritStudent>().Add(NewEntryMeritStudent);

                    var NewHsStudentMeritApproval = new TrStudentMeritApprovalHs
                    {
                        Id = IdEntryMeritStudent,
                        IdUserApproved1 = UserApproval1 == null ? null : UserApproval1.IdBinusian,
                        IdUserApproved2 = UserApproval1 == null ? null : UserApproval2.IdBinusian,
                        IdUserApproved3 = UserApproval1 == null ? null : UserApproval3.IdBinusian
                    };
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Add(NewHsStudentMeritApproval);

                    var IdStudent = GetHomeroomStudent.SingleOrDefault(e => e.Id == ItemBodyMeritDemeritTeacher.IdHomeroomStudent).IdStudent;
                    var GetPointByStudentId = GetPoint.SingleOrDefault(e => e.IdStudent == IdStudent);
                    if (GetPointByStudentId == null)
                    {
                        var NewStudentPoint = new TrStudentPoint
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroomStudent = IdStudent,
                            MeritPoint = GetPointByStudentId.MeritPoint == null ? 0 : Convert.ToInt32(GetPointByStudentId.MeritPoint),
                            IdSanctionMapping = Convert.ToInt32(GetLevelOfInteraction.Name) >= Convert.ToInt32(GetPointByStudentId.LevelOfInfractin) ? GetLevelOfInteraction.Id : GetPointByStudentId.LevelOfInfractin.NameLevelOfInteraction
                        };

                        _dbContext.Entity<TrStudentPoint>().Add(NewStudentPoint);
                    }
                    else
                    {
                        GetPointByStudentId.Merit.MeritPoint = GetPoint.SingleOrDefault(e => e.IdStudent == IdStudent).MeritPoint == null ? 0 : Convert.ToInt32(GetPoint.SingleOrDefault(e => e.IdStudent == IdStudent).MeritPoint + body.Point);
                        GetPointByStudentId.Merit.IdSanctionMapping = Convert.ToInt32(GetLevelOfInteraction.Name) >= Convert.ToInt32(GetPointByStudentId.LevelOfInfractin) ? GetLevelOfInteraction.Id : GetPointByStudentId.LevelOfInfractin.NameLevelOfInteraction;

                        _dbContext.Entity<TrStudentPoint>().Update(GetPointByStudentId.Merit);
                    }
                }
                #endregion
            }
            else // demerit
            {
                #region Demerit
                var GetNoApproval = await _dbContext.Entity<TrEntryDemeritStudent>()
                    .Where(e => body.MeritDemeritTeacher.Select(e => e.IdHomeroomStudent).Contains(e.IdHomeroomStudent) && e.IsHasBeenApproved == true && e.Status.Contains("Waiting Approval"))
                    .ToListAsync(CancellationToken);

                var GetLevelInfraction = await _dbContext.Entity<MsLevelOfInteraction>()
                    .Where(e => e.IdParentLevelOfInteraction == body.IdLevelInfraction)
                    .SingleOrDefaultAsync(CancellationToken);

                if (GetLevelInfraction == null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["LevelOfInfraction"], "Id", body.IdLevelInfraction));

                var GetScoreContinuationSetting = await _dbContext.Entity<MsScoreContinuationSetting>()
                    .Where(e => e.IdGrade == body.IdGrade && e.Category == (MeritDemeritCategory)1)
                    .SingleOrDefaultAsync(CancellationToken);

                var ResetDemerit = GetScoreContinuationSetting == null
                       ? "Semester"
                       : GetScoreContinuationSetting.Category == (MeritDemeritCategory)0 && GetScoreContinuationSetting.ScoreContinueOption == (ScoreContinueOption)0
                       ? GetScoreContinuationSetting.ScoreContinueEvery.ToString() : "NoReset";

                var GetPoint = await _dbContext.Entity<TrStudentPoint>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                    .Where(e => body.MeritDemeritTeacher.Select(e => e.IdHomeroomStudent).Contains(e.Id))
                    .Select(e => new
                    {
                        Demerit = e,
                        DemeritPoint = ResetDemerit == "Semester"
                            ? e.MeritPoint
                            : ResetDemerit == "Academicyear"
                                ? e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear
                                    ? e.MeritPoint : e.MeritPoint - GetScoreContinuationSetting.Score
                                : e.MeritPoint - GetScoreContinuationSetting.Score,
                        IdStudent = e.IdHomeroomStudent,
                        LevelOfInfractin = e.LevelOfInteraction.IdParentLevelOfInteraction == null
                            ? e.LevelOfInteraction
                            : e.LevelOfInteraction.Parent,
                    })
                    .ToListAsync(CancellationToken);

                foreach (var ItemBodyMeritDemeritTeacher in body.MeritDemeritTeacher)
                {
                    if (GetLevelInfraction.IsUseApproval)
                    {
                        var ExsisNoApprovalByStudent = GetNoApproval.Any(e => e.IdHomeroomStudent == ItemBodyMeritDemeritTeacher.IdHomeroomStudent);
                        if (ExsisNoApprovalByStudent)
                        {
                            NoSaveSiswa.Add(ItemBodyMeritDemeritTeacher.IdHomeroomStudent);
                        }
                        else
                        {
                            var IdEntryDemeritStudent = Guid.NewGuid().ToString();
                            var NewEntryDemeritStudent = new TrEntryDemeritStudent
                            {
                                Id = IdEntryDemeritStudent,
                                IdHomeroomStudent = ItemBodyMeritDemeritTeacher.IdHomeroomStudent,
                                IdMeritDemeritMapping = body.IdMeritDemeritMapping,
                                Point = body.Point,
                                Note = ItemBodyMeritDemeritTeacher.Note,
                                RequestType = 0,
                                Status = "Waiting Approval (1)",
                            };
                            _dbContext.Entity<TrEntryDemeritStudent>().Add(NewEntryDemeritStudent);

                            var NewHsStudentDemeritApproval = new TrStudentDemeritApprovalHs
                            {
                                Id = IdEntryDemeritStudent,
                                IdUserApproved1 = UserApproval1 == null ? null : UserApproval1.IdBinusian,
                                IdUserApproved2 = UserApproval1 == null ? null : UserApproval2.IdBinusian,
                                IdUserApproved3 = UserApproval1 == null ? null : UserApproval3.IdBinusian
                            };
                            _dbContext.Entity<TrStudentDemeritApprovalHs>().Add(NewHsStudentDemeritApproval);
                        }
                    }
                    else
                    {
                        var IdEntryDemeritStudent = Guid.NewGuid().ToString();
                        var NewEntryDemeritStudent = new TrEntryDemeritStudent
                        {
                            Id = IdEntryDemeritStudent,
                            IdHomeroomStudent = ItemBodyMeritDemeritTeacher.IdHomeroomStudent,
                            IdMeritDemeritMapping = body.IdMeritDemeritMapping,
                            Point = body.Point,
                            Note = ItemBodyMeritDemeritTeacher.Note,
                            RequestType = 0,
                            Status = "Waiting Approval (1)",
                        };
                        _dbContext.Entity<TrEntryDemeritStudent>().Add(NewEntryDemeritStudent);

                        var NewHsStudentDemeritApproval = new TrStudentDemeritApprovalHs
                        {
                            Id = IdEntryDemeritStudent,
                            IdUserApproved1 = UserApproval1 == null ? null : UserApproval1.IdBinusian,
                            IdUserApproved2 = UserApproval1 == null ? null : UserApproval2.IdBinusian,
                            IdUserApproved3 = UserApproval1 == null ? null : UserApproval3.IdBinusian
                        };
                        _dbContext.Entity<TrStudentDemeritApprovalHs>().Add(NewHsStudentDemeritApproval);

                        var IdStudent = GetHomeroomStudent.SingleOrDefault(e => e.Id == ItemBodyMeritDemeritTeacher.IdHomeroomStudent).IdStudent;
                        var GetPointByStudentId = GetPoint.SingleOrDefault(e => e.IdStudent == IdStudent);
                        if (GetPointByStudentId == null)
                        {
                            var NewStudentPoint = new TrStudentPoint
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdHomeroomStudent = IdStudent,
                                DemeritPoint = GetPoint.SingleOrDefault(e => e.IdStudent == IdStudent).DemeritPoint == null ? 0 : Convert.ToInt32(GetPoint.SingleOrDefault(e => e.IdStudent == IdStudent).DemeritPoint),
                                IdSanctionMapping = Convert.ToInt32(GetLevelOfInteraction.Name) >= Convert.ToInt32(GetPointByStudentId.LevelOfInfractin) ? GetLevelOfInteraction.Id : GetPointByStudentId.LevelOfInfractin.NameLevelOfInteraction
                            };

                            _dbContext.Entity<TrStudentPoint>().Add(NewStudentPoint);
                        }
                        else
                        {
                            GetPointByStudentId.Demerit.DemeritPoint = GetPoint.SingleOrDefault(e => e.IdStudent == IdStudent).DemeritPoint == null ? 0 : Convert.ToInt32(GetPoint.SingleOrDefault(e => e.IdStudent == IdStudent).DemeritPoint + body.Point);
                            GetPointByStudentId.Demerit.IdSanctionMapping = Convert.ToInt32(GetLevelOfInteraction.Name) >= Convert.ToInt32(GetPointByStudentId.LevelOfInfractin) ? GetLevelOfInteraction.Id : GetPointByStudentId.LevelOfInfractin.NameLevelOfInteraction;
                            _dbContext.Entity<TrStudentPoint>().Update(GetPointByStudentId.Demerit);
                        }
                    }
                }
                #endregion
            }

            var GetDataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(e => e.Student)
                    .Where(e => NoSaveSiswa.Contains(e.IdHomeroom))
                    .ToListAsync(CancellationToken);
            foreach (var ItemIdHomeroom in NoSaveSiswa)
            {
                var GetDataStudentById = GetDataStudent.SingleOrDefault(e => e.IdHomeroom == ItemIdHomeroom);

                MeritDemeritTeacherResult.Add(new AddMeritDemeritTeacherResult
                {
                    IdBinusan = GetDataStudentById.Student.Id,
                    Name = (GetDataStudentById.Student.FirstName == null ? "" : GetDataStudentById.Student.FirstName) + (GetDataStudentById.Student.MiddleName == null ? "" : " " + GetDataStudentById.Student.MiddleName) + (GetDataStudentById.Student.LastName == null ? "" : " " + GetDataStudentById.Student.LastName),
                    NameDisipline = body.NameMeritDemeritMapping,
                });
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2(MeritDemeritTeacherResult as object);
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {

            return Request.CreateApiResult2();
        }





    }
}
