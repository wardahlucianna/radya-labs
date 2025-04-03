using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerUniversityPortalHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetGcCornerUniversityPortalHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGcCornerUniversityPortalRequest>();

            string[] _columns = { "UniversityName", "Description", "UniversityWebsite", "ContactPerson", "Email", "CreatedBy" };

            var aliasColumns = new Dictionary<string, string>
            {
                { _columns[0], "Name" },
                { _columns[1], "Description" },
                { _columns[2], "Website" },
                { _columns[3], "ContactPerson" },
                { _columns[4], "Email" },
                { _columns[5], "UserIn" },
            };

            var query = _dbContext.Entity<MsUniversityPortal>()
                        .OrderByDescending(x => x.DateIn)
                        .OrderByDynamic(param, aliasColumns)
                        .Where(e => e.IdSchool == param.IdSchool)
                        .Select(x => new GetGcCornerUniversityPortalResult
                        {
                            Id = x.Id,
                            UnivercityName = x.Name,
                            Description = x.Description,
                            UnivercityWebsite = x.Website,
                            ContactPerson = x.ContactPerson,
                            Email = x.Email,
                            CreatedBy = x.UserIn,
                            FactSheet = x.UniversityPortalFactSheet.Select(e => new FactSheetGcCornerUniversityPortal
                            {
                                Id = e.Id,
                                Url = e.Url,
                                OriginalFilename = e.OriginalName,
                                FileName = e.FileName,
                                FileSize = e.FileSize,
                                FileType = e.FileType,
                            }).ToList(),
                            Logo = x.UniversityPortalLogo.Select(e => new LogoGcCornerUniversityPortal
                            {
                                Id = e.Id,
                                Url = e.Url,
                                OriginalFilename = e.OriginalName,
                                FileName = e.FileName,
                                FileSize = e.FileSize,
                                FileType = e.FileType,
                            }).ToList(),                            
                        }); ;

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.UnivercityName, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
            }


            ////ordering
            //switch (param.OrderBy)
            //{
            //    case "UniversityName":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.UnivercityName)
            //            : query.OrderBy(x => x.UnivercityName);
            //        break;
            //    case "Description":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.Description)
            //            : query.OrderBy(x => x.Description);
            //        break;
            //    case "UniversityWebsite":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.UnivercityWebsite)
            //            : query.OrderBy(x => x.UnivercityWebsite);
            //        break;
            //    case "ContactPerson":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.ContactPerson)
            //            : query.OrderBy(x => x.ContactPerson);
            //        break;
            //    case "Email":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.Email)
            //            : query.OrderBy(x => x.Email);
            //        break;
            //};

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetGcCornerUniversityPortalResult
                    {
                        Id = x.Id,
                        UnivercityName = x.UnivercityName,
                        Description = x.Description,
                        UnivercityWebsite = x.UnivercityWebsite,
                        ContactPerson = x.ContactPerson,
                        Email = x.Email,
                        FactSheet = x.FactSheet,
                        Logo = x.Logo,
                        CreatedBy = x.CreatedBy
                    })
                    .ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetGcCornerUniversityPortalResult
                    {
                        Id = x.Id,
                        UnivercityName = x.UnivercityName,
                        Description = x.Description,
                        UnivercityWebsite = x.UnivercityWebsite,
                        ContactPerson = x.ContactPerson,
                        Email = x.Email,
                        FactSheet = x.FactSheet,
                        Logo = x.Logo,
                        CreatedBy = x.CreatedBy
                    }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.ToList().Count;

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
