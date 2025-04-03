using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class MeritDemeritTeacherHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;

        public MeritDemeritTeacherHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string IdStudent)
        {

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMeritDemeritTeacherRequest>();
            string[] _columns = { "AcademicYear", "Semester", "Level", "Grade", "Homeroom", "IdStudent", "NameStudent", "Merit", "Demerit", "LevelOfInfraction", "Sanction", "LastUpdate" };

            var listLessonByUser = await GetLessonByUser(param.IdUser, param.IdAcademicYear, param.PositionCode);
            var listIdLesson = listLessonByUser.Select(e => e.IdLesson).Distinct();

            var query = (from HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>()
                         join HomeroomEnrollment in _dbContext.Entity<MsHomeroomStudentEnrollment>() on HomeroomStudent.Id equals HomeroomEnrollment.IdHomeroomStudent
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                         join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                         join User in _dbContext.Entity<MsUser>() on Student.Id equals User.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join StudentPoint in _dbContext.Entity<TrStudentPoint>() on HomeroomStudent.Id equals StudentPoint.IdHomeroomStudent into JoinedStudentPoint
                         from StudentPoint in JoinedStudentPoint.DefaultIfEmpty()
                         join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on StudentPoint.IdLevelOfInteraction equals LevelOfInfraction.Id into JoinedLevelOfInteraction
                         from LevelOfInfraction in JoinedLevelOfInteraction.DefaultIfEmpty()
                         join SanctionMapping in _dbContext.Entity<MsSanctionMapping>() on StudentPoint.IdSanctionMapping equals SanctionMapping.Id into JoinedSanctionMapping
                         from SanctionMapping in JoinedSanctionMapping.DefaultIfEmpty()
                         where Level.IdAcademicYear == param.IdAcademicYear && listIdLesson.Contains(HomeroomEnrollment.IdLesson)
                         select new GetMeritDemeritTeacherResult
                         {
                             IdHomeroomStudent = HomeroomStudent.Id,
                             IdAcademicYear = AcademicYear.Id,
                             AcademicYear = AcademicYear.Description,
                             Semester = Homeroom.Semester.ToString(),
                             Level = Level.Description,
                             IdLevel = Level.Id,
                             Grade = Grade.Description,
                             IdGrade = Grade.Id,
                             Homeroom = (Grade.Code) + (Classroom.Code),
                             IdStudent = Student.Id,
                             NameStudent = (Student.FirstName == null ? string.Empty : Student.FirstName + " ") + (Student.MiddleName == null ? string.Empty : string.Empty + Student.MiddleName + " ") + (Student.LastName == null ? string.Empty : Student.LastName),
                             Demerit = StudentPoint != null ? StudentPoint.DemeritPoint : 0,
                             Merit = StudentPoint != null ? StudentPoint.MeritPoint : 0,
                             LevelOfInfraction = LevelOfInfraction.IdParentLevelOfInteraction == null ? LevelOfInfraction == null ? string.Empty : LevelOfInfraction.NameLevelOfInteraction : LevelOfInfraction.Parent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                             LastUpdate = StudentPoint.DateUp == null ? StudentPoint.DateIn.GetValueOrDefault() : StudentPoint.DateUp.GetValueOrDefault(),
                             Sanction = SanctionMapping != null ? SanctionMapping.SanctionName : string.Empty,
                             IdHomeroom = HomeroomStudent.IdHomeroom
                         });

            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                query = query.Where(x => x.Semester == param.Semester.ToString());
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.IdStudent.Contains(param.Search) || x.NameStudent.Contains(param.Search));

            List<GetMeritDemeritTeacherResult> listMeritDemerit = new List<GetMeritDemeritTeacherResult>();
            if (!string.IsNullOrEmpty(param.ScoreSetting))
            {
                if (param.ScoreSetting == "AcademicYear")
                {
                    var getMeritDemerit = await query.ToListAsync(CancellationToken);
                    var listIdStudent = getMeritDemerit.Select(e => e.IdStudent).Distinct().ToList();

                    foreach(var idStudent in listIdStudent)
                    {
                        var getMeritDemeritByIdStudent = getMeritDemerit.Where(e => e.IdStudent == idStudent).OrderBy(e => e.Semester).LastOrDefault();
                        listMeritDemerit.Add(getMeritDemeritByIdStudent);
                    }
                }

                if (param.ScoreSetting == "Semester")
                {
                    if (!string.IsNullOrEmpty(param.Semester.ToString()))
                    {
                        query = query.Where(x => x.Semester == param.Semester.ToString());
                        listMeritDemerit = await query.ToListAsync(CancellationToken);
                    }
                }
            }
            else
            {
                listMeritDemerit = await query.ToListAsync(CancellationToken);
            }

            var queryMeritDemerit = listMeritDemerit.Distinct();
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.AcademicYear)
                        : queryMeritDemerit.OrderBy(x => x.AcademicYear);
                    break;
                case "Level":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.Level)
                        : queryMeritDemerit.OrderBy(x => x.Level);
                    break;
                case "Grade":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.Grade)
                        : queryMeritDemerit.OrderBy(x => x.Grade);
                    break;
                case "Semester":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.Semester)
                        : queryMeritDemerit.OrderBy(x => x.Semester);
                    break;
                case "Homeroom":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.Homeroom)
                        : queryMeritDemerit.OrderBy(x => x.Homeroom);
                    break;
                case "IdStudent":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.IdStudent)
                        : queryMeritDemerit.OrderBy(x => x.IdStudent);
                    break;
                case "NameStudent":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.NameStudent)
                        : queryMeritDemerit.OrderBy(x => x.NameStudent);
                    break;
                case "Merit":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.Merit)
                        : queryMeritDemerit.OrderBy(x => x.Merit);
                    break;
                case "Demerit":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.Demerit)
                        : queryMeritDemerit.OrderBy(x => x.Demerit);
                    break;
                case "LevelOfInfraction":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.LevelOfInfraction)
                        : queryMeritDemerit.OrderBy(x => x.LevelOfInfraction);
                    break;
                case "Sanction":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.Sanction)
                        : queryMeritDemerit.OrderBy(x => x.Sanction);
                    break;
                case "LastUpdate":
                    queryMeritDemerit = param.OrderType == OrderType.Desc
                        ? queryMeritDemerit.OrderByDescending(x => x.LastUpdate)
                        : queryMeritDemerit.OrderBy(x => x.LastUpdate);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = queryMeritDemerit
                    .GroupBy(x => new
                    {
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        IdAcademicYear = x.IdAcademicYear,
                        AcademicYear = x.AcademicYear,
                        Semester = x.Semester,
                        IdLevel = x.IdLevel,
                        Level = x.Level,
                        Grade = x.Grade,
                        IdGrade = x.IdGrade,
                        Homeroom = x.Homeroom,
                        IdStudent = x.IdStudent,
                        NameStudent = x.NameStudent,
                        Merit = x.Merit,
                        Demerit = x.Demerit,
                        LevelOfInfraction = x.LevelOfInfraction,
                        Sanction = x.Sanction,
                        LastUpdate = x.LastUpdate,
                        IdHomeroom = x.IdHomeroom
                    })
                    .Select(x => new GetMeritDemeritTeacherResult
                    {
                        IdHomeroomStudent = x.Key.IdHomeroomStudent,
                        IdAcademicYear = x.Key.IdAcademicYear,
                        AcademicYear = x.Key.AcademicYear,
                        Semester = x.Key.Semester,
                        IdLevel = x.Key.IdLevel,
                        Level = x.Key.Level,
                        Grade = x.Key.Grade,
                        IdGrade = x.Key.IdGrade,
                        Homeroom = x.Key.Homeroom,
                        IdStudent = x.Key.IdStudent,
                        NameStudent = x.Key.NameStudent,
                        Merit = x.Key.Merit,
                        Demerit = x.Key.Demerit,
                        LevelOfInfraction = x.Key.LevelOfInfraction,
                        Sanction = x.Key.Sanction,
                        LastUpdate = x.Key.LastUpdate,
                        IdHomeroom = x.Key.IdHomeroom
                    })
                    .ToList();

                items = result.ToList();
            }
            else
            {
                var result = queryMeritDemerit
                    .GroupBy(x => new 
                    {
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        IdAcademicYear = x.IdAcademicYear,
                        AcademicYear = x.AcademicYear,
                        Semester = x.Semester,
                        IdLevel = x.IdLevel,
                        Level = x.Level,
                        Grade = x.Grade,
                        IdGrade = x.IdGrade,
                        Homeroom = x.Homeroom,
                        IdStudent = x.IdStudent,
                        NameStudent = x.NameStudent,
                        Merit = x.Merit,
                        Demerit = x.Demerit,
                        LevelOfInfraction = x.LevelOfInfraction,
                        Sanction = x.Sanction,
                        LastUpdate = x.LastUpdate,
                        IdHomeroom = x.IdHomeroom
                    })
                    .Select(x => new GetMeritDemeritTeacherResult
                    {
                        IdHomeroomStudent = x.Key.IdHomeroomStudent,
                        IdAcademicYear = x.Key.IdAcademicYear,
                        AcademicYear = x.Key.AcademicYear,
                        Semester = x.Key.Semester,
                        IdLevel = x.Key.IdLevel,
                        Level = x.Key.Level,
                        Grade = x.Key.Grade,
                        IdGrade = x.Key.IdGrade,
                        Homeroom = x.Key.Homeroom,
                        IdStudent = x.Key.IdStudent,
                        NameStudent = x.Key.NameStudent,
                        Merit = x.Key.Merit,
                        Demerit = x.Key.Demerit,
                        LevelOfInfraction = x.Key.LevelOfInfraction,
                        Sanction = x.Key.Sanction,
                        LastUpdate = x.Key.LastUpdate,
                        IdHomeroom = x.Key.IdHomeroom
                    })
                    .SetPagination(param)
                    .ToList();

                items = result.ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryMeritDemerit.Select(x => x.IdStudent).Distinct().Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMeritDemeritTeacherRequest, AddMeritDemeritTeacherValidator>();

            #region ValidationDoubleInputData
            var listStudentExist = new List<string>();
            var dataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Student)
                .Where(e => body.MeritDemeritTeacher.Select(x => x.IdHomeroomStudent).ToList().Contains(e.Id))
                .Select(x => new
                {
                    idHomeroomStudent = x.Id,
                    FullName = (x.Student.FirstName == null ? "" : x.Student.FirstName) + (x.Student.MiddleName == null ? "" : " " + x.Student.MiddleName) + (x.Student.LastName == null ? "" : " " + x.Student.LastName)
                })
               .ToListAsync(CancellationToken);

            if (body.Category == MeritDemeritCategory.Merit)
            {
                
                foreach (var item in body.MeritDemeritTeacher)
                {
                    var exitsData = await _dbContext.Entity<TrEntryMeritStudent>()
                                .Where(e => body.MeritDemeritTeacher.Select(x => x.IdHomeroomStudent).ToList().Contains(e.IdHomeroomStudent) &&
                                            e.IdMeritDemeritMapping == body.IdMeritDemeritMapping &&
                                            e.Point == body.Point &&
                                            e.Note == item.Note &&
                                            e.RequestType == RequestType.Create &&
                                            e.Status == "Approved" &&
                                            e.DateMerit == body.Date &&
                                            e.MeritUserCreate == AuthInfo.UserId &&
                                            e.Type == EntryMeritStudentType.Merit)
                                .AnyAsync(CancellationToken);

                    if (exitsData)
                    {
                        var fullName = dataStudent.Where(x => x.idHomeroomStudent == item.IdHomeroomStudent).Select(x=> x.FullName).FirstOrDefault();
                        listStudentExist.Add(fullName);
                    }
                }

                if (listStudentExist.Count > 0)
                {
                    var strName = string.Join(", ", listStudentExist);
                    throw new BadRequestException(string.Format($"Data is exist for student : {strName}"));

                }
            }
            else
            {

                foreach (var item in body.MeritDemeritTeacher)
                {
                    var exitsData = await _dbContext.Entity<TrEntryDemeritStudent>()
                                .Where(e => body.MeritDemeritTeacher.Select(x => x.IdHomeroomStudent).ToList().Contains(e.IdHomeroomStudent) &&
                                            e.IdMeritDemeritMapping == body.IdMeritDemeritMapping &&
                                            e.Point == body.Point &&
                                            e.Note == item.Note &&
                                            e.RequestType == RequestType.Create &&
                                            e.Status == "Approved" &&
                                            e.DemeritUserCreate == AuthInfo.UserId &&
                                            e.DateDemerit == body.Date)
                                .AnyAsync(CancellationToken);

                    if (exitsData)
                    {
                        var fullName = dataStudent.Where(x => x.idHomeroomStudent == item.IdHomeroomStudent).Select(x => x.FullName).FirstOrDefault();
                        listStudentExist.Add(fullName);
                    }
                }

                if (listStudentExist.Count > 0)
                {
                    var strName = string.Join(", ", listStudentExist);
                    throw new BadRequestException(string.Format($"Data is exist for student : {strName}"));

                }
            }
            #endregion

            List<string> NoSaveHomeroomStudent = new List<string>();
            List<TrEntryMeritStudent> EntryMeritStudent = new List<TrEntryMeritStudent>();
            List<TrEntryDemeritStudent> EntryDemeritStudent = new List<TrEntryDemeritStudent>();
            List<TrStudentPoint> StudentPoint = new List<TrStudentPoint>();
            List<AddMeritDemeritTeacherResult> MeritDemeritTeacherResult = new List<AddMeritDemeritTeacherResult>();

            var GetSanctionMapping = await _dbContext.Entity<MsSanctionMapping>()
                    .Where(e => e.IdAcademicYear == body.IdAcademicYear)
                    .ToListAsync(CancellationToken);

            var GetLevelInfraction = await _dbContext.Entity<MsLevelOfInteraction>()
                .Include(e => e.Parent)
                   .Where(e => e.Id == body.IdLevelInfraction)
                   .FirstOrDefaultAsync(CancellationToken);

            var GetApprovalSetting = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
               .Include(e => e.Level)
                 .Where(e => e.IdLevel == body.IdLevel && e.Level.IdAcademicYear == body.IdAcademicYear)
                 .FirstOrDefaultAsync(CancellationToken);

            var GetMeritDemeritComponentSetting = await _dbContext.Entity<MsMeritDemeritComponentSetting>()
              .Include(e => e.Grade).ThenInclude(e => e.MsLevel)
                .Where(e => e.Grade.IdLevel == body.IdLevel && e.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear && e.Grade.Id == body.IdGrade)
                .FirstOrDefaultAsync(CancellationToken);

            if (GetApprovalSetting == null && body.Category != MeritDemeritCategory.Merit)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ApprovalSetting"], "Id", body.IdLevel));

            List<GetUserGrade> idGrades = new List<GetUserGrade>();

            if (body.Category != MeritDemeritCategory.Merit)
            {
                var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                              .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                              .Include(e => e.User)
                              .Where(e =>
                                      e.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear &&
                                      !string.IsNullOrEmpty(e.Data) &&
                                      (e.MsNonTeachingLoad.TeacherPosition.Id == GetApprovalSetting.Approval1 ||
                                          e.MsNonTeachingLoad.TeacherPosition.Id == GetApprovalSetting.Approval2 ||
                                          e.MsNonTeachingLoad.TeacherPosition.Id == GetApprovalSetting.Approval3)
                                      )
                              .ToListAsync(CancellationToken);

                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal))
                {
                    if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).Count() > 0)
                    {
                        var Principal = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal).ToList();
                        List<string> IdLevels = new List<string>();
                        foreach (var item in Principal)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            if (_levelLH.Id == body.IdLevel)
                            {
                                idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == body.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                }).ToListAsync(CancellationToken));
                            }
                        }
                    }
                }

                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal))
                {
                    if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).Count() > 0)
                    {
                        var VicePrincipal = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal).ToList();
                        List<string> IdLevels = new List<string>();
                        foreach (var item in VicePrincipal)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            if (_levelLH.Id == body.IdLevel)
                            {
                                idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == body.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                }).ToListAsync(CancellationToken));
                            }
                        }
                    }
                }

                if (getPositionByUser.Any(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator))
                {
                    if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).ToList() != null && getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).Count() > 0)
                    {
                        var AffectiveCoordinator = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator).ToList();
                        List<string> IdLevels = new List<string>();
                        foreach (var item in AffectiveCoordinator)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            if (_levelLH.Id == body.IdLevel)
                            {
                                idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == body.IdLevel)
                                .Select(x => new GetUserGrade
                                {
                                    IdGrade = x.Id,
                                    Grade = x.Description,
                                    IdUser = item.User.Id,
                                    Fullname = item.User.DisplayName,
                                    codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                    IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                }).ToListAsync(CancellationToken));
                            }
                        }
                    }
                }

                if (getPositionByUser.Where(y => y.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead).ToList() != null)
                {
                    var LevelHead = getPositionByUser.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead).ToList();
                    foreach (var item in LevelHead)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Grade", out var _GradeLH);
                        if (_GradeLH.Id == body.IdGrade)
                        {
                            idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == body.IdLevel)
                            .Select(x => new GetUserGrade
                            {
                                IdGrade = x.Id,
                                Grade = x.Description,
                                IdUser = item.User.Id,
                                Fullname = item.User.DisplayName,
                                codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                            }).ToListAsync(CancellationToken));
                        }
                    }
                }
            }

            var approvalUserByGrade = idGrades.Where(e => e.IdGrade == body.IdGrade).Distinct().ToList();

            if (GetApprovalSetting != null)
            {
                if (body.Category != MeritDemeritCategory.Merit)
                {
                    if (!string.IsNullOrEmpty(GetApprovalSetting.Approval1))
                    {
                        if (!approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval1))
                            throw new BadRequestException(string.Format("Approval1 dont have user"));
                    }

                    if (!string.IsNullOrEmpty(GetApprovalSetting.Approval2))
                    {
                        if (!approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval2))
                            throw new BadRequestException(string.Format("Approval2 dont have user"));
                    }

                    if (!string.IsNullOrEmpty(GetApprovalSetting.Approval3))
                    {
                        if (!approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval3))
                            throw new BadRequestException(string.Format("Approval3 dont have user"));
                    }
                }
            }

            if (body.Category == MeritDemeritCategory.Merit)
            {
                #region Merit
                var GetPoint = await _dbContext.Entity<TrStudentPoint>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                    .Where(e => body.MeritDemeritTeacher.Select(e => e.IdHomeroomStudent).Contains(e.IdHomeroomStudent)
                        && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                    .Select(e => new
                    {
                        IdHomeroomStudent = e.IdHomeroomStudent,
                        LevelOfInfractin = e.LevelOfInteraction,
                        MeritPoint = e.MeritPoint,
                        Semester = e.HomeroomStudent.Homeroom.Semester,
                        Merit = e,
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
                        RequestType = RequestType.Create,
                        Status = "Approved",
                        DateMerit = body.Date,
                        MeritUserCreate = AuthInfo.UserId,
                        Type = EntryMeritStudentType.Merit
                    };
                    _dbContext.Entity<TrEntryMeritStudent>().Add(NewEntryMeritStudent);
                    EntryMeritStudent.Add(NewEntryMeritStudent);

                    var GetPointByStudentId = GetPoint.FirstOrDefault(e => e.IdHomeroomStudent == ItemBodyMeritDemeritTeacher.IdHomeroomStudent);
                    var MeritPoint = GetPointByStudentId == null ? body.Point : Convert.ToInt32(GetPointByStudentId.MeritPoint) + body.Point;
                    if (GetPointByStudentId == null)
                    {

                        var NewStudentPoint = new TrStudentPoint
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroomStudent = ItemBodyMeritDemeritTeacher.IdHomeroomStudent,
                            MeritPoint = MeritPoint,
                        };

                        _dbContext.Entity<TrStudentPoint>().Add(NewStudentPoint);
                    }
                    else
                    {
                        GetPointByStudentId.Merit.MeritPoint = MeritPoint;
                        _dbContext.Entity<TrStudentPoint>().Update(GetPointByStudentId.Merit);
                    }
                }
                #endregion
            }
            else // demerit
            {
                #region Demerit
                if (GetLevelInfraction == null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["LevelOfInfraction"], "Id", body.IdLevelInfraction));

                var GetNoApproval = await _dbContext.Entity<TrEntryDemeritStudent>()
                    .Where(e => body.MeritDemeritTeacher.Select(e => e.IdHomeroomStudent).Contains(e.IdHomeroomStudent) && e.IsHasBeenApproved == true && e.Status.Contains("Waiting Approval"))
                    .ToListAsync(CancellationToken);

                var GetPoint = await _dbContext.Entity<TrStudentPoint>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                    .Include(e => e.LevelOfInteraction).ThenInclude(e => e.Parent)
                    .Where(e => body.MeritDemeritTeacher.Select(e => e.IdHomeroomStudent).Contains(e.IdHomeroomStudent)
                        && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                    .Select(e => new
                    {
                        IdHomeroomStudent = e.IdHomeroomStudent,
                        LevelOfInfractin = e.LevelOfInteraction,
                        DemeritPoint = e.DemeritPoint,
                        Semester = e.HomeroomStudent.Homeroom.Semester,
                        Demerit = e
                    })
                    .ToListAsync(CancellationToken);

                foreach (var ItemBodyMeritDemeritTeacher in body.MeritDemeritTeacher)
                {
                    if (GetLevelInfraction.IsUseApproval)
                    {
                        var ExsisNoApprovalByStudent = GetNoApproval.Any(e => e.IdHomeroomStudent == ItemBodyMeritDemeritTeacher.IdHomeroomStudent);
                        if (ExsisNoApprovalByStudent)
                        {
                            NoSaveHomeroomStudent.Add(ItemBodyMeritDemeritTeacher.IdHomeroomStudent);
                        }
                        else
                        {
                            var NewEntryDemeritStudent = new TrEntryDemeritStudent
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdHomeroomStudent = ItemBodyMeritDemeritTeacher.IdHomeroomStudent,
                                IdMeritDemeritMapping = body.IdMeritDemeritMapping,
                                Point = body.Point,
                                Note = ItemBodyMeritDemeritTeacher.Note,
                                RequestType = RequestType.Create,
                                IsHasBeenApproved = true,
                                Status = "Waiting Approval (1)",
                                DateDemerit = body.Date,
                                DemeritUserCreate = AuthInfo.UserId
                            };
                            EntryDemeritStudent.Add(NewEntryDemeritStudent);
                            _dbContext.Entity<TrEntryDemeritStudent>().Add(NewEntryDemeritStudent);

                            var NewHsApproval = new TrStudentDemeritApprovalHs
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdUserApproved1 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval1) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval1).IdUser : null,
                                IdUserApproved2 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval2) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval2).IdUser : null,
                                IdUserApproved3 = approvalUserByGrade.Any(e => e.IdTeacherPosition == GetApprovalSetting.Approval3) ? approvalUserByGrade.FirstOrDefault(e => e.IdTeacherPosition == GetApprovalSetting.Approval3).IdUser : null,
                                RequestType = RequestType.Create,
                                Status = NewEntryDemeritStudent.Status,
                                IdTrEntryDemeritStudent = NewEntryDemeritStudent.Id
                            };

                            _dbContext.Entity<TrStudentDemeritApprovalHs>().Add(NewHsApproval);
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
                            RequestType = RequestType.Create,
                            Status = "Approved",
                            DateDemerit = body.Date,
                            DemeritUserCreate = AuthInfo.UserId
                        };
                        EntryDemeritStudent.Add(NewEntryDemeritStudent);
                        _dbContext.Entity<TrEntryDemeritStudent>().Add(NewEntryDemeritStudent);

                        var GetPointByStudentId = GetPoint.FirstOrDefault(e => e.IdHomeroomStudent == ItemBodyMeritDemeritTeacher.IdHomeroomStudent);
                        var DemeritPoint = GetPointByStudentId == null ? body.Point : Convert.ToInt32(GetPointByStudentId.DemeritPoint) + body.Point;
                        if (GetPointByStudentId == null)
                        {
                            var NewStudentPoint = new TrStudentPoint
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdHomeroomStudent = ItemBodyMeritDemeritTeacher.IdHomeroomStudent,
                                DemeritPoint = DemeritPoint,
                                IdSanctionMapping = GetSanctionMapping.Any(e => e.Min <= DemeritPoint && e.Max >= DemeritPoint) ? GetSanctionMapping.FirstOrDefault(e => e.Min <= DemeritPoint && e.Max >= DemeritPoint).Id : null,
                                IdLevelOfInteraction = body.IdLevelInfraction,
                            };
                            _dbContext.Entity<TrStudentPoint>().Add(NewStudentPoint);

                            if (NewStudentPoint.IdSanctionMapping != null)
                                StudentPoint.Add(NewStudentPoint);
                        }
                        else
                        {
                            var IdSanctionMappingOld = GetPointByStudentId.Demerit.IdSanctionMapping;
                            GetPointByStudentId.Demerit.DemeritPoint = DemeritPoint;
                            GetPointByStudentId.Demerit.IdSanctionMapping = GetSanctionMapping.Any(e => e.Min <= DemeritPoint && e.Max >= DemeritPoint) ? GetSanctionMapping.FirstOrDefault(e => e.Min <= DemeritPoint && e.Max >= DemeritPoint).Id : null;
                            GetPointByStudentId.Demerit.IdLevelOfInteraction = GetPointByStudentId.LevelOfInfractin == null
                                ? body.IdLevelInfraction
                                : GetLevelOfInfraction(GetPointByStudentId.LevelOfInfractin, GetLevelInfraction);
                            _dbContext.Entity<TrStudentPoint>().Update(GetPointByStudentId.Demerit);

                            if (IdSanctionMappingOld != GetPointByStudentId.Demerit.IdSanctionMapping)
                                StudentPoint.Add(GetPointByStudentId.Demerit);
                        }
                    }
                }

                #region feedback data student
                var GetDataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(e => e.Student)
                    .Where(e => body.MeritDemeritTeacher.Select(e => e.IdHomeroomStudent).Contains(e.Id))
                    .ToListAsync(CancellationToken);
                foreach (var ItemIdHomeroom in NoSaveHomeroomStudent)
                {
                    var GetDataStudentById = GetDataStudent.FirstOrDefault(e => e.Id == ItemIdHomeroom);

                    MeritDemeritTeacherResult.Add(new AddMeritDemeritTeacherResult
                    {
                        IdBinusan = GetDataStudentById.Student.Id,
                        Name = (GetDataStudentById.Student.FirstName == null ? "" : GetDataStudentById.Student.FirstName) + (GetDataStudentById.Student.MiddleName == null ? "" : " " + GetDataStudentById.Student.MiddleName) + (GetDataStudentById.Student.LastName == null ? "" : " " + GetDataStudentById.Student.LastName),
                        NameDisipline = body.NameMeritDemeritMapping,
                    });
                }
                #endregion

                #endregion
            }

            await _dbContext.SaveChangesAsync(CancellationToken);


            #region Send email
            if (body.Category == MeritDemeritCategory.Merit)
            {
                var GetMeritStudent = await (from EntMeritStudent in _dbContext.Entity<TrEntryMeritStudent>()
                                             join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntMeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                                             join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                                             join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                             join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                                             join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                                             join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                                             join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                                             join School in _dbContext.Entity<MsSchool>() on AcademicYear.IdSchool equals School.Id
                                             join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                             join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntMeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                                             join User in _dbContext.Entity<MsUser>() on EntMeritStudent.UserIn equals User.Id
                                             where EntryMeritStudent.Select(e => e.Id).ToList().Contains(EntMeritStudent.Id)
                                             select new
                                             {
                                                 Id = EntMeritStudent.Id,
                                                 IdStudent = Student.Id,
                                                 StudentName = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                                 Category = body.Category.ToString(),
                                                 DisciplineName = MeritDemeritMapping.DisciplineName,
                                                 Point = EntMeritStudent.Point.ToString(),
                                                 Note = EntMeritStudent.Note,
                                                 TeacherName = User.DisplayName,
                                                 TeachderId = User.Id,
                                                 CreateDate = Convert.ToDateTime(EntMeritStudent.DateMerit).ToString("dd MMM yyyy"),
                                                 SchoolName = School.Name,
                                             }).ToListAsync(CancellationToken);

                KeyValues.Add("GetMeritStudent", GetMeritStudent);

                var Notification = DS1Notification(KeyValues, AuthInfo);
            }
            else
            {
                var GetDemeritStudent = await (from EntDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>()
                                               join HsEntDemeritStudent in _dbContext.Entity<TrStudentDemeritApprovalHs>() on EntDemeritStudent.Id equals HsEntDemeritStudent.IdTrEntryDemeritStudent into JoinedHsEntDemeritStudent
                                               from HsEntDemeritStudent in JoinedHsEntDemeritStudent.DefaultIfEmpty()
                                               join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntDemeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                                               join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                                               join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                               join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                                               join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                                               join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                                               join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                                               join School in _dbContext.Entity<MsSchool>() on AcademicYear.IdSchool equals School.Id
                                               join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                               join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntDemeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                                               join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id into JoinedLevelOfInfraction
                                               from LevelOfInfraction in JoinedLevelOfInfraction.DefaultIfEmpty()
                                               join User in _dbContext.Entity<MsUser>() on EntDemeritStudent.UserIn equals User.Id
                                               where EntryDemeritStudent.Select(e => e.Id).ToList().Contains(EntDemeritStudent.Id)
                                               select new
                                               {
                                                   Id = EntDemeritStudent.Id,
                                                   IdStudent = Student.Id,
                                                   IdHomeroomStudent = HomeroomStudent.Id,
                                                   StudentName = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                                   Category = body.Category.ToString(),
                                                   LevelOfInfracton = LevelOfInfraction.IdParentLevelOfInteraction == null ? LevelOfInfraction.NameLevelOfInteraction : LevelOfInfraction.Parent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                                                   DisciplineName = MeritDemeritMapping.DisciplineName,
                                                   Point = EntDemeritStudent.Point,
                                                   Note = EntDemeritStudent.Note,
                                                   TeacherName = User.DisplayName,
                                                   TeachderId = User.Id,
                                                   CreateDate = Convert.ToDateTime(EntDemeritStudent.DateDemerit).ToString("dd MMM yyyy"),
                                                   RequestType = EntDemeritStudent.RequestType,
                                                   Status = EntDemeritStudent.Status,
                                                   SchoolName = School.Name,
                                                   IdAcademicYear = Level.IdAcademicYear,
                                                   IdHomeroom = Homeroom.Id,
                                                   IdLevel = Level.Id,
                                                   IdGrade = Grade.Id,
                                                   HsEntDemeritStudent = HsEntDemeritStudent
                                               }).ToListAsync(CancellationToken);

                if (GetLevelInfraction.IsUseApproval)
                {
                    List<EmailDemeritRequestApprovalResult> GetDemeritResult = new List<EmailDemeritRequestApprovalResult>();
                    foreach (var item in GetDemeritStudent)
                    {
                        foreach (var itemUser in approvalUserByGrade.Where(e => e.IdTeacherPosition == GetApprovalSetting.Approval1).ToList())
                        {
                            GetDemeritResult.Add(new EmailDemeritRequestApprovalResult
                            {
                                Id = item.HsEntDemeritStudent == null ? item.Id : item.HsEntDemeritStudent.Id,
                                IdStudent = item.IdStudent,
                                StudentName = item.StudentName,
                                Category = item.Category.ToString(),
                                DisciplineName = item.DisciplineName,
                                Point = item.Point.ToString(),
                                Note = item.Note,
                                TeacherName = item.TeacherName,
                                TeacherId = item.TeachderId,
                                CreateDate = item.CreateDate,
                                SchoolName = item.SchoolName,
                                LevelOfInfraction = item.LevelOfInfracton,
                                RequestType = item.RequestType.ToString(),
                                Status = item.Status,
                                IdUserApproval = itemUser.IdUser,
                                ApprovalName = itemUser.Fullname,
                                IdAcademicYear = body.IdAcademicYear,
                                IdGrade = body.IdGrade,
                                IdLevel = body.IdLevel,
                                IdHomeroomStudent = item.IdHomeroomStudent,
                            });
                        }
                    }

                    KeyValues.Add("GetDemeritStudent", GetDemeritResult);
                    var Notification = DS7Notification(KeyValues, AuthInfo);
                }
                else
                {
                    //for student
                    KeyValues.Add("GetDemeritStudent", GetDemeritStudent);
                    var NotificationStudent = DS5Notification(KeyValues, AuthInfo);

                    //for teacher/staff 
                    var GetSanctionMappingAttentionBy = await _dbContext.Entity<MsSanctionMappingAttentionBy>()
                          .Include(e => e.Role).ThenInclude(e => e.RoleGroup)
                          .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
                          .Where(e => StudentPoint.Select(e => e.IdSanctionMapping).Contains(e.IdSanctionMapping) && e.IdTeacherPosition != null)
                          .ToListAsync(CancellationToken);

                    var GetParent = await _dbContext.Entity<MsUser>()
                             .Where(e => GetDemeritStudent.Select(f => "P" + f.IdStudent).Contains(e.Username))
                             .ToListAsync(CancellationToken);

                    var GetHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                            .Where(e => StudentPoint.Select(e => e.HomeroomStudent.IdHomeroom).ToList().Contains(e.IdHomeroom))
                            .ToListAsync(CancellationToken);

                    var GetSubjetTeacher = await (from LessonTeacher in _dbContext.Entity<MsLessonTeacher>()
                                                  join Lesson in _dbContext.Entity<MsLesson>() on LessonTeacher.IdLesson equals Lesson.Id
                                                  join HomeroomStudentEnrollment in _dbContext.Entity<MsHomeroomStudentEnrollment>() on Lesson.Id equals HomeroomStudentEnrollment.IdLesson
                                                  where StudentPoint.Select(e => e.IdHomeroomStudent).ToList().Contains(HomeroomStudentEnrollment.IdHomeroomStudent)
                                                  select new GetTeacherSubject
                                                  {
                                                      IdHomeroomStudent = HomeroomStudentEnrollment.IdHomeroomStudent,
                                                      IdUserTeacher = LessonTeacher.IdUser
                                                  }).ToListAsync(CancellationToken);

                    var GetTeacherNonTeaching = await _dbContext.Entity<TrNonTeachingLoad>()
                                   .Include(e => e.MsNonTeachingLoad)
                                   .Include(e => e.User)
                                   .Where(x => x.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear)
                                   .ToListAsync(CancellationToken);

                    var getDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                         .Include(e => e.Department)
                                         .Where(e => e.Department.IdAcademicYear == body.IdAcademicYear && GetDemeritStudent.Select(e => e.IdLevel).ToList().Contains(e.IdLevel))
                                         .ToListAsync(CancellationToken);

                    var getGrade = await _dbContext.Entity<MsGrade>()
                                        .Include(e => e.MsLevel)
                                        .Where(e => e.MsLevel.IdAcademicYear == body.IdAcademicYear && GetDemeritStudent.Select(e => e.IdGrade).ToList().Contains(e.Id))
                                        .ToListAsync(CancellationToken);

                    List<EmailSanctionResult> GetSanction = new List<EmailSanctionResult>();
                    foreach (var item in StudentPoint)
                    {

                        var IdStudent = GetDemeritStudent.Any(e => e.IdHomeroomStudent == item.IdHomeroomStudent) ? GetDemeritStudent.Where(e => e.IdHomeroomStudent == item.IdHomeroomStudent).FirstOrDefault().IdStudent : "";
                        var idGrade = GetDemeritStudent.Any(e => e.IdHomeroomStudent == item.IdHomeroomStudent) ? GetDemeritStudent.Where(e => e.IdHomeroomStudent == item.IdHomeroomStudent).FirstOrDefault().IdGrade : "";
                        var Idlevel = GetDemeritStudent.Any(e => e.IdHomeroomStudent == item.IdHomeroomStudent) ? GetDemeritStudent.Where(e => e.IdHomeroomStudent == item.IdHomeroomStudent).FirstOrDefault().IdLevel : "";
                        var IdHomeroom = GetDemeritStudent.Any(e => e.IdHomeroomStudent == item.IdHomeroomStudent) ? GetDemeritStudent.Where(e => e.IdHomeroomStudent == item.HomeroomStudent.Id).FirstOrDefault().IdHomeroom : "";
                        var IdHomeroomStudent = GetDemeritStudent.Any(e => e.IdHomeroomStudent == item.IdHomeroomStudent) ? GetDemeritStudent.Where(e => e.IdHomeroomStudent == item.HomeroomStudent.Id).FirstOrDefault().IdHomeroomStudent : "";
                        var SanctionMapping = GetSanctionMapping.Any(e => e.Id == item.IdSanctionMapping) ? GetSanctionMapping.FirstOrDefault(e => e.Id == item.IdSanctionMapping) : null;
                        var msHomeroomStudnetById = await _dbContext.Entity<MsHomeroomStudent>().Include(x => x.Homeroom).ThenInclude(x => x.Grade).Where(x => x.Id == item.IdHomeroomStudent).FirstOrDefaultAsync(CancellationToken);

                        var UserAttandent = await MeritDemeritTeacherHandler.AttandentBy(KeyValues, AuthInfo, msHomeroomStudnetById, GetParent, GetSanctionMappingAttentionBy, GetHomeroomTeacher, GetSubjetTeacher, GetTeacherNonTeaching, idGrade, Idlevel, getDepartmentLevel, getGrade, _dbContext);

                        foreach (var itemUser in UserAttandent)
                        {
                            GetSanction.Add(new EmailSanctionResult
                            {
                                Id = item.Id,
                                IdStudent = IdStudent,
                                StudentName = GetDemeritStudent.Any(e => e.IdHomeroomStudent == item.IdHomeroomStudent) ? GetDemeritStudent.Where(e => e.IdHomeroomStudent == item.IdHomeroomStudent).FirstOrDefault().StudentName : "",
                                DemeritTotal = item.DemeritPoint.ToString(),
                                MeritTotal = item.MeritPoint.ToString(),
                                LevelOfInfraction = GetDemeritStudent.Any(e => e.IdHomeroomStudent == item.IdHomeroomStudent) ? GetDemeritStudent.Where(e => e.IdHomeroomStudent == item.HomeroomStudent.Id).FirstOrDefault().LevelOfInfracton : "",
                                Sanction = GetSanctionMapping.Any(e => e.Id == item.IdSanctionMapping) ? GetSanctionMapping.FirstOrDefault(e => e.Id == item.IdSanctionMapping).SanctionName : "",
                                LastUpdate = item.DateUp == null ? Convert.ToDateTime(item.DateIn).ToString("dd MMM yyyy") : Convert.ToDateTime(item.DateUp).ToString("dd MMM yyyy"),
                                IdUser = itemUser,
                                SchoolName = GetDemeritStudent.Any(e => e.IdHomeroomStudent == item.IdHomeroomStudent) ? GetDemeritStudent.Where(e => e.IdHomeroomStudent == item.IdHomeroomStudent).FirstOrDefault().SchoolName : "",
                                IdHomeroomStudent = item.IdHomeroomStudent,
                                IdAcadYear = body.IdAcademicYear,
                                IdLevel = body.IdLevel,
                                IdHomeroom = IdHomeroom,
                                IsPoint = GetMeritDemeritComponentSetting.IsUsePointSystem,
                                IdGrade = body.IdGrade,
                            });
                        }
                    }


                    KeyValues.Add("GetSanction", GetSanction);
                    var NotificationStaff = DS2Notification(KeyValues, AuthInfo);
                }
            }



            #endregion

            return Request.CreateApiResult2(MeritDemeritTeacherResult as object);
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {

            return Request.CreateApiResult2();
        }

        public static string GetLevelOfInfraction(MsLevelOfInteraction NewLevelPfInfraction, MsLevelOfInteraction OldLevelPfInfraction)
        {
            List<LevelOfInfractionResult> level = new List<LevelOfInfractionResult>();
            level.Add(new LevelOfInfractionResult
            {
                Id = NewLevelPfInfraction.Id,
                Parent = NewLevelPfInfraction.IdParentLevelOfInteraction == null
                ? Convert.ToInt32(NewLevelPfInfraction.NameLevelOfInteraction)
                : Convert.ToInt32(NewLevelPfInfraction.Parent.NameLevelOfInteraction),
                Description = NewLevelPfInfraction.IdParentLevelOfInteraction == null
                ? NewLevelPfInfraction.NameLevelOfInteraction + "-"
                : NewLevelPfInfraction.Parent.NameLevelOfInteraction + NewLevelPfInfraction.NameLevelOfInteraction,
            });

            level.Add(new LevelOfInfractionResult
            {
                Id = OldLevelPfInfraction.Id,
                Parent = NewLevelPfInfraction.IdParentLevelOfInteraction == null
                ? Convert.ToInt32(NewLevelPfInfraction.NameLevelOfInteraction)
                : Convert.ToInt32(NewLevelPfInfraction.Parent.NameLevelOfInteraction),
                Description = OldLevelPfInfraction.IdParentLevelOfInteraction == null
                ? OldLevelPfInfraction.NameLevelOfInteraction + "-"
                : OldLevelPfInfraction.Parent.NameLevelOfInteraction + OldLevelPfInfraction.NameLevelOfInteraction,
            });

            var IdNewLevelOfInfraction = level.OrderBy(x => x.Parent).ThenBy(e => e.Description).LastOrDefault().Id;

            return IdNewLevelOfInfraction;
        }

        public static string DS1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var ObjectMeritDemerit = KeyValues.FirstOrDefault(e => e.Key == "GetMeritStudent").Value;
            var GetMeritDemerit = JsonConvert.DeserializeObject<List<EmailMeritDemeritApprovalResult>>(JsonConvert.SerializeObject(ObjectMeritDemerit));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "DS1")
                {
                    IdRecipients = GetMeritDemerit.Select(e => e.IdStudent),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return "";
        }

        public static string DS2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {

            var ObjectSanction = KeyValues.FirstOrDefault(e => e.Key == "GetSanction").Value;
            var GetSanction = JsonConvert.DeserializeObject<List<EmailSanctionResult>>(JsonConvert.SerializeObject(ObjectSanction));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "DS2")
                {
                    IdRecipients = GetSanction.Select(e => e.IdUser),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return "";
        }

        public static string DS3Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var ObjectSanction = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetSanction = JsonConvert.DeserializeObject<List<EmailMeritDemeritApprovalResult>>(JsonConvert.SerializeObject(ObjectSanction));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "DS3")
                {
                    IdRecipients = GetSanction.Select(e => e.TeacherId),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return "";
        }

        public static string DS4Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {

            var ObjectDemeritStudent = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetDemeritStudent = JsonConvert.DeserializeObject<List<EmailDemeritRequestApprovalResult>>(JsonConvert.SerializeObject(ObjectDemeritStudent));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "DS4")
                {
                    IdRecipients = GetDemeritStudent.Select(e => e.TeacherId),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string DS5Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {

            var ObjectMeritDemerit = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetMeritDemerit = JsonConvert.DeserializeObject<List<EmailMeritDemeritApprovalResult>>(JsonConvert.SerializeObject(ObjectMeritDemerit));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "DS5")
                {
                    IdRecipients = GetMeritDemerit.Select(e => e.IdStudent),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return "";
        }

        public static string DS6Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var ObjectMeritDemerit = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetMeritDemerit = JsonConvert.DeserializeObject<List<EmailDemeritRequestApprovalResult>>(JsonConvert.SerializeObject(ObjectMeritDemerit));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "DS6")
                {
                    IdRecipients = GetMeritDemerit.Select(e => e.IdStudent).ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return "";
        }

        public static string DS7Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var ObjectMeritDemerit = KeyValues.FirstOrDefault(e => e.Key == "GetDemeritStudent").Value;
            var GetMeritDemerit = JsonConvert.DeserializeObject<List<EmailDemeritRequestApprovalResult>>(JsonConvert.SerializeObject(ObjectMeritDemerit));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "DS7")
                {
                    IdRecipients = GetMeritDemerit.Select(e => e.IdUserApproval).ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static async Task<List<string>> UserByTeacherPosition(MsTeacherPosition teacherCode, IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, MsHomeroomStudent HomeroomStudent, List<MsUser> GetParent, List<MsSanctionMappingAttentionBy> GetSanctionMappingAttentionBy, List<MsHomeroomTeacher> GetHomeroomTeacher, List<GetTeacherSubject> GetSubjetTeacher, List<TrNonTeachingLoad> GetTeacherNonTeaching, string idGrade, string IdLevel, List<MsDepartmentLevel> getDepartmentLevel, List<MsGrade> getGrade)
        {
            var IdUser = new List<string>();

            if (PositionConstant.ClassAdvisor == teacherCode.Code || PositionConstant.CoTeacher == teacherCode.Code)
            {
                var IdUserHomeroomTeacher = GetHomeroomTeacher
                                                .Where(e => e.IdHomeroom == HomeroomStudent.IdHomeroom && e.IdTeacherPosition == teacherCode.Id)
                                                .Select(e => e.IdBinusian).ToList();

                IdUser.AddRange(IdUserHomeroomTeacher);
            }
            else if (PositionConstant.SubjectTeacher == teacherCode.Code)
            {
                var IdUserSubjectTeacher = GetSubjetTeacher
                                            .Where(e => e.IdHomeroomStudent == HomeroomStudent.Id)
                                            .Select(e => e.IdUserTeacher).ToList();
                IdUser.AddRange(IdUserSubjectTeacher);
            }
            else
            {
                List<GetUserGrade> idGrades = new List<GetUserGrade>();

                if (PositionConstant.Principal == teacherCode.Code
                        || PositionConstant.VicePrincipal == teacherCode.Code
                        || PositionConstant.AffectiveCoordinator == teacherCode.Code)
                {
                    var Principal = GetTeacherNonTeaching.Where(x => x.MsNonTeachingLoad.IdTeacherPosition == teacherCode.Id).ToList();
                    List<string> IdLevels = new List<string>();
                    foreach (var item in Principal)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Level", out var _levelLH);
                        if (_levelLH.Id == IdLevel)
                        {
                            idGrades.AddRange(getGrade.Where(x => x.IdLevel == IdLevel)
                            .Select(x => new GetUserGrade
                            {
                                IdGrade = x.Id,
                                Grade = x.Description,
                                IdUser = item.User.Id,
                                Fullname = item.User.DisplayName,
                                codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                            }).ToList());
                        }
                    }
                }
                else if (PositionConstant.LevelHead == teacherCode.Code)
                {
                    var Principal = GetTeacherNonTeaching.Where(x => x.MsNonTeachingLoad.TeacherPosition.Id == teacherCode.Id).ToList();
                    List<string> IdLevels = new List<string>();
                    foreach (var item in Principal)
                    {
                        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewLH.TryGetValue("Grade", out var _GradeLH);
                        if (_GradeLH.Id == HomeroomStudent.Homeroom.IdGrade)
                        {
                            idGrades.AddRange(getGrade.Where(x => x.IdLevel == IdLevel)
                            .Select(x => new GetUserGrade
                            {
                                IdGrade = x.Id,
                                Grade = x.Description,
                                IdUser = item.User.Id,
                                Fullname = item.User.DisplayName,
                                codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                            }).ToList());
                        }
                    }
                }
                else
                {
                    var GetTeacherNonTeachingByIdTeacherPosition = GetTeacherNonTeaching.Where(x => x.MsNonTeachingLoad.IdTeacherPosition == teacherCode.Id).ToList();
                    foreach (var item in GetTeacherNonTeachingByIdTeacherPosition)
                    {
                        List<GetUserGrade> idGradesEachPosition = new List<GetUserGrade>();

                        var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewPosition.TryGetValue("Departemen", out var _DepartemenPosition);
                        if (_DepartemenPosition != null)
                        {
                            var getDepartmentLevelbyIdLevel = getDepartmentLevel.Any(e => e.IdDepartment == _DepartemenPosition.Id && e.IdLevel == IdLevel);
                            if (getDepartmentLevelbyIdLevel)
                            {
                                idGrades.AddRange(getGrade.Where(x => x.IdLevel == IdLevel)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    }).ToList());
                            }
                        }


                        //ByGrade or Level
                        _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                        _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                        if (_GradePosition == null)
                        {
                            if (_LevelPosition != null)
                            {
                                if (_LevelPosition.Id == IdLevel)
                                {
                                    idGrades.AddRange(getGrade.Where(x => x.IdLevel == IdLevel)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    }).ToList());
                                }
                            }
                        }
                        else
                        {
                            if (_GradePosition != null)
                            {
                                if (_GradePosition.Id == idGrade)
                                {
                                    idGrades.AddRange(getGrade.Where(x => x.Id == idGrade)
                                    .Select(x => new GetUserGrade
                                    {
                                        IdGrade = x.Id,
                                        Grade = x.Description,
                                        IdUser = item.User.Id,
                                        Fullname = item.User.DisplayName,
                                        codePosition = item.MsNonTeachingLoad.TeacherPosition.Code,
                                        IdTeacherPosition = item.MsNonTeachingLoad.TeacherPosition.Id,
                                    }).ToList());
                                }
                            }
                        }

                    }
                }

                IdUser.AddRange(idGrades.Where(e => e.IdGrade == HomeroomStudent.Homeroom.IdGrade).Select(e => e.IdUser).ToList());
            }

            return IdUser;
        }

        public static async Task<List<string>> AttandentBy(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, MsHomeroomStudent HomeroomStudent, List<MsUser> GetParent, List<MsSanctionMappingAttentionBy> GetSanctionMappingAttentionBy, List<MsHomeroomTeacher> GetHomeroomTeacher, List<GetTeacherSubject> GetSubjetTeacher, List<TrNonTeachingLoad> GetTeacherNonTeaching, string idGrade, string IdLevel, List<MsDepartmentLevel> getDepartmentLevel, List<MsGrade> getGrade, IStudentDbContext _dbContext)
        {

            List<string> IdUser = new List<string>();

            // Sanction by id User
            var GetAttandentByFromIdUser = GetSanctionMappingAttentionBy.Where(e => e.IdUser != null).ToList();
            IdUser.AddRange(GetAttandentByFromIdUser.Select(e => e.IdUser).ToList());

            // Sanction by id Teacher Position 
            var GetAttandentByFromIdPosition = GetSanctionMappingAttentionBy.Where(e => e.IdUser == null).ToList();
            foreach (var itemPosition in GetAttandentByFromIdPosition)
            {
                if (RoleConstant.Teacher == itemPosition.Role.RoleGroup.Code)
                {
                    var idSchool = _dbContext.Entity<MsLevel>().Include(x => x.MsAcademicYear).FirstOrDefault(x => x.Id == IdLevel)?.MsAcademicYear.IdSchool;

                    if (itemPosition.TeacherPosition == null)
                    {
                        var trRolePosition = await _dbContext.Entity<TrRolePosition>().Where(x => x.IdRole == itemPosition.IdRole).ToListAsync();

                        foreach (var itemRolePosition in trRolePosition)
                        {
                            var teacherPosition = await _dbContext.Entity<MsTeacherPosition>().FirstOrDefaultAsync(x => x.Id == itemRolePosition.IdTeacherPosition && x.IdSchool == idSchool);

                            IdUser.AddRange(await UserByTeacherPosition(teacherPosition, KeyValues, AuthInfo, HomeroomStudent, GetParent, GetSanctionMappingAttentionBy, GetHomeroomTeacher, GetSubjetTeacher, GetTeacherNonTeaching, idGrade, IdLevel, getDepartmentLevel, getGrade));
                        }

                    }
                    else
                    {
                        IdUser.AddRange(await UserByTeacherPosition(itemPosition.TeacherPosition, KeyValues, AuthInfo, HomeroomStudent, GetParent, GetSanctionMappingAttentionBy, GetHomeroomTeacher, GetSubjetTeacher, GetTeacherNonTeaching, idGrade, IdLevel, getDepartmentLevel, getGrade));
                    }

                    
                }
                else if (RoleConstant.Teacher == itemPosition.Role.RoleGroup.Code)
                {
                    IdUser.AddRange(GetParent.Select(e => e.Id).ToList());
                }
                else
                {
                    var IdUserStaff = GetTeacherNonTeaching.Where(e => e.MsNonTeachingLoad.IdTeacherPosition == itemPosition.IdTeacherPosition).Select(e => e.IdUser).ToList();
                    IdUser.AddRange(IdUserStaff);
                }
            }

            return IdUser.Distinct().ToList();
        }

        private async Task<IReadOnlyList<LessonByUser>> GetLessonByUser(string IdUser, string IdAcademicYear, string PositionCode)
        {
            List<LessonByUser> listLessonByUser = new List<LessonByUser>();

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                                   .Where(x => x.Id == IdAcademicYear)
                                   .Select(e => e.IdSchool)
                                   .FirstOrDefaultAsync(CancellationToken);

            var listTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                       .Include(e => e.Position)
                                       .Where(x => x.IdSchool == idSchool)
                                       .Select(e => new
                                       {
                                           Id = e.Id,
                                           PositionCode = e.Position.Code,
                                       })
                                       .ToListAsync(CancellationToken);

            var listLesson = await _dbContext.Entity<MsLesson>()
                                   .Include(e => e.Grade).ThenInclude(e => e.MsLevel)
                                   .Where(x => x.Grade.MsLevel.IdAcademicYear == IdAcademicYear)
                                   .Select(e => new
                                   {
                                       IdLevel = e.Grade.IdLevel,
                                       IdGrade = e.IdGrade,
                                       IdLesson = e.Id,
                                       IdSubject = e.IdSubject
                                   })
                                   .ToListAsync(CancellationToken);

            if (PositionCode == "All")
            {
                var listLessonByStaff = listLesson
                        .Select(e => new LessonByUser
                        {
                            IdLevel = e.IdLevel,
                            IdGrade = e.IdGrade,
                            IdLesson = e.IdLesson,
                            PositionCode = "All"
                        })
                        .ToList();
                listLessonByUser.AddRange(listLessonByStaff);
            }
            else
            {
                #region CA
                var listHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                   .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
                   .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                   .Where(x => x.IdBinusian == IdUser && x.Homeroom.Grade.MsLevel.IdAcademicYear == IdAcademicYear)
                   .Select(e => new
                   {
                       e.IdHomeroom,
                       PositionCode = e.TeacherPosition.Position.Code
                   })
                   .Distinct().ToListAsync(CancellationToken);

                var listIdHomeroom = listHomeroomTeacher.Select(e => e.IdHomeroom).ToList();

                var listLessonByCa = await _dbContext.Entity<MsLessonPathway>()
                   .Include(e => e.Lesson).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                   .Include(e => e.HomeroomPathway)
                   .Where(x => listIdHomeroom.Contains(x.HomeroomPathway.IdHomeroom))
                   .Select(e => new
                   {
                       IdLevel = e.Lesson.Grade.IdLevel,
                       IdGrade = e.Lesson.IdGrade,
                       IdLesson = e.IdLesson,
                       IdHomeroom = e.HomeroomPathway.IdHomeroom
                   })
                   .Distinct().ToListAsync(CancellationToken);

                foreach (var itemHomeroomTeacher in listHomeroomTeacher)
                {
                    var listLessonCaByHomeroom = listLessonByCa
                        .Where(e => e.IdHomeroom == itemHomeroomTeacher.IdHomeroom)
                        .Select(e => new LessonByUser
                        {
                            IdLevel = e.IdLevel,
                            IdGrade = e.IdGrade,
                            IdLesson = e.IdLesson,
                            PositionCode = itemHomeroomTeacher.PositionCode
                        })
                        .ToList();
                    listLessonByUser.AddRange(listLessonCaByHomeroom);
                }
                #endregion

                #region ST
                var positionCodeBySubjectTeacher = listTeacherPosition
                                                        .Where(e => e.PositionCode == PositionConstant.SubjectTeacher)
                                                        .Select(e => e.PositionCode)
                                                        .Distinct()
                                                        .ToList();

                var listLessonBySt = await _dbContext.Entity<MsLessonTeacher>()
                                        .Include(e => e.Lesson).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                       .Where(x => x.IdUser == IdUser && x.Lesson.IdAcademicYear == IdAcademicYear)
                                       .Select(e => new LessonByUser
                                       {
                                           IdGrade = e.Lesson.IdGrade,
                                           IdLevel = e.Lesson.Grade.IdLevel,
                                           IdLesson = e.IdLesson,
                                       })
                                       .Distinct()
                                       .ToListAsync(CancellationToken);

                foreach (var itemPositionCode in positionCodeBySubjectTeacher)
                {
                    listLessonBySt.ForEach(d => d.PositionCode = itemPositionCode);
                    listLessonByUser.AddRange(listLessonBySt);
                }

                #endregion

                #region non teaching load
                var listTeacherNonTeaching = await _dbContext.Entity<TrNonTeachingLoad>()
                                    .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                     .Where(x => x.IdUser == IdUser && x.MsNonTeachingLoad.IdAcademicYear == IdAcademicYear)
                                     .ToListAsync(CancellationToken);

                var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                    .Include(e => e.Level).ThenInclude(e => e.MsGrades)
                                     .Where(x => x.Level.IdAcademicYear == IdAcademicYear)
                                     .ToListAsync(CancellationToken);

                foreach (var item in listTeacherNonTeaching)
                {
                    var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                    _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                    _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                    _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                    _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
                    if (_SubjectPosition == null && _GradePosition == null && _LevelPosition == null && _DepartemenPosition != null)
                    {
                        var getDepartmentLevelbyIdLevel = listDepartmentLevel.Where(e => e.IdDepartment == _DepartemenPosition.Id).ToList();

                        foreach (var itemDepartement in getDepartmentLevelbyIdLevel)
                        {
                            var listGrade = itemDepartement.Level.MsGrades.ToList();

                            foreach (var itemGrade in listGrade)
                            {
                                var listLessonByIdGarde = listLesson.Where(e => e.IdGrade == itemGrade.Id).ToList();

                                foreach (var itemLesson in listLessonByIdGarde)
                                {
                                    LessonByUser newSubjectTeacher = new LessonByUser
                                    {
                                        IdGrade = itemLesson.IdGrade,
                                        IdLevel = itemLesson.IdLevel,
                                        IdLesson = itemLesson.IdLesson,
                                        PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                                    };

                                    listLessonByUser.Add(newSubjectTeacher);
                                }
                            }
                        }

                    }
                    else if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null && _DepartemenPosition != null)
                    {
                        var listLessonByIdSubject = listLesson.Where(e => e.IdSubject == _SubjectPosition.Id).ToList();

                        foreach (var itemSubject in listLessonByIdSubject)
                        {
                            LessonByUser newSubjectTeacher = new LessonByUser
                            {
                                IdGrade = itemSubject.IdGrade,
                                IdLevel = itemSubject.IdLevel,
                                IdLesson = itemSubject.IdLesson,
                                PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                            };

                            listLessonByUser.Add(newSubjectTeacher);
                        }

                    }
                    else if (_SubjectPosition == null && _GradePosition != null && _LevelPosition != null)
                    {
                        var listLessonByIdGrade = listLesson.Where(e => e.IdGrade == _GradePosition.Id).ToList();

                        foreach (var itemSubject in listLessonByIdGrade)
                        {
                            LessonByUser newSubjectTeacher = new LessonByUser
                            {
                                IdGrade = itemSubject.IdGrade,
                                IdLevel = itemSubject.IdLevel,
                                IdLesson = itemSubject.IdLesson,
                                PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                            };

                            listLessonByUser.Add(newSubjectTeacher);
                        }
                    }
                    else if (_SubjectPosition == null && _GradePosition == null && _LevelPosition != null)
                    {
                        var listLessonByIdLevel = listLesson.Where(e => e.IdLevel == _LevelPosition.Id).ToList();

                        foreach (var itemSubject in listLessonByIdLevel)
                        {
                            LessonByUser newSubjectTeacher = new LessonByUser
                            {
                                IdGrade = itemSubject.IdGrade,
                                IdLevel = itemSubject.IdLevel,
                                IdLesson = itemSubject.IdLesson,
                                PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                            };

                            listLessonByUser.Add(newSubjectTeacher);
                        }
                    }
                }
                #endregion
            }


            if (!string.IsNullOrEmpty(PositionCode))
            {
                listLessonByUser = listLessonByUser.Where(e => e.PositionCode == PositionCode).ToList();
            }

            return listLessonByUser;
        }

        public class LessonByUser
        {
            public string IdLevel { get; set; }
            public string IdGrade { get; set; }
            public string IdLesson { get; set; }
            public string PositionCode { get; set; }
        }
    }
}
