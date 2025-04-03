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
using BinusSchool.Data.Model.Student.FnStudent.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemerit.Validator;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class ApprovalMeritDemeritTeacherHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;

        public ApprovalMeritDemeritTeacherHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetApprovalMeritDemeritTeacherRequest>();
            string[] _columns = { "AcademicYear", "Semester", "Level", "Grade", "Homeroom", "IdStudent", "NameStudent", "Category", "LevelOfInfraction", "NameDecipline", "Point", "Note", "CreateBy", "RequestType", "Reason", "Status" };

            var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                                            .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                            .Where(e => e.IdUser == param.IdUser &&
                                                    e.MsNonTeachingLoad.IdAcademicYear == param.IdAcademiYear &&
                                                    !string.IsNullOrEmpty(e.Data) &&
                                                    (e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead ||
                                                        e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal ||
                                                        e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal ||
                                                        e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator)
                                                    )
                                            .ToListAsync(CancellationToken);

            if (getPositionByUser == null)
                throw new Exception();
            List<ItemValueVm> idGrades = new List<ItemValueVm>();

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
                        if (_levelLH.Id == param.IdLevel)
                        {
                            idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Id,
                                Description = x.Description
                            }).ToListAsync(CancellationToken);
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
                        if (_levelLH.Id == param.IdLevel)
                        {
                            idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Id,
                                Description = x.Description
                            }).ToListAsync(CancellationToken);
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
                        if (_levelLH.Id == param.IdLevel)
                        {
                            idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Id,
                                Description = x.Description
                            }).ToListAsync(CancellationToken);
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
                    if (_GradeLH.Id == param.IdGrade)
                    {
                        idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == param.IdLevel)
                        .Select(x => new ItemValueVm
                        {
                            Id = x.Id,
                            Description = x.Description,
                        }).ToListAsync(CancellationToken));
                    }
                }
            }

            var _idGrades = idGrades.Select(e => e.Id).Distinct().ToList();
            var _idTeacherPositionByUser = getPositionByUser.Select(e => e.MsNonTeachingLoad.IdTeacherPosition).Distinct().ToList();

            var query = (from HsEntryMeritStudent in _dbContext.Entity<TrStudentMeritApprovalHs>()
                         join EntryMeritStudent in _dbContext.Entity<TrEntryMeritStudent>() on HsEntryMeritStudent.IdTrEntryMeritStudent equals EntryMeritStudent.Id
                         join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntryMeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                         join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntryMeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                         join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id into JoinedLevelInfractin
                         from LevelOfInfraction in JoinedLevelInfractin.DefaultIfEmpty()
                         join User in _dbContext.Entity<MsUser>() on EntryMeritStudent.MeritUserCreate equals User.Id
                         join ApprovalSetting in _dbContext.Entity<MsMeritDemeritApprovalSetting>() on Level.Id equals ApprovalSetting.IdLevel
                         where AcademicYear.Id == param.IdAcademiYear 
                                && EntryMeritStudent.IsHasBeenApproved == true 
                                && _idGrades.Contains(Grade.Id)
                                && EntryMeritStudent.Type==EntryMeritStudentType.Merit
                         select new
                         {
                             Id = HsEntryMeritStudent.Id,
                             AcademicYear = AcademicYear.Description,
                             IdAcademicYear = AcademicYear.Id,
                             Semester = Homeroom.Semester.ToString(),
                             Level = Level.Description,
                             IdLevel = Level.Id,
                             Grade = Grade.Description,
                             IdGrade = Grade.Id,
                             Homeroom = (Grade.Code) + (Classroom.Code),
                             IdHomeroom = Homeroom.Id,
                             IdStudent = Student.Id,
                             NameStudent = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                             Category = MeritDemeritMapping.Category,
                             LevelOfInfraction = LevelOfInfraction != null ?LevelOfInfraction.NameLevelOfInteraction : string.Empty,
                             NameDecipline = MeritDemeritMapping.DisciplineName,
                             Point = EntryMeritStudent.Point,
                             Note = EntryMeritStudent.Note,
                             CreateBy = User.DisplayName,
                             RequestType = HsEntryMeritStudent.RequestType,
                             Reason = HsEntryMeritStudent.RequestReason,
                             Status = HsEntryMeritStudent.Status,
                             IdHomeroomStudent = HomeroomStudent.Id,
                             Approval1 = ApprovalSetting.Approval1,
                             IsShowButtonApproval = HsEntryMeritStudent.Status == "Waiting Approval (1)"
                                                    ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval1)
                                                        ? true : false
                                                    : HsEntryMeritStudent.Status == "Waiting Approval (2)"
                                                        ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval2)
                                                            ? true : false
                                                    : HsEntryMeritStudent.Status == "Waiting Approval (3)"
                                                        ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval3)
                                                            ? true : false
                                                    : false,
                         })
                         .Union(from HsEntryDemeritStudent in _dbContext.Entity<TrStudentDemeritApprovalHs>()
                                join EntryDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>() on HsEntryDemeritStudent.IdTrEntryDemeritStudent equals EntryDemeritStudent.Id
                                join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntryDemeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                                join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                                join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                                join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                                join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                                join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                                join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                                join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                                join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntryDemeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                                join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id into JoinedLevelInfractin
                                from LevelOfInfraction in JoinedLevelInfractin.DefaultIfEmpty()
                                join LevelOfInfractionParent in _dbContext.Entity<MsLevelOfInteraction>() on LevelOfInfraction.IdParentLevelOfInteraction equals LevelOfInfractionParent.Id into JoinedLevelInfractionParent
                                from LevelOfInfractionParent in JoinedLevelInfractionParent.DefaultIfEmpty()
                                join User in _dbContext.Entity<MsUser>() on EntryDemeritStudent.DemeritUserCreate equals User.Id
                                join ApprovalSetting in _dbContext.Entity<MsMeritDemeritApprovalSetting>() on Level.Id equals ApprovalSetting.IdLevel
                                where AcademicYear.Id == param.IdAcademiYear && EntryDemeritStudent.IsHasBeenApproved == true && _idGrades.Contains(Grade.Id)
                                select new
                                {
                                    Id = HsEntryDemeritStudent.Id,
                                    AcademicYear = AcademicYear.Description,
                                    IdAcademicYear = AcademicYear.Id,
                                    Semester = Homeroom.Semester.ToString(),
                                    Level = Level.Description,
                                    IdLevel = Level.Id,
                                    Grade = Grade.Description,
                                    IdGrade = Grade.Id,
                                    Homeroom = (Grade.Code) + (Classroom.Code),
                                    IdHomeroom = Homeroom.Id,
                                    IdStudent = Student.Id,
                                    NameStudent = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                    Category = MeritDemeritMapping.Category,
                                    LevelOfInfraction = LevelOfInfraction.IdParentLevelOfInteraction == null
                                                    ? LevelOfInfraction != null ? LevelOfInfraction.NameLevelOfInteraction: string.Empty
                                                    : LevelOfInfractionParent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                                    NameDecipline = MeritDemeritMapping.DisciplineName,
                                    Point = EntryDemeritStudent.Point,
                                    Note = EntryDemeritStudent.Note,
                                    CreateBy = User.DisplayName,
                                    RequestType = HsEntryDemeritStudent.RequestType,
                                    Reason = HsEntryDemeritStudent.RequestReason,
                                    Status = HsEntryDemeritStudent.Status,
                                    IdHomeroomStudent = HomeroomStudent.Id,
                                    Approval1 = ApprovalSetting.Approval1,
                                    IsShowButtonApproval = HsEntryDemeritStudent.Status == "Waiting Approval (1)"
                                                    ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval1)
                                                        ? true : false
                                                    : HsEntryDemeritStudent.Status == "Waiting Approval (2)"
                                                        ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval2)
                                                            ? true : false
                                                    : HsEntryDemeritStudent.Status == "Waiting Approval (3)"
                                                        ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval3)
                                                            ? true : false
                                                    : false,
                                });
            ;
            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester))
                query = query.Where(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.Status))
            {
                if (param.Status == "WaitingApproval")
                {
                    query = query.Where(x => x.Status.Contains("Waiting Approval"));
                }
                else
                {
                    query = query.Where(x => x.Status == param.Status);
                }
            }
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.IdStudent.Contains(param.Search) || (x.NameStudent).Contains(param.Search));

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
                case "Category":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Category)
                        : query.OrderBy(x => x.Category);
                    break;
                case "LevelOfInfraction":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LevelOfInfraction)
                        : query.OrderBy(x => x.LevelOfInfraction);
                    break;
                case "NameDecipline":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.NameDecipline)
                        : query.OrderBy(x => x.NameDecipline);
                    break;
                case "Point":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Point)
                        : query.OrderBy(x => x.Point);
                    break;
                case "Note":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Note)
                        : query.OrderBy(x => x.Note);
                    break;
                case "CreateBy":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CreateBy)
                        : query.OrderBy(x => x.CreateBy);
                    break;
                case "RequestType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.RequestType)
                        : query.OrderBy(x => x.RequestType);
                    break;
                case "Reason":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Reason)
                        : query.OrderBy(x => x.Reason);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;

            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetApprovalMeritDemeritTeacherResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Semester = x.Semester,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    IdStudent = x.IdStudent,
                    NameStudent = x.NameStudent,
                    Category = x.Category.GetDescription(),
                    LevelOfInfraction = x.LevelOfInfraction,
                    NameDecipline = x.NameDecipline,
                    Point = x.Point,
                    Note = x.Note,
                    CreateBy = x.CreateBy,
                    RequestType = x.RequestType,
                    Reason = x.Reason,
                    Status = x.Status,
                    IdLevel = x.IdLevel,
                    IsShowButtonApproval = x.IsShowButtonApproval,
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    IdAcademicYear = x.IdAcademicYear
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                items = result.Select(x => new GetApprovalMeritDemeritTeacherResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    Semester = x.Semester,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    IdStudent = x.IdStudent,
                    NameStudent = x.NameStudent,
                    Category = x.Category.GetDescription(),
                    LevelOfInfraction = x.LevelOfInfraction,
                    NameDecipline = x.NameDecipline,
                    Point = x.Point,
                    Note = x.Note,
                    CreateBy = x.CreateBy,
                    RequestType = x.RequestType,
                    Reason = x.Reason,
                    Status = x.Status,
                    IdLevel = x.IdLevel,
                    IsShowButtonApproval = x.IsShowButtonApproval,
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    IdAcademicYear = x.IdAcademicYear
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var status = false;
            var body = await Request.ValidateBody<ApprovalMeritDemeritTeacherRequest, ApprovalMeritDemeritTeacherValidator>();
            TrEntryDemeritStudent DemeritStudent = default;
            List<TrStudentPoint> StudentPoint = new List<TrStudentPoint>();

            var SettingApproval = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
                .Where(e => e.IdLevel == body.IdLevel).SingleOrDefaultAsync(CancellationToken);
            if (SettingApproval == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ApprovalSettin"], "IdLevel", body.IdLevel));

            var GetSanctionMapping = await _dbContext.Entity<MsSanctionMapping>()
                  .Where(e => e.IdAcademicYear == body.IdAcademicYear)
                  .ToListAsync(CancellationToken);

            var GetMeritDemeritComponentSetting = await _dbContext.Entity<MsMeritDemeritComponentSetting>()
             .Include(e => e.Grade).ThenInclude(e => e.MsLevel)
               .Where(e => e.Grade.IdLevel == body.IdLevel && e.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear && e.Grade.Id == body.IdGrade)
               .SingleOrDefaultAsync(CancellationToken);

            #region Create Update Merit Demerit Teacher & history
            if (body.Category == MeritDemeritCategory.Merit)
            {
                var HsEntryMeritStudent = await _dbContext.Entity<TrStudentMeritApprovalHs>()
                    .Include(e => e.EntryMeritStudent)
                    .Where(e => e.Id == body.Id).SingleOrDefaultAsync(CancellationToken);

                if (HsEntryMeritStudent.Status == "Waiting Approval (1)")
                {
                    HsEntryMeritStudent.Status = SettingApproval.Approval2 == null ? "Approved" : "Waiting Approval (2)";
                    HsEntryMeritStudent.IsApproved1 = true;
                    HsEntryMeritStudent.IdUserApproved1 = body.IdUser;
                    HsEntryMeritStudent.Note1 = body.Note;
                    HsEntryMeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(HsEntryMeritStudent);
                }
                else if (HsEntryMeritStudent.Status == "Waiting Approval (2)")
                {
                    HsEntryMeritStudent.Status = SettingApproval.Approval3 == null ? "Approved" : "Waiting Approval (3)";
                    HsEntryMeritStudent.IsApproved2 = true;
                    HsEntryMeritStudent.IdUserApproved2 = body.IdUser;
                    HsEntryMeritStudent.Note2 = body.Note;
                    HsEntryMeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(HsEntryMeritStudent);
                }
                else if (HsEntryMeritStudent.Status == "Waiting Approval (3)")
                {
                    HsEntryMeritStudent.Status = "Approved";
                    HsEntryMeritStudent.IsApproved2 = true;
                    HsEntryMeritStudent.IdUserApproved2 = body.IdUser;
                    HsEntryMeritStudent.Note2 = body.Note;
                    HsEntryMeritStudent.Reason = body.Note;
                }

                if (HsEntryMeritStudent.Status == "Approved")
                {
                    HsEntryMeritStudent.EntryMeritStudent.Status = RequestType.Create == body.RequestType ? "Approved" : "Deleted";
                    _dbContext.Entity<TrEntryMeritStudent>().Update(HsEntryMeritStudent.EntryMeritStudent);

                    var GetPointByStudentId = await _dbContext.Entity<TrStudentPoint>()
                       .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                       .Include(e => e.LevelOfInteraction).ThenInclude(e => e.Parent)
                       .Where(e => e.IdHomeroomStudent == body.IdHomeroomStudent
                           && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                       .Select(e => new
                       {
                           IdHomeroomStudent = e.IdHomeroomStudent,
                           LevelOfInfractin = e.LevelOfInteraction,
                           MeritPoint = e.MeritPoint,
                           Semester = e.HomeroomStudent.Homeroom.Semester,
                           Merit = e
                       })
                       .SingleOrDefaultAsync(CancellationToken);

                    var MeritPoint = GetPointByStudentId == null
                            ? body.Point
                                : RequestType.Create == body.RequestType
                                    ? Convert.ToInt32(GetPointByStudentId.MeritPoint) + body.Point
                                        : Convert.ToInt32(GetPointByStudentId.MeritPoint) - body.Point;

                    if (GetPointByStudentId == null)
                    {
                        var NewStudentPoint = new TrStudentPoint
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroomStudent = HsEntryMeritStudent.EntryMeritStudent.IdHomeroomStudent,
                            MeritPoint = MeritPoint,
                        };
                        _dbContext.Entity<TrStudentPoint>().Add(NewStudentPoint);

                        if (NewStudentPoint.IdSanctionMapping != null)
                            StudentPoint.Add(NewStudentPoint);
                    }
                    else
                    {
                        GetPointByStudentId.Merit.MeritPoint = MeritPoint;
                        _dbContext.Entity<TrStudentPoint>().Update(GetPointByStudentId.Merit);
                    }
                }


            }
            else if (body.Category == MeritDemeritCategory.AccountabilityPoints)
            {
                var HsEntryDemeritStudent = await _dbContext.Entity<TrStudentDemeritApprovalHs>()
                    .Include(e => e.EntryDemeritStudent).ThenInclude(e => e.MeritDemeritMapping).ThenInclude(e => e.LevelOfInteraction)
                    .Where(e => e.Id == body.Id).SingleOrDefaultAsync(CancellationToken);

                if (HsEntryDemeritStudent.Status == "Waiting Approval (1)")
                {
                    HsEntryDemeritStudent.Status = SettingApproval.Approval2 == null ? "Approved" : "Waiting Approval (2)";
                    HsEntryDemeritStudent.IsApproved1 = true;
                    HsEntryDemeritStudent.IdUserApproved1 = body.IdUser;
                    HsEntryDemeritStudent.Note1 = body.Note;
                    HsEntryDemeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentDemeritApprovalHs>().Update(HsEntryDemeritStudent);
                    status = true;
                }
                else if (HsEntryDemeritStudent.Status == "Waiting Approval (2)")
                {
                    HsEntryDemeritStudent.Status = SettingApproval.Approval3 == null ? "Approved" : "Waiting Approval (3)";
                    HsEntryDemeritStudent.IsApproved2 = true;
                    HsEntryDemeritStudent.IdUserApproved2 = body.IdUser;
                    HsEntryDemeritStudent.Note2 = body.Note;
                    HsEntryDemeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentDemeritApprovalHs>().Update(HsEntryDemeritStudent);
                    status = true;
                }
                else if (HsEntryDemeritStudent.Status == "Waiting Approval (3)")
                {
                    HsEntryDemeritStudent.Status = "Approved";
                    HsEntryDemeritStudent.IsApproved2 = true;
                    HsEntryDemeritStudent.IdUserApproved2 = body.IdUser;
                    HsEntryDemeritStudent.Note2 = body.Note;
                    HsEntryDemeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentDemeritApprovalHs>().Update(HsEntryDemeritStudent);
                    status = true;
                }

                if (HsEntryDemeritStudent.Status == "Approved" && status)
                {
                    HsEntryDemeritStudent.EntryDemeritStudent.Status = RequestType.Create == body.RequestType ? "Approved" : "Deleted";
                    _dbContext.Entity<TrEntryDemeritStudent>().Update(HsEntryDemeritStudent.EntryDemeritStudent);

                    var GetPointByStudentId = await _dbContext.Entity<TrStudentPoint>()
                   .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                   .Include(e => e.LevelOfInteraction).ThenInclude(e => e.Parent)
                   .Where(e => e.IdHomeroomStudent == body.IdHomeroomStudent
                       && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                   .Select(e => new
                   {
                       IdHomeroomStudent = e.IdHomeroomStudent,
                       LevelOfInfractin = e.LevelOfInteraction,
                       DemeritPoint = e.DemeritPoint,
                       Semester = e.HomeroomStudent.Homeroom.Semester,
                       Demerit = e
                   })
                   .SingleOrDefaultAsync(CancellationToken);

                    var GetLevelInfraction = await _dbContext.Entity<MsLevelOfInteraction>()
                        .Include(e => e.Parent)
                  .Where(e => e.Id == HsEntryDemeritStudent.EntryDemeritStudent.MeritDemeritMapping.IdLevelOfInteraction)
                  .SingleOrDefaultAsync(CancellationToken);

                    var DemeritPoint = GetPointByStudentId == null
                            ? body.Point
                                : RequestType.Create == body.RequestType
                                    ? Convert.ToInt32(GetPointByStudentId.DemeritPoint) + body.Point
                                        : Convert.ToInt32(GetPointByStudentId.DemeritPoint) - body.Point;
                    if (GetPointByStudentId == null)
                    {
                        var NewStudentPoint = new TrStudentPoint
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroomStudent = HsEntryDemeritStudent.EntryDemeritStudent.IdHomeroomStudent,
                            DemeritPoint = DemeritPoint,
                            IdSanctionMapping = GetSanctionMapping.Any(e => e.Min <= DemeritPoint && e.Max >= DemeritPoint) ? GetSanctionMapping.SingleOrDefault(e => e.Min <= DemeritPoint && e.Max >= DemeritPoint).Id : null,
                            IdLevelOfInteraction = HsEntryDemeritStudent.EntryDemeritStudent.MeritDemeritMapping.IdLevelOfInteraction,
                        };
                        _dbContext.Entity<TrStudentPoint>().Add(NewStudentPoint);

                        if (NewStudentPoint.IdSanctionMapping != null)
                            StudentPoint.Add(NewStudentPoint);
                    }
                    else
                    {
                        var IdSanctionMappingOld = GetPointByStudentId.Demerit.IdSanctionMapping;
                        GetPointByStudentId.Demerit.DemeritPoint = DemeritPoint;
                        GetPointByStudentId.Demerit.IdSanctionMapping = GetSanctionMapping.Any(e => e.Min <= DemeritPoint && e.Max >= DemeritPoint) ? GetSanctionMapping.SingleOrDefault(e => e.Min <= DemeritPoint && e.Max >= DemeritPoint).Id : null;
                        GetPointByStudentId.Demerit.IdLevelOfInteraction = GetPointByStudentId.LevelOfInfractin == null
                            ? HsEntryDemeritStudent.EntryDemeritStudent.MeritDemeritMapping.IdLevelOfInteraction
                            : MeritDemeritTeacherHandler.GetLevelOfInfraction(GetPointByStudentId.LevelOfInfractin, GetLevelInfraction);
                        _dbContext.Entity<TrStudentPoint>().Update(GetPointByStudentId.Demerit);

                        if (IdSanctionMappingOld != GetPointByStudentId.Demerit.IdSanctionMapping)
                            StudentPoint.Add(GetPointByStudentId.Demerit);
                    }
                }

                DemeritStudent = HsEntryDemeritStudent.EntryDemeritStudent;
            }
            #endregion

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Send Email
            if (body.Category == MeritDemeritCategory.AccountabilityPoints && status)
            {
                var GetDemeritStudent = await (from HsEntDemeritStudent in _dbContext.Entity<TrStudentDemeritApprovalHs>()
                                               join EntDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>() on HsEntDemeritStudent.IdTrEntryDemeritStudent equals EntDemeritStudent.Id
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
                                               join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id
                                               join User in _dbContext.Entity<MsUser>() on EntDemeritStudent.UserIn equals User.Id
                                               join UserStudent in _dbContext.Entity<MsUser>() on Student.Id equals UserStudent.Id
                                               where HsEntDemeritStudent.Id == body.Id
                                               select new
                                               {
                                                   Id = HsEntDemeritStudent.Id,
                                                   IdStudent = Student.Id,
                                                   StudentName = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                                   Category = MeritDemeritMapping.Category.GetDescription(),
                                                   CategoryEnum = MeritDemeritMapping.Category,
                                                   LevelOfInfracton = LevelOfInfraction.IdParentLevelOfInteraction == null ? LevelOfInfraction.NameLevelOfInteraction : LevelOfInfraction.Parent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                                                   DisciplineName = MeritDemeritMapping.DisciplineName,
                                                   Point = EntDemeritStudent.Point,
                                                   Note = EntDemeritStudent.Note,
                                                   TeacherName = User.DisplayName,
                                                   TeachderId = User.Id,
                                                   CreateDate = Convert.ToDateTime(EntDemeritStudent.DateDemerit).ToString("dd MMM yyyy"),
                                                   RequestType = EntDemeritStudent.RequestType,
                                                   Status = HsEntDemeritStudent.Status,
                                                   SchoolName = School.Name,
                                                   IdHomeroom = Homeroom.Id,
                                                   StudentEmail = UserStudent.Email,
                                                   Idlevel = Level.Id,
                                                   IdGrade = Grade.Id,
                                                   IdHomeroomStudent = HomeroomStudent.Id
                                               }).SingleOrDefaultAsync(CancellationToken);


                if (GetDemeritStudent.Status != "Approved")
                {
                    var IdTeacherPosition = GetDemeritStudent.Status == "Waiting Approval (2)"
                                                ? SettingApproval.Approval2
                                                    : GetDemeritStudent.Status == "Waiting Approval (3)"
                                                        ? SettingApproval.Approval3 : "";
                    var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                                      .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                      .Include(e => e.User)
                                      .Where(e => e.MsNonTeachingLoad.TeacherPosition.Id == IdTeacherPosition && e.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear && !string.IsNullOrEmpty(e.Data))
                                      .ToListAsync(CancellationToken);

                    List<GetUserGrade> idGrades = new List<GetUserGrade>();
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

                    var GetUser = idGrades.Where(e => e.IdGrade == body.IdGrade)
                        .Select(e => new MsUser
                        {
                            Id = e.IdUser,
                            DisplayName = e.Fullname
                        }).Distinct().ToList();

                    List<EmailDemeritRequestApprovalResult> GetDemeritResult = new List<EmailDemeritRequestApprovalResult>();
                    foreach (var itemUser in GetUser)
                    {
                        GetDemeritResult.Add(new EmailDemeritRequestApprovalResult
                        {
                            Id = GetDemeritStudent.Id,
                            IdStudent = GetDemeritStudent.IdStudent,
                            StudentName = GetDemeritStudent.StudentName,
                            Category = GetDemeritStudent.Category.ToString(),
                            CategoryEnum = GetDemeritStudent.CategoryEnum,
                            DisciplineName = GetDemeritStudent.DisciplineName,
                            Point = GetDemeritStudent.Point.ToString(),
                            Note = GetDemeritStudent.Note,
                            TeacherName = GetDemeritStudent.TeacherName,
                            TeacherId = GetDemeritStudent.TeachderId,
                            CreateDate = GetDemeritStudent.CreateDate,
                            SchoolName = GetDemeritStudent.SchoolName,
                            LevelOfInfraction = GetDemeritStudent.LevelOfInfracton,
                            RequestType = GetDemeritStudent.RequestType.ToString(),
                            Status = GetDemeritStudent.Status,
                            IdUserApproval = itemUser.Id,
                            ApprovalName = itemUser.DisplayName,
                            IdAcademicYear = body.IdAcademicYear,
                            IdGrade = body.IdGrade,
                            IdLevel = body.IdLevel,
                            IdHomeroomStudent = body.IdHomeroomStudent,
                        });
                    }

                    KeyValues.Add("GetDemeritStudent", GetDemeritResult);
                    var NotificationApprover = MeritDemeritTeacherHandler.DS7Notification(KeyValues, AuthInfo);
                }
                else
                {
                    //for student
                    List<EmailMeritDemeritApprovalResult> GetDemeritResult = new List<EmailMeritDemeritApprovalResult>();
                    GetDemeritResult.Add(new EmailMeritDemeritApprovalResult
                    {
                        Id = GetDemeritStudent.Id,
                        IdStudent = GetDemeritStudent.IdStudent,
                        StudentName = GetDemeritStudent.StudentName,
                        Category = GetDemeritStudent.Category.ToString(),
                        CategoryEnum = GetDemeritStudent.CategoryEnum,
                        DisciplineName = GetDemeritStudent.DisciplineName,
                        Point = GetDemeritStudent.Point.ToString(),
                        Note = GetDemeritStudent.Note,
                        TeacherName = GetDemeritStudent.TeacherName,
                        TeacherId = GetDemeritStudent.TeachderId,
                        CreateDate = GetDemeritStudent.CreateDate,
                        SchoolName = GetDemeritStudent.SchoolName,
                        LevelOfInfraction = GetDemeritStudent.LevelOfInfracton,
                        RequestType = GetDemeritStudent.RequestType.ToString(),
                        Status = GetDemeritStudent.Status,
                        StudentEmail = GetDemeritStudent.StudentEmail,
                        IdAcademicYear = body.IdAcademicYear,
                        IdGrade = body.IdGrade,
                        IdLevel = body.IdLevel,
                        IdHomeroomStudent = body.IdHomeroomStudent,
                    });

                    KeyValues.Add("GetDemeritStudent", GetDemeritResult);
                    if (GetDemeritStudent.RequestType.ToString() == "Create")
                    {
                        var NotificationStudent = MeritDemeritTeacherHandler.DS5Notification(KeyValues, AuthInfo);
                    }
                    else
                    {
                        var NotificationStudent = MeritDemeritTeacherHandler.DS6Notification(KeyValues, AuthInfo);
                    }

                    //for teacher
                    if (GetDemeritStudent.RequestType.ToString() == "Create")
                    {
                        var GetSanctionMappingAttentionBy = await _dbContext.Entity<MsSanctionMappingAttentionBy>()
                          .Include(e => e.Role).ThenInclude(e => e.RoleGroup)
                          .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
                          .Where(e => StudentPoint.Select(e => e.IdSanctionMapping).Contains(e.IdSanctionMapping) /*&& e.IdTeacherPosition != null*/)
                          .ToListAsync(CancellationToken);

                        var GetHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                             .Where(e => e.IdHomeroom == GetDemeritStudent.IdHomeroom)
                             .ToListAsync(CancellationToken);

                        var GetParent = await _dbContext.Entity<MsUser>()
                             .Where(e => e.Username == "P" + GetDemeritStudent.IdStudent)
                             .ToListAsync(CancellationToken);

                        var GetSubjetTeacher = await (from LessonTeacher in _dbContext.Entity<MsLessonTeacher>()
                                                      join Lesson in _dbContext.Entity<MsLesson>() on LessonTeacher.IdLesson equals Lesson.Id
                                                      join HomeroomStudentEnrollment in _dbContext.Entity<MsHomeroomStudentEnrollment>() on Lesson.Id equals HomeroomStudentEnrollment.IdLesson
                                                      where HomeroomStudentEnrollment.IdHomeroomStudent == GetDemeritStudent.IdHomeroomStudent
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

                        var GetUserRole = await _dbContext.Entity<MsUserRole>()
                                      .Where(x => GetSanctionMappingAttentionBy.Select(e => e.IdRole).ToList().Contains(x.IdRole))
                                      .ToListAsync(CancellationToken);

                        var getAllPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                                             .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                             .Include(e => e.User)
                                             .Where(e => e.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear)
                                             .ToListAsync(CancellationToken);

                        var getDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                             .Include(e => e.Department)
                                             .Where(e => e.Department.IdAcademicYear == body.IdAcademicYear && e.IdLevel == GetDemeritStudent.Idlevel)
                                             .ToListAsync(CancellationToken);

                        var getGrade = await _dbContext.Entity<MsGrade>()
                                            .Include(e => e.MsLevel)
                                            .Where(e => e.MsLevel.IdAcademicYear == body.IdAcademicYear && e.Id == GetDemeritStudent.IdGrade)
                                            .ToListAsync(CancellationToken);

                        List<EmailSanctionResult> GetSanction = new List<EmailSanctionResult>();
                        foreach (var item in StudentPoint)
                        {

                            var IdStudent = GetDemeritStudent != null ? GetDemeritStudent.IdStudent : "";
                            var IdHomeroom = GetDemeritStudent != null ? GetDemeritStudent.IdHomeroom : "";
                            var idGrade = GetDemeritStudent != null ? GetDemeritStudent.IdGrade : "";
                            var Idlevel = GetDemeritStudent != null ? GetDemeritStudent.Idlevel : "";
                            var SanctionMapping = GetSanctionMapping.Any(e => e.Id == item.IdSanctionMapping) ? GetSanctionMapping.SingleOrDefault(e => e.Id == item.IdSanctionMapping) : null;

                            var msHomeroomStudnetById = await _dbContext.Entity<MsHomeroomStudent>().Include(x => x.Homeroom).ThenInclude(x => x.Grade).Where(x => x.Id == item.IdHomeroomStudent).FirstOrDefaultAsync(CancellationToken);

                            var UserAttandent = await MeritDemeritTeacherHandler.AttandentBy(KeyValues, AuthInfo, msHomeroomStudnetById, GetParent, GetSanctionMappingAttentionBy, GetHomeroomTeacher, GetSubjetTeacher, GetTeacherNonTeaching, idGrade, Idlevel, getDepartmentLevel, getGrade, _dbContext);

                            foreach (var itemUser in UserAttandent)
                            {
                                GetSanction.Add(new EmailSanctionResult
                                {
                                    Id = item.Id,
                                    IdStudent = IdStudent,
                                    StudentName = GetDemeritStudent != null ? GetDemeritStudent.StudentName : "",
                                    DemeritTotal = item.DemeritPoint.ToString(),
                                    MeritTotal = item.MeritPoint.ToString(),
                                    LevelOfInfraction = GetDemeritStudent != null ? GetDemeritStudent.LevelOfInfracton : "",
                                    Sanction = GetSanctionMapping.Any(e => e.Id == item.IdSanctionMapping) ? GetSanctionMapping.SingleOrDefault(e => e.Id == item.IdSanctionMapping).SanctionName : "",
                                    LastUpdate = item.DateUp == null ? Convert.ToDateTime(item.DateIn).ToString("dd MMM yyyy") : Convert.ToDateTime(item.DateUp).ToString("dd MMM yyyy"),
                                    IdUser = itemUser,
                                    SchoolName = GetDemeritStudent != null ? GetDemeritStudent.SchoolName : "",
                                    IdHomeroomStudent = item.IdHomeroomStudent,
                                    IdAcadYear = body.IdAcademicYear,
                                    IdLevel = body.IdLevel,
                                    IdHomeroom = IdHomeroom,
                                    IsPoint = GetMeritDemeritComponentSetting.IsUsePointSystem,
                                    IdGrade = body.IdGrade
                                });
                            }
                        }


                        KeyValues.Add("GetSanction", GetSanction);
                        var NotificationStaff = MeritDemeritTeacherHandler.DS2Notification(KeyValues, AuthInfo);
                        var NotificationStudent = MeritDemeritTeacherHandler.DS3Notification(KeyValues, AuthInfo);
                    }
                }
            }
            else
            {
                var GetMeritStudent = await (
                                                from HsEntMeritStudent in _dbContext.Entity<TrStudentMeritApprovalHs>()
                                                join EntMeritStudent in _dbContext.Entity<TrEntryMeritStudent>() on HsEntMeritStudent.IdTrEntryMeritStudent equals EntMeritStudent.Id
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
                                                where HsEntMeritStudent.Id == body.Id
                                                select new
                                                {
                                                    Id = EntMeritStudent.Id,
                                                    IdStudent = Student.Id,
                                                    StudentName = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                                    Category = MeritDemeritMapping.Category.GetDescription(),
                                                    CategoryEnum = MeritDemeritMapping.Category,
                                                    LevelOfInfracton = "",
                                                    DisciplineName = MeritDemeritMapping.DisciplineName,
                                                    Point = EntMeritStudent.Point,
                                                    Note = EntMeritStudent.Note,
                                                    TeacherName = User.DisplayName,
                                                    TeachderId = User.Id,
                                                    CreateDate = Convert.ToDateTime(EntMeritStudent.DateMerit).ToString("dd MMM yyyy"),
                                                    RequestType = EntMeritStudent.RequestType,
                                                    Status = HsEntMeritStudent.Status,
                                                    SchoolName = School.Name,
                                                }).SingleOrDefaultAsync(CancellationToken);

                if (GetMeritStudent.Status == "Approved")
                {
                    //for student
                    if (GetMeritStudent.RequestType.ToString() == "Delete")
                    {
                        List<EmailDemeritRequestApprovalResult> GetMeritResult = new List<EmailDemeritRequestApprovalResult>();
                        GetMeritResult.Add(new EmailDemeritRequestApprovalResult
                        {
                            Id = GetMeritStudent.Id,
                            IdStudent = GetMeritStudent.IdStudent,
                            StudentName = GetMeritStudent.StudentName,
                            Category = GetMeritStudent.Category.ToString(),
                            CategoryEnum = GetMeritStudent.CategoryEnum,
                            DisciplineName = GetMeritStudent.DisciplineName,
                            Point = GetMeritStudent.Point.ToString(),
                            Note = GetMeritStudent.Note,
                            TeacherName = GetMeritStudent.TeacherName,
                            TeacherId = GetMeritStudent.TeachderId,
                            CreateDate = GetMeritStudent.CreateDate,
                            SchoolName = GetMeritStudent.SchoolName,
                            LevelOfInfraction = GetMeritStudent.LevelOfInfracton,
                            RequestType = GetMeritStudent.RequestType.ToString(),
                            Status = GetMeritStudent.Status,
                            IdAcademicYear = body.IdAcademicYear,
                            IdGrade = body.IdGrade,
                            IdLevel = body.IdLevel,
                            IdHomeroomStudent = body.IdHomeroomStudent,
                        });

                        KeyValues.Add("GetDemeritStudent", GetMeritResult);
                        var Notification = MeritDemeritTeacherHandler.DS6Notification(KeyValues, AuthInfo);
                    }
                }
            }
            #endregion

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<ApprovalMeritDemeritTeacherRequest, ApprovalMeritDemeritTeacherValidator>();
            TrEntryDemeritStudent DemeritStudent = default;

            var GetApprovalSetting = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
              .Include(e => e.Level)
                .Where(e => e.IdLevel == body.IdLevel && e.Level.IdAcademicYear == body.IdAcademicYear)
                .SingleOrDefaultAsync(CancellationToken);

            if (GetApprovalSetting == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ApprovalSetting"], "Id", body.IdLevel));

            #region Craete Update Merit Demerit Teacher & history
            if (body.Category == MeritDemeritCategory.Merit)
            {
                var HsEntryMeritStudent = await _dbContext.Entity<TrStudentMeritApprovalHs>()
                    .Include(e => e.EntryMeritStudent)
                    .Where(e => e.Id == body.Id).SingleOrDefaultAsync(CancellationToken);

                if (HsEntryMeritStudent.Status == "Waiting Approval (1)")
                {
                    HsEntryMeritStudent.Status = "Declined";
                    HsEntryMeritStudent.IsApproved1 = true;
                    HsEntryMeritStudent.IdUserApproved1 = body.IdUser;
                    HsEntryMeritStudent.Note1 = body.Note;
                    HsEntryMeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(HsEntryMeritStudent);
                }
                else if (HsEntryMeritStudent.Status == "Waiting Approval (2)")
                {
                    HsEntryMeritStudent.Status = "Declined";
                    HsEntryMeritStudent.IsApproved2 = true;
                    HsEntryMeritStudent.IdUserApproved2 = body.IdUser;
                    HsEntryMeritStudent.Note2 = body.Note;
                    HsEntryMeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(HsEntryMeritStudent);
                }
                else if (HsEntryMeritStudent.Status == "Waiting Approval (3)")
                {
                    HsEntryMeritStudent.Status = "Declined";
                    HsEntryMeritStudent.IsApproved3 = true;
                    HsEntryMeritStudent.IdUserApproved3 = body.IdUser;
                    HsEntryMeritStudent.Note1 = body.Note;
                    HsEntryMeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentMeritApprovalHs>().Update(HsEntryMeritStudent);
                }

                if (HsEntryMeritStudent.Status == "Declined")
                {
                    HsEntryMeritStudent.EntryMeritStudent.Status = "Approved";
                    HsEntryMeritStudent.EntryMeritStudent.RequestType = RequestType.Create;
                    HsEntryMeritStudent.EntryMeritStudent.IsDeleted = false;

                    _dbContext.Entity<TrEntryMeritStudent>().Update(HsEntryMeritStudent.EntryMeritStudent);
                }

            }
            else if (body.Category == MeritDemeritCategory.AccountabilityPoints)
            {
                var HsEntryDemeritStudent = await _dbContext.Entity<TrStudentDemeritApprovalHs>()
                    .Include(e => e.EntryDemeritStudent)
                    .Where(e => e.Id == body.Id).SingleOrDefaultAsync(CancellationToken);


                if (HsEntryDemeritStudent.Status == "Waiting Approval (1)")
                {
                    HsEntryDemeritStudent.Status = "Declined";
                    HsEntryDemeritStudent.IsApproved1 = true;
                    HsEntryDemeritStudent.IdUserApproved1 = body.IdUser;
                    HsEntryDemeritStudent.Note1 = body.Note;
                    HsEntryDemeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentDemeritApprovalHs>().Update(HsEntryDemeritStudent);
                }
                else if (HsEntryDemeritStudent.Status == "Waiting Approval (2)")
                {
                    HsEntryDemeritStudent.Status = "Declined";
                    HsEntryDemeritStudent.IsApproved2 = true;
                    HsEntryDemeritStudent.IdUserApproved2 = body.IdUser;
                    HsEntryDemeritStudent.Note2 = body.Note;
                    HsEntryDemeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentDemeritApprovalHs>().Update(HsEntryDemeritStudent);
                }
                else if (HsEntryDemeritStudent.Status == "Waiting Approval (3)")
                {
                    HsEntryDemeritStudent.Status = "Declined";
                    HsEntryDemeritStudent.IsApproved2 = true;
                    HsEntryDemeritStudent.IdUserApproved2 = body.IdUser;
                    HsEntryDemeritStudent.Note2 = body.Note;
                    HsEntryDemeritStudent.Reason = body.Note;
                    _dbContext.Entity<TrStudentDemeritApprovalHs>().Update(HsEntryDemeritStudent);
                }

                if (HsEntryDemeritStudent.Status == "Declined")
                {
                    HsEntryDemeritStudent.EntryDemeritStudent.Status = "Decline";
                    HsEntryDemeritStudent.EntryDemeritStudent.RequestType = RequestType.Create;
                    HsEntryDemeritStudent.EntryDemeritStudent.IsDeleted = false;

                    _dbContext.Entity<TrEntryDemeritStudent>().Update(HsEntryDemeritStudent.EntryDemeritStudent);
                }
                DemeritStudent = HsEntryDemeritStudent.EntryDemeritStudent;

            }
            #endregion
            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Send Email
            if (body.Category == MeritDemeritCategory.AccountabilityPoints)
            {
                var GetDemeritStudent = await (from EntDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>()
                                               join HsEntDemeritStudent in _dbContext.Entity<TrStudentDemeritApprovalHs>() on EntDemeritStudent.Id equals HsEntDemeritStudent.IdTrEntryDemeritStudent
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
                                               join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id
                                               join User in _dbContext.Entity<MsUser>() on EntDemeritStudent.UserIn equals User.Id
                                               where HsEntDemeritStudent.Id == body.Id
                                               select new
                                               {
                                                   Id = HsEntDemeritStudent.Id,
                                                   IdStudent = Student.IdBinusian,
                                                   StudentName = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                                   Category = MeritDemeritMapping.Category.GetDescription(),
                                                   CategoryEnum = MeritDemeritMapping.Category,
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
                                               }).SingleOrDefaultAsync(CancellationToken);

                List<EmailDemeritRequestApprovalResult> GetMeritResult = new List<EmailDemeritRequestApprovalResult>();
                GetMeritResult.Add(new EmailDemeritRequestApprovalResult
                {
                    Id = GetDemeritStudent.Id,
                    IdStudent = GetDemeritStudent.IdStudent,
                    StudentName = GetDemeritStudent.StudentName,
                    Category = GetDemeritStudent.Category.ToString(),
                    CategoryEnum = GetDemeritStudent.CategoryEnum,
                    DisciplineName = GetDemeritStudent.DisciplineName,
                    Point = GetDemeritStudent.Point.ToString(),
                    Note = GetDemeritStudent.Note,
                    TeacherName = GetDemeritStudent.TeacherName,
                    TeacherId = GetDemeritStudent.TeachderId,
                    CreateDate = GetDemeritStudent.CreateDate,
                    SchoolName = GetDemeritStudent.SchoolName,
                    LevelOfInfraction = GetDemeritStudent.LevelOfInfracton,
                    RequestType = GetDemeritStudent.RequestType.ToString(),
                    Status = GetDemeritStudent.Status,
                    IdAcademicYear = body.IdAcademicYear,
                    IdGrade = body.IdGrade,
                    IdLevel = body.IdLevel,
                    IdHomeroomStudent = body.IdHomeroomStudent,
                });

                KeyValues.Add("GetDemeritStudent", GetMeritResult);
                var Notification = MeritDemeritTeacherHandler.DS4Notification(KeyValues, AuthInfo);
            }
            #endregion
            return Request.CreateApiResult2();
        }

    }
}
