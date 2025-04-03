using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink
{
    public class UnivInformationManagementUsefulLinkHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IStudentDbContext _dbContext;
        public UnivInformationManagementUsefulLinkHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetUnivInformationManagementUsefulLink = await _dbContext.Entity<MsUsefulLink>()
           .Where(x => ids.Contains(x.Id))
           .ToListAsync(CancellationToken);

            var GetUnivInformationManagementUsefulLinkGrade = await _dbContext.Entity<MsUsefulLinkGrade>()
           .Where(x => ids.Contains(x.IdUsefulLink))
           .ToListAsync(CancellationToken);

            GetUnivInformationManagementUsefulLink.ForEach(x => x.IsActive = false);

            GetUnivInformationManagementUsefulLinkGrade.ForEach(x => x.IsActive = false);

            _dbContext.Entity<MsUsefulLink>().UpdateRange(GetUnivInformationManagementUsefulLink);

            _dbContext.Entity<MsUsefulLinkGrade>().UpdateRange(GetUnivInformationManagementUsefulLinkGrade);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsUsefulLink>()
            .Include(p => p.AcademicYear)
            .Where(x => x.Id == id)
            .Select(x => new GetUnivInformationManagementUsefulLinkResult
            {
                Id = x.Id,
                AcademicYear = new CodeWithIdVm
                {
                    Id = x.AcademicYear.Id,
                    Code = x.AcademicYear.Code,
                    Description = x.AcademicYear.Description
                },
                Grade = _dbContext.Entity<MsUsefulLinkGrade>()
                    .Include(p => p.Grade)
                    .Where(x => x.IdUsefulLink == id)
                    .Select(e => new CodeWithIdVm
                    {
                        Id = e.Grade.Id,
                        Code = e.Grade.Code,
                        Description = e.Grade.Description
                    }).ToList(),
                Link = x.Link,
                Description = x.Description
            }).FirstOrDefaultAsync(CancellationToken);


            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetUnivInformationManagementUsefulLinkRequest>();

            var columns = new[] { "AcademicYear", "Grade", "Description", "Link" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "AcademicYear.Description" },
                { columns[1], "Grade.Description" },
                { columns[2], "LinkDescription" },
                { columns[3], "Link" }
            };

            var query = _dbContext.Entity<MsUsefulLink>()
                .Include(x => x.AcademicYear)
                .Include(x => x.UsefulLinkGrade).ThenInclude(e => e.Grade).AsQueryable();
            //.OrderByDescending(x => x.DateIn)
            //.OrderByDynamic(param, aliasColumns)


            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Link, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
            }

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
            {
                query = query.Where(x => EF.Functions.Like(x.AcademicYear.Id, param.IdAcademicYear));
            }

            if (!string.IsNullOrEmpty(param.GradeId))
            {
                query = query.Where(x => x.UsefulLinkGrade.Any(y => y.IdGrade == param.GradeId));
            }

            ////ordering
            query = query.OrderByDescending(x => x.DateIn);

            switch (param.OrderBy)
            {
                case "academicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.UsefulLinkGrade.Select(y => y.Grade.OrderNumber).FirstOrDefault())
                        : query.OrderBy(x => x.UsefulLinkGrade.Select(y => y.Grade.OrderNumber).FirstOrDefault());
                    break;
                case "description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
                case "link":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Link)
                        : query.OrderBy(x => x.Link);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                        .Select(x => new GetUnivInformationManagementUsefulLinkResult
                        {
                            Id = x.Id,
                            AcademicYear = new CodeWithIdVm
                            {
                                Id = x.AcademicYear.Id,
                                Code = x.AcademicYear.Code,
                                Description = x.AcademicYear.Description
                            },
                            Grade = x.UsefulLinkGrade.OrderBy(x => x.Grade.OrderNumber).Select(y => new CodeWithIdVm
                            {
                                Id = y.IdGrade,
                                Code = y.Grade.Code,
                                Description = y.Grade.Description
                            }).ToList(),
                            Link = x.Link,
                            Description = x.Description
                        }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                        .Select(x => new GetUnivInformationManagementUsefulLinkResult
                        {
                            Id = x.Id,
                            AcademicYear = new CodeWithIdVm
                            {
                                Id = x.AcademicYear.Id,
                                Code = x.AcademicYear.Code,
                                Description = x.AcademicYear.Description
                            },
                            Grade = x.UsefulLinkGrade.OrderBy(x => x.Grade.OrderNumber).Select(y => new CodeWithIdVm
                            {
                                Id = y.IdGrade,
                                Code = y.Grade.Code,
                                Description = y.Grade.Description
                            }).ToList(),
                            Link = x.Link,
                            Description = x.Description
                        }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.ToList().Count;

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));

        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<AddUnivInformationManagementUsefulLinkRequest, AddUnivInformationManagementUsefulLinkValidator>();

            if (body.Link.StartsWith("http://") == false && body.Link.StartsWith("https://") == false)
            {
                body.Link = "http://" + body.Link;
            }

            var existsData = _dbContext.Entity<MsUsefulLink>()
                .Any(x => x.IdAcademicYear == body.IdAcademicYear && x.Link == body.Link);

            if (existsData)
            {
                throw new BadRequestException($"Link { body.Link} already exists.");
            }

            var existsData1 = _dbContext.Entity<MsUsefulLink>()
            .Any(x => x.IdAcademicYear == body.IdAcademicYear&& x.Description == body.LinkDescription);

            if (existsData1)
            {
                throw new BadRequestException($"Link description { body.LinkDescription} already exists.");
            }



            var idUnivInformationManagementUsefulLink = Guid.NewGuid().ToString();

            var newUnivInformationManagementUsefulLink = new MsUsefulLink
            {
                Id = idUnivInformationManagementUsefulLink,
                IdAcademicYear = body.IdAcademicYear,
                Link = body.Link,
                Description = body.LinkDescription,
            };

            if (body.GradeIds != null)
            {
                foreach (var Gradeid in body.GradeIds)
                {
                    var newUnivInformationManagementUsefulLinkGrade = new MsUsefulLinkGrade
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUsefulLink = idUnivInformationManagementUsefulLink,
                        IdGrade = Gradeid.Id
                    };

                    _dbContext.Entity<MsUsefulLinkGrade>().Add(newUnivInformationManagementUsefulLinkGrade);
                }
            }

            _dbContext.Entity<MsUsefulLink>().Add(newUnivInformationManagementUsefulLink);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateUnivInformationManagementUsefulLinkRequest, UpdateUnivInformationManagementUsefulLinkValidator>();
            var GetUnivInformationManagementUsefulLink = await _dbContext.Entity<MsUsefulLink>().Where(e => e.Id == body.IdUnivInformationManagementUsefulLink).SingleOrDefaultAsync(CancellationToken);
            var GetUnivInformationManagementUsefulLinkGrade = await _dbContext.Entity<MsUsefulLinkGrade>().Where(e => e.IdUsefulLink == body.IdUnivInformationManagementUsefulLink).ToListAsync(CancellationToken);

            if (GetUnivInformationManagementUsefulLink is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["University Information Management Useful Link"], "Id", body.IdUnivInformationManagementUsefulLink));
            }

            if (!string.IsNullOrEmpty(body.Link))
            {
                if (body.Link.StartsWith("http://") == false && body.Link.StartsWith("https://") == false)
                {
                    body.Link = "http://" + body.Link;
                }
            }

            if (GetUnivInformationManagementUsefulLink.Link != body.Link )
            {
                var academicYearId = GetUnivInformationManagementUsefulLink.IdAcademicYear;
                var checklink = _dbContext.Entity<MsUsefulLink>().Where(x => x.Link == body.Link && x.IdAcademicYear == academicYearId).FirstOrDefault();

                if (checklink != null)
                {
                    throw new BadRequestException($"Link {body.Link} already exists");
                }
            }



            GetUnivInformationManagementUsefulLink.Link = body.Link;

            if ( GetUnivInformationManagementUsefulLink.Description != body.LinkDescription)
            {
                var academicYearId = GetUnivInformationManagementUsefulLink.IdAcademicYear;

                var checklink1 = _dbContext.Entity<MsUsefulLink>().Where(x => x.Description == body.LinkDescription && x.IdAcademicYear == academicYearId).FirstOrDefault();

                if (checklink1 != null)
                {
                    throw new BadRequestException($"Link description {body.LinkDescription} already exists");
                }

                GetUnivInformationManagementUsefulLink.Description = body.LinkDescription;
            }

            _dbContext.Entity<MsUsefulLink>().Update(GetUnivInformationManagementUsefulLink);

            //update data in MsUsefullLinkGrade
            //remove
            foreach (var ItemGrade in GetUnivInformationManagementUsefulLinkGrade)
            {
                var ExsisBodyGradeId = body.GradeIds.Any(e => e.Id == ItemGrade.IdGrade);

                if (!ExsisBodyGradeId)
                {
                    ItemGrade.IsActive = false;
                    _dbContext.Entity<MsUsefulLinkGrade>().Update(ItemGrade);
                }
            }

            ////Add Grade
            if (body.GradeIds != null)
            {
                foreach (var GradeId in body.GradeIds)
                {
                    var ExsistdbId = GetUnivInformationManagementUsefulLinkGrade.Where(e => e.IdGrade == GradeId.Id && e.IdUsefulLink == GetUnivInformationManagementUsefulLink.Id).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newArticleManagementUsefulLink = new MsUsefulLinkGrade
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUsefulLink = GetUnivInformationManagementUsefulLink.Id,
                            IdGrade = GradeId.Id
                        };

                        _dbContext.Entity<MsUsefulLinkGrade>().Add(newArticleManagementUsefulLink);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
