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
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing
{
    public class ArticleManagementPersonalWellBeingHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IStudentDbContext _dbContext;
        public ArticleManagementPersonalWellBeingHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetArticleManagementPersonalWellBeing = await _dbContext.Entity<TrPersonalWellBeing>()
           .Where(x => ids.Contains(x.Id))
           .ToListAsync(CancellationToken);

            var GetArticleManagementPersonalWellBeingLevel = await _dbContext.Entity<TrPersonalWellBeingLevel>()
           .Where(x => ids.Contains(x.IdPersonalWellBeing))
           .ToListAsync(CancellationToken);

            var GetArticleManagementPersonalWellBeingAttachment = await _dbContext.Entity<TrPersonalWellBeingAttachment>()
           .Where(x => ids.Contains(x.IdPersonalWellBeing))
           .ToListAsync(CancellationToken);

            GetArticleManagementPersonalWellBeing.ForEach(x => x.IsActive = false);

            GetArticleManagementPersonalWellBeingLevel.ForEach(x => x.IsActive = false);

            GetArticleManagementPersonalWellBeingAttachment.ForEach(x => x.IsActive = false);

            _dbContext.Entity<TrPersonalWellBeing>().UpdateRange(GetArticleManagementPersonalWellBeing);

            _dbContext.Entity<TrPersonalWellBeingLevel>().UpdateRange(GetArticleManagementPersonalWellBeingLevel);

            _dbContext.Entity<TrPersonalWellBeingAttachment>().UpdateRange(GetArticleManagementPersonalWellBeingAttachment);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            var query = await _dbContext.Entity<TrPersonalWellBeing>()
            .Include(p => p.AcademicYear)
            .Where(x => x.Id == id)
            .Select(x => new GetArticleManagementPersonalWellBeingResult
            {
                Id = x.Id,
                AcademicYear = new CodeWithIdVm
                {
                    Id = x.AcademicYear.Id,
                    Code = x.AcademicYear.Code,
                    Description = x.AcademicYear.Description
                },
                Level = _dbContext.Entity<TrPersonalWellBeingLevel>()
                    .Include(p => p.Level)
                    .Where(x => x.IdPersonalWellBeing == id)
                    .Select(e => new CodeWithIdVm
                    {
                        Id = e.Level.Id,
                        Code = e.Level.Code,
                        Description = e.Level.Description
                    }).ToList(),
                ArticleName = x.ArticleName,
                Description = x.Description,
                ViewFor = x.For.ToString(),
                Link = x.Link,
                NotifyRecipient = x.NotifRecipient,
                Attachments = x.PersonalWellBeingAttachment.Where(e => e.IdPersonalWellBeing == id).Select(e => new AttachmentArticleManagementPersonalWellBeing
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
            
            var param = Request.ValidateParams<GetArticleManagementPersonalWellBeingRequest>();

            var columns = new[] { "AcademicYear", "Level", "ArticleName", "Description", "ViewFor", "Link" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "AcademicYear.Description" },
                { columns[1], "Level.Description" },
                { columns[2], "ArticleName" },
                { columns[3], "Description" },
                { columns[4], "ViewFor" },
                { columns[5], "Link" }
            };

            var query = _dbContext.Entity<TrPersonalWellBeing>()
                       .Include(e => e.AcademicYear)
                       .Include(e => e.PersonalWellBeingLevel).ThenInclude(e => e.Level)
                       .Include(e => e.PersonalWellBeingAttachment).AsQueryable();
            //.OrderByDescending(x => x.DateIn);
            //.OrderByDescending(x => x.DateIn);
            //.OrderByDynamic(param, aliasColumns);


            var EnumViewFor = PersonalWellBeingFor.All;

            if (!string.IsNullOrEmpty(param.ViewFor))
                EnumViewFor = (PersonalWellBeingFor)Enum.Parse(typeof(PersonalWellBeingFor), param.ViewFor);


            //filter
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                query = query.Where(x => EF.Functions.Like(x.AcademicYear.Id, param.IdAcademicYear));
            if (!string.IsNullOrEmpty(param.LevelId))
                query = query.Where(x => x.PersonalWellBeingLevel.Any(y => y.IdLevel == param.LevelId));
            if (!string.IsNullOrEmpty(param.ViewFor))
                query = query.Where(x => x.For == EnumViewFor);
            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.ArticleName, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
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
                case "level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.PersonalWellBeingLevel.Select(y => y.Level.OrderNumber).FirstOrDefault())
                        : query.OrderBy(x => x.PersonalWellBeingLevel.Select(y => y.Level.OrderNumber).FirstOrDefault());
                    break;
                case "articleName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ArticleName)
                        : query.OrderBy(x => x.ArticleName);
                    break;
                case "description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
                case "viewFor":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.For)
                        : query.OrderBy(x => x.For);
                    break;
                case "link":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Link)
                        : query.OrderBy(x => x.Link);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetArticleManagementPersonalWellBeingResult
                {
                    Id = x.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.IdAcademicYear,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    ArticleName = x.ArticleName,
                    Description = x.Description,
                    ViewFor = x.For.ToString(),
                    Link = x.Link,
                    Level = x.PersonalWellBeingLevel.Select(y => new CodeWithIdVm
                    {
                        Id = y.IdLevel,
                        Code = y.Level.Code,
                        Description = y.Level.Description
                    }).ToList(),
                    Attachments = x.PersonalWellBeingAttachment.Select(e => new AttachmentArticleManagementPersonalWellBeing
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

                items = result.Select(x => new GetArticleManagementPersonalWellBeingResult
                {
                    Id = x.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.IdAcademicYear,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    ArticleName = x.ArticleName,
                    Description = x.Description,
                    ViewFor = x.For.ToString(),
                    Link = x.Link,
                    Level = x.PersonalWellBeingLevel.Select(y => new CodeWithIdVm
                    {
                        Id = y.IdLevel,
                        Code = y.Level.Code,
                        Description = y.Level.Description
                    }).ToList(),
                    Attachments = x.PersonalWellBeingAttachment.Select(e => new AttachmentArticleManagementPersonalWellBeing
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
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));

        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<AddArticleManagementPersonalWellBeingRequest, AddArticleManagementPersonalWellBeingValidator>();

            var existsData = _dbContext.Entity<TrPersonalWellBeing>()
                .Any(x => x.AcademicYear.Id == body.IdAcademicYear && x.ArticleName == body.ArticleName);

            if (existsData)
            {
                throw new BadRequestException($"Article name { body.ArticleName} already exists.");
            }

            if (!string.IsNullOrEmpty(body.Link))
            {
                if (body.Link.StartsWith("http://") == false && body.Link.StartsWith("https://") == false)
                {
                    body.Link = "http://" + body.Link;
                }
            }

            var existsData2 = _dbContext.Entity<TrPersonalWellBeing>()
                .Any(x => x.AcademicYear.Id == body.IdAcademicYear && x.Link == body.Link);

            if (existsData2)
            {
                throw new BadRequestException($"Link { body.Link} already exists.");
            }


            var idArticleManagementPersonalWellBeing = Guid.NewGuid().ToString();

            var newArticleManagementPersonalWellBeing = new TrPersonalWellBeing
            {
                Id = idArticleManagementPersonalWellBeing,
                IdAcademicYear = body.IdAcademicYear,
                ArticleName = body.ArticleName,
                Description = body.ArticleDescription,
                For = body.ViewFor,
                Link = body.Link == null ? "" : body.Link,
                NotifRecipient = body.NotifyRecipient,
            };

            if (body.LevelIds != null)
            {
                foreach (var Levelid in body.LevelIds)
                {
                    var newArticleManagementPersonalWellBeingLevel = new TrPersonalWellBeingLevel
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdPersonalWellBeing = idArticleManagementPersonalWellBeing,
                        IdLevel = Levelid.Id
                    };

                    _dbContext.Entity<TrPersonalWellBeingLevel>().Add(newArticleManagementPersonalWellBeingLevel);
                }
            }

            if (body.Attachments != null)
            {
                foreach (var ItemAttachment in body.Attachments)
                {
                    var newArticleManagementPersonalWellBeingAttachment = new TrPersonalWellBeingAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdPersonalWellBeing = newArticleManagementPersonalWellBeing.Id,
                        OriginalName = ItemAttachment.OriginalFilename,
                        Url = ItemAttachment.Url,
                        FileName = ItemAttachment.FileName,
                        FileType = ItemAttachment.FileType,
                        FileSize = ItemAttachment.FileSize,
                    };
                    _dbContext.Entity<TrPersonalWellBeingAttachment>().Add(newArticleManagementPersonalWellBeingAttachment);

                }
            }

            _dbContext.Entity<TrPersonalWellBeing>().Add(newArticleManagementPersonalWellBeing);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateArticleManagementPersonalWellBeingRequest, UpdateArticleManagementPersonalWellBeingValidator>();
            var GetArticleManagementPersonalWellBeing = await _dbContext.Entity<TrPersonalWellBeing>().Where(e => e.Id == body.IdArticleManagementPersonalWellBeing).SingleOrDefaultAsync(CancellationToken);
            var GetArticleManagementPersonalWellBeingLevel = await _dbContext.Entity<TrPersonalWellBeingLevel>().Where(e => e.IdPersonalWellBeing == body.IdArticleManagementPersonalWellBeing).ToListAsync(CancellationToken);
            var GetArticleManagementPersonalWellBeingAttachment = await _dbContext.Entity<TrPersonalWellBeingAttachment>().Where(e => e.IdPersonalWellBeing == body.IdArticleManagementPersonalWellBeing).ToListAsync(CancellationToken);

            if (GetArticleManagementPersonalWellBeing is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Article Management Personal Well Being"], "Id", body.IdArticleManagementPersonalWellBeing));
            }

            //update data in TrPersonalWellBeing
            if (GetArticleManagementPersonalWellBeing.ArticleName != body.ArticleName)
            {
                var academicYearId = GetArticleManagementPersonalWellBeing.IdAcademicYear;
                var checkArticleName = _dbContext.Entity<TrPersonalWellBeing>().Where(x => x.AcademicYear.Id == academicYearId && x.ArticleName == body.ArticleName).FirstOrDefault();


                if (checkArticleName != null)
                {
                    throw new BadRequestException($"Article name {body.ArticleName} already exists");
                }

                GetArticleManagementPersonalWellBeing.ArticleName = body.ArticleName;
            }

            if (!string.IsNullOrEmpty(body.Link))
            {
                if (body.Link.StartsWith("http://") == false && body.Link.StartsWith("https://") == false)
                {
                    body.Link = "http://" + body.Link;
                }
            }
            if (GetArticleManagementPersonalWellBeing.Link != body.Link)
            {
                var academicYearId = GetArticleManagementPersonalWellBeing.IdAcademicYear;
                var checkArticleName1 = _dbContext.Entity<TrPersonalWellBeing>().Where(x => x.AcademicYear.Id == academicYearId && x.Link == body.Link).FirstOrDefault();

                if (checkArticleName1 != null)
                {
                    throw new BadRequestException($"Link {body.Link} already exists");
                }
            }


            GetArticleManagementPersonalWellBeing.Link = body.Link == null ? "" : body.Link;
            //update data in TrPersonalWellBeing
            GetArticleManagementPersonalWellBeing.Description = body.ArticleDescription;
            GetArticleManagementPersonalWellBeing.For = body.ViewFor;
            GetArticleManagementPersonalWellBeing.NotifRecipient = body.NotifyRecipient;
            _dbContext.Entity<TrPersonalWellBeing>().Update(GetArticleManagementPersonalWellBeing);

            //update data in TrPersonalWellBeingLevel
            //remove level
            foreach (var ItemLevel in GetArticleManagementPersonalWellBeingLevel)
            {
                var ExsisBodyLevelId = body.LevelIds.Any(e => e.Id == ItemLevel.IdLevel);

                if (!ExsisBodyLevelId)
                {
                    ItemLevel.IsActive = false;
                    _dbContext.Entity<TrPersonalWellBeingLevel>().Update(ItemLevel);
                }
            }
            ////Add level
            if (body.LevelIds != null)
            {
                foreach (var Levelid in body.LevelIds)
                {
                    var ExsistdbId = GetArticleManagementPersonalWellBeingLevel.Where(e => e.IdLevel == Levelid.Id && e.IdPersonalWellBeing == GetArticleManagementPersonalWellBeing.Id).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newArticleManagementPersonalWellBeingLevel = new TrPersonalWellBeingLevel
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdPersonalWellBeing = GetArticleManagementPersonalWellBeing.Id,
                            IdLevel = Levelid.Id
                        };

                        _dbContext.Entity<TrPersonalWellBeingLevel>().Add(newArticleManagementPersonalWellBeingLevel);
                    }
                }
            }

            //update data in TrPersonalWellBeingAttachment
            //remove attachment
            foreach (var ItemAttachment in GetArticleManagementPersonalWellBeingAttachment)
            {
                var ExsisBodyAttachment = body.Attachments.Any(e => e.Id == ItemAttachment.Id);

                if (!ExsisBodyAttachment)
                {
                    ItemAttachment.IsActive = false;
                    _dbContext.Entity<TrPersonalWellBeingAttachment>().Update(ItemAttachment);
                }
            }

            //Add attachment
            foreach (var ItemAttachment in body.Attachments.Where(e => e.Id == null || e.Id == "").ToList())
            {
                var newArticleManagementPersonalWellBeingAttachment = new TrPersonalWellBeingAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdPersonalWellBeing = GetArticleManagementPersonalWellBeing.Id,
                    OriginalName = ItemAttachment.OriginalFilename,
                    Url = ItemAttachment.Url,
                    FileName = ItemAttachment.FileName,
                    FileType = ItemAttachment.FileType,
                    FileSize = ItemAttachment.FileSize,
                };
                _dbContext.Entity<TrPersonalWellBeingAttachment>().Add(newArticleManagementPersonalWellBeingAttachment);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);


            return Request.CreateApiResult2();
        }
    }
}
