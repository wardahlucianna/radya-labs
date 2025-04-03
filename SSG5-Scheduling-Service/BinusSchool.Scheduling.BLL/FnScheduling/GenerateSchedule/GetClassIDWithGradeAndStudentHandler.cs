using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetClassIDWithGradeAndStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetClassIDWithGradeAndStudentHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassIDByGradeAndStudentRequest>(
               nameof(GetClassIDByGradeAndStudentRequest.IdAscTimetable));

            var result = new GetClassIDByGradeAndStudentResult
            {
                Students = new List<StudentVm>()
            };

            var dataGradeByAsc = _dbContext.Entity<MsGrade>()
                .Include(x => x.Lessons).ThenInclude(x => x.AscTimetableLessons).ThenInclude(x => x.AscTimetable)
                .Where(x => x.Lessons.Any(x => x.AscTimetableLessons.Any(x => x.AscTimetable.Id == param.IdAscTimetable)))
                .Select(x => new {Id = x.Id, Code = x.Code, Description = x.Description})
                .Distinct()
                .ToList();

            var idSessionSet = await _dbContext.Entity<TrAscTimetable>().Where(x => x.Id == param.IdAscTimetable).Select(x => x.IdSessionSet).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(param.IdStudent))
            {
                //var homeroomByGrade = await _dbContext.Entity<MsHomeroom>()
                //    .Include(x => x.Grade)
                //    .Include(x => x.GradePathwayClassroom)
                //        .ThenInclude(x => x.Classroom)
                //.Where(x => x.IdGrade == param.IdGrade)
                //.Select(x => x.Id)
                //.ToListAsync(CancellationToken);
                //var lessonByGrade = await _dbContext.Entity<MsLesson>()
                //    .Where(x => x.IdGrade == param.IdGrade)
                //    .Where(x => x.TotalPerWeek != 0)
                //    .Select(x => x.Id)
                //    .ToListAsync(CancellationToken);
                //var lessonByAsctimetable =
                //    await _dbContext.Entity<TrAscTimetableLesson>()
                //    .Include(x => x.Lesson)
                //    .Where(x => lessonByGrade.Contains(x.IdLesson))
                //    .Where(x => !string.IsNullOrWhiteSpace(param.ClassID) ? x.Lesson.ClassIdGenerated == param.ClassID : true)
                //    .Where(x => x.IdAscTimetable == param.IdAscTimetable)
                //    .GroupBy(x => x.IdLesson)
                //    .Select(x => x.Key)
                //    .ToListAsync(CancellationToken);

                var predicatePeriod = PredicateBuilder.Create<MsPeriod>(x => true);
                var predicateLesson = PredicateBuilder.Create<MsLesson>(x => true);

                if(!string.IsNullOrWhiteSpace(param.IdGrade))
                {
                    predicatePeriod = predicatePeriod.And(x => x.IdGrade == param.IdGrade);
                    predicateLesson = predicateLesson.And(x => x.IdGrade == param.IdGrade);
                }
                else
                {
                    predicatePeriod = predicatePeriod.And(x => dataGradeByAsc.First().Id == x.IdGrade);
                    predicateLesson = predicateLesson.And(x => dataGradeByAsc.Select(y => y.Id).ToList().Contains(x.IdGrade));
                }

                var periodByGrade = await _dbContext.Entity<MsPeriod>()
                    .Include(x => x.Grade)
                    .Where(predicatePeriod)
                    .ToListAsync(CancellationToken);

                var dataByGrade = await _dbContext.Entity<MsLesson>()
                    .Include(x => x.AscTimetableLessons)
                    .Where(x => x.AscTimetableLessons.Any(y => y.IdAscTimetable == param.IdAscTimetable))
                    .Where(predicateLesson)
                    // .Where(x => x.IdGrade == param.IdGrade)
                    .Where(x => !string.IsNullOrEmpty(param.ClassID) ? x.ClassIdGenerated == param.ClassID : true)
               .ToListAsync(CancellationToken);
                var idLessons = dataByGrade.Select(x => x.Id).Distinct().ToList();
                var subjectByLesson = await (from _subject in _dbContext.Entity<MsSubject>()
                                             join _lesson in _dbContext.Entity<MsLesson>() on _subject.Id equals _lesson.IdSubject
                                             where
                                                idLessons.Contains(_lesson.Id)
                                             select new
                                             {
                                                 idSubject = _subject.Id,
                                                 codeSubject = _subject.Code,
                                                 descriptionSubject = _subject.Description,
                                                 idLesson = _lesson.Id
                                             }).ToListAsync();
                var homeroomByLesson = await (from _lessonPathway in _dbContext.Entity<MsLessonPathway>()
                                              join _homeroomPathway in _dbContext.Entity<MsHomeroomPathway>() on _lessonPathway.IdHomeroomPathway equals _homeroomPathway.Id
                                              join _gradePathwayDetail in _dbContext.Entity<MsGradePathwayDetail>() on _homeroomPathway.IdGradePathwayDetail equals _gradePathwayDetail.Id
                                              join _pathway in _dbContext.Entity<MsPathway>() on _gradePathwayDetail.IdPathway equals _pathway.Id
                                              join _homeroom in _dbContext.Entity<MsHomeroom>() on _homeroomPathway.IdHomeroom equals _homeroom.Id
                                              join _grade in _dbContext.Entity<MsGrade>() on _homeroom.IdGrade equals _grade.Id
                                              join _gradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on _homeroom.IdGradePathwayClassRoom equals _gradePathwayClassroom.Id
                                              join _classRoom in _dbContext.Entity<MsClassroom>() on _gradePathwayClassroom.IdClassroom equals _classRoom.Id
                                              where
                                                  idLessons.Contains(_lessonPathway.IdLesson)
                                              group _homeroom by new
                                              {
                                                  idHomeroom = _homeroom.Id,
                                                  codeClassroom = _classRoom.Code,
                                                  idLesson = _lessonPathway.IdLesson,
                                                  codeGrade = _grade.Code
                                              } into homeroomLesson
                                              select new
                                              {
                                                  idHomeroom = homeroomLesson.Key.idHomeroom,
                                                  codeClassroom = homeroomLesson.Key.codeClassroom,
                                                  idLesson = homeroomLesson.Key.idLesson,
                                                  codeGrade = homeroomLesson.Key.codeGrade
                                              }

                                        ).ToListAsync();
                var scheduleByLesson = await (from _schedule in _dbContext.Entity<MsSchedule>()
                                              join _lesson in _dbContext.Entity<MsLesson>() on _schedule.IdLesson equals _lesson.Id
                                              join _venue in _dbContext.Entity<MsVenue>() on _schedule.IdVenue equals _venue.Id
                                              join _weekVarianDetail in _dbContext.Entity<MsWeekVariantDetail>() on _schedule.IdWeekVarianDetail equals _weekVarianDetail.Id
                                              join _week in _dbContext.Entity<MsWeek>() on _weekVarianDetail.IdWeek equals _week.Id
                                              join _session in _dbContext.Entity<MsSession>() on _schedule.IdSession equals _session.Id
                                              join _day in _dbContext.Entity<LtDay>() on _schedule.IdDay equals _day.Id
                                              join _user in _dbContext.Entity<MsStaff>() on _schedule.IdUser equals _user.IdBinusian
                                              where
                                                  idLessons.Contains(_schedule.IdLesson)
                                                  && _session.IdSessionSet == idSessionSet
                                                  && _schedule.AscTimetableSchedules.Any(e=>e.IdAscTimetable== param.IdAscTimetable)
                                              select new
                                              {
                                                  idWeekVarianDetail = _weekVarianDetail.Id,
                                                  idWeek = _week.Id,
                                                  codeWeek = _week.Code,
                                                  descriptionWeek = _week.Description,
                                                  idLesson = _schedule.IdLesson,
                                                  codeLesson = _lesson.ClassIdGenerated,
                                                  semesterLesson = _lesson.Semester,
                                                  idSession = _schedule.IdSession,
                                                  sessionId = _session.SessionID,
                                                  startTime = _session.StartTime,
                                                  endTime = _session.EndTime,
                                                  dayofweek = _day.Description,
                                                  semester = _schedule.Semester,
                                                  idUser = _user.IdBinusian,
                                                  shortName = _user.ShortName,
                                                  firstName = _user.FirstName,
                                                  lastName = _user.LastName,
                                                  idVenue = _venue.Id,
                                                  codeVenue = _venue.Code,
                                                  descriptionVenue = _venue.Description
                                              }
                                        ).ToListAsync();

                var _student = new StudentVm();

                var _listStudent = new StudentVm();

                if(string.IsNullOrWhiteSpace(param.IdGrade))
                {
                    var noStudent = 0;
                    foreach (var gradeAsc in dataGradeByAsc)
                    {
                         _listStudent = new StudentVm
                        {
                            StudentId = "RL00"+noStudent,
                            StudentName = "John Doe "+noStudent,
                            Grade = new GradeWithPeriodVm
                            {
                                Id = gradeAsc.Id,
                                Code = gradeAsc.Code,
                                Description = gradeAsc.Description,
                                Periods = periodByGrade.Where(x => x.IdGrade == gradeAsc.Id).Select(x => new PeriodVm
                                {
                                    AttendanceStartDate = x.AttendanceStartDate,
                                    AttendanceEndDate = x.AttendanceEndDate,
                                    Semester = x.Semester
                                }).ToList()
                            }
                        };

                        noStudent++;

                        foreach (var item in dataByGrade.Where(y => y.IdGrade == gradeAsc.Id).GroupBy(x => x.Id))
                        {
                            var _classID = new LessonClassIDVM();
                            _classID.IdLesson = item.Key;
                            _classID.Subject = subjectByLesson.Where(x => x.idLesson == item.Key)
                                .Select(x =>
                                new CodeWithIdVm
                                {
                                    Id = x.idSubject,
                                    Code = x.codeSubject,
                                    Description = x.descriptionSubject,
                                }).FirstOrDefault();
                            _classID.Homeroom = homeroomByLesson.Where(x => x.idLesson == item.Key)
                                .Select(x => new CodeWithIdVm
                                {
                                    Id = x.idHomeroom,
                                    Code = x.codeGrade,
                                    Description = string.Join(",", homeroomByLesson.Where(x => x.idLesson == item.Key).Select(x => x.codeClassroom).ToList())
                                }).FirstOrDefault();
                            _classID.ClassIdFormat = item.FirstOrDefault().ClassIdGenerated;
                            var schedules = scheduleByLesson.Where(x => x.idLesson == item.Key).GroupBy(x => new
                            {
                                x.idWeekVarianDetail,
                                x.idWeek,
                                x.codeWeek,
                                x.descriptionWeek
                            }).ToList();
                            foreach (var schdule in schedules)
                            {
                                var _schedule = new WeekNewVm();
                                _schedule.IdWeek = schdule.Key.idWeek;
                                _schedule.Week = new CodeWithIdVm
                                {
                                    Id = schdule.Key.idWeek,
                                    Code = schdule.Key.codeWeek,
                                    Description = schdule.Key.descriptionWeek
                                };
                                _schedule.WeekNewDetails = new List<WeekNewDetailVm>();
                                var sessions = scheduleByLesson.Where(x => x.idWeekVarianDetail == schdule.Key.idWeekVarianDetail && x.idLesson == item.Key).GroupBy(x => new
                                {
                                    x.idSession,
                                    x.semester,
                                    x.idLesson,
                                    x.codeLesson,
                                    x.semesterLesson,
                                    x.sessionId,
                                    x.startTime,
                                    x.endTime,
                                    x.dayofweek,
                                    x.idUser,
                                    x.firstName,
                                    x.lastName,
                                    x.shortName,
                                    x.idVenue,
                                    x.codeVenue,
                                    x.descriptionVenue
                                })
                                .ToList();
                                foreach (var session in sessions)
                                {
                                    WeekNewDetailVm weekNewDetail = new WeekNewDetailVm();
                                    weekNewDetail.Semester = session.Key.semester;
                                    weekNewDetail.Lesson = new CodeWithIdVm
                                    {
                                        Id = session.Key.idLesson,
                                        Code = session.Key.codeLesson,
                                        Description = session.Key.semesterLesson.ToString()
                                    };
                                    weekNewDetail.Session = new SessionVm
                                    {
                                        Id = session.Key.idSession,
                                        SessionID = session.Key.sessionId.ToString(),
                                        StartTime = session.Key.startTime,
                                        EndTime = session.Key.endTime,
                                        Dayofweek = session.Key.dayofweek
                                    };
                                    weekNewDetail.Teacher = new CodeWithIdVm
                                    {
                                        Id = session.Key.idUser,
                                        Code = session.Key.shortName,
                                        Description = string.Format("{0} {1}", session.Key.firstName, session.Key.lastName)
                                    };
                                    weekNewDetail.Venue = new CodeWithIdVm
                                    {
                                        Id = session.Key.idVenue,
                                        Code = session.Key.codeVenue,
                                        Description = session.Key.descriptionVenue
                                    };
                                    _schedule.WeekNewDetails.Add(weekNewDetail);
                                }
                                _classID.WeeksNew.Add(_schedule);
                            }
                            foreach (var schedule in schedules)
                            {
                                var sessions = scheduleByLesson.Where(x => x.idWeekVarianDetail == schedule.Key.idWeekVarianDetail && x.idLesson == item.Key).GroupBy(x => new
                                {
                                    x.idSession,
                                    x.semester,
                                    x.idLesson,
                                    x.codeLesson,
                                    x.semesterLesson,
                                    x.sessionId,
                                    x.startTime,
                                    x.endTime,
                                    x.dayofweek,
                                    x.idUser,
                                    x.firstName,
                                    x.lastName,
                                    x.shortName,
                                    x.idVenue,
                                    x.codeVenue,
                                    x.descriptionVenue,
                                    x.idWeek,
                                    x.codeWeek,
                                    x.descriptionWeek
                                })
                                .ToList();
                                foreach (var session in sessions)
                                {
                                    var weekNew = new WeekVm();
                                    weekNew.IdWeek = session.Key.idWeek;
                                    weekNew.Week = new CodeWithIdVm
                                    {
                                        Id = session.Key.idWeek,
                                        Code = session.Key.codeWeek,
                                        Description = session.Key.descriptionWeek
                                    };
                                    weekNew.Semester = session.Key.semester;
                                    weekNew.Lesson = new CodeWithIdVm
                                    {
                                        Id = session.Key.idLesson,
                                        Code = session.Key.codeLesson,
                                        Description = session.Key.semesterLesson.ToString()
                                    };
                                    weekNew.Session = new SessionVm
                                    {
                                        Id = session.Key.idSession,
                                        SessionID = session.Key.sessionId.ToString(),
                                        StartTime = session.Key.startTime,
                                        EndTime = session.Key.endTime,
                                        Dayofweek = session.Key.dayofweek
                                    };
                                    weekNew.Teacher = new CodeWithIdVm
                                    {
                                        Id = session.Key.idUser,
                                        Code = session.Key.shortName,
                                        Description = string.Format("{0} {1}", session.Key.firstName, session.Key.lastName)
                                    };
                                    weekNew.Venue = new CodeWithIdVm
                                    {
                                        Id = session.Key.idVenue,
                                        Code = session.Key.codeVenue,
                                        Description = session.Key.descriptionVenue
                                    };
                                    _classID.Weeks2.Add(weekNew);
                                }

                            }
                            if (_classID.WeeksNew.Count > 0 && _classID.Weeks2.Count >0)
                            {
                                _listStudent.ClassIds.Add(_classID);
                            }
                        }

                        result.Students.Add(_listStudent);
                    };
                }
                else
                {
                        _student.StudentId = "RL000";
                        _student.StudentName = "John Doe";
                        _student.Grade = new GradeWithPeriodVm
                        {
                            Id = periodByGrade.FirstOrDefault().Grade.Id,
                            Code = periodByGrade.FirstOrDefault().Grade.Code,
                            Description = periodByGrade.FirstOrDefault().Grade.Description,
                            Periods = periodByGrade.Select(x => new PeriodVm
                            {
                                AttendanceStartDate = x.AttendanceStartDate,
                                AttendanceEndDate = x.AttendanceEndDate,
                                Semester = x.Semester
                            }).ToList()
                        };

                    result.Students.Add(_student);

                    foreach (var item in dataByGrade.GroupBy(x => x.Id))
                    {
                        var _classID = new LessonClassIDVM();
                        _classID.IdLesson = item.Key;
                        _classID.Subject = subjectByLesson.Where(x => x.idLesson == item.Key)
                            .Select(x =>
                            new CodeWithIdVm
                            {
                                Id = x.idSubject,
                                Code = x.codeSubject,
                                Description = x.descriptionSubject,
                            }).FirstOrDefault();
                        _classID.Homeroom = homeroomByLesson.Where(x => x.idLesson == item.Key)
                            .Select(x => new CodeWithIdVm
                            {
                                Id = x.idHomeroom,
                                Code = x.codeGrade,
                                Description = string.Join(",", homeroomByLesson.Where(x => x.idLesson == item.Key).Select(x => x.codeClassroom).ToList())
                            }).FirstOrDefault();
                        _classID.ClassIdFormat = item.FirstOrDefault().ClassIdGenerated;
                        var schedules = scheduleByLesson.Where(x => x.idLesson == item.Key).GroupBy(x => new
                        {
                            x.idWeekVarianDetail,
                            x.idWeek,
                            x.codeWeek,
                            x.descriptionWeek
                        }).ToList();
                        foreach (var schdule in schedules)
                        {
                            var _schedule = new WeekNewVm();
                            _schedule.IdWeek = schdule.Key.idWeek;
                            _schedule.Week = new CodeWithIdVm
                            {
                                Id = schdule.Key.idWeek,
                                Code = schdule.Key.codeWeek,
                                Description = schdule.Key.descriptionWeek
                            };
                            _schedule.WeekNewDetails = new List<WeekNewDetailVm>();
                            var sessions = scheduleByLesson.Where(x => x.idWeekVarianDetail == schdule.Key.idWeekVarianDetail && x.idLesson == item.Key).GroupBy(x => new
                            {
                                x.idSession,
                                x.semester,
                                x.idLesson,
                                x.codeLesson,
                                x.semesterLesson,
                                x.sessionId,
                                x.startTime,
                                x.endTime,
                                x.dayofweek,
                                x.idUser,
                                x.firstName,
                                x.lastName,
                                x.shortName,
                                x.idVenue,
                                x.codeVenue,
                                x.descriptionVenue
                            })
                            .ToList();
                            foreach (var session in sessions)
                            {
                                WeekNewDetailVm weekNewDetail = new WeekNewDetailVm();
                                weekNewDetail.Semester = session.Key.semester;
                                weekNewDetail.Lesson = new CodeWithIdVm
                                {
                                    Id = session.Key.idLesson,
                                    Code = session.Key.codeLesson,
                                    Description = session.Key.semesterLesson.ToString()
                                };
                                weekNewDetail.Session = new SessionVm
                                {
                                    Id = session.Key.idSession,
                                    SessionID = session.Key.sessionId.ToString(),
                                    StartTime = session.Key.startTime,
                                    EndTime = session.Key.endTime,
                                    Dayofweek = session.Key.dayofweek
                                };
                                weekNewDetail.Teacher = new CodeWithIdVm
                                {
                                    Id = session.Key.idUser,
                                    Code = session.Key.shortName,
                                    Description = string.Format("{0} {1}", session.Key.firstName, session.Key.lastName)
                                };
                                weekNewDetail.Venue = new CodeWithIdVm
                                {
                                    Id = session.Key.idVenue,
                                    Code = session.Key.codeVenue,
                                    Description = session.Key.descriptionVenue
                                };
                                _schedule.WeekNewDetails.Add(weekNewDetail);
                            }
                            _classID.WeeksNew.Add(_schedule);
                        }
                        foreach (var schedule in schedules)
                        {
                            var sessions = scheduleByLesson.Where(x => x.idWeekVarianDetail == schedule.Key.idWeekVarianDetail && x.idLesson == item.Key).GroupBy(x => new
                            {
                                x.idSession,
                                x.semester,
                                x.idLesson,
                                x.codeLesson,
                                x.semesterLesson,
                                x.sessionId,
                                x.startTime,
                                x.endTime,
                                x.dayofweek,
                                x.idUser,
                                x.firstName,
                                x.lastName,
                                x.shortName,
                                x.idVenue,
                                x.codeVenue,
                                x.descriptionVenue,
                                x.idWeek,
                                x.codeWeek,
                                x.descriptionWeek
                            })
                            .ToList();
                            foreach (var session in sessions)
                            {
                                var weekNew = new WeekVm();
                                weekNew.IdWeek = session.Key.idWeek;
                                weekNew.Week = new CodeWithIdVm
                                {
                                    Id = session.Key.idWeek,
                                    Code = session.Key.codeWeek,
                                    Description = session.Key.descriptionWeek
                                };
                                weekNew.Semester = session.Key.semester;
                                weekNew.Lesson = new CodeWithIdVm
                                {
                                    Id = session.Key.idLesson,
                                    Code = session.Key.codeLesson,
                                    Description = session.Key.semesterLesson.ToString()
                                };
                                weekNew.Session = new SessionVm
                                {
                                    Id = session.Key.idSession,
                                    SessionID = session.Key.sessionId.ToString(),
                                    StartTime = session.Key.startTime,
                                    EndTime = session.Key.endTime,
                                    Dayofweek = session.Key.dayofweek
                                };
                                weekNew.Teacher = new CodeWithIdVm
                                {
                                    Id = session.Key.idUser,
                                    Code = session.Key.shortName,
                                    Description = string.Format("{0} {1}", session.Key.firstName, session.Key.lastName)
                                };
                                weekNew.Venue = new CodeWithIdVm
                                {
                                    Id = session.Key.idVenue,
                                    Code = session.Key.codeVenue,
                                    Description = session.Key.descriptionVenue
                                };
                                _classID.Weeks2.Add(weekNew);
                            }

                        }
                        if (_classID.WeeksNew.Count > 0 && _classID.Weeks2.Count >0)
                        {
                            _student.ClassIds.Add(_classID);
                        }
                    }
                }

                // result.Students.Add(_student);
            }
            else
            {
                var predicate = PredicateBuilder.Create<MsHomeroomStudentEnrollment>(p
              => p.Lesson.TotalPerWeek != 0
                                           && p.Lesson.AscTimetableLessons.Any(x => x.IdAscTimetable == param.IdAscTimetable));

                if(!string.IsNullOrWhiteSpace(param.IdGrade))
                {
                    predicate = predicate.And(x => x.HomeroomStudent.Homeroom.IdGrade == param.IdGrade && x.Lesson.IdGrade == param.IdGrade);
                }
                else
                {
                    predicate = predicate.And(x => dataGradeByAsc.Select(y => y.Id).ToList().Contains(x.HomeroomStudent.Homeroom.IdGrade) && dataGradeByAsc.Select(y => y.Id).ToList().Contains(x.Lesson.IdGrade));
                    
                }

                var currentSemester = await _dbContext.Entity<MsPeriod>()
                .Where(x => x.IdGrade == param.IdGrade)
                .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
                .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
                .Select(x => new
               {
                   Semester = x.Semester
               }).FirstOrDefaultAsync();

                predicate = predicate.And(x => x.HomeroomStudent.Homeroom.Semester == currentSemester.Semester);

                if (!string.IsNullOrWhiteSpace(param.IdStudent))
                    predicate = predicate.And(p => p.HomeroomStudent.IdStudent == param.IdStudent);
                if (!string.IsNullOrWhiteSpace(param.ClassID))
                    predicate = predicate.And(p => p.Lesson.ClassIdGenerated == param.ClassID);
                var getData2 = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                    .Include(p => p.HomeroomStudent)
                                        .ThenInclude(p => p.Homeroom)
                                    .Include(p => p.HomeroomStudent).
                                        ThenInclude(x => x.Student)
                                    .Include(p => p.Lesson)
                                        .ThenInclude(p => p.Grade)
                                            .ThenInclude(x => x.Periods)
                                    .Include(p => p.Lesson)
                                        .ThenInclude(p => p.Subject)
                                    .Include(p => p.HomeroomStudent)
                                        .ThenInclude(p => p.Homeroom)
                                            .ThenInclude(p => p.GradePathwayClassroom)
                                                .ThenInclude(p => p.Classroom)
                                    .Include(p => p.Lesson)
                                        .ThenInclude(p => p.Schedules)
                                            .ThenInclude(p => p.WeekVarianDetail.Week)
                                    .Include(p => p.Lesson)
                                        .ThenInclude(p => p.Schedules)
                                            .ThenInclude(p => p.User)
                                    .Include(p => p.Lesson)
                                        .ThenInclude(p => p.Schedules)
                                            .ThenInclude(p => p.Sessions)
                                    //.ThenInclude(p => p.SessionSet)
                                    .Include(p => p.Lesson)
                                        .ThenInclude(p => p.Schedules)
                                            .ThenInclude(p => p.Sessions)
                                                .ThenInclude(x => x.Day)
                                    .Include(p => p.Lesson)
                                        .ThenInclude(p => p.Schedules)
                                            .ThenInclude(p => p.Venue)
                                    .Include(x => x.Lesson)
                                        .ThenInclude(x => x.LessonPathways)
                                            .ThenInclude(x => x.HomeroomPathway)
                                                .ThenInclude(x => x.Homeroom)
                                                    .ThenInclude(x => x.GradePathwayClassroom)
                                                        .ThenInclude(x => x.Classroom)
                                    .Include(x => x.Lesson)
                                        .ThenInclude(x => x.LessonPathways)
                                            .ThenInclude(x => x.HomeroomPathway)
                                                .ThenInclude(x => x.GradePathwayDetail)
                                                    .ThenInclude(x => x.Pathway)
                                    //.Include(p => p.Lesson)
                                    //    .ThenInclude(p => p.AscTimetableLessons)
                                    //.ThenInclude(p => p.AscTimetable)
                                    //    .ThenInclude(x => x.SessionSet)
                                    .Where(predicate);

                if (!getData2.Any())
                {
                    return Request.CreateApiResult2(result as object);
                }
                var grpupData = getData2.AsEnumerable().GroupBy(p => new
                {
                    p.HomeroomStudent.IdStudent,
                    p.HomeroomStudent.Student.FirstName
                }).ToList();

                foreach (var item in grpupData)
                {
                    var _studen = new StudentVm();
                    _studen.StudentId = item.Key.IdStudent;
                    _studen.StudentName = item.Key.FirstName;
                    _studen.Grade = new GradeWithPeriodVm
                    {
                        Id = item.FirstOrDefault().Lesson.IdGrade,
                        Code = item.FirstOrDefault().Lesson.Grade.Code,
                        Description = item.FirstOrDefault().Lesson.Grade.Description,
                        Periods = item.FirstOrDefault().Lesson.Grade.Periods.OrderBy(x => x.Semester).Select(x => new PeriodVm
                        {
                            AttendanceStartDate = x.AttendanceStartDate,
                            AttendanceEndDate = x.AttendanceEndDate,
                            Semester = x.Semester
                        }).ToList()
                    };
                    var classIds = item.GroupBy(x => x.Lesson.IdSubject).ToList();
                    foreach (var itemClassiDs in classIds)
                    {
                        var _classID = new LessonClassIDVM();
                        _classID.IdLesson = itemClassiDs.First().IdLesson; //TODO : confirmation why this object never assigned
                        _classID.Subject = new CodeWithIdVm
                        {
                            Id = itemClassiDs.Key,
                            Code = itemClassiDs.FirstOrDefault().Subject.SubjectID,
                            Description = itemClassiDs.FirstOrDefault().Subject.Description,
                        };

                        var desc1 = string.Join(",", item.Where(x => x.Lesson.Id == _classID.IdLesson).FirstOrDefault().Lesson.LessonPathways.OrderBy(x => x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
                            .Select(x => string.Format("{0} {1}{2}", x.HomeroomPathway.Homeroom.Grade.Code,
                            x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                            x.HomeroomPathway.GradePathwayDetail.Pathway.Code.Equals("No Pathway", StringComparison.OrdinalIgnoreCase))));

                        _classID.Homeroom = new CodeWithIdVm
                        {
                            Id = item.FirstOrDefault().HomeroomStudent.Homeroom.Id,
                            Code = item.FirstOrDefault().HomeroomStudent.Homeroom.Grade.Code,
                            Description = string.Join(",", item.Where(x => x.Lesson.Id == _classID.IdLesson).FirstOrDefault().Lesson.LessonPathways.OrderBy(x => x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
                            .Select(x => string.Format("{0} {1}{2}", x.HomeroomPathway.Homeroom.Grade.Code,
                            x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                            x.HomeroomPathway.GradePathwayDetail.Pathway.Code.Equals("No Pathway", StringComparison.OrdinalIgnoreCase)
                            ? string.Empty
                            : " " + x.HomeroomPathway.GradePathwayDetail.Pathway.Code)))
                        };
                        _classID.ClassIdFormat = itemClassiDs.FirstOrDefault().Lesson.ClassIdGenerated;
                        var semester1 = itemClassiDs.FirstOrDefault().Lesson.Schedules;
                        var semester2 = itemClassiDs.LastOrDefault().Lesson.Schedules;

                        var scheduleUsing = semester1.Count > 0 ? semester1 : semester2;

                        //foreach (var x in scheduleUsing)
                        //{
                        //    var _schedule = new WeekVm();
                        //    _schedule.Semester = x.Semester;
                        //    _schedule.IdWeek = x.IdWeekVarianDetail;
                        //    _schedule.Week = new CodeWithIdVm
                        //    {
                        //        Id = x.WeekVarianDetail.IdWeek,
                        //        Code = x.WeekVarianDetail.Week.Code,
                        //        Description = x.WeekVarianDetail.Week.Description,
                        //    };
                        //    _schedule.Session = new SessionVm
                        //    {
                        //        Id = x.Sessions.Id,
                        //        SessionID = x.Sessions.SessionID.ToString(),
                        //        StartTime = x.Sessions.StartTime,
                        //        EndTime = x.Sessions.EndTime,
                        //        Dayofweek = x.Sessions.Day.Description
                        //    };
                        //    _schedule.Teacher = new CodeWithIdVm
                        //    {
                        //        Id = x.IdUser,
                        //        Code = x.User.ShortName,
                        //        Description = string.Format("{0} {1}", x.User.FirstName, x.User.LastName)
                        //    };
                        //    _schedule.Venue = new CodeWithIdVm
                        //    {
                        //        Id = x.IdVenue,
                        //        Code = x.Venue.Code,
                        //        Description = x.Venue.Description
                        //    };
                        //    _classID.Weeks.Add(_schedule);
                        //}
                        //var idSessionSet = itemClassiDs.Select(x => x.Lesson.Schedules.Select(x => x.Sessions.IdSessionSet).FirstOrDefault()).FirstOrDefault();
                        if (idSessionSet != null)
                        {
                            foreach (var x in scheduleUsing.Where(x => x.Sessions.IdSessionSet == idSessionSet).GroupBy(x => x.WeekVarianDetail))
                            {
                                var _schedule = new WeekNewVm();
                                _schedule.IdWeek = x.Key.IdWeek;
                                _schedule.Week = new CodeWithIdVm
                                {
                                    Id = x.Key.IdWeek,
                                    Code = x.Key.Week.Code,
                                    Description = x.Key.Week.Description,
                                };
                                _schedule.WeekNewDetails = new List<WeekNewDetailVm>();
                                foreach (var s in x
                                    .Where(x => x.Sessions.IdSessionSet == idSessionSet)
                                    .GroupBy(x => x)
                                    .OrderBy(x => x.Key.Sessions.StartTime)
                                    .ThenBy(x => x.Key.Day.Code.Length)
                                    .Select(x => x).ToList())
                                {
                                    WeekNewDetailVm weekNewDetail = new WeekNewDetailVm();
                                    weekNewDetail.Semester = s.Key.Semester;
                                    weekNewDetail.Session = new SessionVm
                                    {
                                        Id = s.Key.Sessions.IdSessionSet,
                                        SessionID = s.Key.Sessions.SessionID.ToString(),
                                        StartTime = s.Key.Sessions.StartTime,
                                        EndTime = s.Key.Sessions.EndTime,
                                        Dayofweek = s.Key.Sessions.Day.Description
                                    };
                                    weekNewDetail.Lesson = new CodeWithIdVm
                                    {
                                        Id = s.Key.Lesson.Id,
                                        Code = s.Key.Lesson.ClassIdGenerated,
                                        Description = s.Key.Lesson.Semester.ToString()
                                    };
                                    weekNewDetail.Teacher = new CodeWithIdVm
                                    {
                                        Id = s.Key.IdUser,
                                        Code = s.Key.User.ShortName,
                                        Description = string.Format("{0} {1}", s.Key.User.FirstName, s.Key.User.LastName)
                                    };
                                    weekNewDetail.Venue = new CodeWithIdVm
                                    {
                                        Id = s.Key.IdVenue,
                                        Code = s.Key.Venue.Code,
                                        Description = s.Key.Venue.Description
                                    };
                                    _schedule.WeekNewDetails.Add(weekNewDetail);


                                }
                                _classID.WeeksNew.Add(_schedule);
                            }
                            foreach (var schedules in itemClassiDs.Select(x => x.Lesson.Schedules))
                            {
                                foreach (var itemSchedule in schedules.OrderBy(x => x.Sessions.StartTime).ThenBy(x => x.Day.Code.Length).Where(x => x.Sessions.IdSessionSet == idSessionSet).ToList())
                                {
                                    var _schedule = new WeekVm();
                                    _schedule.Semester = itemSchedule.Semester;
                                    _schedule.IdWeek = itemSchedule.IdWeekVarianDetail;
                                    _schedule.Week = new CodeWithIdVm
                                    {
                                        Id = itemSchedule.WeekVarianDetail.IdWeek,
                                        Code = itemSchedule.WeekVarianDetail.Week.Code,
                                        Description = itemSchedule.WeekVarianDetail.Week.Description,
                                    };
                                    _schedule.Session = new SessionVm
                                    {
                                        Id = itemSchedule.Sessions.Id,
                                        SessionID = itemSchedule.Sessions.SessionID.ToString(),
                                        StartTime = itemSchedule.Sessions.StartTime,
                                        EndTime = itemSchedule.Sessions.EndTime,
                                        Dayofweek = itemSchedule.Sessions.Day.Description
                                    };
                                    _schedule.Teacher = new CodeWithIdVm
                                    {
                                        Id = itemSchedule.IdUser,
                                        Code = itemSchedule.User.ShortName,
                                        Description = string.Format("{0} {1}", itemSchedule.User.FirstName, itemSchedule.User.LastName)
                                    };
                                    _schedule.Venue = new CodeWithIdVm
                                    {
                                        Id = itemSchedule.IdVenue,
                                        Code = itemSchedule.Venue.Code,
                                        Description = itemSchedule.Venue.Description
                                    };
                                    _schedule.Lesson = new CodeWithIdVm
                                    {
                                        Id = itemSchedule.Lesson.Id,
                                        Code = itemSchedule.Lesson.ClassIdGenerated,
                                        Description = ""
                                    };
                                    _classID.Weeks2.Add(_schedule);
                                }
                            }
                            _studen.ClassIds.Add(_classID);
                        }
                    }
                    result.Students.Add(_studen);
                }
            }
            return Request.CreateApiResult2(result as object);
        }
    }
}
