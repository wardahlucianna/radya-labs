using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeacherHomeroomsV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetTeacherHomeroomsV2Handler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherHomeroomsRequest>(nameof(GetTeacherHomeroomsRequest.IdUser));
            var columns = new[] { "description" };

            var listSchedule = new List<MsSchedule>();
            var listIdLesson = new List<string>();
            IReadOnlyList<IItemValueVm> items = null;
            int count = 0;

            var queryListHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>().Include(x => x.Homeroom)
                .Where(x => x.IdBinusian == param.IdUser && x.Homeroom.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.IdAcadYear))
                queryListHomeroomTeacher = queryListHomeroomTeacher.Where(x => x.Homeroom.IdAcademicYear == param.IdAcadYear);
            if (!string.IsNullOrEmpty(param.IdGrade))
                queryListHomeroomTeacher = queryListHomeroomTeacher.Where(x => x.Homeroom.IdGrade == param.IdGrade);

            var queryHomeroomTeacher = await queryListHomeroomTeacher.ToListAsync(CancellationToken);

            var queryTrNonTeachingLoad = await _dbContext.Entity<TrNonTeachingLoad>()
                .Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition)
                .Where(x => x.IdUser == param.IdUser && x.MsNonTeachingLoad.IdAcademicYear == param.IdAcadYear).ToListAsync(CancellationToken);

            var query = await GetHomeroomStudentEnrollment(param, _dbContext, CancellationToken);

            if (!queryTrNonTeachingLoad.Any() && !queryHomeroomTeacher.Any())
            {
                //ambil data dari lesson
                var querySchedule = _dbContext.Entity<MsSchedule>()
                                .Include(e => e.Lesson).ThenInclude(e => e.Grade)
                                .Where(e => e.IdUser == param.IdUser);

                if (!string.IsNullOrWhiteSpace(param.IdAcadYear))
                    querySchedule = querySchedule.Where(x => x.Lesson.IdAcademicYear == param.IdAcadYear);

                if (!string.IsNullOrWhiteSpace(param.IdGrade))
                    querySchedule = querySchedule.Where(x => x.Lesson.IdGrade == param.IdGrade);

                if (param.Semester.HasValue)
                    querySchedule = querySchedule.Where(x => x.Semester == param.Semester);

                listSchedule = await querySchedule.ToListAsync(CancellationToken);
                listIdLesson = listSchedule.Select(e => e.IdLesson).Distinct().ToList();

                var queryResult = query.Where(e => listIdLesson.Contains(e.IdLesson))
                                      .GroupBy(e => new
                                      {
                                          e.Homeroom.Id,
                                          codeGrade = e.Grade.Code,
                                          codeClassroom = e.ClassroomCode,
                                          IdAcademicYear = e.AcademicYear.Id,
                                          IdGrade = e.Grade.Id,
                                          Semester = e.Semester
                                      })
                                      .Select(x => new
                                      {
                                          IdHomeroom = x.Key.Id,
                                          HomeroomName = x.Key.codeGrade + x.Key.codeClassroom,
                                          x.Key.IdAcademicYear,
                                          x.Key.IdGrade,
                                          x.Key.Semester
                                      })
                                      .Distinct();

                if (!string.IsNullOrWhiteSpace(param.Search))
                    queryResult = queryResult.Where(x => x.HomeroomName.Contains(param.Search));

                if (!string.IsNullOrEmpty(param.OrderBy))
                {
                    if (param.OrderType == OrderType.Asc)
                        queryResult = queryResult.OrderBy(x => x.HomeroomName);
                    else
                        queryResult = queryResult.OrderByDescending(x => x.HomeroomName);
                }
                else
                    queryResult = queryResult.OrderBy(x => x.HomeroomName);


                if (param.Return == CollectionType.Lov)
                    items = queryResult
                        .Select(x => new CodeWithIdVm(x.IdHomeroom, x.HomeroomName, x.HomeroomName))
                        .ToList();
                else
                    items = queryResult
                        .SetPagination(param)
                        .Select(x => new CodeWithIdVm(x.IdHomeroom, x.HomeroomName, x.HomeroomName))
                        .ToList();

                count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : queryResult.Count();
            }
            else if (queryHomeroomTeacher.Any() && !queryTrNonTeachingLoad.Any())
            {
                //ambil data dari homeroomteacher
                var queryHomeRoomTeacher = await GetHomeroomTeacher(param, _dbContext, CancellationToken);
                var queryResult = queryHomeRoomTeacher/*.Where(e => Level.Select(x => x.Id).Contains(e.Level.Id))*/
                                      .GroupBy(e => new
                                      {
                                          e.Homeroom.Id,
                                          codeGrade = e.Grade.Code,
                                          codeClassroom = e.ClassroomCode,
                                          IdAcademicYear = e.AcademicYear.Id,
                                          IdGrade = e.Grade.Id,
                                          Semester = e.Semester
                                      })
                                      .Select(x => new
                                      {
                                          IdHomeroom = x.Key.Id,
                                          HomeroomName = x.Key.codeGrade + x.Key.codeClassroom,
                                          x.Key.IdAcademicYear,
                                          x.Key.IdGrade,
                                          x.Key.Semester
                                      }).Distinct();

                if (!string.IsNullOrWhiteSpace(param.Search))
                    queryResult = queryResult.Where(x => x.HomeroomName.Contains(param.Search));

                if (!string.IsNullOrEmpty(param.OrderBy))
                {
                    if (param.OrderType == OrderType.Asc)
                        queryResult = queryResult.OrderBy(x => x.HomeroomName);
                    else
                        queryResult = queryResult.OrderByDescending(x => x.HomeroomName);
                }
                else
                    queryResult = queryResult.OrderBy(x => x.HomeroomName);


                if (param.Return == CollectionType.Lov)
                    items = queryResult
                        .Select(x => new CodeWithIdVm(x.IdHomeroom, x.HomeroomName, x.HomeroomName)).ToList();
                else
                    items = queryResult
                        .SetPagination(param)
                        .Select(x => new CodeWithIdVm(x.IdHomeroom, x.HomeroomName, x.HomeroomName))
                        .ToList();

                count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : queryResult.Count();
            }
            else
            {
                var Level = new List<ItemValueVm>();

                //diluar ST dan CA
                foreach (var itemData in queryTrNonTeachingLoad.Select(x => x.Data))
                {
                    var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(itemData);
                    _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                    _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                    _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);

                    Level.Add(_LevelPosition);
                }

                var queryResult = query.Where(e => Level.Select(x => x.Id).Contains(e.Level.Id))
                                      .GroupBy(e => new
                                      {
                                          e.Homeroom.Id,
                                          codeGrade = e.Grade.Code,
                                          codeClassroom = e.ClassroomCode,
                                          IdAcademicYear = e.AcademicYear.Id,
                                          IdGrade = e.Grade.Id,
                                          Semester = e.Semester
                                      })
                                      .Select(x => new
                                      {
                                          IdHomeroom = x.Key.Id,
                                          HomeroomName = x.Key.codeGrade + x.Key.codeClassroom,
                                          x.Key.IdAcademicYear,
                                          x.Key.IdGrade,
                                          x.Key.Semester
                                      })
                                      .Distinct();

                if (!string.IsNullOrWhiteSpace(param.Search))
                    queryResult = queryResult.Where(x => x.HomeroomName.Contains(param.Search));

                if (!string.IsNullOrEmpty(param.OrderBy))
                {
                    if (param.OrderType == OrderType.Asc)
                        queryResult = queryResult.OrderBy(x => x.HomeroomName);
                    else
                        queryResult = queryResult.OrderByDescending(x => x.HomeroomName);
                }
                else
                    queryResult = queryResult.OrderBy(x => x.HomeroomName);


                if (param.Return == CollectionType.Lov)
                    items = queryResult
                        .Select(x => new CodeWithIdVm(x.IdHomeroom, x.HomeroomName, x.HomeroomName)).ToList();
                else
                    items = queryResult
                        .SetPagination(param)
                        .Select(x => new CodeWithIdVm(x.IdHomeroom, x.HomeroomName, x.HomeroomName))
                        .ToList();

                count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : queryResult.Count();
            }

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
        public static async Task<List<GetHomeroom>> GetHomeroomTeacher(GetTeacherHomeroomsRequest param, ISchedulingDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {
            var listHomeroomTeacher = new List<GetHomeroom>();

            var queryHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                                    .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                    .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                                    .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                    .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                    .Include(e => e.Staff)
                                    .Where(x => x.Homeroom.IdAcademicYear == param.IdAcadYear);

            if (!string.IsNullOrWhiteSpace(param.IdUser))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Staff.IdBinusian == param.IdUser);

            if (!string.IsNullOrWhiteSpace(param.IdGrade))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Grade.Id == param.IdGrade);

            if (param.Semester.HasValue)
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Semester == param.Semester);

            if (param.Ids != null && param.Ids.Any())
                queryHomeroomTeacher = queryHomeroomTeacher.Where(x => param.Ids.Contains(x.Homeroom.Id));

            listHomeroomTeacher = await queryHomeroomTeacher
                                        .GroupBy(e => new GetHomeroom
                                        {
                                            Homeroom = new ItemValueVm
                                            {
                                                Id = e.Homeroom.Id,
                                                Description = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code
                                            },
                                            IdClassroom = e.Homeroom.GradePathwayClassroom.Classroom.Id,
                                            ClassroomCode = e.Homeroom.GradePathwayClassroom.Classroom.Code,
                                            ClassroomName = e.Homeroom.GradePathwayClassroom.Classroom.Description,
                                            Grade = new CodeWithIdVm
                                            {
                                                Id = e.Homeroom.IdGrade,
                                                Code = e.Homeroom.Grade.Code,
                                                Description = e.Homeroom.Grade.Description
                                            },
                                            Level = new CodeWithIdVm
                                            {
                                                Id = e.Homeroom.Grade.Level.Id,
                                                Code = e.Homeroom.Grade.Level.Code,
                                                Description = e.Homeroom.Grade.Level.Description
                                            },
                                            AcademicYear = new CodeWithIdVm
                                            {
                                                Id = e.Homeroom.Grade.Level.AcademicYear.Id,
                                                Code = e.Homeroom.Grade.Level.AcademicYear.Code,
                                                Description = e.Homeroom.Grade.Level.AcademicYear.Description,
                                            },
                                            Semester = e.Homeroom.Semester,
                                            BinusianID = e.Staff.IdBinusian

                                        })
                                        .Select(e => e.Key)
                                        .ToListAsync(CancellationToken);
            return listHomeroomTeacher;
        }

        public static async Task<List<GetHomeroom>> GetHomeroomStudentEnrollment(GetTeacherHomeroomsRequest param, ISchedulingDbContext _dbContext, System.Threading.CancellationToken CancellationToken)
        {
            var listHomeroomStudentEnrollment = new List<GetHomeroom>();

            var queryHomeroomStudentEnrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                    .Include(e => e.Lesson)
                    .Include(e => e.Subject)
                    .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == param.IdAcadYear);

            if (!string.IsNullOrWhiteSpace(param.IdGrade))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.Homeroom.Grade.Id == param.IdGrade);

            if (param.Semester.HasValue)
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.Semester == param.Semester);

            if (param.Ids != null && param.Ids.Any())
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(x => param.Ids.Contains(x.Id));

            listHomeroomStudentEnrollment = await queryHomeroomStudentEnrollment
                .AsNoTracking()
                .GroupBy(e => new GetHomeroom
                {
                    Homeroom = new ItemValueVm
                    {
                        Id = e.HomeroomStudent.IdHomeroom,
                        Description = e.HomeroomStudent.Homeroom.Grade.Code + e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                    },
                    IdClassroom = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                    ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    ClassroomName = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                    Grade = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.IdGrade,
                        Code = e.HomeroomStudent.Homeroom.Grade.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                        Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                    },
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.Grade.Level.AcademicYear.Id,
                        Code = e.HomeroomStudent.Homeroom.Grade.Level.AcademicYear.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Level.AcademicYear.Description,
                    },
                    Semester = e.HomeroomStudent.Homeroom.Semester,
                    IdLesson = e.IdLesson
                })
                .Select(e => e.Key)
                .ToListAsync(CancellationToken);

            return listHomeroomStudentEnrollment;
        }
    }
}
