using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementPortal.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class UnivInformationManagementPortalHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IStudentDbContext _dbContext;

        public UnivInformationManagementPortalHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetUnivInformationManagementPortal = await _dbContext.Entity<MsUniversityPortal>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(CancellationToken);

            var GetUnivInformationManagementPortalSheet = await _dbContext.Entity<MsUniversityPortalFactSheet>()
           .Where(x => ids.Contains(x.IdUniversityPortal))
           .ToListAsync(CancellationToken);

            var GetUnivInformationManagementPortalLogo = await _dbContext.Entity<MsUniversityPortalLogo>()
           .Where(x => ids.Contains(x.IdUniversityPortal))
           .ToListAsync(CancellationToken);

            var GetUnivInformationManagementPortalApproval = await _dbContext.Entity<MsUniversityPortalApproval>()
            .Where(x => ids.Contains(x.IdUniversityPortal))
            .ToListAsync(CancellationToken);

            GetUnivInformationManagementPortal.ForEach(x => x.IsActive = false);

            GetUnivInformationManagementPortalSheet.ForEach(x => x.IsActive = false);

            GetUnivInformationManagementPortalLogo.ForEach(x => x.IsActive = false);

            GetUnivInformationManagementPortalApproval.ForEach(x => x.IsActive = false);

            _dbContext.Entity<MsUniversityPortal>().UpdateRange(GetUnivInformationManagementPortal);

            _dbContext.Entity<MsUniversityPortalFactSheet>().UpdateRange(GetUnivInformationManagementPortalSheet);

            _dbContext.Entity<MsUniversityPortalLogo>().UpdateRange(GetUnivInformationManagementPortalLogo);

            _dbContext.Entity<MsUniversityPortalApproval>().UpdateRange(GetUnivInformationManagementPortalApproval);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsUniversityPortal>()
            .Where(x => x.Id == id)
            .Select(x => new GetUnivInformationManagementPortalResult
            {
                Id = x.Id,
                UnivercityName = x.Name,
                Description = x.Description,
                UnivercityWebsite = x.Website,
                ContactPerson = x.ContactPerson,
                Email = x.Email,
                IsSquareLogo = x.IsLogoAsSquareImage,
                IsShare = x.IsShareOtherSchool,
                FactSheet = x.UniversityPortalFactSheet.Select(e => new FactSheetUnivInformationManagementPortal
                {
                    Id = e.Id,
                    Url = e.Url,
                    OriginalFilename = e.OriginalName,
                    FileName = e.FileName,
                    FileSize = e.FileSize,
                    FileType = e.FileType,
                }).ToList(),
                Logo = x.UniversityPortalLogo.Select(e => new LogoUnivInformationManagementPortal
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
            var param = Request.ValidateParams<GetUnivInformationManagementPortalRequest>();

            var columns = new[] { "UnivercityName", "Description", "UnivercityWebsite", "ContactPerson", "Email" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Name" },
                { columns[1], "Description" },
                { columns[2], "Website" },
                { columns[3], "ContactPerson" },
                { columns[4], "Email" },
            };

            //var GetTrSchoolUniversityPortal = _dbContext.Entity<TrSchoolUnivesityPortal>().Where(e => e.IdSchool == param.IdSchool);

            var query = _dbContext.Entity<MsUniversityPortal>()
                        .OrderByDescending(x => x.DateIn)
                        .OrderByDynamic(param, aliasColumns)
                        .Where(e=>e.IdSchool == param.IdSchool)
                        .Select(x => new GetUnivInformationManagementPortalResult
                        {
                            Id = x.Id,
                            UnivercityName = x.Name,
                            Description = x.Description,
                            UnivercityWebsite = x.Website,
                            ContactPerson = x.ContactPerson,
                            Email = x.Email,
                            IsShare = x.IsShareOtherSchool,
                            IsSquareLogo = x.IsLogoAsSquareImage,
                            FactSheet = x.UniversityPortalFactSheet.Select(e => new FactSheetUnivInformationManagementPortal
                            {
                                Id = e.Id,
                                Url = e.Url,
                                OriginalFilename = e.OriginalName,
                                FileName = e.FileName,
                                FileSize = e.FileSize,
                                FileType = e.FileType,
                            }).ToList(),
                            Logo = x.UniversityPortalLogo.Select(e => new LogoUnivInformationManagementPortal
                            {
                                Id = e.Id,
                                Url = e.Url,
                                OriginalFilename = e.OriginalName,
                                FileName = e.FileName,
                                FileSize = e.FileSize,
                                FileType = e.FileType,
                            }).ToList(),
                            //CanModified = x.IdSchool == x.UniversityPortal.IdSchool ? true : false
                            //CanModified = x.IdSchool == (GetTrSchoolUniversityPortal.Where(e => e.IdUniversityPortal == x.Id).SingleOrDefault() == null ? "" : GetTrSchoolUniversityPortal.Where(e => e.IdUniversityPortal == x.Id).SingleOrDefault().IdSchool)
                            //        ? true
                            //        : false
                        }); ;

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.UnivercityName, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
            }

            //ordering
            switch (param.OrderBy)
            {
                case "univercityName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.UnivercityName)
                        : query.OrderBy(x => x.UnivercityName);
                    break;
                case "description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
                case "univercityWebsite":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.UnivercityWebsite)
                        : query.OrderBy(x => x.UnivercityWebsite);
                    break;
                case "contactPerson":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ContactPerson)
                        : query.OrderBy(x => x.ContactPerson);
                    break;
                case "email":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Email)
                        : query.OrderBy(x => x.Email);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetUnivInformationManagementPortalResult
                    {
                        Id = x.Id,
                        UnivercityName = x.UnivercityName,
                        Description = x.Description,
                        UnivercityWebsite = x.UnivercityWebsite,
                        ContactPerson = x.ContactPerson,
                        Email = x.Email,
                        IsShare = x.IsShare,
                        IsSquareLogo = x.IsSquareLogo,
                        FactSheet = x.FactSheet,
                        Logo = x.Logo,
                        //CanModified = x.CanModified
                    })
                    .ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetUnivInformationManagementPortalResult
                    {
                        Id = x.Id,
                        UnivercityName = x.UnivercityName,
                        Description = x.Description,
                        UnivercityWebsite = x.UnivercityWebsite,
                        ContactPerson = x.ContactPerson,
                        Email = x.Email,
                        IsShare = x.IsShare,
                        IsSquareLogo = x.IsSquareLogo,
                        FactSheet = x.FactSheet,
                        Logo = x.Logo,
                        //CanModified = x.CanModified
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

            var body = await Request.ValidateBody<AddUnivInformationManagementPortalRequest, AddUnivInformationManagementPortalValidator>();

            var existsData = _dbContext.Entity<MsUniversityPortal>()
                .Any(x => x.Name == body.UnivercityName && x.IdSchool == body.IdSchool);

            if (existsData)
            {
                throw new BadRequestException($"University name { body.UnivercityName} already exists.");
            }

            if (body.UnivercityWebsite.StartsWith("http://") == false && body.UnivercityWebsite.StartsWith("https://") == false)
            {
                body.UnivercityWebsite = "http://" + body.UnivercityWebsite;
            }

            var existsData1 = _dbContext.Entity<MsUniversityPortal>()
            .Any(x => x.Website == body.UnivercityWebsite && x.IdSchool == body.IdSchool);

            if (existsData1)
            {
                throw new BadRequestException($"Website { body.UnivercityWebsite} already exists.");
            }



            var idUnivInformationManagementPortal = Guid.NewGuid().ToString();

            var newUnivInformationManagementPortal = new MsUniversityPortal
            {
                Id = idUnivInformationManagementPortal,
                IdSchool = body.IdSchool,
                IdSchoolFrom = body.IdSchool, //menandakan bahawa dia dibuat di transaksi master
                Name = body.UnivercityName,
                Description = body.UnivercityDescription == null ? "" : body.UnivercityDescription,
                Website = body.UnivercityWebsite,
                Email = body.Email == null ? "" : body.Email,
                ContactPerson = body.ContactPerson == null ? "" : body.ContactPerson,
                IsLogoAsSquareImage = body.IsSquareLogo,
                IsShareOtherSchool = body.IsShare
            };

            if (body.FactSheet != null)
            {
                foreach (var ItemFactSheet in body.FactSheet)
                {
                    var newUnivInformationManagementPortalFactSheet = new MsUniversityPortalFactSheet
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUniversityPortal = newUnivInformationManagementPortal.Id,
                        OriginalName = ItemFactSheet.OriginalFilename,
                        Url = ItemFactSheet.Url,
                        FileName = ItemFactSheet.FileName,
                        FileType = ItemFactSheet.FileType,
                        FileSize = ItemFactSheet.FileSize,
                    };
                    _dbContext.Entity<MsUniversityPortalFactSheet>().Add(newUnivInformationManagementPortalFactSheet);

                }
            }

            if (body.Logo != null)
            {
                foreach (var ItemLogo in body.Logo)
                {
                    var newUnivInformationManagementPortalLogo = new MsUniversityPortalLogo
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUniversityPortal = newUnivInformationManagementPortal.Id,
                        OriginalName = ItemLogo.OriginalFilename,
                        Url = ItemLogo.Url,
                        FileName = ItemLogo.FileName,
                        FileType = ItemLogo.FileType,
                        FileSize = ItemLogo.FileSize,
                    };
                    _dbContext.Entity<MsUniversityPortalLogo>().Add(newUnivInformationManagementPortalLogo);

                }
            }

            if (body.IsShare)
            {
                var schoolData = await _dbContext.Entity<MsSchool>().Where(e => e.Id != body.IdSchool).ToListAsync(CancellationToken);

                if (schoolData != null)
                {
                    foreach (var schoolId in schoolData)
                    {
                        var newShareSchool = new MsUniversityPortalApproval
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdUniversityPortal = newUnivInformationManagementPortal.Id,
                            IdSchool = schoolId.Id,
                            StatusApproval = "Waiting approval (1)"
                        };
                        _dbContext.Entity<MsUniversityPortalApproval>().Add(newShareSchool);
                    }
                }
            }

            //var trSchool = new TrSchoolUnivesityPortal
            //{
            //    Id = Guid.NewGuid().ToString(),
            //    IdUniversityPortal = idUnivInformationManagementPortal,
            //    IdSchool = body.IdSchool
            //};

            //_dbContext.Entity<TrSchoolUnivesityPortal>().Add(trSchool);


            _dbContext.Entity<MsUniversityPortal>().Add(newUnivInformationManagementPortal);


            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateUnivInformationManagementPortalRequest, UpdateUnivInformationManagementPortalValidator>();

            //load data utama
            var GetUnivInformationManagementPortal = await _dbContext.Entity<MsUniversityPortal>().Where(e => e.Id == body.IdUnivInformationManagementPortal).SingleOrDefaultAsync(CancellationToken);
            var GetUnivInformationManagementPortalSheet = await _dbContext.Entity<MsUniversityPortalFactSheet>().Where(e => e.IdUniversityPortal == body.IdUnivInformationManagementPortal).ToListAsync(CancellationToken);
            var GetUnivInformationManagementPortalLogo = await _dbContext.Entity<MsUniversityPortalLogo>().Where(e => e.IdUniversityPortal == body.IdUnivInformationManagementPortal).ToListAsync(CancellationToken);
            var GetUnivInformationManagementPortalApproval = await _dbContext.Entity<MsUniversityPortalApproval>().Where(e => e.IdUniversityPortal == body.IdUnivInformationManagementPortal).ToListAsync(CancellationToken);
            var temporaryFlagIsShare = GetUnivInformationManagementPortal.IsShareOtherSchool;

            if (GetUnivInformationManagementPortal is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["University Information Management Portal"], "Id", body.IdUnivInformationManagementPortal));
            }

            ////update data in MsUniversityPortal
            if (GetUnivInformationManagementPortal.Name != body.UnivercityName &&  GetUnivInformationManagementPortal.IdSchool == body.IdSchool)
            {
                var checkPortal = _dbContext.Entity<MsUniversityPortal>().Where(x => x.Name == body.UnivercityName && x.IdSchool == body.IdSchool).FirstOrDefault();

                if (checkPortal != null)
                {
                    throw new BadRequestException($"University name {body.UnivercityName} already exists");
                }

            }

            if (body.UnivercityWebsite.StartsWith("http://") == false && body.UnivercityWebsite.StartsWith("https://") == false)
            {
                body.UnivercityWebsite = "http://" + body.UnivercityWebsite;
            }

            if ( GetUnivInformationManagementPortal.Website != body.UnivercityWebsite && GetUnivInformationManagementPortal.IdSchool == body.IdSchool)
            {
                var checkPortal1 = _dbContext.Entity<MsUniversityPortal>().Where(x => x.Website == body.UnivercityWebsite && x.IdSchool == body.IdSchool).FirstOrDefault();

                if (checkPortal1 != null)
                {
                    throw new BadRequestException($"Website {body.UnivercityWebsite} already exists");
                }
            }



            GetUnivInformationManagementPortal.Name = body.UnivercityName;
            GetUnivInformationManagementPortal.Website = body.UnivercityWebsite;
            GetUnivInformationManagementPortal.IdSchool = body.IdSchool;
            GetUnivInformationManagementPortal.IdSchoolFrom = body.IdSchool;
            GetUnivInformationManagementPortal.Description = body.UnivercityDescription == null ? "" : body.UnivercityDescription;
            GetUnivInformationManagementPortal.Website = body.UnivercityWebsite;
            GetUnivInformationManagementPortal.ContactPerson = body.ContactPerson == null ? "" : body.ContactPerson;
            GetUnivInformationManagementPortal.Email = body.Email == null ? "" : body.Email;
            GetUnivInformationManagementPortal.IsLogoAsSquareImage = body.IsSquareLogo;
            GetUnivInformationManagementPortal.IsShareOtherSchool = body.IsShare;

            _dbContext.Entity<MsUniversityPortal>().Update(GetUnivInformationManagementPortal);

             //update data in MsCountryFactSheet
            //remove FactSheet
            foreach (var ItemSheet in GetUnivInformationManagementPortalSheet)
            {
                var ExsisBodySheet = body.FactSheet.Any(e => e.Id == ItemSheet.Id);

                if (!ExsisBodySheet)
                {
                    ItemSheet.IsActive = false;
                    _dbContext.Entity<MsUniversityPortalFactSheet>().Update(ItemSheet);
                }
            }

            //Add FactSheet
            foreach (var ItemSheet in body.FactSheet.Where(e => e.Id == null || e.Id == "").ToList())
            {
                var newFactSheetUnivInformationManagementportal = new MsUniversityPortalFactSheet
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUniversityPortal = GetUnivInformationManagementPortal.Id,
                    OriginalName = ItemSheet.OriginalFilename,
                    Url = ItemSheet.Url,
                    FileName = ItemSheet.FileName,
                    FileType = ItemSheet.FileType,
                    FileSize = ItemSheet.FileSize,
                };
                _dbContext.Entity<MsUniversityPortalFactSheet>().Add(newFactSheetUnivInformationManagementportal);
            }

            //update data in MsCountryFactLogo
            //remove Logo
            foreach (var ItemLogo in GetUnivInformationManagementPortalLogo)
            {
                var ExsisBodyLogo = body.Logo.Any(e => e.Id == ItemLogo.Id);

                if (!ExsisBodyLogo)
                {
                    ItemLogo.IsActive = false;
                    _dbContext.Entity<MsUniversityPortalLogo>().Update(ItemLogo);
                }
            }

            //Add Logo
            foreach (var ItemLogo in body.Logo.Where(e => e.Id == null || e.Id == "").ToList())
            {
                var LogoUnivInformationManagementPortal = new MsUniversityPortalLogo
                {
                    Id = Guid.NewGuid().ToString(),
                    IdUniversityPortal = GetUnivInformationManagementPortal.Id,
                    OriginalName = ItemLogo.OriginalFilename,
                    Url = ItemLogo.Url,
                    FileName = ItemLogo.FileName,
                    FileType = ItemLogo.FileType,
                    FileSize = ItemLogo.FileSize,
                };
                _dbContext.Entity<MsUniversityPortalLogo>().Add(LogoUnivInformationManagementPortal);
            }

            if (!temporaryFlagIsShare)
            {
                if (body.IsShare)
                {
                    var schoolData = await _dbContext.Entity<MsSchool>().Where(e => e.Id != body.IdSchool).ToListAsync(CancellationToken);

                    if (schoolData != null)
                    {
                        foreach (var schoolId in schoolData)
                        {
                            var newShareSchool = new MsUniversityPortalApproval
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdUniversityPortal = GetUnivInformationManagementPortal.Id,
                                IdSchool = schoolId.Id,
                                StatusApproval = "Waiting approval (1)"
                            };
                            _dbContext.Entity<MsUniversityPortalApproval>().Add(newShareSchool);
                        }
                    }
                }
            }
            else
            {
                if (!body.IsShare)
                {
                    //remove Approval
                    if (GetUnivInformationManagementPortal != null)
                    {
                        foreach (var ItemApproval in GetUnivInformationManagementPortalApproval)
                        {
                            ItemApproval.IsActive = false;
                            _dbContext.Entity<MsUniversityPortalApproval>().Update(ItemApproval);
                        }
                    }
                }
            }


            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);


            return Request.CreateApiResult2();
        }
    }
}
