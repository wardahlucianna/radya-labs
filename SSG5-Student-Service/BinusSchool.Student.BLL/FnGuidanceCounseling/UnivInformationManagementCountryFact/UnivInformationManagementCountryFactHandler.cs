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
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact
{
    public class UnivInformationManagementCountryFactHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IStudentDbContext _dbContext;
        public UnivInformationManagementCountryFactHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetUnivInformationManagementCountryFact = await _dbContext.Entity<MsCountryFact>()
           .Where(x => ids.Contains(x.Id))
           .ToListAsync(CancellationToken);

            var GetUnivInformationManagementCountryFactLevel = await _dbContext.Entity<MsCountryFactLevel>()
           .Where(x => ids.Contains(x.IdCountryFact))
           .ToListAsync(CancellationToken);

            var GetUnivInformationManagementCountryFactSheet = await _dbContext.Entity<MsCountryFactSheet>()
           .Where(x => ids.Contains(x.IdCountryFact))
           .ToListAsync(CancellationToken);

            var GetUnivInformationManagementCountryFactLogo = await _dbContext.Entity<MsCountryFactLogo>()
           .Where(x => ids.Contains(x.IdCountryFact))
           .ToListAsync(CancellationToken);

            GetUnivInformationManagementCountryFact.ForEach(x => x.IsActive = false);

            GetUnivInformationManagementCountryFactLevel.ForEach(x => x.IsActive = false);

            GetUnivInformationManagementCountryFactSheet.ForEach(x => x.IsActive = false);

            GetUnivInformationManagementCountryFactLogo.ForEach(x => x.IsActive = false);

            _dbContext.Entity<MsCountryFact>().UpdateRange(GetUnivInformationManagementCountryFact);

            _dbContext.Entity<MsCountryFactLevel>().UpdateRange(GetUnivInformationManagementCountryFactLevel);

            _dbContext.Entity<MsCountryFactSheet>().UpdateRange(GetUnivInformationManagementCountryFactSheet);

            _dbContext.Entity<MsCountryFactLogo>().UpdateRange(GetUnivInformationManagementCountryFactLogo);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsCountryFact>()
            .Include(p => p.AcademicYear)
            .Where(x => x.Id == id)
            .Select(x => new GetUnivInformationManagementCountryFactResult
            {
                Id = x.Id,
                AcademicYear = new CodeWithIdVm
                {
                    Id = x.AcademicYear.Id,
                    Code = x.AcademicYear.Code,
                    Description = x.AcademicYear.Description
                },
                Level = _dbContext.Entity<MsCountryFactLevel>()
                    .Include(p => p.Level)
                    .Where(x => x.IdCountryFact == id)
                    .Select(e => new CodeWithIdVm
                    {
                        Id = e.Level.Id,
                        Code = e.Level.Code,
                        Description = e.Level.Description
                    }).ToList(),
                CountryName = x.Name,
                Description = x.Description,
                CountryWebsite = x.Website,
                ContactPerson = x.ContactPerson,
                FactSheet = x.CountryFactSheet.Select(e => new FactSheetUnivInformationManagementCountryFact
                {
                    Id = e.Id,
                    Url = e.Url,
                    OriginalFilename = e.OriginalName,
                    FileName = e.FileName,
                    FileSize = e.FileSize,
                    FileType = e.FileType,
                }).ToList(),
                Logo = x.CountryFactLogo.Select(e => new LogoUnivInformationManagementCountryFact
                {
                    Id = e.Id,
                    Url = e.Url,
                    OriginalFilename = e.OriginalName,
                    FileName = e.FileName,
                    FileSize = e.FileSize,
                    FileType = e.FileType,
                }).ToList(),
            }).FirstOrDefaultAsync(CancellationToken);


            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetUnivInformationManagementCountryFactRequest>();

            var columns = new[] { "AcademicYear", "Level", "CountryName", "Description", "CountryWebsite", "ContactPerson" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "AcademicYear.Description" },
                { columns[1], "Level.Description" },
                { columns[2], "CountryName" },
                { columns[3], "Description" },
                { columns[4], "Website" },
                { columns[5], "ContactPerson" }
            };

            var query = _dbContext.Entity<MsCountryFact>()
                        .Include(e => e.AcademicYear)
                        .Include(e => e.CountryFactLevel).ThenInclude(e => e.Level)
                        .Include(e => e.CountryFactSheet)
                        .Include(e => e.CountryFactLogo).AsQueryable();
            //.OrderByDescending(x => x.DateIn)
            //.OrderByDynamic(param, aliasColumns)




            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Name, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
            }

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
            {
                query = query.Where(x => EF.Functions.Like(x.AcademicYear.Id, param.IdAcademicYear));
            }

            if (!string.IsNullOrEmpty(param.LevelId))
            {
                query = query.Where(x => x.CountryFactLevel.Any(y => y.IdLevel == param.LevelId));
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
                case "level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CountryFactLevel.Select(y => y.Level.Code).FirstOrDefault())
                        : query.OrderBy(x => x.CountryFactLevel.Select(y => y.Level.Code).FirstOrDefault());
                    break;
                case "countryName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Name)
                        : query.OrderBy(x => x.Name);
                    break;
                case "description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
                case "countryWebsite":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Website)
                        : query.OrderBy(x => x.Website);
                    break;
                case "contactPerson":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ContactPerson)
                        : query.OrderBy(x => x.ContactPerson);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                     .Select(x => new GetUnivInformationManagementCountryFactResult
                     {
                         Id = x.Id,
                         AcademicYear = new CodeWithIdVm
                         {
                             Id = x.IdAcademicYear,
                             Code = x.AcademicYear.Code,
                             Description = x.AcademicYear.Description
                         },
                         CountryName = x.Name,
                         Description = x.Description,
                         CountryWebsite = x.Website,
                         ContactPerson = x.ContactPerson,
                         Level = x.CountryFactLevel.Select(y => new CodeWithIdVm
                         {
                             Id = y.IdLevel,
                             Code = y.Level.Code,
                             Description = y.Level.Description
                         }).ToList(),
                         FactSheet = x.CountryFactSheet.Select(e => new FactSheetUnivInformationManagementCountryFact
                         {
                             Id = e.Id,
                             Url = e.Url,
                             OriginalFilename = e.OriginalName,
                             FileName = e.FileName,
                             FileSize = e.FileSize,
                             FileType = e.FileType,
                         }).ToList(),
                         Logo = x.CountryFactLogo.Select(e => new LogoUnivInformationManagementCountryFact
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
                     .Select(x => new GetUnivInformationManagementCountryFactResult
                     {
                         Id = x.Id,
                         AcademicYear = new CodeWithIdVm
                         {
                             Id = x.IdAcademicYear,
                             Code = x.AcademicYear.Code,
                             Description = x.AcademicYear.Description
                         },
                         CountryName = x.Name,
                         Description = x.Description,
                         CountryWebsite = x.Website,
                         ContactPerson = x.ContactPerson,
                         Level = x.CountryFactLevel.Select(y => new CodeWithIdVm
                         {
                             Id = y.IdLevel,
                             Code = y.Level.Code,
                             Description = y.Level.Description
                         }).ToList(),
                         FactSheet = x.CountryFactSheet.Select(e => new FactSheetUnivInformationManagementCountryFact
                         {
                             Id = e.Id,
                             Url = e.Url,
                             OriginalFilename = e.OriginalName,
                             FileName = e.FileName,
                             FileSize = e.FileSize,
                             FileType = e.FileType,
                         }).ToList(),
                         Logo = x.CountryFactLogo.Select(e => new LogoUnivInformationManagementCountryFact
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

            var body = await Request.ValidateBody<AddUnivInformationManagementCountryFactRequest, AddUnivInformationManagementCountryFactValidator>();

            var existsData = _dbContext.Entity<MsCountryFact>()
                .Any(x => x.Name == body.CountryName && x.IdAcademicYear == body.IdAcademicYear);

            if (existsData)
            {
                throw new BadRequestException($"Country name { body.CountryName} already exists.");
            }
            if (!string.IsNullOrEmpty(body.CountryWebsite))
            {
                if (body.CountryWebsite.StartsWith("http://") == false && body.CountryWebsite.StartsWith("https://") == false)
                {
                    body.CountryWebsite = "http://" + body.CountryWebsite;
                }
                if (body.CountryWebsite.StartsWith("https://"))
                {
                    body.CountryWebsite = "http://" + body.CountryWebsite[8..];
                }
            }
            if (!string.IsNullOrEmpty(body.CountryWebsite))
            {
                var existsData1 = _dbContext.Entity<MsCountryFact>()
                .Any(x => x.Website == body.CountryWebsite && x.IdAcademicYear == body.IdAcademicYear);

                if (existsData1)
                {
                    throw new BadRequestException($"Website { body.CountryWebsite} already exists.");
                }
            }

            var idUnivInformationManagementCountryFact = Guid.NewGuid().ToString();

            var newUnivInformationManagementCountryFact = new MsCountryFact
            {
                Id = idUnivInformationManagementCountryFact,
                IdAcademicYear = body.IdAcademicYear,
                Name = body.CountryName,
                Description = body.CountryDescription == null ? "" : body.CountryDescription,
                Website = body.CountryWebsite == null ? "" : body.CountryWebsite,
                ContactPerson = body.ContactPerson == null ? "" : body.ContactPerson,
            };

            if (body.LevelIds != null)
            {
                foreach (var Levelid in body.LevelIds)
                {
                    var newUnivInformationManagementCountryFactLevel = new MsCountryFactLevel
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdCountryFact = idUnivInformationManagementCountryFact,
                        IdLevel = Levelid.Id
                    };

                    _dbContext.Entity<MsCountryFactLevel>().Add(newUnivInformationManagementCountryFactLevel);
                }
            }

            if (body.FactSheet != null)
            {
                foreach (var ItemFactSheet in body.FactSheet)
                {
                    var newUnivInformationManagementCountryFactSheet = new MsCountryFactSheet
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdCountryFact = newUnivInformationManagementCountryFact.Id,
                        OriginalName = ItemFactSheet.OriginalFilename,
                        Url = ItemFactSheet.Url,
                        FileName = ItemFactSheet.FileName,
                        FileType = ItemFactSheet.FileType,
                        FileSize = ItemFactSheet.FileSize,
                    };
                    _dbContext.Entity<MsCountryFactSheet>().Add(newUnivInformationManagementCountryFactSheet);

                }
            }

            if (body.Logo != null)
            {
                foreach (var ItemLogo in body.Logo)
                {
                    var newUnivInformationManagementCountryLogo = new MsCountryFactLogo
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdCountryFact = newUnivInformationManagementCountryFact.Id,
                        OriginalName = ItemLogo.OriginalFilename,
                        Url = ItemLogo.Url,
                        FileName = ItemLogo.FileName,
                        FileType = ItemLogo.FileType,
                        FileSize = ItemLogo.FileSize,
                    };
                    _dbContext.Entity<MsCountryFactLogo>().Add(newUnivInformationManagementCountryLogo);

                }
            }

            _dbContext.Entity<MsCountryFact>().Add(newUnivInformationManagementCountryFact);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateUnivInformationManagementCountryFactRequest, UpdateUnivInformationManagementCountryFactValidator>();
            var GetUnivInformationManagementCountryFact = await _dbContext.Entity<MsCountryFact>().Where(e => e.Id == body.IdUnivInformationManagementCountryFact).SingleOrDefaultAsync(CancellationToken);
            var GetUnivInformationManagementCountryFactLevel = await _dbContext.Entity<MsCountryFactLevel>().Where(e => e.IdCountryFact == body.IdUnivInformationManagementCountryFact).ToListAsync(CancellationToken);
            var GetUnivInformationManagementCountryFactSheet = await _dbContext.Entity<MsCountryFactSheet>().Where(e => e.IdCountryFact == body.IdUnivInformationManagementCountryFact).ToListAsync(CancellationToken);
            var GetUnivInformationManagementCountryFactLogo = await _dbContext.Entity<MsCountryFactLogo>().Where(e => e.IdCountryFact == body.IdUnivInformationManagementCountryFact).ToListAsync(CancellationToken);

            if (GetUnivInformationManagementCountryFact is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["University Information Management Country Fact"], "Id", body.IdUnivInformationManagementCountryFact));
            }

            ////update data in MsCountryFactLevel
            if (GetUnivInformationManagementCountryFact.Name != body.CountryName )
            {
                var academicYearId = GetUnivInformationManagementCountryFact.IdAcademicYear;
                var checkCountryFact = _dbContext.Entity<MsCountryFact>().Where(x => x.Name == body.CountryName && x.IdAcademicYear == academicYearId).FirstOrDefault();

                if (checkCountryFact != null)
                {
                    throw new BadRequestException($"Country Name {body.CountryName} already exists");
                }

                GetUnivInformationManagementCountryFact.Name = body.CountryName;
            }
            if (!string.IsNullOrEmpty(body.CountryWebsite))
            {
                if (body.CountryWebsite.StartsWith("http://") == false && body.CountryWebsite.StartsWith("https://") == false)
                {
                    body.CountryWebsite = "http://" + body.CountryWebsite;
                }
                if (body.CountryWebsite.StartsWith("https://"))
                {
                    body.CountryWebsite = "http://" + body.CountryWebsite[8..];
                }
            }

            if (!string.IsNullOrEmpty(body.CountryWebsite))
            {
                if (GetUnivInformationManagementCountryFact.Website != body.CountryWebsite)
                {
                    var academicYearId = GetUnivInformationManagementCountryFact.IdAcademicYear;

                    var checkCountryFact1 = _dbContext.Entity<MsCountryFact>().Where(x => x.Website == body.CountryWebsite && x.IdAcademicYear == academicYearId).FirstOrDefault();

                    if (checkCountryFact1 != null)
                    {
                        throw new BadRequestException($"Website {body.CountryWebsite} already exists");
                    }
                }
            }

            GetUnivInformationManagementCountryFact.Website = body.CountryWebsite == null ? "" : body.CountryWebsite;
            GetUnivInformationManagementCountryFact.Description = body.CountryDescription == null ? "" : body.CountryDescription;
            GetUnivInformationManagementCountryFact.ContactPerson = body.ContactPerson == null ? "" : body.ContactPerson;

            _dbContext.Entity<MsCountryFact>().Update(GetUnivInformationManagementCountryFact);

            //update data in MsCountryFactLevel
            //remove Level
            foreach (var ItemLevel in GetUnivInformationManagementCountryFactLevel)
            {
                var ExsisBodyLevelId = body.LevelIds.Any(e => e.Id == ItemLevel.IdLevel);

                if (!ExsisBodyLevelId)
                {
                    ItemLevel.IsActive = false;
                    _dbContext.Entity<MsCountryFactLevel>().Update(ItemLevel);
                }
            }
            ////Add level
            if (body.LevelIds != null)
            {
                foreach (var Levelid in body.LevelIds)
                {
                    var ExsistdbId = GetUnivInformationManagementCountryFactLevel.Where(e => e.IdLevel == Levelid.Id && e.IdCountryFact == GetUnivInformationManagementCountryFact.Id).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newArticleManagementCountryFact = new MsCountryFactLevel
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdCountryFact = GetUnivInformationManagementCountryFact.Id,
                            IdLevel = Levelid.Id
                        };

                        _dbContext.Entity<MsCountryFactLevel>().Add(newArticleManagementCountryFact);
                    }
                }
            }


            //update data in MsCountryFactSheet
            //remove FactSheet
            foreach (var ItemSheet in GetUnivInformationManagementCountryFactSheet)
            {
                var ExsisBodySheet = body.FactSheet.Any(e => e.Id == ItemSheet.Id);

                if (!ExsisBodySheet)
                {
                    ItemSheet.IsActive = false;
                    _dbContext.Entity<MsCountryFactSheet>().Update(ItemSheet);
                }
            }

            //Add FactSheet
            foreach (var ItemSheet in body.FactSheet.Where(e => e.Id == null || e.Id == "").ToList())
            {
                var newFactSheetUnivInformationManagementCountryFact = new MsCountryFactSheet
                {
                    Id = Guid.NewGuid().ToString(),
                    IdCountryFact = GetUnivInformationManagementCountryFact.Id,
                    OriginalName = ItemSheet.OriginalFilename,
                    Url = ItemSheet.Url,
                    FileName = ItemSheet.FileName,
                    FileType = ItemSheet.FileType,
                    FileSize = ItemSheet.FileSize,
                };
                _dbContext.Entity<MsCountryFactSheet>().Add(newFactSheetUnivInformationManagementCountryFact);
            }

            //update data in MsCountryFactLogo
            //remove Logo
            foreach (var ItemLogo in GetUnivInformationManagementCountryFactLogo)
            {
                var ExsisBodyLogo = body.Logo.Any(e => e.Id == ItemLogo.Id);

                if (!ExsisBodyLogo)
                {
                    ItemLogo.IsActive = false;
                    _dbContext.Entity<MsCountryFactLogo>().Update(ItemLogo);
                }
            }

            //Add Logo
            foreach (var ItemLogo in body.Logo.Where(e => e.Id == null || e.Id == "").ToList())
            {
                var LogoUnivInformationManagementCountryFact = new MsCountryFactLogo
                {
                    Id = Guid.NewGuid().ToString(),
                    IdCountryFact = GetUnivInformationManagementCountryFact.Id,
                    OriginalName = ItemLogo.OriginalFilename,
                    Url = ItemLogo.Url,
                    FileName = ItemLogo.FileName,
                    FileType = ItemLogo.FileType,
                    FileSize = ItemLogo.FileSize,
                };
                _dbContext.Entity<MsCountryFactLogo>().Add(LogoUnivInformationManagementCountryFact);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);


            return Request.CreateApiResult2();
        }
    }
}
