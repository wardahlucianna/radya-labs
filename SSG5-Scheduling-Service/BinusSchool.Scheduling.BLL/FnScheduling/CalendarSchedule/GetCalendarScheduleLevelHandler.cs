using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using NPOI.Util;
using Org.BouncyCastle.Operators;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using System.Linq;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using FluentEmail.Core;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetCalendarScheduleLevelHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetCalendarScheduleLevelHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCalendarScheduleLevelRequest>();
            var listUserLogin = await GetCalendarScheduleV2Handler.GetLessonByUser(_dbContext, CancellationToken, param.IdUser, param.IdAcademicYear);

            List<GetCalendarScheduleLevelResult> listLevel = new List<GetCalendarScheduleLevelResult>();
            var _listLevel = listUserLogin
                            .GroupBy(e => new 
                            {
                                Id = e.Level.Id,
                                Description = e.Level.Description
                            })
                            .Select(e => e.Key)
                            .OrderBy(e=>e.Description)
                            .ToList();

            foreach (var itemLevel in _listLevel)
            {
                var _listGrade = listUserLogin.Where(e => e.Level.Id == itemLevel.Id)
                    .GroupBy(e => new 
                    {
                        Id = e.Grade.Id,
                        Description = e.Grade.Description,
                    })
                    .Select(e => e.Key)
                    .OrderBy(e => e.Description)
                    .ToList();

                List<GetCalendarScheduleGrade> listGrade = new List<GetCalendarScheduleGrade>();
                foreach (var itemGrade in _listGrade)
                {
                    var _listHomeroom = listUserLogin.Where(e => e.Grade.Id == itemGrade.Id)
                       .GroupBy(e => new 
                       {
                           Id = e.Homeroom.Id,
                           Description = e.Homeroom.Description,
                           Semester = e.Semester,
                       })
                       .Select(e => e.Key)
                       .OrderBy(e => e.Description)
                       .ToList();

                    List<GetCalendarScheduleHomeroom> listHomeroom = new List<GetCalendarScheduleHomeroom>();
                    foreach (var itemHomeroom in _listHomeroom)
                    {
                        var listSubject = listUserLogin.Where(e => e.Homeroom.Id == itemHomeroom.Id)
                           .GroupBy(e => new 
                           {
                               IdLesson = e.IdLesson,
                               ClassId = e.ClassId,
                               Subject = new ItemValueVm
                               {
                                   Id = e.Subject.Id,
                                   Description = e.Subject.Description,
                               },
                               Teacher = new ItemValueVm
                               {
                                   Id = e.Teacher.Id,
                                   Description = e.Teacher.Description,
                               },
                           })
                           .Select(e => new GetCalendarScheduleLesson
                           {
                               IdLesson = e.Key.IdLesson,
                               ClassId = e.Key.ClassId,
                               Subject = new ItemValueVm
                               {
                                   Id = e.Key.Subject.Id,
                                   Description = e.Key.Subject.Description,
                               },
                               Teacher = new ItemValueVm
                               {
                                   Id = e.Key.Teacher.Id,
                                   Description = e.Key.Teacher.Description,
                               },
                           })
                           .OrderBy(e => e.Subject.Description)
                           .ToList();

                        GetCalendarScheduleHomeroom newHomeroom = new GetCalendarScheduleHomeroom
                        {
                            Id = itemHomeroom.Id,
                            Description = itemHomeroom.Description,
                            Semester = itemHomeroom.Semester,
                            Subject = listSubject
                        };

                        listHomeroom.Add(newHomeroom);

                    };

                    GetCalendarScheduleGrade NewGrade = new GetCalendarScheduleGrade
                    {
                        Id = itemGrade.Id,
                        Description = itemGrade.Description,
                        Homeroom = listHomeroom,
                    };

                    listGrade.Add(NewGrade);
                }


                GetCalendarScheduleLevelResult NewLevel = new GetCalendarScheduleLevelResult
                {
                    Id = itemLevel.Id,
                    Description = itemLevel.Description,
                    Grade = listGrade,
                };

                listLevel.Add(NewLevel);
            }

            return Request.CreateApiResult2(listLevel as object);
        }
    }
}
