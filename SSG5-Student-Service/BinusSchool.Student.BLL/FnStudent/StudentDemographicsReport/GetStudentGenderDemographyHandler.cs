using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.StudentDemographicsReport.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentGenderDemographyHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentGenderDemographyHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetStudentGenderDemographyRequest, GetStudentGenderDemographyValidator>();

            var items = new List<GetStudentGenderDemographyResult>();

            var getHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Student.Country)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear
                    && (body.Level == null ? true : body.Level.Any(b => b == a.Homeroom.Grade.IdLevel))
                    && (body.Grade == null ? true : body.Grade.Any(b => b == a.Homeroom.IdGrade))
                    && (body.Semester == null ? true : a.Semester == body.Semester))
                .ToListAsync();

            if (body.ViewCategoryType == "grade")
            {
                if (body.Semester != null)
                {
                    var content = new List<GenderDataList>();

                    var getHomeroomStudentResult = getHomeroomStudent.ToList();

                    var groupedData = getHomeroomStudentResult
                        .GroupBy(a => new { IdGrade = a.Homeroom.IdGrade })
                        .Select(b => new
                        {
                            IdGrade = b.Key.IdGrade,
                            MaleCount = b.Count(c => c.Student.Gender == Gender.Male),
                            FemaleCount = b.Count(c => c.Student.Gender == Gender.Female)
                        })
                        .Distinct()
                        .ToList();

                    var allGrade = getHomeroomStudentResult
                        .Select(a => new { IdGrade = a.Homeroom.IdGrade })
                        .Distinct()
                        .ToList();

                    var missingGrade = allGrade
                        .SelectMany(a => allGrade)
                        .Where(b => !groupedData.Any(c => c.IdGrade == b.IdGrade))
                        .Select(b => new
                        {
                            IdGrade = b.IdGrade,
                            MaleCount = 0,
                            FemaleCount = 0
                        })
                        .Distinct()
                        .ToList();

                    var finalData = groupedData.Concat(missingGrade)
                        .Select(a => new GenderDataList
                        {
                            CategoryType = new ItemValueVm
                            {
                                Id = a.IdGrade,
                                Description = getHomeroomStudentResult.FirstOrDefault(b => b.Homeroom.IdGrade == a.IdGrade)?.Homeroom.Grade.Description
                            },
                            Male = a.MaleCount,
                            Female = a.FemaleCount,
                            Total = a.MaleCount + a.FemaleCount
                        })
                        .OrderBy(a => a.CategoryType.Description.Length)
                            .ThenBy(a => a.CategoryType.Description)
                        .ToList();

                    content.AddRange(finalData);

                    var totalStudent = new GenderTotalStudent
                    {
                        Male = getHomeroomStudentResult.Count(a => a.Student.Gender == Gender.Male),
                        Female = getHomeroomStudentResult.Count(a => a.Student.Gender == Gender.Female),
                        Total = getHomeroomStudentResult.Select(a => a.Student).Count()
                    };

                    var insertData = new GetStudentGenderDemographyResult()
                    {
                        Semester = body.Semester,
                        DataList = finalData,
                        TotalStudent = totalStudent
                    };

                    items.Add(insertData);
                }
                else
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        var content = new List<GenderDataList>();

                        var getHomeroomStudentResult = getHomeroomStudent.Where(a => a.Semester == i).ToList();

                        var groupedData = getHomeroomStudentResult
                            .GroupBy(a => new { IdGrade = a.Homeroom.IdGrade })
                            .Select(b => new
                            {
                                IdGrade = b.Key.IdGrade,
                                MaleCount = b.Count(c => c.Student.Gender == Gender.Male),
                                FemaleCount = b.Count(c => c.Student.Gender == Gender.Female)
                            })
                            .Distinct()
                            .ToList();

                        var allGrade = getHomeroomStudentResult
                            .Select(a => new { IdGrade = a.Homeroom.IdGrade })
                            .Distinct()
                            .ToList();

                        var missingGrade = allGrade
                            .SelectMany(a => allGrade)
                            .Where(b => !groupedData.Any(c => c.IdGrade == b.IdGrade))
                            .Select(b => new
                            {
                                IdGrade = b.IdGrade,
                                MaleCount = 0,
                                FemaleCount = 0
                            })
                            .Distinct()
                            .ToList();

                        var finalData = groupedData.Concat(missingGrade)
                            .Select(a => new GenderDataList
                            {
                                CategoryType = new ItemValueVm
                                {
                                    Id = a.IdGrade,
                                    Description = getHomeroomStudentResult.FirstOrDefault(b => b.Homeroom.IdGrade == a.IdGrade)?.Homeroom.Grade.Description
                                },
                                Male = a.MaleCount,
                                Female = a.FemaleCount,
                                Total = a.MaleCount + a.FemaleCount
                            })
                            .OrderBy(a => a.CategoryType.Description.Length)
                                .ThenBy(a => a.CategoryType.Description)
                            .ToList();

                        content.AddRange(finalData);

                        var totalStudent = new GenderTotalStudent
                        {
                            Male = getHomeroomStudentResult.Count(a => a.Student.Gender == Gender.Male),
                            Female = getHomeroomStudentResult.Count(a => a.Student.Gender == Gender.Female),
                            Total = getHomeroomStudentResult.Select(a => a.Student).Count()
                        };

                        var insertData = new GetStudentGenderDemographyResult()
                        {
                            Semester = i,
                            DataList = finalData,
                            TotalStudent = totalStudent
                        };

                        items.Add(insertData);
                    }
                }
            }
            else if (body.ViewCategoryType == "homeroom")
            {
                if (body.Semester != null)
                {
                    var content = new List<GenderDataList>();

                    var getHomeroomStudentResult = getHomeroomStudent.ToList();

                    var groupedData = getHomeroomStudentResult
                        .GroupBy(a => new { IdHomeroom = a.IdHomeroom })
                        .Select(b => new
                        {
                            IdHomeroom = b.Key.IdHomeroom,
                            MaleCount = b.Count(c => c.Student.Gender == Gender.Male),
                            FemaleCount = b.Count(c => c.Student.Gender == Gender.Female)
                        })
                        .Distinct()
                        .ToList();

                    var allGrade = getHomeroomStudentResult
                        .Select(a => new { IdHomeroom = a.IdHomeroom })
                        .Distinct()
                        .ToList();

                    var missingGrade = allGrade
                        .SelectMany(a => allGrade)
                        .Where(b => !groupedData.Any(c => c.IdHomeroom == b.IdHomeroom))
                        .Select(b => new
                        {
                            IdHomeroom = b.IdHomeroom,
                            MaleCount = 0,
                            FemaleCount = 0
                        })
                        .Distinct()
                        .ToList();

                    var finalData = groupedData.Concat(missingGrade)
                        .Select(a => new GenderDataList
                        {
                            CategoryType = new ItemValueVm
                            {
                                Id = a.IdHomeroom,
                                Description = getHomeroomStudentResult.FirstOrDefault(b => b.IdHomeroom == a.IdHomeroom)?.Homeroom.Grade.Code +
                                              getHomeroomStudentResult.FirstOrDefault(b => b.IdHomeroom == a.IdHomeroom)?.Homeroom.MsGradePathwayClassroom.Classroom.Code
                            },
                            Male = a.MaleCount,
                            Female = a.FemaleCount,
                            Total = a.MaleCount + a.FemaleCount
                        })
                        .OrderBy(a => a.CategoryType.Description.Length)
                            .ThenBy(a => a.CategoryType.Description)
                        .ToList();

                    content.AddRange(finalData);

                    var totalStudent = new GenderTotalStudent
                    {
                        Male = getHomeroomStudentResult.Count(a => a.Student.Gender == Gender.Male),
                        Female = getHomeroomStudentResult.Count(a => a.Student.Gender == Gender.Female),
                        Total = getHomeroomStudentResult.Select(a => a.Student).Count()
                    };

                    var insertData = new GetStudentGenderDemographyResult()
                    {
                        Semester = body.Semester,
                        DataList = finalData,
                        TotalStudent = totalStudent
                    };

                    items.Add(insertData);
                }
                else
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        var content = new List<GenderDataList>();

                        var getHomeroomStudentResult = getHomeroomStudent.Where(a => a.Semester == i).ToList();

                        var groupedData = getHomeroomStudentResult
                            .GroupBy(a => new { IdHomeroom = a.IdHomeroom })
                            .Select(b => new
                            {
                                IdHomeroom = b.Key.IdHomeroom,
                                MaleCount = b.Count(c => c.Student.Gender == Gender.Male),
                                FemaleCount = b.Count(c => c.Student.Gender == Gender.Female)
                            })
                            .Distinct()
                            .ToList();

                        var allGrade = getHomeroomStudentResult
                            .Select(a => new { IdHomeroom = a.IdHomeroom })
                            .Distinct()
                            .ToList();

                        var missingGrade = allGrade
                            .SelectMany(a => allGrade)
                            .Where(b => !groupedData.Any(c => c.IdHomeroom == b.IdHomeroom))
                            .Select(b => new
                            {
                                IdHomeroom = b.IdHomeroom,
                                MaleCount = 0,
                                FemaleCount = 0
                            })
                            .Distinct()
                            .ToList();

                        var finalData = groupedData.Concat(missingGrade)
                            .Select(a => new GenderDataList
                            {
                                CategoryType = new ItemValueVm
                                {
                                    Id = a.IdHomeroom,
                                    Description = getHomeroomStudentResult.FirstOrDefault(b => b.IdHomeroom == a.IdHomeroom)?.Homeroom.Grade.Code +
                                                  getHomeroomStudentResult.FirstOrDefault(b => b.IdHomeroom == a.IdHomeroom)?.Homeroom.MsGradePathwayClassroom.Classroom.Code
                                },
                                Male = a.MaleCount,
                                Female = a.FemaleCount,
                                Total = a.MaleCount + a.FemaleCount
                            })
                            .OrderBy(a => a.CategoryType.Description.Length)
                                .ThenBy(a => a.CategoryType.Description)
                            .ToList();

                        content.AddRange(finalData);

                        var totalStudent = new GenderTotalStudent
                        {
                            Male = getHomeroomStudentResult.Count(a => a.Student.Gender == Gender.Male),
                            Female = getHomeroomStudentResult.Count(a => a.Student.Gender == Gender.Female),
                            Total = getHomeroomStudentResult.Select(a => a.Student).Count()
                        };

                        var insertData = new GetStudentGenderDemographyResult()
                        {
                            Semester = i,
                            DataList = finalData,
                            TotalStudent = totalStudent
                        };

                        items.Add(insertData);
                    }
                }
            }

            return Request.CreateApiResult2(items as object);
        }
    }
}
