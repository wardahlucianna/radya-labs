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
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanGradeByPositionV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public LessonPlanGradeByPositionV2Handler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            #region Get Position User
            var param = Request.ValidateParams<LessonPlanGradeByPositionV2Request>();

            List<string> avaiablePosition = new List<string>();
            var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
            .Include(x => x.Homeroom)
                .ThenInclude(x => x.Grade)
            .Where(x => x.IdBinusian == param.IdUser)
            .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear)
            .Distinct()
            .Select(x => x).ToListAsync(CancellationToken);

            var dataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Grade)
            .Where(x => x.IdUser == param.IdUser)
            .Where(x => x.Lesson.IdAcademicYear == param.IdAcademicYear)
            .Distinct()
            .Select(x => x).ToListAsync(CancellationToken);
            var positionUser = await _dbContext.Entity<TrNonTeachingLoad>().Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.Position)
                .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdAcademicYear)
                .Where(x => x.IdUser == param.IdUser)
                .Select(x => new
                {
                    x.Data,
                    x.MsNonTeachingLoad.TeacherPosition.Position.Code
                }).ToListAsync(CancellationToken);
            if (positionUser.Count == 0 && dataHomeroomTeacher.Count == 0 && dataLessonTeacher.Count == 0)
                throw new BadRequestException($"You dont have any position.");
            if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                avaiablePosition.Add(param.SelectedPosition);
            if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                avaiablePosition.Add(PositionConstant.SubjectTeacher);
            foreach (var pu in positionUser)
            {
                avaiablePosition.Add(pu.Code);
            }
            if (avaiablePosition.Where(x => x == param.SelectedPosition).Count() == 0)
                throw new BadRequestException($"You dont assign as {param.SelectedPosition}.");
            List<string> idLevelPrincipalAndVicePrincipal = new List<string>();
            List<ItemValueVm> idGrades = new List<ItemValueVm>();
            if (positionUser.Count > 0)
            {
                if (param.SelectedPosition == PositionConstant.Principal)
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

                                var grades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == _levelLH.Id)
                                        .OrderBy(x => x.Level.AcademicYear.OrderNumber)
                                            .ThenBy(x => x.Level.OrderNumber)
                                                .ThenBy(x => x.OrderNumber)
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.Description
                                    }).ToListAsync(CancellationToken);

                                idGrades.AddRange(grades);
                            }
                        }
                    }
                }
                if (param.SelectedPosition == PositionConstant.VicePrincipal)
                {
                    if (positionUser.Any(y => y.Code == PositionConstant.VicePrincipal))
                    {
                        if (positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).Count() > 0)
                        {
                            var Principal = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();
                            List<string> IdLevels = new List<string>();

                            foreach (var item in Principal)
                            {
                                var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                                _dataNewLH.TryGetValue("Level", out var _levelLH);

                                var grades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == _levelLH.Id)
                                    .OrderBy(x => x.Level.AcademicYear.OrderNumber)
                                        .ThenBy(x => x.Level.OrderNumber)
                                            .ThenBy(x => x.OrderNumber)
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.Description
                                    }).ToListAsync(CancellationToken);

                                idGrades.AddRange(grades);
                            }
                        }
                    }
                }
                if (param.SelectedPosition == PositionConstant.LevelHead)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.LevelHead).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.LevelHead).ToList();
                        List<string> IdGrade = new List<string>();

                        foreach (var item in LevelHead)
                        {
                            var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewLH.TryGetValue("Level", out var _levelLH);

                            _dataNewLH.TryGetValue("Grade", out var _gradeLH);
                            IdGrade.Add(_gradeLH.Id);

                            var grades = await _dbContext.Entity<MsGrade>().Where(x => x.IdLevel == _levelLH.Id)
                                    .Where(x => IdGrade.Contains(x.Id))
                                    .OrderBy(x => x.Level.AcademicYear.OrderNumber)
                                        .ThenBy(x => x.Level.OrderNumber)
                                            .ThenBy(x => x.OrderNumber)
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.Description
                                    }).ToListAsync(CancellationToken);

                            idGrades.AddRange(grades);
                        }
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectHead)
                {
                    if (positionUser.Where(y => y.Code == PositionConstant.SubjectHead).ToList() != null)
                    {
                        var LevelHead = positionUser.Where(x => x.Code == PositionConstant.SubjectHead).ToList();
                        List<string> IdGrade = new List<string>();
                        List<string> IdSubject = new List<string>();
                        foreach (var item in LevelHead)
                        {
                            var _dataNewSH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                            _dataNewSH.TryGetValue("Level", out var _leveltSH);

                            _dataNewSH.TryGetValue("Grade", out var _gradeSH);
                            IdGrade.Add(_gradeSH.Id);

                            var grades = await _dbContext.Entity<MsGrade>()
                            .Include(x => x.Level)
                            .Where(x => x.IdLevel == _leveltSH.Id)
                            .Where(x => IdGrade.Contains(x.Id))
                            .OrderBy(x => x.Level.AcademicYear.OrderNumber)
                                .ThenBy(x => x.Level.OrderNumber)
                                    .ThenBy(x => x.OrderNumber)
                            .Select(x => new ItemValueVm
                            {
                                Id = x.Id,
                                Description = x.Description
                            }).ToListAsync(CancellationToken);

                            var dataGrade = grades.Where(x => !idGrades.Select(x => x.Id).Contains(x.Id));

                            idGrades.AddRange(dataGrade);
                        }
                    }
                }
                if (param.SelectedPosition == PositionConstant.ClassAdvisor)
                {
                    List<string> IdGrade = new List<string>();
                    if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                    {
                        IdGrade = dataHomeroomTeacher.GroupBy(x => new
                        {
                            x.Homeroom.IdGrade,
                            x.Homeroom.Grade.Description
                        }).Select(x => x.Key.IdGrade).ToList();

                        var grades = await _dbContext.Entity<MsGrade>()
                        .Include(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                        .OrderBy(x => x.Level.AcademicYear.OrderNumber)
                            .ThenBy(x => x.Level.OrderNumber)
                                .ThenBy(x => x.OrderNumber)
                        .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
                        .Where(x => IdGrade.Contains(x.Id))
                        .Select(x => new ItemValueVm
                        {
                            Id = x.Id,
                            Description = x.Description
                        }).ToListAsync(CancellationToken);


                        idGrades.AddRange(grades);
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectTeacher)
                {
                    List<string> IdGrade = new List<string>();
                    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                    {
                        var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
                        idGrades = await _dbContext.Entity<MsLessonPathway>()
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.Grade)
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.GradePathwayClassroom)
                                        .ThenInclude(x => x.Classroom)
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
            else
            {
                if (param.SelectedPosition == PositionConstant.ClassAdvisor || param.SelectedPosition == PositionConstant.CoTeacher)
                {
                    List<string> IdGrade = new List<string>();
                    if (dataHomeroomTeacher != null && dataHomeroomTeacher.Count > 0)
                    {
                        IdGrade = dataHomeroomTeacher.GroupBy(x => new
                        {
                            x.Homeroom.IdGrade,
                            x.Homeroom.Grade.Description
                        }).Select(x => x.Key.IdGrade).ToList();

                        idGrades = await _dbContext.Entity<MsGrade>()
                        .Include(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                        .OrderBy(x => x.Level.AcademicYear.OrderNumber)
                            .ThenBy(x => x.Level.OrderNumber)
                                .ThenBy(x => x.OrderNumber)
                        .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
                        .Where(x => IdGrade.Contains(x.Id))
                        .Select(x => new ItemValueVm
                        {
                            Id = x.Id,
                            Description = x.Description
                        }).ToListAsync(CancellationToken);
                    }
                }
                if (param.SelectedPosition == PositionConstant.SubjectTeacher)
                {
                    List<string> IdGrade = new List<string>();
                    if (dataLessonTeacher != null && dataLessonTeacher.Count > 0)
                    {
                        var idLessons = dataLessonTeacher.Select(x => x.Lesson.Id).Distinct().ToList();
                        idGrades = await _dbContext.Entity<MsLessonPathway>()
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.Grade)
                            .Include(x => x.HomeroomPathway)
                                .ThenInclude(x => x.Homeroom)
                                    .ThenInclude(x => x.GradePathwayClassroom)
                                        .ThenInclude(x => x.Classroom)
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
            #endregion

            return Request.CreateApiResult2(idGrades as object);
        }
    }
}
