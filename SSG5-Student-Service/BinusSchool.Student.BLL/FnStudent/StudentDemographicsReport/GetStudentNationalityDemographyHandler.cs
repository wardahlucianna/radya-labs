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
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.StudentDemographicsReport.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class GetStudentNationalityDemographyHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentNationalityDemographyHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetStudentNationalityDemographyRequest, GetStudentNationalityDemographyValidator>();

            var items = new List<GetStudentNationalityDemographyResult>();

            var getHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Student.Country)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                    && (param.IdLevel == null ? true : param.IdLevel.Any(b => b == a.Homeroom.Grade.IdLevel))
                    && (param.IdGrade == null ? true : param.IdGrade.Any(b => b == a.Homeroom.IdGrade))
                    && (param.Semester == null ? true : a.Semester == param.Semester))
                .ToListAsync();

            if (param.ViewCategoryType == "grade")
            {
                if (param.Semester != null)
                {
                    var header = new List<string>();

                    var getHeader = getHomeroomStudent
                        .Select(a => a.Homeroom.Grade.Description)
                        .OrderBy(a => a.Length)
                            .ThenBy(a => a)
                        .Distinct()
                        .ToList();

                    header.AddRange(getHeader);

                    var body = new List<Body>();

                    var getHomeroomStudentResult = getHomeroomStudent.ToList();

                    var groupedData = getHomeroomStudentResult
                        .GroupBy(a => new { a.Student.IdCountry, a.Homeroom.IdGrade })
                        .Select(group => new
                        {
                            CountryId = group.Key.IdCountry,
                            GradeId = group.Key.IdGrade,
                            MaleCount = group.Count(s => s.Student.Gender == Gender.Male),
                            FemaleCount = group.Count(s => s.Student.Gender == Gender.Female)
                        })
                        .Distinct()
                        .ToList();

                    var allCountryGradeCombinations = getHomeroomStudentResult
                        .Select(a => new { CountryId = a.Student.IdCountry, GradeId = a.Homeroom.IdGrade })
                        .Distinct()
                        .ToList();

                    var missingGrades = allCountryGradeCombinations
                        .SelectMany(gc => allCountryGradeCombinations
                            .Where(allGrade => !groupedData.Any(gd => gd.CountryId == gc.CountryId && gd.GradeId == allGrade.GradeId))
                            .Select(allGrade => new
                            {
                                CountryId = gc.CountryId,
                                GradeId = allGrade.GradeId,
                                MaleCount = 0,
                                FemaleCount = 0
                            })
                        )
                        .Distinct()
                        .ToList();

                    var finalData = groupedData.Concat(missingGrades)
                        .GroupBy(result => result.CountryId)
                        .Select(groupedResult => new Body
                        {
                            Country = new ItemValueVm
                            {
                                Id = groupedResult.Key,
                                Description = getHomeroomStudentResult.FirstOrDefault(c => c.Student.IdCountry == groupedResult.Key)?.Student.Country.CountryName
                            },
                            ListData = groupedResult
                                .Select(data => new ListData
                                {
                                    Male = data.MaleCount,
                                    Female = data.FemaleCount,
                                    Data = new CodeWithIdVm
                                    {
                                        Id = data.GradeId,
                                        Description = getHomeroomStudentResult.FirstOrDefault(g => g.Homeroom.IdGrade == data.GradeId)?.Homeroom.Grade.Description,
                                        Code = "idgrade"
                                    }
                                })
                                .OrderBy(data => data.Data.Description.Length)
                                    .ThenBy(data => data.Data.Description)
                                .ToList(),
                            Total = groupedResult.Sum(item => item.MaleCount + item.FemaleCount)
                        })
                        .OrderByDescending(a => a.Country.Description == "Indonesia")
                        .ThenBy(a => a.Country.Description)
                        .ToList();

                    body.AddRange(finalData);

                    var getTotalStudent = new TotalStudent
                    {
                        ListData = getHomeroomStudent
                            .GroupBy(a => new
                            {
                                a.Homeroom.IdGrade,
                                a.Homeroom.Grade.Description
                            })
                            .Select(group => new ListData
                            {
                                Male = group.Count(b => b.Student.Gender == Gender.Male),
                                Female = group.Count(b => b.Student.Gender == Gender.Female),
                                Data = new CodeWithIdVm
                                {
                                    Id = group.Key.IdGrade,
                                    Description = group.Key.Description,
                                    Code = "idgrade"
                                }
                            }).OrderBy(group => group.Data.Description.Length).ThenBy(group => group.Data.Description).ToList(),
                        Total = getHomeroomStudent.Select(a => a.Student).Count()
                    };

                    var data = getHomeroomStudent.Select(a => new GetStudentNationalityDemographyResult
                    {
                        Semester = a.Semester,
                        Header = header,
                        Body = body,
                        TotalStudent = getTotalStudent
                    })
                        .FirstOrDefault();

                    items.Add(data);
                    #region unused code
                    /*var getBody = getHomeroomStudent
                        .GroupBy(a => new
                        {
                            a.Student.IdCountry,
                            a.Student.Country.CountryName,
                            a.Homeroom.IdGrade,
                            a.Homeroom.Grade.Description
                        })
                        .Select(group => new
                        {
                            Country = new ItemValueVm
                            {
                                Id = group.Key.IdCountry,
                                Description = group.Key.CountryName
                            },
                            ListData = new List<ListData>
                            {
                                new ListData
                                {
                                    Male = group.Count(b => b.Student.Gender == Gender.Male),
                                    Female = group.Count(b => b.Student.Gender == Gender.Female),
                                    Data = new CodeWithIdVm
                                    {
                                        Id = group.Key.IdGrade,
                                        Description = group.Key.Description,
                                        Code = "idgrade"
                                    }
                                }
                            },
                            Total = group.Count()
                        })
                        .GroupBy(result => new
                        {
                            result.Country.Id,
                            result.Country.Description
                        })
                        .Select(groupedResult => new Body
                        {
                            Country = new ItemValueVm
                            {
                                Id = groupedResult.Key.Id,
                                Description = groupedResult.Key.Description
                            },
                            ListData = groupedResult
                                .SelectMany(item => item.ListData)
                                .GroupBy(listData => new
                                {
                                    listData.Data.Id,
                                    listData.Data.Description
                                })
                                .Select(groupedListData => new ListData
                                {
                                    Male = groupedListData.Sum(data => data.Male),
                                    Female = groupedListData.Sum(data => data.Female),
                                    Data = new CodeWithIdVm
                                    {
                                        Id = groupedListData.Key.Id,
                                        Description = groupedListData.Key.Description,
                                        Code = "idgrade"
                                    }
                                })
                                .ToList(),
                            Total = groupedResult.Sum(item => item.Total)
                        })
                        .OrderBy(a => a.Country.Description)
                        .ToList();*/

                    /*                var getBody = getHomeroomStudent
                                        .GroupBy(a => new
                                        {
                                            a.Student.IdCountry,
                                            a.Student.Country.CountryName
                                        })
                                        .Select(group => new Body
                                        {
                                            Country = new ItemValueVm
                                            {
                                                Id = group.Key.IdCountry,
                                                Description = group.Key.CountryName
                                            },
                                            ListData = group.Select(g => new ListData
                                            {
                                                Male = group.Count(b => b.Student.Gender == Gender.Male),
                                                Female = group.Count(b => b.Student.Gender == Gender.Female),
                                                Data = new CodeWithIdVm
                                                {
                                                    Id = g.Homeroom.IdGrade,
                                                    Description = g.Homeroom.Grade.Description,
                                                    Code = "idgrade"
                                                }
                                            })
                                            .GroupBy(listData => new { listData.Data.Id, listData.Data.Description })
                                            .Select(groupedListData => new ListData
                                            {
                                                Male = groupedListData,
                                                Female = groupedListData.First().Female,
                                                Data = new CodeWithIdVm
                                                {
                                                    Id = groupedListData.Key.Id,
                                                    Description = groupedListData.Key.Description,
                                                    Code = "idgrade"
                                                }
                                            })
                                            .ToList(),
                                            Total = group.Count()
                                        }).ToList();*/

                    //var getBody = getHomeroomStudent
                    //    .GroupBy(a => new
                    //    {
                    //        a.Student.IdCountry,
                    //        a.Student.Country.CountryName
                    //        //a.Homeroom.IdGrade,
                    //        //a.Homeroom.Grade.Description
                    //    })
                    //    .Select(group => new Body
                    //    {
                    //        Country = new ItemValueVm
                    //        {
                    //            Id = group.Key.IdCountry,
                    //            Description = group.Key.CountryName
                    //        },
                    //        ListData = group.Select(g => new ListData
                    //        {
                    //            Male = group.Count(b => b.Student.Gender == Gender.Male),
                    //            Female = group.Count(b => b.Student.Gender == Gender.Female),
                    //            Data = new CodeWithIdVm
                    //            {
                    //                Id = g.Homeroom.IdGrade,
                    //                Description = g.Homeroom.Grade.Description,
                    //                Code = "idgrade"
                    //            }
                    //        }).ToList(),
                    //        Total = group.Count()
                    //    }).ToList();

                    /*var getBody = getHomeroomStudent.Select(a => new Body
                    {
                        Country = new ItemValueVm
                        {
                            Id = a.Student.IdCountry,
                            Description = a.Student.Country.CountryName
                        },
                        ListData = new List<ListData>
                        {
                            new ListData
                            {
                                Male = getHomeroomStudent.Where(b => b.Student.Gender.Equals("Male") && b.Student.IdCountry == a.Student.IdCountry).Select(b => b.Student).Count(),
                                Female = getHomeroomStudent.Where(b => b.Student.Gender.Equals("Female") && b.Student.IdCountry == a.Student.IdCountry).Select(b => b.Student).Count(),
                                Data = new CodeWithIdVm
                                {
                                    Id = a.Homeroom.IdGrade,
                                    Description = a.Homeroom.Grade.Description,
                                    Code = "idgrade"
                                }
                            }
                        },
                        Total = getHomeroomStudent.Where(b => b.Student.IdCountry == a.Student.IdCountry).Select(b => b.Student).Count()
                    })
                        .ToList();*/
                    #endregion
                }
                else
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        var header = new List<string>();

                        var getHeader = getHomeroomStudent
                            .Select(a => a.Homeroom.Grade.Description)
                            .OrderBy(a => a.Length)
                                .ThenBy(a => a)
                            .Distinct()
                            .ToList();

                        header.AddRange(getHeader);

                        var body = new List<Body>();

                        var getHomeroomStudentResult = getHomeroomStudent.Where(a => a.Semester == i).ToList();

                        var groupedData = getHomeroomStudentResult
                            .GroupBy(a => new { a.Student.IdCountry, a.Homeroom.IdGrade })
                            .Select(group => new
                            {
                                CountryId = group.Key.IdCountry,
                                GradeId = group.Key.IdGrade,
                                MaleCount = group.Count(s => s.Student.Gender == Gender.Male),
                                FemaleCount = group.Count(s => s.Student.Gender == Gender.Female)
                            })
                            .Distinct()
                            .ToList();

                        var allCountryGradeCombinations = getHomeroomStudentResult
                            .Select(a => new { CountryId = a.Student.IdCountry, GradeId = a.Homeroom.IdGrade })
                            .Distinct()
                            .ToList();

                        var missingGrades = allCountryGradeCombinations
                            .SelectMany(gc => allCountryGradeCombinations
                                .Where(allGrade => !groupedData.Any(gd => gd.CountryId == gc.CountryId && gd.GradeId == allGrade.GradeId))
                                .Select(allGrade => new
                                {
                                    CountryId = gc.CountryId,
                                    GradeId = allGrade.GradeId,
                                    MaleCount = 0,
                                    FemaleCount = 0
                                })
                            )
                            .Distinct()
                            .ToList();

                        var finalData = groupedData.Concat(missingGrades)
                            .GroupBy(result => result.CountryId)
                            .Select(groupedResult => new Body
                            {
                                Country = new ItemValueVm
                                {
                                    Id = groupedResult.Key,
                                    Description = getHomeroomStudentResult.FirstOrDefault(c => c.Student.IdCountry == groupedResult.Key)?.Student.Country.CountryName
                                },
                                ListData = groupedResult
                                    .Select(data => new ListData
                                    {
                                        Male = data.MaleCount,
                                        Female = data.FemaleCount,
                                        Data = new CodeWithIdVm
                                        {
                                            Id = data.GradeId,
                                            Description = getHomeroomStudentResult.FirstOrDefault(g => g.Homeroom.IdGrade == data.GradeId)?.Homeroom.Grade.Description,
                                            Code = "idgrade"
                                        }
                                    })
                                    .OrderBy(data => data.Data.Description.Length)
                                        .ThenBy(data => data.Data.Description)
                                    .ToList(),
                                Total = groupedResult.Sum(item => item.MaleCount + item.FemaleCount)
                            })
                            .OrderByDescending(a => a.Country.Description == "Indonesia")
                            .ThenBy(a => a.Country.Description)
                            .ToList();

                        body.AddRange(finalData);

                        var getTotalStudent = new TotalStudent
                        {
                            ListData = getHomeroomStudentResult
                                .GroupBy(a => new
                                {
                                    a.Homeroom.IdGrade,
                                    a.Homeroom.Grade.Description
                                })
                                .Select(group => new ListData
                                {
                                    Male = group.Count(b => b.Student.Gender == Gender.Male),
                                    Female = group.Count(b => b.Student.Gender == Gender.Female),
                                    Data = new CodeWithIdVm
                                    {
                                        Id = group.Key.IdGrade,
                                        Description = group.Key.Description,
                                        Code = "idgrade"
                                    }
                                }).OrderBy(group => group.Data.Description.Length).ThenBy(group => group.Data.Description).ToList(),
                            Total = getHomeroomStudentResult.Select(a => a.Student).Count()
                        };

                        var data = getHomeroomStudentResult.Select(a => new GetStudentNationalityDemographyResult
                        {
                            Semester = a.Semester,
                            Header = header,
                            Body = body,
                            TotalStudent = getTotalStudent
                        })
                            .FirstOrDefault();

                        items.Add(data);
                    }
                }
            }
            else if (param.ViewCategoryType == "homeroom")
            {
                if (param.Semester != null)
                {
                    var header = new List<string>();

                    var getHeader = getHomeroomStudent
                        .Select(a => a.Homeroom.Grade.Code + a.Homeroom.MsGradePathwayClassroom.Classroom.Code)
                        .OrderBy(a => a.Length)
                            .ThenBy(a => a)
                        .Distinct()
                        .ToList();

                    header.AddRange(getHeader);

                    var body = new List<Body>();

                    var getHomeroomStudentResult = getHomeroomStudent.ToList();

                    var groupedData = getHomeroomStudentResult
                        .GroupBy(a => new { a.Student.IdCountry, a.IdHomeroom })
                        .Select(group => new
                        {
                            CountryId = group.Key.IdCountry,
                            HomeroomId = group.Key.IdHomeroom,
                            MaleCount = group.Count(s => s.Student.Gender == Gender.Male),
                            FemaleCount = group.Count(s => s.Student.Gender == Gender.Female)
                        })
                        .Distinct()
                        .ToList();

                    var allCountryGradeCombinations = getHomeroomStudentResult
                        .Select(a => new { CountryId = a.Student.IdCountry, HomeroomId = a.IdHomeroom })
                        .Distinct()
                        .ToList();

                    var missingGrades = allCountryGradeCombinations
                        .SelectMany(gc => allCountryGradeCombinations
                            .Where(allGrade => !groupedData.Any(gd => gd.CountryId == gc.CountryId && gd.HomeroomId == allGrade.HomeroomId))
                            .Select(allGrade => new
                            {
                                CountryId = gc.CountryId,
                                HomeroomId = allGrade.HomeroomId,
                                MaleCount = 0,
                                FemaleCount = 0
                            })
                        )
                        .Distinct()
                        .ToList();

                    var finalData = groupedData.Concat(missingGrades)
                        .GroupBy(result => result.CountryId)
                        .Select(groupedResult => new Body
                        {
                            Country = new ItemValueVm
                            {
                                Id = groupedResult.Key,
                                Description = getHomeroomStudentResult.FirstOrDefault(c => c.Student.IdCountry == groupedResult.Key)?.Student.Country.CountryName
                            },
                            ListData = groupedResult
                                .Select(data => new ListData
                                {
                                    Male = data.MaleCount,
                                    Female = data.FemaleCount,
                                    Data = new CodeWithIdVm
                                    {
                                        Id = data.HomeroomId,
                                        Description = getHomeroomStudentResult.FirstOrDefault(g => g.IdHomeroom == data.HomeroomId)?.Homeroom.Grade.Code + getHomeroomStudentResult.FirstOrDefault(g => g.IdHomeroom == data.HomeroomId)?.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                                        Code = "idhomeroom"
                                    }
                                })
                                .OrderBy(data => data.Data.Description.Length)
                                    .ThenBy(data => data.Data.Description)
                                .ToList(),
                            Total = groupedResult.Sum(item => item.MaleCount + item.FemaleCount)
                        })
                        .OrderByDescending(a => a.Country.Description == "Indonesia")
                        .ThenBy(a => a.Country.Description)
                        .ToList();

                    body.AddRange(finalData);

                    var getTotalStudent = new TotalStudent
                    {
                        ListData = getHomeroomStudent
                            .GroupBy(a => new
                            {
                                a.IdHomeroom,
                                HomeroomDesc = a.Homeroom.Grade.Code + a.Homeroom.MsGradePathwayClassroom.Classroom.Code
                            })
                            .Select(group => new ListData
                            {
                                Male = group.Count(b => b.Student.Gender == Gender.Male),
                                Female = group.Count(b => b.Student.Gender == Gender.Female),
                                Data = new CodeWithIdVm
                                {
                                    Id = group.Key.IdHomeroom,
                                    Description = group.Key.HomeroomDesc,
                                    Code = "idhomeroom"
                                }
                            }).OrderBy(group => group.Data.Description.Length).ThenBy(group => group.Data.Description).ToList(),
                        Total = getHomeroomStudent.Select(a => a.Student).Count()
                    };

                    var data = getHomeroomStudent.Select(a => new GetStudentNationalityDemographyResult
                    {
                        Semester = a.Semester,
                        Header = header,
                        Body = body,
                        TotalStudent = getTotalStudent
                    })
                        .FirstOrDefault();

                    items.Add(data);
                    #region unused code
                    /*var getBody = getHomeroomStudent
                    .GroupBy(a => new
                    {
                        a.Student.IdCountry,
                        a.Student.Country.CountryName,
                        a.IdHomeroom,
                        HomeroomDesc = a.Homeroom.Grade.Code + a.Homeroom.MsGradePathwayClassroom.Classroom.Code
                    })
                    .Select(group => new
                    {
                        Country = new ItemValueVm
                        {
                            Id = group.Key.IdCountry,
                            Description = group.Key.CountryName
                        },
                        ListData = new List<ListData>
                        {
                            new ListData
                            {
                                Male = group.Count(b => b.Student.Gender == Gender.Male),
                                Female = group.Count(b => b.Student.Gender == Gender.Female),
                                Data = new CodeWithIdVm
                                {
                                    Id = group.Key.IdHomeroom,
                                    Description = group.Key.HomeroomDesc,
                                    Code = "idhomeroom"
                                }
                            }
                        },
                        Total = group.Count()
                    })
                    .GroupBy(result => new
                    {
                        result.Country.Id,
                        result.Country.Description
                    })
                    .Select(groupedResult => new Body
                    {
                        Country = new ItemValueVm
                        {
                            Id = groupedResult.Key.Id,
                            Description = groupedResult.Key.Description
                        },
                        ListData = groupedResult
                            .SelectMany(item => item.ListData)
                            .GroupBy(listData => new
                            {
                                listData.Data.Id,
                                listData.Data.Description
                            })
                            .Select(groupedListData => new ListData
                            {
                                Male = groupedListData.Sum(data => data.Male),
                                Female = groupedListData.Sum(data => data.Female),
                                Data = new CodeWithIdVm
                                {
                                    Id = groupedListData.Key.Id,
                                    Description = groupedListData.Key.Description,
                                    Code = "idhomeroom"
                                }
                            })
                            .ToList(),
                        Total = groupedResult.Sum(item => item.Total)
                    })
                    .OrderBy(a => a.Country.Description)
                    .ToList();*/

                    /*                var getBody = getHomeroomStudent
                                        .GroupBy(a => new
                                        {
                                            a.Student.IdCountry,
                                            a.Student.Country.CountryName
                                        })
                                        .Select(group => new Body
                                        {
                                            Country = new ItemValueVm
                                            {
                                                Id = group.Key.IdCountry,
                                                Description = group.Key.CountryName
                                            },
                                            ListData = group.Select(g => new ListData
                                            {
                                                Male = group.Count(b => b.Student.Gender == Gender.Male),
                                                Female = group.Count(b => b.Student.Gender == Gender.Female),
                                                Data = new CodeWithIdVm
                                                {
                                                    Id = g.Homeroom.IdGrade,
                                                    Description = g.Homeroom.Grade.Description,
                                                    Code = "idgrade"
                                                }
                                            })
                                            .GroupBy(listData => new { listData.Data.Id, listData.Data.Description })
                                            .Select(groupedListData => new ListData
                                            {
                                                Male = groupedListData,
                                                Female = groupedListData.First().Female,
                                                Data = new CodeWithIdVm
                                                {
                                                    Id = groupedListData.Key.Id,
                                                    Description = groupedListData.Key.Description,
                                                    Code = "idgrade"
                                                }
                                            })
                                            .ToList(),
                                            Total = group.Count()
                                        }).ToList();*/

                    //var getBody = getHomeroomStudent
                    //    .GroupBy(a => new
                    //    {
                    //        a.Student.IdCountry,
                    //        a.Student.Country.CountryName
                    //        //a.Homeroom.IdGrade,
                    //        //a.Homeroom.Grade.Description
                    //    })
                    //    .Select(group => new Body
                    //    {
                    //        Country = new ItemValueVm
                    //        {
                    //            Id = group.Key.IdCountry,
                    //            Description = group.Key.CountryName
                    //        },
                    //        ListData = group.Select(g => new ListData
                    //        {
                    //            Male = group.Count(b => b.Student.Gender == Gender.Male),
                    //            Female = group.Count(b => b.Student.Gender == Gender.Female),
                    //            Data = new CodeWithIdVm
                    //            {
                    //                Id = g.Homeroom.IdGrade,
                    //                Description = g.Homeroom.Grade.Description,
                    //                Code = "idgrade"
                    //            }
                    //        }).ToList(),
                    //        Total = group.Count()
                    //    }).ToList();

                    /*var getBody = getHomeroomStudent.Select(a => new Body
                    {
                        Country = new ItemValueVm
                        {
                            Id = a.Student.IdCountry,
                            Description = a.Student.Country.CountryName
                        },
                        ListData = new List<ListData>
                        {
                            new ListData
                            {
                                Male = getHomeroomStudent.Where(b => b.Student.Gender.Equals("Male") && b.Student.IdCountry == a.Student.IdCountry).Select(b => b.Student).Count(),
                                Female = getHomeroomStudent.Where(b => b.Student.Gender.Equals("Female") && b.Student.IdCountry == a.Student.IdCountry).Select(b => b.Student).Count(),
                                Data = new CodeWithIdVm
                                {
                                    Id = a.Homeroom.IdGrade,
                                    Description = a.Homeroom.Grade.Description,
                                    Code = "idgrade"
                                }
                            }
                        },
                        Total = getHomeroomStudent.Where(b => b.Student.IdCountry == a.Student.IdCountry).Select(b => b.Student).Count()
                    })
                        .ToList();*/
                    #endregion
                }
                else
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        var header = new List<string>();

                        var getHeader = getHomeroomStudent
                            .Select(a => a.Homeroom.Grade.Code + a.Homeroom.MsGradePathwayClassroom.Classroom.Code)
                            .OrderBy(a => a.Length)
                                .ThenBy(a => a)
                            .Distinct()
                            .ToList();

                        header.AddRange(getHeader);

                        var body = new List<Body>();

                        var getHomeroomStudentResult = getHomeroomStudent.Where(a => a.Semester == i).ToList();

                        var groupedData = getHomeroomStudentResult
                            .GroupBy(a => new { a.Student.IdCountry, a.IdHomeroom })
                            .Select(group => new
                            {
                                CountryId = group.Key.IdCountry,
                                HomeroomId = group.Key.IdHomeroom,
                                MaleCount = group.Count(s => s.Student.Gender == Gender.Male),
                                FemaleCount = group.Count(s => s.Student.Gender == Gender.Female)
                            })
                            .Distinct()
                            .ToList();

                        var allCountryGradeCombinations = getHomeroomStudentResult
                            .Select(a => new { CountryId = a.Student.IdCountry, HomeroomId = a.IdHomeroom })
                            .Distinct()
                            .ToList();

                        var missingGrades = allCountryGradeCombinations
                            .SelectMany(gc => allCountryGradeCombinations
                                .Where(allGrade => !groupedData.Any(gd => gd.CountryId == gc.CountryId && gd.HomeroomId == allGrade.HomeroomId))
                                .Select(allGrade => new
                                {
                                    CountryId = gc.CountryId,
                                    HomeroomId = allGrade.HomeroomId,
                                    MaleCount = 0,
                                    FemaleCount = 0
                                })
                            )
                            .Distinct()
                            .ToList();

                        var finalData = groupedData.Concat(missingGrades)
                            .GroupBy(result => result.CountryId)
                            .Select(groupedResult => new Body
                            {
                                Country = new ItemValueVm
                                {
                                    Id = groupedResult.Key,
                                    Description = getHomeroomStudentResult.FirstOrDefault(c => c.Student.IdCountry == groupedResult.Key)?.Student.Country.CountryName
                                },
                                ListData = groupedResult
                                    .Select(data => new ListData
                                    {
                                        Male = data.MaleCount,
                                        Female = data.FemaleCount,
                                        Data = new CodeWithIdVm
                                        {
                                            Id = data.HomeroomId,
                                            Description = getHomeroomStudentResult.FirstOrDefault(g => g.IdHomeroom == data.HomeroomId)?.Homeroom.Grade.Code + getHomeroomStudentResult.FirstOrDefault(g => g.IdHomeroom == data.HomeroomId)?.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                                            Code = "idhomeroom"
                                        }
                                    })
                                    .OrderBy(data => data.Data.Description.Length)
                                        .ThenBy(data => data.Data.Description)
                                    .ToList(),
                                Total = groupedResult.Sum(item => item.MaleCount + item.FemaleCount)
                            })
                            .OrderByDescending(a => a.Country.Description == "Indonesia")
                            .ThenBy(a => a.Country.Description)
                            .ToList();

                        body.AddRange(finalData);

                        var getTotalStudent = new TotalStudent
                        {
                            ListData = getHomeroomStudentResult
                                .GroupBy(a => new
                                {
                                    a.IdHomeroom,
                                    HomeroomDesc = a.Homeroom.Grade.Code + a.Homeroom.MsGradePathwayClassroom.Classroom.Code
                                })
                                .Select(group => new ListData
                                {
                                    Male = group.Count(b => b.Student.Gender == Gender.Male),
                                    Female = group.Count(b => b.Student.Gender == Gender.Female),
                                    Data = new CodeWithIdVm
                                    {
                                        Id = group.Key.IdHomeroom,
                                        Description = group.Key.HomeroomDesc,
                                        Code = "idhomeroom"
                                    }
                                }).OrderBy(group => group.Data.Description.Length).ThenBy(group => group.Data.Description).ToList(),
                            Total = getHomeroomStudentResult.Select(a => a.Student).Count()
                        };

                        var data = getHomeroomStudentResult.Select(a => new GetStudentNationalityDemographyResult
                        {
                            Semester = a.Semester,
                            Header = header,
                            Body = body,
                            TotalStudent = getTotalStudent
                        })
                            .FirstOrDefault();

                        items.Add(data);
                    }
                }
            }

            return Request.CreateApiResult2(items as object);
        }
    }
}
