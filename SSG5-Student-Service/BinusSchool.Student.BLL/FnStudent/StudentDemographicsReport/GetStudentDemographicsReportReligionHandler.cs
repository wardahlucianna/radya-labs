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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentDemographicsReportReligionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentDemographicsReportReligionHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetSDRReligionReportsRequest>();

            var items = new List<GetSDRReligionReportsResult>();

            var getHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Student.Religion)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                    && (param.Level == null ? true : param.Level.Any(b => b == a.Homeroom.Grade.IdLevel))
                    && (param.Grade == null ? true : param.Grade.Any(b => b == a.Homeroom.IdGrade))
                    && (param.Semester == null ? true : a.Semester == param.Semester));

            int s = 0;
            int n = 0;

            if (param.Semester == null)
            {
                s = 1;
                n = 2;
            }
            else
            {
                s = (int)param.Semester;
                n = (int)param.Semester;
            }

            for (int i = s; i <= n; i++)
            {
                var header = new List<string>();

                var getHeader = getHomeroomStudent
                    .Select(a => new { Id = a.Student.IdReligion, Description = a.Student.Religion.ReligionName })
                    .Distinct().OrderBy(a => a.Id).ToList();

                var orderHeader = getHeader
                    .Select(a => a.Description)
                    .ToList();

                header.AddRange(orderHeader);

                var body = new List<ReligionBody>();
                var totalStudent = new ReligionTotalStudent();

                var homeroomStudentResult = getHomeroomStudent.Where(a => a.Semester == i).ToList();

                if (param.ViewCategoryType == "grade")
                {
                    var dataGroup = homeroomStudentResult
                        .GroupBy(a => new { a.Student.IdReligion, a.Homeroom.IdGrade })
                        .Select(g => new
                        {
                            IdReligion = g.Key.IdReligion,
                            IdGrade = g.Key.IdGrade,
                            MaleCount = g.Count(m => m.Student.Gender == Gender.Male),
                            FemaleCount = g.Count(f => f.Student.Gender == Gender.Female)
                        })
                        .Distinct().ToList();

                    var religionCombination = homeroomStudentResult
                        .Select(a => new { IdReligion = a.Student.IdReligion, IdGrade = a.Homeroom.IdGrade })
                        .Distinct().ToList();

                    var emptyReligion = religionCombination
                        .SelectMany(a => religionCombination
                        .Where(b => !dataGroup.Any(c => c.IdGrade == a.IdGrade && c.IdReligion == b.IdReligion))
                        .Select(b => new
                        {
                            IdReligion = b.IdReligion,
                            IdGrade = a.IdGrade,
                            MaleCount = 0,
                            FemaleCount = 0
                        }))
                        .Distinct().ToList();

                    var finalData = dataGroup.Concat(emptyReligion)
                        .GroupBy(a => a.IdGrade)
                        .Select(b => new ReligionBody
                        {
                            CategoryType = new CodeWithIdVm
                            {
                                Id = b.Key,
                                Description = homeroomStudentResult.FirstOrDefault(c => c.Homeroom.IdGrade == b.Key)?.Homeroom.Grade.Description,
                                Code = "idgrade"
                            },
                            DataList = b.Select(d => new ReligionDataList
                            {
                                Male = d.MaleCount,
                                Female = d.FemaleCount,
                                Religion = new ItemValueVm
                                {
                                    Id = d.IdReligion,
                                    Description = homeroomStudentResult.FirstOrDefault(c => c.Student.IdReligion == d.IdReligion)?.Student.Religion.ReligionName
                                }
                            })
                            .OrderBy(a => a.Religion.Id)
                            .ToList(),
                            Total = b.Sum(item => item.MaleCount + item.FemaleCount)
                        })
                        .OrderBy(a => a.CategoryType.Description.Length)
                            .ThenBy(a => a.CategoryType.Description)
                        .ToList();

                    body.AddRange(finalData);

                    var getTotalStudent = new ReligionTotalStudent
                    {
                        DataList = homeroomStudentResult.GroupBy(a => new
                        {
                            a.Student.IdReligion,
                            a.Student.Religion.ReligionName
                        })
                        .Select(group => new ReligionDataList
                        {
                            Male = group.Count(b => b.Student.Gender == Gender.Male),
                            Female = group.Count(b => b.Student.Gender == Gender.Female),
                            Religion = new ItemValueVm
                            {
                                Id = group.Key.IdReligion,
                                Description = group.Key.ReligionName
                            }
                        }).OrderBy(group => group.Religion.Id).ToList(),
                        Total = homeroomStudentResult.Select(a => a.Student).Count()
                    };

                    totalStudent.DataList = getTotalStudent.DataList;
                    totalStudent.Total = getTotalStudent.Total;
                }
                else if (param.ViewCategoryType == "homeroom")
                {
                    var dataGroup = homeroomStudentResult
                        .GroupBy(a => new { a.Student.IdReligion, a.IdHomeroom })
                        .Select(g => new
                        {
                            IdReligion = g.Key.IdReligion,
                            IdHomeroom = g.Key.IdHomeroom,
                            MaleCount = g.Count(m => m.Student.Gender == Gender.Male),
                            FemaleCount = g.Count(f => f.Student.Gender == Gender.Female)
                        })
                        .Distinct().ToList();

                    var religionCombination = homeroomStudentResult
                        .Select(a => new { IdReligion = a.Student.IdReligion, IdHomeroom = a.IdHomeroom })
                        .Distinct().ToList();

                    var emptyReligion = religionCombination
                        .SelectMany(a => religionCombination
                        .Where(b => !dataGroup.Any(c => c.IdHomeroom == a.IdHomeroom && c.IdReligion == b.IdReligion))
                        .Select(b => new
                        {
                            IdReligion = b.IdReligion,
                            IdHomeroom = a.IdHomeroom,
                            MaleCount = 0,
                            FemaleCount = 0
                        }))
                        .Distinct().ToList();

                    var finalData = dataGroup.Concat(emptyReligion)
                        .GroupBy(a => a.IdHomeroom)
                        .Select(b => new ReligionBody
                        {
                            CategoryType = new CodeWithIdVm
                            {
                                Id = b.Key,
                                Description = homeroomStudentResult.FirstOrDefault(c => c.IdHomeroom == b.Key)?.Homeroom.Grade.Code +
                                              homeroomStudentResult.FirstOrDefault(c => c.IdHomeroom == b.Key)?.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                                Code = "idhomeroom"
                            },
                            DataList = b.Select(d => new ReligionDataList
                            {
                                Male = d.MaleCount,
                                Female = d.FemaleCount,
                                Religion = new ItemValueVm
                                {
                                    Id = d.IdReligion,
                                    Description = homeroomStudentResult.FirstOrDefault(c => c.Student.IdReligion == d.IdReligion)?.Student.Religion.ReligionName
                                }
                            })
                            .OrderBy(a => a.Religion.Id)
                            .ToList(),
                            Total = b.Sum(item => item.MaleCount + item.FemaleCount)
                        })
                        .OrderByDescending(a => a.CategoryType.Description.Length)
                            .ThenBy(a => a.CategoryType.Description)
                        .ToList();

                    body.AddRange(finalData);

                    var getTotalStudent = new ReligionTotalStudent
                    {
                        DataList = homeroomStudentResult.GroupBy(a => new
                        {
                            a.Student.IdReligion,
                            a.Student.Religion.ReligionName
                        })
                        .Select(group => new ReligionDataList
                        {
                            Male = group.Count(b => b.Student.Gender == Gender.Male),
                            Female = group.Count(b => b.Student.Gender == Gender.Female),
                            Religion = new ItemValueVm
                            {
                                Id = group.Key.IdReligion,
                                Description = group.Key.ReligionName
                            }
                        }).OrderBy(group => group.Religion.Id).ToList(),
                        Total = homeroomStudentResult.Select(a => a.Student).Count()
                    };

                    totalStudent.DataList = getTotalStudent.DataList;
                    totalStudent.Total = getTotalStudent.Total;
                }

                var data = homeroomStudentResult.Select(a => new GetSDRReligionReportsResult
                {
                    ViewCategoryType = param.ViewCategoryType == "grade" ? "grade" : "homeroom",
                    Semester = a.Semester,
                    Header = header,
                    Body = body,
                    TotalStudent = totalStudent
                }).FirstOrDefault();

                items.Add(data);
            }

            return Request.CreateApiResult2(items as object);
        }
    }
}
