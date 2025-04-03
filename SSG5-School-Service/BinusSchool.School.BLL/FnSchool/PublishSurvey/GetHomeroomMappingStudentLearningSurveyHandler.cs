using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;

namespace BinusSchool.School.FnSchool.PublishSurvey
{
    public class GetHomeroomMappingStudentLearningSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetHomeroomMappingStudentLearningSurveyHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomMappingStudentLearningSurveyRequest>();

            var predicate = PredicateBuilder.Create<TrPublishSurveyMapping>(x => x.IsActive && x.IsMapping && x.IdPublishSurvey==param.IdPublishSurvey);

            var listHomeroom = await _dbContext.Entity<TrPublishSurveyMapping>()
                                .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.AcademicYear)
                                .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                                .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                .Where(predicate)
                                .GroupBy(e => new
                                {
                                    IdAcademicYear = e.HomeroomStudent.Homeroom.AcademicYear.Id,
                                    AcademicYear = e.HomeroomStudent.Homeroom.AcademicYear.Description,
                                    IdLevel = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                                    Level = e.HomeroomStudent.Homeroom.Grade.Level.Description,
                                    IdGrade = e.HomeroomStudent.Homeroom.Grade.Id,
                                    Grade = e.HomeroomStudent.Homeroom.Grade.Description,
                                    GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                                    IdHomeroom = e.HomeroomStudent.Homeroom.Id,
                                    Semester = e.HomeroomStudent.Homeroom.Semester,
                                    ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                })
                                .Select(e=>e.Key)
                                .ToListAsync(CancellationToken);

            var _listAcademicYear = listHomeroom
                .GroupBy(e => new
                {
                    e.IdAcademicYear,
                    e.AcademicYear,
                })
                .Select(e => e.Key)
                .ToList();

            GetHomeroomMappingStudentLearningSurveyResult getAcademicYear = new GetHomeroomMappingStudentLearningSurveyResult();
            foreach(var itemAcademicYear in _listAcademicYear)
            {
                var _listLevel = listHomeroom
                .GroupBy(e => new
                {
                    e.IdLevel,
                    e.Level,
                })
                .Select(e => e.Key)
                .ToList();

                List<LevelMappingStudentLearning> listLevel = new List<LevelMappingStudentLearning>();
                foreach (var itemLevel in _listLevel)
                {
                    var _listGrade = listHomeroom
                    .Where(e=>e.IdLevel==itemLevel.IdLevel)
                    .GroupBy(e => new
                    {
                        e.IdGrade,
                        e.Grade,
                    })
                    .Select(e => e.Key)
                    .ToList();

                    List<GradeMappingStudentLearning> listGrade = new List<GradeMappingStudentLearning>();
                    foreach (var itemGrade in _listGrade)
                    {
                        var _listHomeroom = listHomeroom
                        .Where(e => e.IdGrade == itemGrade.IdGrade)
                        .GroupBy(e => new HomeroomMappingStudentLearning
                        {
                            Id = e.IdHomeroom,
                            Description = e.ClassroomCode,
                            Semester = e.Semester,
                        })
                        .Select(e => e.Key)
                        .ToList();

                        GradeMappingStudentLearning newGrade = new GradeMappingStudentLearning
                        {
                            Id = itemGrade.IdGrade,
                            Description = itemGrade.Grade,
                            Homerooms = _listHomeroom
                        };

                        listGrade.Add(newGrade);
                    }

                    LevelMappingStudentLearning newLevel = new LevelMappingStudentLearning
                    {
                        Id = itemLevel.IdLevel,
                        Description = itemLevel.Level,
                        Grades = listGrade
                    };

                    listLevel.Add(newLevel);
                }

                getAcademicYear = new GetHomeroomMappingStudentLearningSurveyResult
                {
                    Id = itemAcademicYear.IdAcademicYear,
                    Description = itemAcademicYear.AcademicYear,
                    Levels = listLevel
                };
            }

            return Request.CreateApiResult2(getAcademicYear as object);
        }
    }
}
