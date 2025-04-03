using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
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
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailApprovalMeritDemeritHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetDetailApprovalMeritDemeritHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetDetailApprovalMeritDemeritRequest, GetDetailApprovalMeritDemeritValidator>();

            var getPositionByUser = await _dbContext.Entity<TrNonTeachingLoad>()
                                          .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                          .Where(e => e.IdUser == body.IdUser &&
                                                  e.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear &&
                                                  !string.IsNullOrEmpty(e.Data) &&
                                                  (e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.LevelHead ||
                                                      e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.VicePrincipal ||
                                                      e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.Principal ||
                                                      e.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.AffectiveCoordinator)
                                                  )
                                          .ToListAsync(CancellationToken);

            if (getPositionByUser == null)
                throw new Exception("User Dont Have Teaching Load");

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
                        if (_levelLH.Id == body.IdLevel)
                        {
                            idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == body.IdLevel)
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
                        if (_levelLH.Id == body.IdLevel)
                        {
                            idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == body.IdLevel)
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
                        if (_levelLH.Id == body.IdLevel)
                        {
                            idGrades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == body.IdLevel)
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
                    if (_GradeLH.Id == body.IdGrade)
                    {
                        idGrades.AddRange(await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == body.IdLevel)
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

            GetDetailApprovalMeritDemeritResult items = null;
            if (body.Category == MeritDemeritCategory.Merit)
            {
                items = await (from HsEntMeritStudent in _dbContext.Entity<TrStudentMeritApprovalHs>()
                               join EntMeritStudent in _dbContext.Entity<TrEntryMeritStudent>() on HsEntMeritStudent.IdTrEntryMeritStudent equals EntMeritStudent.Id
                               join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntMeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                               join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                               join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                               join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                               join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                               join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                               join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                               join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                               join UserCreate in _dbContext.Entity<MsUser>() on HsEntMeritStudent.UserIn equals UserCreate.Id
                               join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                               join UserStudent in _dbContext.Entity<MsUser>() on Student.Id equals UserStudent.Id
                               join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntMeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                               join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id into JoinedLevelOfInfraction
                               from LevelOfInfraction in JoinedLevelOfInfraction.DefaultIfEmpty()
                               join LevelOfInfractionParent in _dbContext.Entity<MsLevelOfInteraction>() on LevelOfInfraction.IdParentLevelOfInteraction equals LevelOfInfractionParent.Id into JoinedLevelOfInfractionParent
                               from LevelOfInfractionParent in JoinedLevelOfInfractionParent.DefaultIfEmpty()
                               join UserApproval1 in _dbContext.Entity<MsUser>() on HsEntMeritStudent.IdUserApproved1 equals UserApproval1.Id into JoinedUserApproval1
                               from UserApproval1 in JoinedUserApproval1.DefaultIfEmpty()
                               join UserApproval2 in _dbContext.Entity<MsUser>() on HsEntMeritStudent.IdUserApproved2 equals UserApproval2.Id into JoinedUserApproval2
                               from UserApproval2 in JoinedUserApproval2.DefaultIfEmpty()
                               join UserApproval3 in _dbContext.Entity<MsUser>() on HsEntMeritStudent.IdUserApproved3 equals UserApproval3.Id into JoinedUserApproval3
                               from UserApproval3 in JoinedUserApproval3.DefaultIfEmpty()
                               join ApprovalSetting in _dbContext.Entity<MsMeritDemeritApprovalSetting>() on Level.Id equals ApprovalSetting.IdLevel
                               where HsEntMeritStudent.Id == body.Id
                               select new GetDetailApprovalMeritDemeritResult
                               {
                                   Id = HsEntMeritStudent.Id,
                                   AcademicYear = AcademicYear.Description,
                                   RequestType = HsEntMeritStudent.RequestType.ToString(),
                                   Semester = Homeroom.Semester.ToString(),
                                   CreateBy = UserCreate.DisplayName,
                                   Student = Student.Id + " - " + (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                   Homeroom = Grade.Code + Classroom.Code,
                                   Category = MeritDemeritMapping.Category.GetDescription(),
                                   LevelOfInfraction = "",
                                   DisciplineName = MeritDemeritMapping.DisciplineName,
                                   Point = EntMeritStudent.Point.ToString(),
                                   Note = EntMeritStudent.Note,
                                   UserApproval1 = UserApproval1.DisplayName == null ? "" : UserApproval1.DisplayName,
                                   UserApproval2 = UserApproval2.DisplayName == null ? "" : UserApproval2.DisplayName,
                                   UserApproval3 = UserApproval3.DisplayName == null ? "" : UserApproval3.DisplayName,
                                   NoteApproval1 = HsEntMeritStudent.Note1 == null ? "" : HsEntMeritStudent.Note1,
                                   NoteApproval2 = HsEntMeritStudent.Note2 == null ? "" : HsEntMeritStudent.Note2,
                                   NoteApproval3 = HsEntMeritStudent.Note3 == null ? "" : HsEntMeritStudent.Note3,
                                   Status = HsEntMeritStudent.Status,
                                   Reason = HsEntMeritStudent.RequestReason,
                                   IsShowButtonApproval = HsEntMeritStudent.Status == "Waiting Approval (1)"
                                                           ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval1)
                                                               ? true : false
                                                           : HsEntMeritStudent.Status == "Waiting Approval (2)"
                                                               ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval2)
                                                                   ? true : false
                                                           : HsEntMeritStudent.Status == "Waiting Approval (3)"
                                                               ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval3)
                                                                   ? true : false
                                                           : false,
                               }).FirstOrDefaultAsync(CancellationToken);
            }
            else
            {
                items = await (from HsEntDemeritStudent in _dbContext.Entity<TrStudentDemeritApprovalHs>()
                               join EntDemeritStudent in _dbContext.Entity<TrEntryDemeritStudent>() on HsEntDemeritStudent.IdTrEntryDemeritStudent equals EntDemeritStudent.Id
                               join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on EntDemeritStudent.IdHomeroomStudent equals HomeroomStudent.Id
                               join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                               join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                               join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                               join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                               join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                               join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                               join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                               join UserCreate in _dbContext.Entity<MsUser>() on HsEntDemeritStudent.UserIn equals UserCreate.Id
                               join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                               join UserStudent in _dbContext.Entity<MsUser>() on Student.Id equals UserStudent.Id
                               join MeritDemeritMapping in _dbContext.Entity<MsMeritDemeritMapping>() on EntDemeritStudent.IdMeritDemeritMapping equals MeritDemeritMapping.Id
                               join LevelOfInfraction in _dbContext.Entity<MsLevelOfInteraction>() on MeritDemeritMapping.IdLevelOfInteraction equals LevelOfInfraction.Id
                               join LevelOfInfractionParent in _dbContext.Entity<MsLevelOfInteraction>() on LevelOfInfraction.IdParentLevelOfInteraction equals LevelOfInfractionParent.Id into JoinedLevelOfInfractionParent
                               from LevelOfInfractionParent in JoinedLevelOfInfractionParent.DefaultIfEmpty()
                               join UserApproval1 in _dbContext.Entity<MsUser>() on HsEntDemeritStudent.IdUserApproved1 equals UserApproval1.Id into JoinedUserApproval1
                               from UserApproval1 in JoinedUserApproval1.DefaultIfEmpty()
                               join UserApproval2 in _dbContext.Entity<MsUser>() on HsEntDemeritStudent.IdUserApproved2 equals UserApproval2.Id into JoinedUserApproval2
                               from UserApproval2 in JoinedUserApproval2.DefaultIfEmpty()
                               join UserApproval3 in _dbContext.Entity<MsUser>() on HsEntDemeritStudent.IdUserApproved3 equals UserApproval3.Id into JoinedUserApproval3
                               from UserApproval3 in JoinedUserApproval3.DefaultIfEmpty()
                               join ApprovalSetting in _dbContext.Entity<MsMeritDemeritApprovalSetting>() on Level.Id equals ApprovalSetting.IdLevel
                               where HsEntDemeritStudent.Id == body.Id
                               select new GetDetailApprovalMeritDemeritResult
                               {
                                   Id = HsEntDemeritStudent.Id,
                                   AcademicYear = AcademicYear.Description,
                                   RequestType = HsEntDemeritStudent.RequestType.ToString(),
                                   Semester = Homeroom.Semester.ToString(),
                                   CreateBy = UserCreate.DisplayName,
                                   Student = Student.Id + " - " + (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                   Homeroom = Grade.Code + Classroom.Code,
                                   Category = MeritDemeritMapping.Category.GetDescription(),
                                   LevelOfInfraction = LevelOfInfraction.IdParentLevelOfInteraction == null
                                       ? LevelOfInfraction.NameLevelOfInteraction
                                       : LevelOfInfractionParent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                                   DisciplineName = MeritDemeritMapping.DisciplineName,
                                   Point = EntDemeritStudent.Point.ToString(),
                                   Note = EntDemeritStudent.Note,
                                   UserApproval1 = UserApproval1.DisplayName==null?"":UserApproval1.DisplayName,
                                   UserApproval2 = UserApproval2.DisplayName == null ? "" : UserApproval2.DisplayName,
                                   UserApproval3 = UserApproval3.DisplayName == null ? "" : UserApproval3.DisplayName,
                                   NoteApproval1 = HsEntDemeritStudent.Note1 == null ? "" : HsEntDemeritStudent.Note1,
                                   NoteApproval2 = HsEntDemeritStudent.Note2 == null ? "" : HsEntDemeritStudent.Note2,
                                   NoteApproval3 = HsEntDemeritStudent.Note3 == null ? "" : HsEntDemeritStudent.Note3,
                                   Status = HsEntDemeritStudent.Status,
                                   Reason = HsEntDemeritStudent.RequestReason,
                                   IsShowButtonApproval = HsEntDemeritStudent.Status == "Waiting Approval (1)"
                                                           ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval1)
                                                               ? true : false
                                                           : HsEntDemeritStudent.Status == "Waiting Approval (2)"
                                                               ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval2)
                                                                   ? true : false
                                                           : HsEntDemeritStudent.Status == "Waiting Approval (3)"
                                                               ? _idTeacherPositionByUser.Contains(ApprovalSetting.Approval3)
                                                                   ? true : false
                                                           : false,
                               }).FirstOrDefaultAsync(CancellationToken);
            }
            return Request.CreateApiResult2(items as object);
        }

    }
}
