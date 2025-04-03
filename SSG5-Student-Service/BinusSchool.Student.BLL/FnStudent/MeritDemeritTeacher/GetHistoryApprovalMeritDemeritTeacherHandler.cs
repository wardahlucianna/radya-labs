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
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetHistoryApprovalMeritDemeritTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetHistoryApprovalMeritDemeritTeacherHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHistoryApprovalMeritDemeritTeacherRequest>();
            string[] _columns = { "AcademicYear", "Semester", "Level", "Grade", "Homeroom", "IdStudent", "NameStudent", "Category", "LevelOfInfraction", "NameDecipline", "Point", "Note", "CreateBy", "Approver1", "NoteApprover1", "Approver2", "NoteApprover2", "Approver3", "NoteApprover3", "RequestType", "RequestReason", "Status" };

            var HomeroomTeacherByIdUser = await _dbContext.Entity<MsHomeroomTeacher>()
                                        .Where(e => e.IdBinusian == param.IdAcademicYear)
                                        .SingleOrDefaultAsync(CancellationToken);

            var GetMeritDemeritApprovalSetting = await _dbContext.Entity<MsMeritDemeritApprovalSetting>()
                                                    .ToListAsync(CancellationToken);

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
                         join LevelOfInfractionParent in _dbContext.Entity<MsLevelOfInteraction>() on LevelOfInfraction.IdParentLevelOfInteraction equals LevelOfInfractionParent.Id into JoinedLevelInfractionParent
                         from LevelOfInfractionParent in JoinedLevelInfractionParent.DefaultIfEmpty()
                         join User in _dbContext.Entity<MsUser>() on EntryMeritStudent.MeritUserCreate equals User.Id
                         join UserApprovar1 in _dbContext.Entity<MsUser>() on HsEntryMeritStudent.IdUserApproved1 equals UserApprovar1.Id into JoinedUserApprovar1
                         from UserApprovar1 in JoinedUserApprovar1.DefaultIfEmpty()
                         join UserApprovar2 in _dbContext.Entity<MsUser>() on HsEntryMeritStudent.IdUserApproved2 equals UserApprovar2.Id into JoinedUserApprovar2
                         from UserApprovar2 in JoinedUserApprovar2.DefaultIfEmpty()
                         join UserApprovar3 in _dbContext.Entity<MsUser>() on HsEntryMeritStudent.IdUserApproved3 equals UserApprovar3.Id into JoinedUserApprovar3
                         from UserApprovar3 in JoinedUserApprovar3.DefaultIfEmpty()
                         where Level.IdAcademicYear == param.IdAcademicYear 
                                        && EntryMeritStudent.MeritUserCreate==param.IdUser 
                                        && EntryMeritStudent.IsHasBeenApproved == true
                                        && EntryMeritStudent.Type==EntryMeritStudentType.Merit
                         select new
                         {
                             Id = EntryMeritStudent.Id,
                             AcademicYear = AcademicYear.Description,
                             Semester = Homeroom.Semester.ToString(),
                             Level = Level.Description,
                             IdLevel = Level.Id,
                             Grade = Grade.Description,
                             IdGrade = Grade.Id,
                             Homeroom = (Grade.Code) + (Classroom.Code),
                             IdHomeroom = Homeroom.Id,
                             IdStudent = Student.Id,
                             NameStudent = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                             LevelOfInfraction = LevelOfInfraction.IdParentLevelOfInteraction == null
                                                    ? LevelOfInfraction != null ? LevelOfInfraction.NameLevelOfInteraction : string.Empty
                                                    : LevelOfInfractionParent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                             NameDecipline = MeritDemeritMapping.DisciplineName,
                             Point = EntryMeritStudent.Point,
                             Note = EntryMeritStudent.Note,
                             CreateBy = User.DisplayName,
                             Approver1 = UserApprovar1.DisplayName == null ? "" : UserApprovar1.DisplayName,
                             Approver2 = UserApprovar2.DisplayName == null ? "" : UserApprovar2.DisplayName,
                             Approver3 = UserApprovar3.DisplayName == null ? "" : UserApprovar3.DisplayName,
                             NoteApprover1 = HsEntryMeritStudent.Note1 == null ? "" : HsEntryMeritStudent.Note1,
                             NoteApprover2 = HsEntryMeritStudent.Note2 == null ? "" : HsEntryMeritStudent.Note2,
                             NoteApprover3 = HsEntryMeritStudent.Note3 == null ? "" : HsEntryMeritStudent.Note3,
                             RequestType = HsEntryMeritStudent.RequestType,
                             RequestReason = HsEntryMeritStudent.RequestReason,
                             Status = HsEntryMeritStudent.Status,
                             Category = MeritDemeritMapping.Category,
                         })
                         .Union(
                            from HsEntryDemeritStudent in _dbContext.Entity<TrStudentDemeritApprovalHs>()
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
                            join UserApprovar1 in _dbContext.Entity<MsUser>() on HsEntryDemeritStudent.IdUserApproved1 equals UserApprovar1.Id into JoinedUserApprovar1
                            from UserApprovar1 in JoinedUserApprovar1.DefaultIfEmpty()
                            join UserApprovar2 in _dbContext.Entity<MsUser>() on HsEntryDemeritStudent.IdUserApproved2 equals UserApprovar2.Id into JoinedUserApprovar2
                            from UserApprovar2 in JoinedUserApprovar2.DefaultIfEmpty()
                            join UserApprovar3 in _dbContext.Entity<MsUser>() on HsEntryDemeritStudent.IdUserApproved3 equals UserApprovar3.Id into JoinedUserApprovar3
                            from UserApprovar3 in JoinedUserApprovar3.DefaultIfEmpty()
                            where Level.IdAcademicYear == param.IdAcademicYear && EntryDemeritStudent.DemeritUserCreate == param.IdUser && EntryDemeritStudent.IsHasBeenApproved==true
                            select new
                            {
                                Id = EntryDemeritStudent.Id,
                                AcademicYear = AcademicYear.Description,
                                Semester = Homeroom.Semester.ToString(),
                                Level = Level.Description,
                                IdLevel = Level.Id,
                                Grade = Grade.Description,
                                IdGrade = Grade.Id,
                                Homeroom = (Grade.Code) + (Classroom.Code),
                                IdHomeroom = Homeroom.Id,
                                IdStudent = Student.Id,
                                NameStudent = (Student.FirstName == null ? "" : Student.FirstName) + (Student.MiddleName == null ? "" : " " + Student.MiddleName) + (Student.LastName == null ? "" : " " + Student.LastName),
                                LevelOfInfraction = LevelOfInfraction.IdParentLevelOfInteraction == null
                                                    ? LevelOfInfraction != null ? LevelOfInfraction.NameLevelOfInteraction : string.Empty
                                                    : LevelOfInfractionParent.NameLevelOfInteraction + LevelOfInfraction.NameLevelOfInteraction,
                                NameDecipline = MeritDemeritMapping.DisciplineName,
                                Point = EntryDemeritStudent.Point,
                                Note = EntryDemeritStudent.Note,
                                CreateBy = User.DisplayName,
                                Approver1 = UserApprovar1.DisplayName == null ? "" : UserApprovar1.DisplayName,
                                Approver2 = UserApprovar2.DisplayName == null ? "" : UserApprovar2.DisplayName,
                                Approver3 = UserApprovar3.DisplayName == null ? "" : UserApprovar3.DisplayName,
                                NoteApprover1 = HsEntryDemeritStudent.Note1 == null ? "" : HsEntryDemeritStudent.Note1,
                                NoteApprover2 = HsEntryDemeritStudent.Note2 == null ? "" : HsEntryDemeritStudent.Note2,
                                NoteApprover3 = HsEntryDemeritStudent.Note3 == null ? "" : HsEntryDemeritStudent.Note3,
                                RequestType = HsEntryDemeritStudent.RequestType,
                                RequestReason = HsEntryDemeritStudent.RequestReason,
                                Status = HsEntryDemeritStudent.Status,
                                Category = MeritDemeritMapping.Category,
                            });

            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                query = query.Where(x => x.Semester == param.Semester.ToString());
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
                case "Approver1":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Approver1)
                        : query.OrderBy(x => x.Approver1);
                    break;
                case "Approver2":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Approver2)
                        : query.OrderBy(x => x.Approver2);
                    break;
                case "Approver3":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Approver3)
                        : query.OrderBy(x => x.Approver3);
                    break;
                case "NoteApprover1":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.NoteApprover1)
                        : query.OrderBy(x => x.NoteApprover1);
                    break;
                case "NoteApprover2":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.NoteApprover2)
                        : query.OrderBy(x => x.NoteApprover2);
                    break;
                case "NoteApprover3":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.NoteApprover3)
                        : query.OrderBy(x => x.NoteApprover3);
                    break;
                case "RequestType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.RequestType)
                        : query.OrderBy(x => x.RequestType);
                    break;
                case "RequestReason":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.RequestReason)
                        : query.OrderBy(x => x.RequestReason);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
            };

            List<GetHistoryApprovalMeritDemeritTeacherResult> items = default;

            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetHistoryApprovalMeritDemeritTeacherResult
                {
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
                    Approver1 = x.Approver1,
                    Approver2 = x.Approver2,
                    Approver3 = x.Approver3,
                    NoteApprover1 = x.NoteApprover1,
                    NoteApprover2 = x.NoteApprover2,
                    NoteApprover3 = x.NoteApprover3,
                    RequestType = x.RequestType,
                    RequestReason = x.RequestReason,
                    Status = x.Status,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                items = result.Select(x => new GetHistoryApprovalMeritDemeritTeacherResult
                {
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
                    Approver1 = x.Approver1,
                    Approver2 = x.Approver2,
                    Approver3 = x.Approver3,
                    NoteApprover1 = x.NoteApprover1,
                    NoteApprover2 = x.NoteApprover2,
                    NoteApprover3 = x.NoteApprover3,
                    RequestType = x.RequestType,
                    RequestReason = x.RequestReason,
                    Status = x.Status,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
