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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetDocumentRequestTypeListHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "DocumentName" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "name" },
        };

        private readonly IDocumentDbContext _dbContext;

        public GetDocumentRequestTypeListHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestTypeListRequest>(
                            nameof(GetDocumentRequestTypeListRequest.IdSchool));

            var predicate = PredicateBuilder.True<MsDocumentReqType>();

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    //=> EF.Functions.Like(x.Name, $"%{param.Search}%")
                    => EF.Functions.Like(x.Name, param.SearchPattern())
                    );

            var getGradeCodeByLevelCodeList = await _dbContext.Entity<MsGrade>()
                                            .Where(x => x.Level.AcademicYear.IdSchool == param.IdSchool &&
                                                        (string.IsNullOrEmpty(param.AcademicYearCode) ? true : x.Level.AcademicYear.Code.CompareTo(param.AcademicYearCode) == 0) &&
                                                        (string.IsNullOrEmpty(param.LevelCode) ? true : x.Level.Code == param.LevelCode) &&
                                                        (string.IsNullOrEmpty(param.GradeCode) ? true : x.Code == param.GradeCode)
                                                        )
                                            .Select(x => x.Code)
                                            .Distinct()
                                            .ToListAsync(CancellationToken);

            var query = _dbContext.Entity<MsDocumentReqType>()
                            .Include(x => x.DocumentReqTypeGradeMappings)
                            .Include(x => x.DocumentReqDefaultPICs)
                                .ThenInclude(x => x.Staff)
                            .Include(x => x.DocumentReqDefaultPICGroups)
                                .ThenInclude(x => x.Role)
                            .Include(x => x.DocumentReqApplicantDetails)
                            .Include(x => x.StartAcademicYear)
                            .Include(x => x.EndAcademicYear)
                            .Where(predicate)
                            .Where(x =>
                                        x.IdSchool == param.IdSchool &&
                                        (string.IsNullOrEmpty(param.AcademicYearCode) ? true : 
                                            (x.StartAcademicYear == null ? true : x.StartAcademicYear.Code.CompareTo(param.AcademicYearCode) <= 0) &&
                                            (x.EndAcademicYear == null ? true : x.EndAcademicYear.Code.CompareTo(param.AcademicYearCode) >= 0)
                                        ) &&
                                        (param.ActiveStatus == null ? true : (x.Status == param.ActiveStatus)) &&
                                        (param.VisibleToParent == null ? true : (x.VisibleToParent == param.VisibleToParent)) &&
                                        (param.PaidDocument == null ? true : (param.PaidDocument.Value ? x.Price > 0 : x.Price <= 0)) &&
                                        (!x.IsUsingGradeMapping ? true : x.DocumentReqTypeGradeMappings.Where(y => getGradeCodeByLevelCodeList.Any(z => z == y.CodeGrade)).Select(y => y.CodeGrade).Any())
                                        )
                            .OrderByDynamic(param, _aliasColumns);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                                .Select(x => new ItemValueVm
                                {
                                    Id = x.Id,
                                    Description = x.Name
                                })
                                .ToListAsync(CancellationToken);
            else
            {
                items = await query
                            .Select(x => new GetDocumentRequestTypeListResult
                            {
                                IdDocumentReqType = x.Id,
                                ActiveStatus = x.Status,
                                CanDelete = x.DocumentReqApplicantDetails.Any() ? false : true,
                                IdSchool = x.IdSchool,
                                DocumentName = x.Name,
                                DocumentDescription = x.Description,
                                Price = x.Price,
                                InvoiceDueHoursPayment = x.InvoiceDueHoursPayment,
                                DefaultNoOfProcessDay = x.DefaultNoOfProcessDay,
                                HardCopyAvailable = x.HardCopyAvailable,
                                SoftCopyAvailable = x.SoftCopyAvailable,
                                AcademicYearStart = string.IsNullOrEmpty(x.IdAcademicYearStart) ? null : new ItemValueVm
                                {
                                    Id = x.IdAcademicYearStart,
                                    Description = x.StartAcademicYear.Description
                                },
                                AcademicYearEnd = string.IsNullOrEmpty(x.IdAcademicYearEnd) ? null : new ItemValueVm
                                {
                                    Id = x.IdAcademicYearEnd,
                                    Description = x.EndAcademicYear.Description
                                },
                                IsAcademicDocument = x.IsAcademicDocument,
                                DocumentHasTerm = x.DocumentHasTerm,
                                IsUsingNoOfPages = x.IsUsingNoOfPages,
                                DefaultNoOfPages = x.DefaultNoOfPages,
                                IsUsingNoOfCopy = x.IsUsingNoOfCopy,
                                MaxNoOfCopy = x.MaxNoOfCopy,
                                VisibleToParent = x.VisibleToParent,
                                ParentNeedApproval = x.ParentNeedApproval,
                                IsUsingGradeMapping = x.IsUsingGradeMapping,
                                GradeMappingList = !x.IsUsingGradeMapping ? null : x.DocumentReqTypeGradeMappings.Select(x => new GetDocumentRequestTypeListResult_GradeMapping
                                {
                                    GradeCode = x.CodeGrade
                                })
                                .OrderBy(y => y.GradeCode.Length)
                                .ThenBy(y => y.GradeCode)
                                .ToList(),
                                PICIndividualList = x.DocumentReqDefaultPICs.Select(x => new GetDocumentRequestTypeListResult_PICIndividual
                                {
                                    Binusian = new NameValueVm
                                    {
                                        Id = x.IdBinusian,
                                        Name = x.Staff.FirstName + (string.IsNullOrWhiteSpace(x.Staff.LastName) ? "" : " " + x.Staff.LastName)
                                    }
                                }).OrderBy(x => x.Binusian.Name).ToList(),
                                PICGroupList = x.DocumentReqDefaultPICGroups.Select(x => new GetDocumentRequestTypeListResult_PICGroup
                                {
                                    RoleGroup = new ItemValueVm
                                    {
                                        Id = x.IdRole,
                                        Description = x.Role.Description
                                    }
                                }).OrderBy(x => x.RoleGroup.Description).ToList()
                            })
                            .ToListAsync(CancellationToken);



                // get all grade code description
                var getGradeCodeDescriptionList = await _dbContext.Entity<MsGrade>()
                                                .Include(x => x.Level)
                                                    .ThenInclude(x => x.AcademicYear)
                                                .Where(x => x.Level.AcademicYear.IdSchool == param.IdSchool)
                                                .Select(x => new
                                                {
                                                    GradeCode = x.Code,
                                                    GradeDescription = x.Description,
                                                    AcademicYearCode = x.Level.AcademicYear.Code
                                                })
                                                .OrderByDescending(x => x.AcademicYearCode)
                                                .Distinct()
                                                .ToListAsync(CancellationToken);

                // add grade description
                foreach (GetDocumentRequestTypeListResult item in items)
                {
                    if(item.GradeMappingList != null && item.GradeMappingList.Any())
                    {
                        foreach (var gradeMapping in item.GradeMappingList)
                        {
                            gradeMapping.GradeDescription = getGradeCodeDescriptionList
                                                                .Where(x => x.GradeCode == gradeMapping.GradeCode)
                                                                .Select(x => x.GradeDescription)
                                                                .FirstOrDefault();
                        }
                    }
                }
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
