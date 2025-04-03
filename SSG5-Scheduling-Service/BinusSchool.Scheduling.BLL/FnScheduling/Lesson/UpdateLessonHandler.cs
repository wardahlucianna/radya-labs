using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.Lesson.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class UpdateLessonHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;

        public UpdateLessonHandler(ISchedulingDbContext dbContext, IMessage messageService, IEventSchool eventSchool)
        {
            _dbContext = dbContext;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateLessonRequest, UpdateLessonValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var existLesson = await _dbContext.Entity<MsLesson>()
                .Include(x => x.AcademicYear)
                .Include(x => x.LessonPathways)
                .Include(x => x.LessonTeachers)
                .Include(x => x.Schedules).ThenInclude(x => x.AscTimetableSchedules)
                .FirstOrDefaultAsync(x => x.Id == body.Id, CancellationToken);
            if (existLesson is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Lesson"], "Id", body.Id));

            var existLessonSimilar = await _dbContext.Entity<MsLesson>()
                .Where(x
                    => x.Id != body.Id
                    && x.IdAcademicYear == existLesson.IdAcademicYear
                    && x.Semester == existLesson.Semester
                    && x.IdGrade == existLesson.IdGrade
                    && x.IdSubject == existLesson.IdSubject)
                .ToListAsync(CancellationToken);
            var existClassIds = existLessonSimilar.Where(x => body.ClassIdGenerated == x.ClassIdGenerated);
            if (existClassIds.Any())
                throw new BadRequestException(string.Format(Localizer["ExAlreadyExist"], Localizer["Lesson"], "ClassID", body.ClassIdGenerated));

            // if semester 1, update semester 2 too
            var existLesson2 = default(MsLesson);
            if (existLesson.Semester == 1)
            {
                existLesson2 = await _dbContext.Entity<MsLesson>()
                    .Include(x => x.LessonPathways)
                    .Include(x => x.LessonTeachers)
                    .Include(x => x.Schedules).ThenInclude(x => x.AscTimetableSchedules)
                    .FirstOrDefaultAsync(x
                        => x.IdAcademicYear == existLesson.IdAcademicYear
                        && x.Semester == 2
                        && x.IdGrade == existLesson.IdGrade
                        && x.IdSubject == existLesson.IdGrade
                        && x.ClassIdGenerated == existLesson.ClassIdGenerated,
                        CancellationToken);
            }

            var updatedTeacher = new List<MsLessonTeacher>();
            var AddTeacher = new List<MsLessonTeacher>();
            var removeScheduleTeacher = new List<MsLessonTeacher>();

            foreach (var lesson in new[] { existLesson, existLesson2 })
            {
                if (lesson != null)
                {
                    lesson.ClassIdGenerated = body.ClassIdGenerated;
                    lesson.IdWeekVariant = body.IdWeekVarian;
                    lesson.TotalPerWeek = body.TotalPerWeek;
                    lesson.HomeroomSelected = string.Join(", ", body.Homerooms.Select(x => x.Homeroom));
                    _dbContext.Entity<MsLesson>().Update(lesson);

                    foreach (var teacher in body.Teachers)
                    {
                        // select existing teacher to update
                        var existTeacher = lesson.LessonTeachers.FirstOrDefault(x => x.IdUser == teacher.IdTeacher);

                        // create new if not found
                        if (existTeacher is null)
                        {
                            var newLessonTeacher = new MsLessonTeacher
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdLesson = lesson.Id,
                                IdUser = teacher.IdTeacher,
                                IsAttendance = teacher.HasAttendance,
                                IsScore = teacher.HasScore,
                                IsPrimary = teacher.IsPrimary,
                                IsClassDiary = teacher.IsClassDiary,
                                IsLessonPlan = teacher.IsLessonPlan,
                            };
                            _dbContext.Entity<MsLessonTeacher>().Add(newLessonTeacher);
                            AddTeacher.Add(newLessonTeacher);
                        }
                        else
                        {
                            if (existTeacher.IsAttendance == false && teacher.HasAttendance == true)
                            {
                                existTeacher.IsAttendance = teacher.HasAttendance;
                                //AddTeacher.Add(existTeacher);
                            }else if(existTeacher.IsAttendance == true && teacher.HasAttendance == false)
                            {
                                existTeacher.IsAttendance = teacher.HasAttendance;
                                //removeScheduleTeacher.Add(existTeacher);
                            }

                            updatedTeacher.Add(existTeacher);

                            existTeacher.IdUser = teacher.IdTeacher;
                            existTeacher.IsAttendance = teacher.HasAttendance;
                            existTeacher.IsScore = teacher.HasScore;
                            existTeacher.IsPrimary = teacher.IsPrimary;
                            existTeacher.IsClassDiary = teacher.IsClassDiary;
                            existTeacher.IsLessonPlan = teacher.IsLessonPlan;

                            _dbContext.Entity<MsLessonTeacher>().Update(existTeacher);
                        }
                    }

                    // inactive schedule
                    // foreach (var schedule in lesson.Schedules)
                    // {
                    //     schedule.IsActive = false;
                    //     _dbContext.Entity<MsSchedule>().Update(schedule);

                    //     foreach (var ascSchedule in schedule.AscTimetableSchedules)
                    //     {
                    //         ascSchedule.IsActive = false;
                    //         _dbContext.Entity<TrAscTimetableSchedule>().Update(ascSchedule);
                    //     }
                    // }

                    //foreach (var schedule in lesson.Schedules)
                    //{
                    //    schedule.IdUser = body.Teachers.Where(x => x.IsPrimary == true).First().IdTeacher;
                    //    _dbContext.Entity<MsSchedule>().Update(schedule);
                    //}
                }
            }

            // remove unupdated lesson teacher
            var unupdatedTeacher = existLesson.LessonTeachers
                .Where(x => x.IsActive) // except new teacher
                .Except(updatedTeacher)
                .ToList();
            if (unupdatedTeacher.Count > 0)
            {
                foreach (var unupdated in unupdatedTeacher)
                {
                    unupdated.IsActive = false;
                    _dbContext.Entity<MsLessonTeacher>().Update(unupdated);
                }
            }

            #region Schedule
            //get schedule by lesson
            var listSchedule = await _dbContext.Entity<MsSchedule>()
                               .Where(x => x.IdLesson == body.Id)
                               .ToListAsync(CancellationToken);

            var listScheduleGroup = listSchedule
                                .GroupBy(e => new
                                {
                                    e.IdLesson,
                                    e.IdVenue,
                                    e.IdWeekVarianDetail,
                                    e.IdSession,
                                    e.IdDay,
                                    e.SessionNo,
                                    e.Semester,
                                    e.IdWeek,
                                })
                                .Select(e=>e.Key)
                                .ToList();

            //get data for asc lesson
            var listIdAsc = await _dbContext.Entity<TrAscTimetableLesson>()
                    .Where(x => x.IdLesson == body.Id)
                    .ToListAsync(CancellationToken);

            //add schedule
            foreach (var schedule in listScheduleGroup)
            {
                foreach(var teacher in AddTeacher.Where(e=>e.IsAttendance).ToList())
                {
                    var exsisSchedule = listSchedule.Where(e => e.IdUser == teacher.IdUser && e.IdLesson == teacher.IdLesson && e.IdDay == schedule.IdDay && e.IdSession == schedule.IdSession).ToList();
                    if (exsisSchedule.Any())
                        continue;

                    MsSchedule newSchedule = new MsSchedule
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdLesson = schedule.IdLesson,
                        IdVenue = schedule.IdVenue,
                        IdWeekVarianDetail = schedule.IdWeekVarianDetail,
                        IdSession = schedule.IdSession,
                        IdUser = teacher.IdUser,
                        SessionNo = schedule.SessionNo,
                        Semester = schedule.Semester,
                        IdDay = schedule.IdDay,
                        IdWeek = schedule.IdWeek,
                    };

                    _dbContext.Entity<MsSchedule>().Add(newSchedule);

                    foreach (var idAsc in listIdAsc)
                    {
                        TrAscTimetableSchedule newAscSchedule = new TrAscTimetableSchedule
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdSchedule = newSchedule.Id,
                            IdAscTimetable = idAsc.IdAscTimetable,
                            IsFromMaster = true
                        };
                        _dbContext.Entity<TrAscTimetableSchedule>().Add(newAscSchedule);
                    }
                }
            }

            //remove schedule
            var listIdTeacherNoRemove = updatedTeacher.Union(AddTeacher).Select(e => e.IdUser).ToList();
            var listScheduleRemove = listSchedule.Where(e => !listIdTeacherNoRemove.Contains(e.IdUser)).ToList();
            listScheduleRemove.ForEach(e => e.IsActive = false);

            _dbContext.Entity<MsSchedule>().UpdateRange(listScheduleRemove);

            var listIdTeacherRemove = removeScheduleTeacher.Select(e => e.IdUser).ToList();
            var uncheckAttendanceRemovSchedulee = listSchedule.Where(e => listIdTeacherRemove.Contains(e.IdUser)).ToList();
            uncheckAttendanceRemovSchedulee.ForEach(e => e.IsActive = false);

            _dbContext.Entity<MsSchedule>().UpdateRange(uncheckAttendanceRemovSchedulee);

            #endregion


            var idHomerooms = body.Homerooms.Select(x => x.IdHomeroom);
            var existHomerooms = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.HomeroomPathways)
                .Where(x => idHomerooms.Contains(x.Id))
                .ToListAsync(CancellationToken);


            var updatedPathway = new List<MsLessonPathway>();
            foreach (var homeroom in body.Homerooms)
            {
                var existHomeroom = existHomerooms.Find(x => homeroom.IdHomeroom == x.Id);
                if (existHomeroom is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", homeroom.IdHomeroom));

                foreach (var pathway in homeroom.IdPathways)
                {
                    // select existing pathway to update
                    var existPathway = existLesson.LessonPathways.FirstOrDefault(x => x.IdHomeroomPathway == pathway);
                    if (existPathway is null)
                    {
                        var newLessonPathway = new MsLessonPathway
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdLesson = existLesson.Id,
                            IdHomeroomPathway = pathway
                        };
                        _dbContext.Entity<MsLessonPathway>().Add(newLessonPathway);
                    }
                    else
                    {
                        updatedPathway.Add(existPathway);

                        existPathway.IdHomeroomPathway = pathway;
                        _dbContext.Entity<MsLessonPathway>().Update(existPathway);
                    }
                }
            }

            // remove unupdated lesson pathway
            var unupdatedPathway = existLesson.LessonPathways
                .Where(x => x.IsActive) // except new teacher
                .Except(updatedPathway)
                .ToArray();
            if (unupdatedPathway.Length != 0)
            {
                foreach (var unupdated in unupdatedPathway)
                {
                    unupdated.IsActive = false;
                    _dbContext.Entity<MsLessonPathway>().Update(unupdated);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            var idSchool = existLesson.AcademicYear.IdSchool;

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = idSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = idSchool
            });

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }
    }
}
