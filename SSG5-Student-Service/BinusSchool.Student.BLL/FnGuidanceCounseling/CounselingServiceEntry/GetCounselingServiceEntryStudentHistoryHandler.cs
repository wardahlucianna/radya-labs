using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselingServiceEntryStudentHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetCounselingServiceEntryStudentHistoryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCounselingServiceEntryStudentHistoryRequest>();

            var columns = new[] { "AcademicYear", "CounselingCategory", "CounselorName", "CounselingDate" };

            var query = _dbContext.Entity<TrCounselingServicesEntry>()
                        .Include(f => f.CounselingServicesEntryConcern)
                        .Include(f => f.CounselingServicesEntryConcern).ThenInclude(f => f.ConcernCategory)
                        .Include(f => f.CounselingServicesEntryAttachment)
                        .Include(f => f.CounselingCategory)
                        .Include(f => f.Counselor)
                        .Include(f => f.Counselor).ThenInclude(f => f.User)
                        .Include(f => f.AcademicYear)
                        .Include(f => f.Student)
                        .Include(f => f.Student).ThenInclude(f => f.StudentParents).ThenInclude(f => f.Parent)
                        .Include(f => f.Student).ThenInclude(f => f.MsHomeroomStudents).ThenInclude(f => f.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom)
                        .Include(f => f.Student).ThenInclude(f => f.MsHomeroomStudents).ThenInclude(f => f.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel)
                        .Where(x => x.IdStudent == param.IdStudent)
                        .Select(x => new GetCounselingServiceEntryStudentHistoryResult
                        {
                            Id = x.Id,
                            AcademicYear = new CodeWithIdVm
                            {
                                Id = x.AcademicYear.Id,
                                Code = x.AcademicYear.Code,
                                Description = x.AcademicYear.Description
                            },
                            CounselorName = x.Counselor.User.DisplayName,
                            CounselingCategory = new NameValueVm
                            {
                                Id = x.CounselingCategory.Id,
                                Name = x.CounselingCategory.CounselingCategoryName
                            },
                            CounselingDate = x.DateTime,
                            Students = new StudentData
                            {
                                StudentName = string.IsNullOrEmpty(x.Student.LastName) ? $"{x.Student.FirstName}{x.Student.MiddleName}" : $"{x.Student.FirstName}{x.Student.LastName}",
                                IdBinusian = x.Student.IdBinusian
                            }
                        });

            //filter
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
            {
                query = query.Where(x => EF.Functions.Like(x.AcademicYear.Id, param.IdAcademicYear));
            }
            if (!string.IsNullOrEmpty(param.IdCounselingCategory))
            {
                query = query.Where(x => EF.Functions.Like(x.CounselingCategory.Id, param.IdCounselingCategory));
            }
            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.CounselorName, param.SearchPattern()) ||
                EF.Functions.Like(x.CounselingCategory.Name, param.SearchPattern()));
            }

            //ordering
            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                switch (param.OrderBy.ToLower())
                {
                    case "academicyear":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.AcademicYear.Description)
                            : query.OrderBy(x => x.AcademicYear.Description);
                        break;
                    case "counselingdate":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.CounselingDate)
                            : query.OrderBy(x => x.CounselingDate);
                        break;
                    case "counselingcategory":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.CounselingCategory.Name)
                            : query.OrderBy(x => x.CounselingCategory.Name);
                        break;
                    case "counselorname":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.CounselorName)
                            : query.OrderBy(x => x.CounselorName);
                        break;
                }
            }

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = query
                    .Select(x => new ItemValueVm(x.Id, x.Id))
                    .ToList();
            }
            else
            {
                var result = await query
                .SetPagination(param).ToListAsync(CancellationToken);

                items = result
                .Select(x => new GetCounselingServiceEntryStudentHistoryResult
                {
                    Id = x.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.AcademicYear.Id,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    Students = new StudentData
                    {
                        StudentName = x.Students.StudentName,
                        IdBinusian = x.Students.IdBinusian
                    },
                    CounselingCategory = new NameValueVm
                    {
                        Id = x.CounselingCategory.Id,
                        Name = x.CounselingCategory.Name
                    },
                    CounselorName = x.CounselorName,
                    CounselingDate = x.CounselingDate
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
