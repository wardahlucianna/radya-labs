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
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Auth.Abstractions;
using Microsoft.Azure.Cosmos.Linq;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using BinusSchool.Data.Model.Attendance.FnAttendance.LevelByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.GradeByPosition;
using BinusSchool.Common.Constants;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BinusSchool.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Student.FnStudent;

namespace BinusSchool.Scheduling.FnSchedule.StudentCertification
{
    public class GetListStudentCertificationHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListStudentCertificationRequest.IdAcadyear),
        };
        private static readonly string[] _columns = { "AcadYear", "Level", "Grade", "Homeroom", "Period", "StudentName", "BinusianId" };
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _datetime;

        public GetListStudentCertificationHandler(ISchedulingDbContext SchoolEventDbContext, IMachineDateTime datetime)
        {
            _dbContext = SchoolEventDbContext;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListStudentCertificationRequest>();

            //var query = await (from s in _dbContext.Entity<MsStudent>()
            //                   join sg in _dbContext.Entity<MsStudentGrade>() on s.Id equals sg.IdStudent
            //                   join g in _dbContext.Entity<MsGrade>() on sg.IdGrade equals g.Id
            //                   //join p in _dbContext.Entity<MsPeriod>() on g.Id equals p.IdGrade
            //                   join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
            //                   join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
            //                   join hs in _dbContext.Entity<MsHomeroomStudent>() on s.Id equals hs.IdStudent
            //                   join h in _dbContext.Entity<MsHomeroom>() on hs.IdHomeroom equals h.Id
            //                   join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
            //                   join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
            //                   where h.IdAcademicYear == param.IdAcadyear

            //                   select new GetListStudentCertificationResult
            //                   {
            //                       Id = s.Id,
            //                       //AcadYear = ay.Description,
            //                       AcadYear = h.AcademicYear.Description,
            //                       IdLevel = l.Id,
            //                       Level = l.Description,
            //                       IdGrade = g.Id,
            //                       Grade = g.Description,
            //                       IdHomeroom = h.Id,
            //                       Homeroom = g.Code + c.Description,
            //                       //IdPeriod = p.Id,
            //                       Period = h.Semester.ToString(),
            //                       StudentName = s.FirstName != null ? s.FirstName + " " + s.LastName : s.LastName,
            //                       BinusianId = s.Id
            //                   })
            //                      .Distinct()
            //                      .ToListAsync(CancellationToken);


            var idHomeRoom = new List<String>();

            var LevelIds = new List<ItemValueVm>();
            var GradeId = new List<ItemValueVm>();
            var idLevelPrincipalAndVicePrincipal = new List<string>();

            idHomeRoom = param.Position == "Class Advisor" || param.Position == "Co-Teacher" || string.IsNullOrEmpty(param.Position)
                ? _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(x => x.Homeroom)
                                .Where(x => x.IdBinusian == AuthInfo.UserId && x.Homeroom.IdAcademicYear == param.IdAcadyear)
                                .Select(x => x.IdHomeroom).ToList()
                : _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(x => x.Homeroom)
                                .Where(x => x.Homeroom.IdAcademicYear == param.IdAcadyear)
                                .Select(x => x.IdHomeroom).ToList();

            var Position = await _dbContext.Entity<LtPosition>()
                .Where(x => x.Description == param.Position)
                .Select(x => x.Code).FirstOrDefaultAsync(CancellationToken);

            var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                .Where(x => x.IdBinusian == AuthInfo.UserId)
                .Where(x => x.Homeroom.IdAcademicYear == param.IdAcadyear)
                .Distinct()
                .OrderBy(x => x.Homeroom.Grade.OrderNumber)
                .Select(x => x).ToListAsync(CancellationToken);

            var dataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                .Include(x => x.Lesson)
                    .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                .Where(x => x.IdUser == AuthInfo.UserId)
                .Where(x => x.Lesson.IdAcademicYear == param.IdAcadyear)
                .Distinct()
                .OrderBy(x => x.Lesson.Grade.Level.OrderNumber)
                .Select(x => x).ToListAsync(CancellationToken);


            var positionUser = await _dbContext.Entity<TrNonTeachingLoad>().Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.Position)
                .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear)
                .Where(x => x.IdUser == AuthInfo.UserId)
                .Select(x => new
                {
                    x.Data,
                    x.MsNonTeachingLoad.TeacherPosition.Position.Code
                }).ToListAsync(CancellationToken);

            if (positionUser.Count > 0)
            {
                if (Position == PositionConstant.Principal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.Principal)) //check P Or VP
                    {
                        if (positionUser.Where(y => y.Code == PositionConstant.Principal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.Principal).Count() > 0)
                        {
                            var Principal = positionUser.Where(x => x.Code == PositionConstant.Principal).ToList();

                            foreach (var item in Principal)
                            {
                                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);
                                idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
                            }
                            LevelIds = await _dbContext.Entity<MsLevel>()
                                .Where(x => idLevelPrincipalAndVicePrincipal.Contains(x.Id))
                                .OrderBy(x => x.AcademicYear.OrderNumber)
                                .ThenBy(x => x.OrderNumber)
                                .Select(x => new ItemValueVm
                                {
                                    Id = x.Id,
                                    Description = x.Description
                                }).ToListAsync(CancellationToken);
                        }
                    }
                }
                if (Position == PositionConstant.VicePrincipal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.VicePrincipal))
                    {
                        if (positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).Count() > 0)
                        {
                            var Principal = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();
                            List<string> LevelId = new List<string>();
                            foreach (var item in Principal)
                            {
                                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);
                                idLevelPrincipalAndVicePrincipal.Add(_levelLH.Id);
                            }
                            LevelIds = await _dbContext.Entity<MsLevel>()
                            .Where(x => idLevelPrincipalAndVicePrincipal.Contains(x.Id))
                            .OrderBy(x => x.AcademicYear.OrderNumber)
                            .ThenBy(x => x.OrderNumber)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Id,
                                Description = x.Description
                            }).ToListAsync(CancellationToken);
                        }
                    }
                }
                if (Position == PositionConstant.LevelHead)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.LevelHead).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.LevelHead).ToList();
                        var idLevel = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);
                            _dataNewLH.TryGetValue("Grade", out var _gradeLH);
                            idLevel.Add(_levelLH.Id);
                        }
                        LevelIds = await _dbContext.Entity<MsLevel>()
                            .Where(x => idLevel.Contains(x.Id))
                            .OrderBy(x => x.AcademicYear.OrderNumber)
                            .ThenBy(x => x.OrderNumber)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Id,
                                Description = x.Description
                            }).ToListAsync(CancellationToken);
                    }
                }
                if (Position == PositionConstant.SubjectHead)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.SubjectHead).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHead).ToList();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        List<string> IdLevel = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Level", out var _leveltSH);
                            _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                            IdLevel.Add(_leveltSH.Id);
                        }
                        LevelIds = await _dbContext.Entity<MsLevel>()
                        .Include(x => x.MappingAttendances)
                        .Where(x => IdLevel.Contains(x.Id))
                        .Where(x => x.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
                        .OrderBy(x => x.AcademicYear.OrderNumber)
                        .ThenBy(x => x.OrderNumber)
                        .Select(x => new ItemValueVm
                        {
                            Id = x.Id,
                            Description = x.Description
                        }).ToListAsync(CancellationToken);
                    }
                }
                if (Position == PositionConstant.SubjectHeadAssitant)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.SubjectHeadAssitant).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHeadAssitant).ToList();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        List<string> IdLevel = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Level", out var _leveltSH);
                            _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                            _dataNewSH.TryGetValue("Department", out var _departmentSH);
                            _dataNewSH.TryGetValue("Subject", out var _subjectSH);
                            IdLevel.Add(_leveltSH.Id);
                        }
                        LevelIds = await _dbContext.Entity<MsLevel>()
                        .Include(x => x.MappingAttendances)
                        .Where(x => IdLevel.Contains(x.Id))
                        .Where(x => x.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
                        .OrderBy(x => x.AcademicYear.OrderNumber)
                        .ThenBy(x => x.OrderNumber)
                        .Select(x => new ItemValueVm
                        {
                            Id = x.Id,
                            Description = x.Description
                        }).ToListAsync(CancellationToken);

                        var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
                        GradeId = await _dbContext.Entity<MsLessonPathway>()
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.Grade)
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.GradePathwayClassroom)
                                        .ThenInclude(x => x.Classroom)
                            .Where(x => IdLevel.Contains(x.Lesson.Grade.IdLevel))
                            .Where(x => idLessons.Contains(x.IdLesson))
                            .GroupBy(x => new
                            {
                                orderNumber = x.HomeroomPathway.Homeroom.Grade.OrderNumber,
                                x.HomeroomPathway.Homeroom.IdGrade,
                                grade = x.HomeroomPathway.Homeroom.Grade.Code
                            })
                            .OrderBy(x => x.Key.orderNumber)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Key.IdGrade,
                                Description = x.Key.grade
                            }).ToListAsync(CancellationToken);
                    }
                }
                if (Position == PositionConstant.HeadOfDepartment)
                {
                    List<string> IdLevel = new List<string>();
                    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                    {
                        IdLevel = dataLessonTeacher.GroupBy(x => new
                        {
                            x.Lesson.Grade.IdLevel,
                            x.Lesson.Grade.Level.Description
                        }).Select(x => x.Key.IdLevel).ToList();
                        LevelIds = await _dbContext.Entity<MsLevel>()
                                  .Include(x => x.AcademicYear)
                                  .Include(x => x.MappingAttendances)
                                  .Where(x => x.IdAcademicYear == param.IdAcadyear)
                                  .Where(x => IdLevel.Contains(x.Id))
                                  //.Where(x => x.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
                                  .OrderBy(x => x.AcademicYear.OrderNumber)
                                    .ThenBy(x => x.OrderNumber)
                                      .ThenBy(x => x.OrderNumber)
                              .Select(x => new ItemValueVm
                              {
                                  Id = x.Id,
                                  Description = x.Description
                              }).ToListAsync(CancellationToken);
                        var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
                        GradeId = await _dbContext.Entity<MsLessonPathway>()
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.Grade)
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.GradePathwayClassroom)
                                        .ThenInclude(x => x.Classroom)
                            .Where(x => IdLevel.Contains(x.Lesson.Grade.IdLevel))
                            .Where(x => idLessons.Contains(x.IdLesson))
                            .GroupBy(x => new
                            {
                                orderNumber = x.HomeroomPathway.Homeroom.Grade.OrderNumber,
                                x.HomeroomPathway.Homeroom.IdGrade,
                                grade = x.HomeroomPathway.Homeroom.Grade.Code
                            })
                            .OrderBy(x => x.Key.orderNumber)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Key.IdGrade,
                                Description = x.Key.grade
                            }).ToListAsync(CancellationToken);
                    }
                }
                if (Position == PositionConstant.ClassAdvisor || Position == PositionConstant.CoTeacher)
                {
                    List<string> IdLevel = new List<string>();
                    if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                    {
                        IdLevel = dataHomeroomTeacher
                        .GroupBy(x => new
                        {
                            x.Homeroom.Grade.IdLevel,
                            x.Homeroom.Grade.Level.Description
                        })
                        .Select(x => x.Key.IdLevel).ToList();

                        LevelIds = await _dbContext.Entity<MsLevel>()
                                   .Include(x => x.AcademicYear)
                                   .Where(x => x.IdAcademicYear == param.IdAcadyear)
                                   .Where(x => IdLevel.Contains(x.Id))
                                   .OrderBy(x => x.AcademicYear.OrderNumber)
                                       .ThenBy(x => x.OrderNumber)
                               .Select(x => new ItemValueVm
                               {
                                   Id = x.Id,
                                   Description = x.Description
                               }).ToListAsync(CancellationToken);
                    }
                }
            }
            else
            {
                if (Position == PositionConstant.SubjectTeacher)
                {
                    List<string> IdLevel = new List<string>();
                    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                    {
                        IdLevel = dataLessonTeacher.GroupBy(x => new
                        {
                            x.Lesson.Grade.IdLevel,
                            x.Lesson.Grade.Level.Description
                        }).Select(x => x.Key.IdLevel).ToList();
                        LevelIds = await _dbContext.Entity<MsLevel>()
                                  .Include(x => x.AcademicYear)
                                  .Include(x => x.MappingAttendances)
                                  .Where(x => x.IdAcademicYear == param.IdAcadyear)
                                  .Where(x => IdLevel.Contains(x.Id))
                                  //.Where(x => x.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session))
                                  .OrderBy(x => x.AcademicYear.OrderNumber)
                                    .ThenBy(x => x.OrderNumber)
                                      .ThenBy(x => x.OrderNumber)
                              .Select(x => new ItemValueVm
                              {
                                  Id = x.Id,
                                  Description = x.Description
                              }).ToListAsync(CancellationToken);
                        var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
                        GradeId = await _dbContext.Entity<MsLessonPathway>()
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.Grade)
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.GradePathwayClassroom)
                                        .ThenInclude(x => x.Classroom)
                            .Where(x => IdLevel.Contains(x.Lesson.Grade.IdLevel))
                            .Where(x => idLessons.Contains(x.IdLesson))
                            .GroupBy(x => new
                            {
                                orderNumber = x.HomeroomPathway.Homeroom.Grade.OrderNumber,
                                x.HomeroomPathway.Homeroom.IdGrade,
                                grade = x.HomeroomPathway.Homeroom.Grade.Code
                            })
                            .OrderBy(x => x.Key.orderNumber)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Key.IdGrade,
                                Description = x.Key.grade
                            }).ToListAsync(CancellationToken);
                    }
                }
            }

            var termPeriod = param.IdPeriod == "1" ? "2" : "4";

            var queryHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                .Include(e => e.Lesson)
                .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == param.IdAcadyear).ToListAsync(CancellationToken);

            var queryData = await (from h in _dbContext.Entity<MsHomeroom>()
                               join hs in _dbContext.Entity<MsHomeroomStudent>() on h.Id equals hs.IdHomeroom
                               join s in _dbContext.Entity<MsStudent>() on hs.IdStudent equals s.Id
                               join g in _dbContext.Entity<MsGrade>() on h.IdGrade equals g.Id
                               join p in _dbContext.Entity<MsPeriod>() on g.Id equals p.IdGrade
                               join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                               //join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id                              
                               join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
                               join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                               where h.IdAcademicYear == param.IdAcadyear && h.Semester.ToString() == param.IdPeriod 
                                    && p.Semester.ToString() == param.IdPeriod 
                                    && (p.Description == termPeriod || p.Description == String.Concat("Term ", termPeriod))

                               select new
                               {
                                   Id = s.Id,
                                   //AcadYear = ay.Description,
                                   AcadYear = h.AcademicYear.Description,
                                   IdLevel = l.Id,
                                   Level = l.Description,
                                   IdGrade = g.Id,
                                   Grade = g.Description,
                                   IdHomeroom = h.Id,
                                   Homeroom = g.Code + c.Description,
                                   //IdPeriod = p.Id,
                                   Period = h.Semester.ToString(),
                                   StudentName = s.FirstName != null ? s.FirstName + " " + s.LastName : s.LastName,
                                   BinusianId = s.Id,
                                   IdHomeroomStudent = hs.Id,
                                   PeriodStartDate = p.StartDate,
                                   PeriodEndDate = p.EndDate
                               })
              .Distinct()
              .ToListAsync(CancellationToken);

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                               .Include(e => e.Student)
                               .Where(e => e.IdAcademicYear == param.IdAcadyear
                                       && e.ActiveStatus
                                       && e.StartDate.Date <= _datetime.ServerTime.Date
                                       && (e.EndDate == null || e.EndDate >= _datetime.ServerTime.Date)
                                       && (queryData.Select(y => y.Id).ToList().Any(y => y == e.IdStudent))
                                       )
                               .Select(e => new
                               {
                                   e.IdStudent,
                                   e.StartDate,
                                   EndDate = e.EndDate == null
                                              ? _datetime.ServerTime.Date
                                              : Convert.ToDateTime(e.EndDate),
                               })
                               .ToListAsync(CancellationToken);

            var listIdStudent = listStudentStatus.Select(e => e.IdStudent).ToList();

            var query = queryData
                .Where(e => listIdStudent.Contains(e.BinusianId))
                .Select(x => new GetListStudentCertificationResult
                {
                    AcadYear = x.AcadYear,
                    IdLevel = x.IdLevel,
                    Level = x.Level,
                    IdGrade = x.IdGrade,
                    Grade = x.Grade,
                    IdHomeroom = x.IdHomeroom,
                    Homeroom = x.Homeroom,
                    Period = x.Period,
                    StudentName = x.StudentName,
                    BinusianId = x.BinusianId,
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    Id = x.Id
                })
                .ToList();

            if (queryHomeroomStudentEnrollment.Count > 0)
            {
                foreach (var item in query)
                {
                    var getListClassId = queryHomeroomStudentEnrollment.Where(x => x.IdHomeroomStudent == item.IdHomeroomStudent).Select(y => y.Lesson != null ? y.Lesson.ClassIdGenerated : "").Distinct().ToList();

                    item.ClassIds = getListClassId;
                }
            }

            if (LevelIds.Count > 0)
            {
                query = query.Where(x => LevelIds.Select(x=>x.Id).Contains(x.IdLevel)).ToList();
            }

            if (GradeId.Count > 0)
            {
                query = query.Where(x => GradeId.Select(x => x.Id).Contains(x.IdGrade)).ToList();
            }

            if (idHomeRoom.Count > 0 )
                query = query.Where(x => idHomeRoom.Contains(x.IdHomeroom)).ToList();

            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel).ToList();

            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade).ToList();

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom).ToList();

            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.AcadYear.ToLower().Contains(param.Search.ToLower()) || x.Level.ToLower().Contains(param.Search.ToLower()) || x.Grade.ToLower().Contains(param.Search.ToLower()) || x.Homeroom.ToLower().Contains(param.Search.ToLower()) || x.Period.ToLower().Contains(param.Search.ToLower()) || x.StudentName.ToLower().Contains(param.Search.ToLower()) || x.BinusianId.ToLower().Contains(param.Search.ToLower())).ToList();
            
            if (!string.IsNullOrEmpty(param.ClassId))
                query = query.Where(x => x.ClassIds.Contains(param.ClassId)).ToList();

            //ordering
            switch (param.OrderBy)
            {
                case "AcadYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcadYear).ToList()
                        : query.OrderBy(x => x.AcadYear).ToList();
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level).ToList()
                        : query.OrderBy(x => x.Level).ToList();
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade).ToList()
                        : query.OrderBy(x => x.Grade).ToList();
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom).ToList()
                        : query.OrderBy(x => x.Homeroom).ToList();
                    break;
                case "Period":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Period).ToList()
                        : query.OrderBy(x => x.Period).ToList();
                    break;
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName).ToList()
                        : query.OrderBy(x => x.StudentName).ToList();
                    break;
                case "BinusianId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusianId).ToList()
                        : query.OrderBy(x => x.BinusianId).ToList();
                    break;
            };


            var dataStudentPagination = query
                .SetPagination(param)
                .Select(x => new GetListStudentCertificationResult
                {
                    Id = x.Id,
                    AcadYear = x.AcadYear,
                    IdLevel = x.IdLevel,
                    Level = x.Level,
                    IdGrade = x.IdGrade,
                    Grade = x.Grade,
                    IdHomeroom = x.IdHomeroom,
                    Homeroom = x.Homeroom,
                    //IdPeriod = x.IdPeriod,
                    Period = x.Period,
                    StudentName = x.StudentName,
                    BinusianId = x.BinusianId,
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    ClassIds = x.ClassIds
                })
                .ToList();
            var count = param.CanCountWithoutFetchDb(dataStudentPagination.Count)
                ? dataStudentPagination.Count
                : query.Count;

            return Request.CreateApiResult2(dataStudentPagination as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
