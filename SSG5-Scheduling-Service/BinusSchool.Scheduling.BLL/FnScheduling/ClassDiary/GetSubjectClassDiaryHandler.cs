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
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class GetSubjectClassDiaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRolePosition _serviceRolePosition;

        public GetSubjectClassDiaryHandler(ISchedulingDbContext schoolDbContext, IRolePosition serviceRolePosition)
        {
            _dbContext = schoolDbContext;
            _serviceRolePosition = serviceRolePosition;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSubjectClassDiaryRequest>();

            List<string> listCodePosition = new List<string>()
                                            {
                                                PositionConstant.HeadOfDepartment,
                                                PositionConstant.SubjectHead,
                                                PositionConstant.LevelHead,
                                                PositionConstant.ClassAdvisor,
                                                PositionConstant.AffectiveCoordinator,
                                                PositionConstant.SubjectTeacher,
                                                PositionConstant.VicePrincipal,
                                                PositionConstant.Principal
                                            };

            var listIdTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                            .Where(e => listCodePosition.Contains(e.Position.Code))
                                            .Select(e => e.Id)
                                            .ToListAsync(CancellationToken);

            var paramPositionByUser = new GetSubjectByUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdUser = param.IdUser,
                ListIdTeacherPositions = listIdTeacherPosition,
                IsClassDiary = true
            };
            var getApiSubjectByUser = await _serviceRolePosition.GetSubjectByUser(paramPositionByUser);
            var getSubjectByUser = getApiSubjectByUser.IsSuccess ? getApiSubjectByUser.Payload : null;
            getSubjectByUser = getSubjectByUser.Where(e => e.Homeroom.Semester == param.Semester).ToList();

            var getIdLessonByUser = new List<GetLevelClassDiaryResult>();
            List<GetLevelClassDiaryResult> items = new List<GetLevelClassDiaryResult>();
            if (getSubjectByUser != null)
            {
                var listLevel = getSubjectByUser
                                .OrderBy(e=>e.Level.OrderNumber)
                                .GroupBy(e => new
                                {
                                    IdLevel = e.Level.Id,
                                    Level = e.Level.Description,
                                    LevelCode = e.Level.Code,
                                    OrderNUmber = e.Level.OrderNumber
                                });

                foreach (var itemLevel in listLevel)
                {
                    var _listGrade = itemLevel
                        .Where(e => e.Level.Id == itemLevel.Key.IdLevel)
                        .OrderBy(e=>e.Grade.OrderNumber)
                        .GroupBy(e => new
                        {
                            IdGrade = e.Grade.Id,
                            Grade = e.Grade.Description,
                            GradeCode = e.Grade.Code
                        });

                    List<GetGradeClassDiary> listGrade = new List<GetGradeClassDiary>();
                    foreach (var itemGrade in _listGrade)
                    {
                        var _listSubject = itemGrade
                            .Where(e => e.Grade.Id == itemGrade.Key.IdGrade)
                            .OrderBy(e=>e.Subject.Description)
                            .GroupBy(e => new
                            {
                                IdSubject = e.Subject.Id,
                                Subject = e.Subject.Description,
                                SubjectCode= e.Subject.Code
                            });

                        List<GetSubjectClassDiary> listSubject = new List<GetSubjectClassDiary>();
                        foreach (var itemSubject in _listSubject)
                        {
                            var _listHomeroom = itemSubject
                                .Where(e => e.Subject.Id == itemSubject.Key.IdSubject)
                                .OrderBy(e=>e.Homeroom.Description)
                                .GroupBy(e => new
                                {
                                    IdHomeroom = e.Homeroom.Id,
                                    Homeroom = e.Homeroom.Description,
                                    Semester = e.Homeroom.Semester
                                });

                            List<GetHomeroomClassDiary> listHomeroom = new List<GetHomeroomClassDiary>();
                            foreach (var itemHomeroom in _listHomeroom)
                            {
                                var _listClassId = itemHomeroom
                                    .OrderBy(e=>e.Lesson.ClassId)
                                    .Where(e => e.Subject.Id == itemSubject.Key.IdSubject && e.Homeroom.Id == itemHomeroom.Key.IdHomeroom)
                                    .GroupBy(e => new
                                    {
                                        e.Lesson.ClassId
                                    })
                                    .Select(e => e.Key.ClassId).OrderBy(e => e).ToList();

                                listHomeroom.Add(new GetHomeroomClassDiary
                                {
                                    Id = itemHomeroom.Key.IdHomeroom,
                                    Description = itemHomeroom.Key.Homeroom,
                                    ClassId = _listClassId
                                });
                            }

                            GetSubjectClassDiary newSubject = new GetSubjectClassDiary
                            {
                                Id = itemSubject.Key.IdSubject,
                                Description = itemSubject.Key.Subject,
                                Homeroom = listHomeroom
                            };

                            listSubject.Add(newSubject);
                        }

                        GetGradeClassDiary newGrade = new GetGradeClassDiary
                        {
                            Id = itemGrade.Key.IdGrade,
                            Description = itemGrade.Key.Grade,
                            Subject = listSubject
                        };

                        listGrade.Add(newGrade);
                    }


                    GetLevelClassDiaryResult newLevel = new GetLevelClassDiaryResult
                    {
                        Id = itemLevel.Key.IdLevel,
                        Description = itemLevel.Key.Level,
                        Grade = listGrade,
                        OrderNumber = itemLevel.Key.OrderNUmber
                    };
                    items.Add(newLevel);
                }
            }

            items = items.OrderBy(e => e.OrderNumber).Distinct().ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
