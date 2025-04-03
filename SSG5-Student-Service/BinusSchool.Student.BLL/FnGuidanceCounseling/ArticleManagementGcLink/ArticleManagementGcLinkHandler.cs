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
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementGcLink;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Scheduling.FnSchedule.ArticleManagementGcLink.Validator;
using BinusSchool.Student.FnGuidanceCounseling.ArticleManagementGcLink.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnGuidanceCounseling.ArticleManagementGcLink
{
    public class ArticleManagementGcLinkHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IStudentDbContext _dbContext;
        public ArticleManagementGcLinkHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetArticleManagementGcLink = await _dbContext.Entity<MsGcLink>()
           .Where(x => ids.Contains(x.Id))
           .ToListAsync(CancellationToken);

            var GetArticleManagementGcLinkGrade = await _dbContext.Entity<MsGcLinkGrade>()
           .Where(x => ids.Contains(x.IdGcLink))
           .ToListAsync(CancellationToken);

            var GetArticleManagementGcLinkLogo = await _dbContext.Entity<MsGcLinkLogo>()
           .Where(x => ids.Contains(x.IdGcLink))
           .ToListAsync(CancellationToken);

            GetArticleManagementGcLink.ForEach(x => x.IsActive = false);

            GetArticleManagementGcLinkGrade.ForEach(x => x.IsActive = false);

            GetArticleManagementGcLinkLogo.ForEach(x => x.IsActive = false);

            _dbContext.Entity<MsGcLink>().UpdateRange(GetArticleManagementGcLink);

            _dbContext.Entity<MsGcLinkGrade>().UpdateRange(GetArticleManagementGcLinkGrade);

            _dbContext.Entity<MsGcLinkLogo>().UpdateRange(GetArticleManagementGcLinkLogo);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsGcLink>()
            .Include(p => p.AcademicYear)
            .Where(x => x.Id == id)
            .Select(x => new GetArticleManagementGcLinkResult
            {
                Id = x.Id,
                AcademicYear = new CodeWithIdVm
                {
                    Id = x.AcademicYear.Id,
                    Code = x.AcademicYear.Code,
                    Description = x.AcademicYear.Description
                },
                Grade = _dbContext.Entity<MsGcLinkGrade>()
                            .Include(p => p.Grade)
                            .Where(x => x.IdGcLink == id)
                            .Select(e => new CodeWithIdVm
                            {
                                Id = e.Grade.Id,
                                Code = e.Grade.Code,
                                Description = e.Grade.Description
                            }).ToList(),
                Link = x.Link,
                Description = x.LinkDescription,
                Logo = x.GcLinkLogo.Where(e => e.IdGcLink == id).Select(e => new LogoArticleManagementGcLink
                {
                    Id = e.Id,
                    Url = e.Url,
                    OriginalFilename = e.OriginalName,
                    FileName = e.FileName,
                    FileSize = e.FileSize,
                    FileType = e.FileType,
                }).ToList()
            }).FirstOrDefaultAsync(CancellationToken);


            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetArticleManagementGcLinkRequest>();

            var columns = new[] { "AcademicYear", "Grade", "Description", "Link" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "AcademicYear.Description" },
                { columns[1], "Grade.Description" },
                { columns[2], "LinkDescription" },
                { columns[3], "Link" }
            };

            var query = _dbContext.Entity<MsGcLink>()
                       .Include(e => e.AcademicYear)
                       .Include(e => e.GcLinkGrades).ThenInclude(e => e.Grade)
                       .Include(e => e.GcLinkLogo).AsQueryable();
            //.OrderByDescending(x => x.DateIn)
            //.OrderByDynamic(param, aliasColumns)


            //filter
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                query = query.Where(x => EF.Functions.Like(x.AcademicYear.Id, param.IdAcademicYear));
            if (!string.IsNullOrEmpty(param.GradeId))
                query = query.Where(x => x.GcLinkGrades.Any(y => y.IdGrade == param.GradeId));
            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Link, param.SearchPattern()) ||
                        EF.Functions.Like(x.LinkDescription, param.SearchPattern()));
            }

            //ordering
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
                        ? query.OrderByDescending(x => x.GcLinkGrades.Select(y => y.Grade.OrderNumber).FirstOrDefault())
                        : query.OrderBy(x => x.GcLinkGrades.Select(y => y.Grade.OrderNumber).FirstOrDefault());
                    break;
                case "description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LinkDescription)
                        : query.OrderBy(x => x.LinkDescription);
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
                    .Select(x => new GetArticleManagementGcLinkResult
                    {
                        Id = x.Id,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.IdAcademicYear,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        Link = x.Link,
                        Description = x.LinkDescription,
                        Grade = x.GcLinkGrades.OrderBy(x=>x.Grade.OrderNumber).Select(y => new CodeWithIdVm
                        {
                            Id = y.IdGrade,
                            Code = y.Grade.Code,
                            Description = y.Grade.Description
                        }).ToList(),
                        Logo = x.GcLinkLogo.Select(e => new LogoArticleManagementGcLink
                        {
                            Id = e.Id,
                            Url = e.Url,
                            OriginalFilename = e.OriginalName,
                            FileName = e.FileName,
                            FileSize = e.FileSize,
                            FileType = e.FileType,
                        }).ToList()
                    }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetArticleManagementGcLinkResult
                    {
                        Id = x.Id,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.IdAcademicYear,
                            Code = x.AcademicYear.Code,
                            Description = x.AcademicYear.Description
                        },
                        Link = x.Link,
                        Description = x.LinkDescription,
                        Grade = x.GcLinkGrades.OrderBy(x => x.Grade.OrderNumber).Select(y => new CodeWithIdVm
                        {
                            Id = y.IdGrade,
                            Code = y.Grade.Code,
                            Description = y.Grade.Description
                        }).ToList(),
                        Logo = x.GcLinkLogo.Select(e => new LogoArticleManagementGcLink
                        {
                            Id = e.Id,
                            Url = e.Url,
                            OriginalFilename = e.OriginalName,
                            FileName = e.FileName,
                            FileSize = e.FileSize,
                            FileType = e.FileType,
                        }).ToList()
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

            var body = await Request.ValidateBody<AddArticleManagementGcLinkRequest, AddArticleManagementGcLinkValidator>();

            if (body.Link.StartsWith("http://") == false && body.Link.StartsWith("https://") == false)
            {
                body.Link = "http://" + body.Link;
            }
            var existsData = _dbContext.Entity<MsGcLink>()
            .Any(x => x.Link == body.Link && x.IdAcademicYear == body.IdAcademicYear);

            if (existsData)
            {
                throw new BadRequestException($"Link { body.Link} already exists.");
            }

            var existsData1 = _dbContext.Entity<MsGcLink>()
.           Any(x => x.LinkDescription == body.LinkDescription && x.IdAcademicYear == body.IdAcademicYear);

            if (existsData1)
            {
                throw new BadRequestException($"Link description { body.LinkDescription} already exists.");
            }



            var idArticleManagementGcLink = Guid.NewGuid().ToString();

            var newArticleManagementGcLink = new MsGcLink
            {
                Id = idArticleManagementGcLink,
                IdAcademicYear = body.IdAcademicYear,
                Link = body.Link,
                LinkDescription = body.LinkDescription,
            };


            if (body.GradeIds != null)
            {
                foreach (var Gradeid in body.GradeIds)
                {
                    var newArticleManagementGcLinkGrade = new MsGcLinkGrade
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdGcLink = idArticleManagementGcLink,
                        IdGrade = Gradeid.Id
                    };

                    _dbContext.Entity<MsGcLinkGrade>().Add(newArticleManagementGcLinkGrade);
                }
            }

            if (body.Logo != null)
            {
                foreach (var ItemLogo in body.Logo)
                {
                    var newArticleManagementGcLinkLogo = new MsGcLinkLogo
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdGcLink = newArticleManagementGcLink.Id,
                        OriginalName = ItemLogo.OriginalFilename,
                        Url = ItemLogo.Url,
                        FileName = ItemLogo.FileName,
                        FileType = ItemLogo.FileType,
                        FileSize = ItemLogo.FileSize,
                    };
                    _dbContext.Entity<MsGcLinkLogo>().Add(newArticleManagementGcLinkLogo);

                }
            }

            _dbContext.Entity<MsGcLink>().Add(newArticleManagementGcLink);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateArticleManagementGcLinkRequest, UpdateArticleManagementGcLinkValidator>();
            var GetArticleManagementGcLink = await _dbContext.Entity<MsGcLink>().Where(e => e.Id == body.IdArticleManagementPersonalGcLink).SingleOrDefaultAsync(CancellationToken);
            var GetArticleManagementGcLinkGrade = await _dbContext.Entity<MsGcLinkGrade>().Where(e => e.IdGcLink == body.IdArticleManagementPersonalGcLink).ToListAsync(CancellationToken);
            var GetArticleManagementGcLinkLogo = await _dbContext.Entity<MsGcLinkLogo>().Where(e => e.IdGcLink == body.IdArticleManagementPersonalGcLink).ToListAsync(CancellationToken);

            if (GetArticleManagementGcLink is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Article Management Gc Link"], "Id", body.IdArticleManagementPersonalGcLink));
            }
            if (!string.IsNullOrEmpty(body.Link))
            {
                if (body.Link.StartsWith("http://") == false && body.Link.StartsWith("https://") == false)
                {
                    body.Link = "http://" + body.Link;
                }
            }
            ////update data in MsGcLink
            if (GetArticleManagementGcLink.Link != body.Link)
            {
                var academicYearId = GetArticleManagementGcLink.IdAcademicYear;
                var checkArticleName = _dbContext.Entity<MsGcLink>().Where(x => x.Link == body.Link && academicYearId == x.IdAcademicYear).FirstOrDefault();

                if (checkArticleName != null)
                {
                    throw new BadRequestException($"Link {body.Link} already exists");
                }


            }
            GetArticleManagementGcLink.Link = body.Link;

            if (GetArticleManagementGcLink.LinkDescription != body.LinkDescription)
            {
                var academicYearId = GetArticleManagementGcLink.IdAcademicYear;

                var checkArticleName1 = _dbContext.Entity<MsGcLink>().Where(x => x.LinkDescription == body.LinkDescription && academicYearId == x.IdAcademicYear).FirstOrDefault();

                if (checkArticleName1 != null)
                {
                    throw new BadRequestException($"Link description {body.LinkDescription} already exists");
                }
                GetArticleManagementGcLink.LinkDescription = body.LinkDescription;

            }


            _dbContext.Entity<MsGcLink>().Update(GetArticleManagementGcLink);

            //update data in MsGcLinkGrade
            //remove Grade
            foreach (var ItemGrade in GetArticleManagementGcLinkGrade)
            {
                var ExsisBodyGradeId = body.GradeIds.Any(e => e.Id == ItemGrade.IdGrade);

                if (!ExsisBodyGradeId)
                {
                    ItemGrade.IsActive = false;
                    _dbContext.Entity<MsGcLinkGrade>().Update(ItemGrade);
                }
            }

            ////Add Grade
            if (body.GradeIds != null)
            {
                foreach (var Gradeid in body.GradeIds)
                {
                    var ExsistdbId = GetArticleManagementGcLinkGrade.Where(e => e.IdGrade == Gradeid.Id && e.IdGcLink == GetArticleManagementGcLink.Id).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newArticleManagementGcLink = new MsGcLinkGrade
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdGcLink = GetArticleManagementGcLink.Id,
                            IdGrade = Gradeid.Id
                        };

                        _dbContext.Entity<MsGcLinkGrade>().Add(newArticleManagementGcLink);
                    }
                }
            }

            //update data in MsGcLinkLogo
            //remove Logo
            foreach (var ItemLogo in GetArticleManagementGcLinkLogo)
            {
                var ExsisBodyLogo = body.Logo.Any(e => e.Id == ItemLogo.Id);

                if (!ExsisBodyLogo)
                {
                    ItemLogo.IsActive = false;
                    _dbContext.Entity<MsGcLinkLogo>().Update(ItemLogo);
                }
            }

            //Add Logo
            foreach (var ItemLogo in body.Logo.Where(e => e.Id == null || e.Id == "").ToList())
            {
                var newArticleManagementGcLinkLogo = new MsGcLinkLogo
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGcLink = GetArticleManagementGcLink.Id,
                    OriginalName = ItemLogo.OriginalFilename,
                    Url = ItemLogo.Url,
                    FileName = ItemLogo.FileName,
                    FileType = ItemLogo.FileType,
                    FileSize = ItemLogo.FileSize,
                };
                _dbContext.Entity<MsGcLinkLogo>().Add(newArticleManagementGcLinkLogo);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
