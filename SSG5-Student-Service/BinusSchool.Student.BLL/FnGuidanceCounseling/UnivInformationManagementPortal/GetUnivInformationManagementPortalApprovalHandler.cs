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
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using static BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal.GetUnivInformationManagementPortalApprovalResult;

namespace BinusSchool.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class GetUnivInformationManagementPortalApprovalHandler : FunctionsHttpSingleHandler
    {

        private readonly IStudentDbContext _dbContext;
        public GetUnivInformationManagementPortalApprovalHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnivInformationManagementPortalApprovalRequest>();

            string[] _columns = { "FromSchool","CreatedBy", "UniversityName", "Description", "UniversityWebsite", "ContactPerson"};

            var aliasColumns = new Dictionary<string, string>
            {
                { _columns[0], "School.Description" },
                { _columns[1], "CreatedBy" },
                { _columns[2], "Name" },
                { _columns[3], "Description" },
                { _columns[4], "Website" },
                { _columns[5], "ContactPerson" },
            };

            var query = _dbContext.Entity<MsUniversityPortalApproval>()
                         .Include(e => e.UniversityPortal).ThenInclude(e=>e.School)
                         .Include(e => e.UniversityPortal).ThenInclude(e=>e.UniversityPortalLogo)
                        .OrderByDescending(x => x.DateIn)
                        .OrderByDynamic(param, aliasColumns)
                        .Where(e=> e.IdSchool == param.IdSchool && e.StatusApproval == "Waiting approval (1)")
                        .Select(x => new GetUnivInformationManagementPortalApprovalResult
                        {
                            Id = x.Id,
                            FromSchool = new CodeWithIdVm
                            {
                                Id = x.UniversityPortal.IdSchoolFrom,
                                Code = x.UniversityPortal.SchoolFrom.Name,
                                Description = x.UniversityPortal.SchoolFrom.Description
                            },
                            UnivercityName = x.UniversityPortal.Name,
                            Description = x.UniversityPortal.Description,
                            UnivercityWebsite = x.UniversityPortal.Website,
                            ContactPerson = x.UniversityPortal.ContactPerson,
                            CreatedBy = x.UserIn,
                            IdUnivInformationManagementPortal = x.UniversityPortal.Id,
                            Logo = x.UniversityPortal.UniversityPortalLogo.Select(e => new LogoUnivInformationManagementPortalApproval
                            {
                                Id = e.Id,
                                Url = e.Url,
                                OriginalFilename = e.OriginalName,
                                FileName = e.FileName,
                                FileSize = e.FileSize,
                                FileType = e.FileType,
                            }).ToList(),
                        });
            



            if (!string.IsNullOrEmpty(param.IdFromSchool))
                query = query.Where(x => x.FromSchool.Id.Contains(param.IdFromSchool));

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.UnivercityName, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
            }


            ////ordering
            //switch (param.OrderBy)
            //{
            //    case "FromSchool":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.FromSchool.Description)
            //            : query.OrderBy(x => x.FromSchool.Description);
            //        break;
            //    case "CreatedBy":
            //        query = param.OrderType == OrderType.Desc
            //            ? query.OrderByDescending(x => x.CreatedBy)
            //            : query.OrderBy(x => x.CreatedBy);
            //        break;
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
            //};

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetUnivInformationManagementPortalApprovalResult
                    {
                        Id = x.Id,
                        FromSchool = x.FromSchool,
                        CreatedBy = x.CreatedBy,
                        UnivercityName = x.UnivercityName,
                        Description = x.Description,
                        UnivercityWebsite = x.UnivercityWebsite,
                        ContactPerson = x.ContactPerson,
                        IdUnivInformationManagementPortal = x.IdUnivInformationManagementPortal,
                        Logo = x.Logo
                        
                    })
                    .ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetUnivInformationManagementPortalApprovalResult
                    {
                        Id = x.Id,
                        FromSchool = x.FromSchool,
                        CreatedBy = x.CreatedBy,
                        UnivercityName = x.UnivercityName,
                        Description = x.Description,
                        UnivercityWebsite = x.UnivercityWebsite,
                        ContactPerson = x.ContactPerson,
                        IdUnivInformationManagementPortal = x.IdUnivInformationManagementPortal,
                        Logo = x.Logo
                    }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.ToList().Count;

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
