using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Auth.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnBanner.Banner;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnBanner.Banner.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.School.FnBanner.Banner
{
    public class BannerHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _schoolDbContext;

        public BannerHandler(ISchoolDbContext schoolDbContext, ICurrentUser currentAcademicYear)
        {
            _schoolDbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> IdBanners)
        {
            var banners = _schoolDbContext.Entity<MsBanner>()
                .Where(x => IdBanners.Any(y => y == x.Id))
                .ToList();

            if (!banners.Any())
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Banner"], "Id", IdBanners.FirstOrDefault()));

            foreach (var banner in banners)
            {
                banner.IsActive = false;
            }

            _schoolDbContext.Entity<MsBanner>().UpdateRange(banners);

            await _schoolDbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _schoolDbContext.Entity<MsBanner>()
                .Include(x => x.School)
                .Select(x => new GetBannerDetailResult
                {
                    Id = x.Id,
                    ImageUrl = x.UrlImage,
                    Name = x.Name,
                    Content = x.Content,
                    BannerOption = x.Option,
                    IsPin = x.IsPin,
                    Link = x.Link,
                    PublishStartDate = x.PublishStartDate,
                    PublishEndDate = x.PublishEndDate,
                    Attachments = new List<ViewAttachment>(),
                    Level = new List<ViewLevel>(),
                    Grade = new List<ViewGrade>(),
                    Role = new List<ViewRole>(),
                    Audit = x.GetRawAuditResult2(),
                    Description = x.Description,
                    IdSchool = x.IdSchool,
                    NameSchool = x.School.Name
                })
            .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            if (DateTime.Now >= query.PublishStartDate && DateTime.Now <= query.PublishEndDate)
                query.Status = BannerStatus.Active;
            else if (DateTime.Now <= query.PublishStartDate)
                query.Status = BannerStatus.Upcoming;
            else if (DateTime.Now >= query.PublishEndDate)
                query.Status = BannerStatus.Expired;

            if (query != null)
            {
                var queryAttachment = await _schoolDbContext.Entity<MsBannerAttachment>()
                .Where(x => x.IdBanner == query.Id)
                .Select(x => new ViewAttachment
                {
                    Id = x.Id,
                    OriginalFilename = x.OriginalFilename,
                    Url = x.Url,
                    Filename = x.Filename,
                    Filetype = x.Filetype,
                    Filesize = x.Filesize,
                    IsImage = x.IsImage
                })
                .ToListAsync(CancellationToken);

                if (queryAttachment.Any())
                {
                    query.Attachments.AddRange(queryAttachment);
                }

                var queryRole = await _schoolDbContext.Entity<MsBannerRole>()
                    .Include(x => x.RoleGroup)
                .Where(x => x.IdBanner == query.Id)
                .Select(x => new ViewRole
                {
                    Id = x.RoleGroup.Id,
                    Code = x.RoleGroup.Code,
                    Description = x.RoleGroup.Description
                })
                .ToListAsync(CancellationToken);

                if (queryRole.Any())
                {
                    query.Role.AddRange(queryRole);
                }

                var queryLevelGrade = await _schoolDbContext.Entity<MsBannerLevelGrade>()
                    .Include(x => x.Grade).ThenInclude(x => x.Level)
                .Where(x => x.IdBanner == query.Id)
                .ToListAsync(CancellationToken);

                if (queryLevelGrade.Any())
                {
                    var dataGrade = queryLevelGrade.Select(x => new ViewGrade
                    {
                        Id = x.Grade.Id,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description
                    }).ToList();

                    query.Grade.AddRange(dataGrade);

                    var dataLevel = queryLevelGrade.GroupBy(x => new { Id = x.Grade.Level.Id, Code = x.Grade.Level.Code, Description = x.Grade.Level.Description }).Select(x => new ViewLevel
                    {
                        Id = x.Key.Id,
                        Code = x.Key.Code,
                        Description = x.Key.Description
                    }).ToList();

                    query.Level.AddRange(dataLevel);
                }
            }

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.GetParams<GetBannerRequest>();

            var predicate = PredicateBuilder.Create<MsBanner>(x => x.IsActive == true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, param.SearchPattern())
                    || EF.Functions.Like(x.Content, param.SearchPattern()));

            if (!string.IsNullOrEmpty(param.IdSchool))
                predicate = predicate.And(x => x.IdSchool == param.IdSchool);

            if (param.IdRoleGroup != null)
                predicate = predicate.And(x => x.BannerRoles.Any(item => param.IdRoleGroup.Contains(item.RoleGroup.Id)));

            if (param.IdLevel != null)
                predicate = predicate.And(x => x.BannerLevelGrades.Any(item => param.IdLevel.Contains(item.Grade.Level.Id)));

            if (param.IdGrade != null)
                predicate = predicate.And(x => x.BannerLevelGrades.Any(item => param.IdGrade.Contains(item.Grade.Id)));


            if (param.Status != null)
            {
                if (!param.Status.Any(y => y == BannerStatusFilter.All))
                {
                    var multipleParamStatus = 0;
                    if (param.Status.Any(y => y == BannerStatusFilter.Active))
                    {
                        multipleParamStatus = multipleParamStatus + 1;
                        predicate = predicate.And(x => DateTime.Now >= x.PublishStartDate && DateTime.Now <= x.PublishEndDate.AddDays(1));
                    }


                    if (param.Status.Any(y => y == BannerStatusFilter.Upcoming))
                    {
                        if (multipleParamStatus != 0)
                        {
                            predicate = predicate.Or(x => DateTime.Now <= x.PublishStartDate);
                        }
                        else
                        {
                            predicate = predicate.And(x => DateTime.Now <= x.PublishStartDate);
                        }
                        multipleParamStatus = multipleParamStatus + 1;
                    }


                    if (param.Status.Any(y => y == BannerStatusFilter.Expired))
                    {
                        if (multipleParamStatus != 0)
                        {
                            predicate = predicate.Or(x => DateTime.Now >= x.PublishEndDate.AddDays(1));
                        }
                        else
                        {
                            predicate = predicate.And(x => DateTime.Now >= x.PublishEndDate.AddDays(1));
                        }
                        multipleParamStatus = multipleParamStatus + 1;
                    }

                }
            }

            var getDataLevel = _schoolDbContext.Entity<MsLevel>();

            var query = _schoolDbContext.Entity<MsBanner>()
                .Include(x => x.BannerAttachments)
                .Include(x => x.BannerLevelGrades)
                    .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                .Include(x => x.BannerRoles)
                    .ThenInclude(x => x.RoleGroup)
                .Where(predicate)
                .SearchByIds(param)
                .SearchByDynamic(param)
                .OrderByDescending(x => x.IsPin)
                .OrderByDynamic(param);


            IReadOnlyList<IItemValueVm> items;

            items = await query
                .SetPagination(param)
                .Select(x => new GetBannerResult
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImageUrl = x.UrlImage,
                    CreateDate = x.DateIn.GetValueOrDefault(),
                    LastModified = x.DateUp.GetValueOrDefault(),
                    PublishStartDate = x.PublishStartDate,
                    PublishEndDate = x.PublishEndDate,
                    Content = (!string.IsNullOrEmpty(x.Content)) ? x.Content : string.Empty,
                    IsPin = x.IsPin,
                    Link = x.Link,
                    BannerOption = x.Option,
                    IdSchool = x.IdSchool,
                    Role = x.BannerRoles.Select(x => new ViewRole
                    {
                        Id = x.RoleGroup.Id,
                        Code = x.RoleGroup.Code,
                        Description = x.RoleGroup.Description,
                    }).ToList(),
                    Grade = x.BannerLevelGrades.Select(x => new ViewGrade
                    {
                        Id = x.Grade.Id,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description
                    }).ToList(),
                    Level = getDataLevel.Where(y => x.BannerLevelGrades.Any(z => z.Grade.IdLevel == y.Id))
                    .Select(q => new ViewLevel
                    {
                        Id = q.Id,
                        Code = q.Code,
                        Description = q.Description,
                    })
                    .ToList(),
                    Status = (DateTime.Now >= x.PublishStartDate && DateTime.Now <= x.PublishEndDate.AddDays(1)) ? BannerStatus.Active :
                        DateTime.Now <= x.PublishStartDate ? BannerStatus.Upcoming :
                            DateTime.Now >= x.PublishEndDate.AddDays(1) ? BannerStatus.Expired : BannerStatus.Active
                })
                //.OrderByDescending(x => x.IsPin)
                //.ThenBy(x => x.Status)
                .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddBannerRequest, AddBannerValidator>();

            var banner = new MsBanner
            {
                Id = Guid.NewGuid().ToString(),
                Name = body.Name,
                Option = body.Option,
                Content = (!string.IsNullOrEmpty(body.Content)) ? body.Content : string.Empty,
                Description = (!string.IsNullOrEmpty(body.Content)) ? SubStringContent(body.Content, 128) : string.Empty,
                IsPin = body.IsPin,
                Link = (!string.IsNullOrEmpty(body.Link)) ? body.Link : string.Empty,
                PublishStartDate = body.PublishStartDate,
                PublishEndDate = body.PublishEndDate,
                UrlImage = body.ImageUrl,
                Code = string.Empty,
                IdSchool = body.IdSchool
            };

            _schoolDbContext.Entity<MsBanner>().Add(banner);

            var bannerAttachments = new List<MsBannerAttachment>();
            if (body.Attachments != null)
            {
                foreach (var item in body.Attachments)
                {
                    var bannerAttachment = new MsBannerAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBanner = banner.Id,
                        OriginalFilename = item.OriginalFilename,
                        Url = item.Url,
                        Filename = item.Filename,
                        Filetype = item.Filetype,
                        Filesize = item.Filesize,
                        IsImage = item.IsImage,
                        Code = string.Empty,
                        Description = string.Empty
                    };

                    bannerAttachments.Add(bannerAttachment);
                }
            }

            _schoolDbContext.Entity<MsBannerAttachment>().AddRange(bannerAttachments);

            var bannerLevelGrades = new List<MsBannerLevelGrade>();
            if (body.GradeId != null)
            {
                if (body.GradeId.Any(y => y == "All") && body.LevelId.Any(y => y == "All"))
                {
                    body.GradeId = _schoolDbContext.Entity<MsGrade>()
                        .Include(x => x.Level)
                        .Where(x => x.Level.IdAcademicYear == body.AcademicYear).Select(x => x.Id).ToList();
                }

                foreach (var data in body.GradeId)
                {
                    var bannerLevelGrade = new MsBannerLevelGrade
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBanner = banner.Id,
                        IdGrade = data,
                        Code = string.Empty,
                        Description = string.Empty
                    };

                    bannerLevelGrades.Add(bannerLevelGrade);
                }

                _schoolDbContext.Entity<MsBannerLevelGrade>().AddRange(bannerLevelGrades);
            }

            var bannerRoles = new List<MsBannerRole>();
            foreach (var data in body.RoleGroupId)
            {
                var bannerRole = new MsBannerRole
                {
                    Id = Guid.NewGuid().ToString(),
                    IdBanner = banner.Id,
                    IdRoleGroup = data,
                    Code = string.Empty,
                    Description = string.Empty
                };

                bannerRoles.Add(bannerRole);
            }

            _schoolDbContext.Entity<MsBannerRole>().AddRange(bannerRoles);

            await _schoolDbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateBannerRequest, UpdateBannerValidator>();

            var banner = await _schoolDbContext.Entity<MsBanner>().FindAsync(body.Id);
            if (banner is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Banner"], "Id", body.Id));

            banner.Name = body.Name;
            banner.Option = body.Option;
            banner.Content = (!string.IsNullOrEmpty(body.Content)) ? body.Content : string.Empty;
            banner.Description = (!string.IsNullOrEmpty(body.Content)) ? SubStringContent(body.Content, 128) : string.Empty;
            banner.IsPin = body.IsPin;
            banner.Link = (!string.IsNullOrEmpty(body.Link)) ? body.Link : string.Empty;
            banner.PublishStartDate = body.PublishStartDate;
            banner.PublishEndDate = body.PublishEndDate;
            banner.UrlImage = body.ImageUrl;
            banner.IdSchool = body.IdSchool;

            _schoolDbContext.Entity<MsBanner>().Update(banner);

            await RemoveDerivedDataBanner(body.Id);

            if (body.Attachments != null)
            {
                var bannerAttachments = new List<MsBannerAttachment>();
                foreach (var item in body.Attachments)
                {
                    var bannerAttachment = new MsBannerAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBanner = banner.Id,
                        OriginalFilename = item.OriginalFilename,
                        Url = item.Url,
                        Filename = item.Filename,
                        Filetype = item.Filetype,
                        Filesize = item.Filesize,
                        IsImage = item.IsImage,
                        Code = string.Empty,
                        Description = string.Empty
                    };

                    bannerAttachments.Add(bannerAttachment);
                }

                _schoolDbContext.Entity<MsBannerAttachment>().AddRange(bannerAttachments);
            }


            var bannerLevelGrades = new List<MsBannerLevelGrade>();
            if (body.GradeId != null)
            {
                if (body.GradeId.Any(y => y == "All") && body.LevelId.Any(y => y == "All"))
                {
                    body.GradeId = _schoolDbContext.Entity<MsGrade>()
                        .Include(x => x.Level)
                        .Where(x => x.Level.IdAcademicYear == body.AcademicYear).Select(x => x.Id).ToList();
                }

                foreach (var data in body.GradeId)
                {
                    var bannerLevelGrade = new MsBannerLevelGrade
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBanner = banner.Id,
                        IdGrade = data,
                        Code = string.Empty,
                        Description = string.Empty
                    };

                    bannerLevelGrades.Add(bannerLevelGrade);
                }

                _schoolDbContext.Entity<MsBannerLevelGrade>().AddRange(bannerLevelGrades);
            }

            if (body.RoleGroupId != null)
            {
                var bannerRoles = new List<MsBannerRole>();
                foreach (var data in body.RoleGroupId)
                {
                    var bannerRole = new MsBannerRole
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdBanner = banner.Id,
                        IdRoleGroup = data,
                        Code = string.Empty,
                        Description = string.Empty
                    };

                    bannerRoles.Add(bannerRole);
                }

                _schoolDbContext.Entity<MsBannerRole>().AddRange(bannerRoles);
            }

            await _schoolDbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
        private async Task RemoveDerivedDataBanner(string id)
        {
            var bannerAttachments = await _schoolDbContext.Entity<MsBannerAttachment>().Where(x => x.IdBanner.Equals(id)).ToListAsync();
            if (bannerAttachments.Any())
            {
                _schoolDbContext.Entity<MsBannerAttachment>().RemoveRange(bannerAttachments);
            }

            var bannerRoles = await _schoolDbContext.Entity<MsBannerRole>().Where(x => x.IdBanner.Equals(id)).ToListAsync();
            if (bannerRoles.Any())
            {
                _schoolDbContext.Entity<MsBannerRole>().RemoveRange(bannerRoles);
            }

            var bannerLevelGrade = await _schoolDbContext.Entity<MsBannerLevelGrade>().Where(x => x.IdBanner.Equals(id)).ToListAsync();
            if (bannerLevelGrade.Any())
            {
                _schoolDbContext.Entity<MsBannerLevelGrade>().RemoveRange(bannerLevelGrade);
            }
        }

        private string SubStringContent(string content, int MaxLenght)
        {
            var resultText = content;
            //128
            if (content.Length >= MaxLenght)
            {
                resultText = content.Substring(0, MaxLenght - 3) + "...";
            }

            return resultText;
        }

    }
}
