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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.TextbookPreparationSubjectGroup.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class TextbookPreparationSubjectGroupHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public TextbookPreparationSubjectGroupHandler(ISchoolDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetSubjectGroupDetail = await _dbContext.Entity<MsTextbookSubjectGroupDetail>()
                                 .Where(e => ids.Contains(e.IdTextbookSubjectGroup))
                                 .ToListAsync(CancellationToken);

            if (!GetSubjectGroupDetail.Any())
                throw new BadRequestException("Subject Group Detail is not found");

            GetSubjectGroupDetail.ForEach(e => e.IsActive = false);
            _dbContext.Entity<MsTextbookSubjectGroupDetail>().UpdateRange(GetSubjectGroupDetail);

            var GetSubjectGroup = await _dbContext.Entity<MsTextbookSubjectGroup>()
                                  .Where(e => ids.Contains(e.Id))
                                  .ToListAsync(CancellationToken);

            if (!GetSubjectGroup.Any())
                throw new BadRequestException("Subject Group is not found");

            GetSubjectGroup.ForEach(e => e.IsActive = false);
            _dbContext.Entity<MsTextbookSubjectGroup>().UpdateRange(GetSubjectGroup);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var items = await _dbContext.Entity<MsTextbookSubjectGroup>()
                .Include(e => e.TextbookSubjectGroupDetails).ThenInclude(e => e.Subject).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                .Include(e => e.AcademicYear)
                .Where(e=>e.Id==id)
               .Select(x => new DetailTextbookPreparationSubjectGroupResult
               {
                   Id = x.Id,
                   AcademicYear = new NameValueVm
                   {
                       Id=x.AcademicYear.Id,
                       Name = x.AcademicYear.Description
                   },
                   SubjectGroupName = x.SubjectGroupName,
                   Subject = x.TextbookSubjectGroupDetails.Select(f => new TextbookPreparationSubject
                   {
                       Subject = new NameValueVm
                       {
                           Id = f.Subject.Id,
                           Name = f.Subject.Description
                       },
                       Level = new NameValueVm
                       {
                           Id = f.Subject.Grade.Level.Id,
                           Name = f.Subject.Grade.Level.Description,
                       },
                       Grade = new NameValueVm
                       {
                           Id = f.Subject.Grade.Id,
                           Name = f.Subject.Grade.Description,
                       }
                   }).ToList(),
               }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetTextbookPreparationSubjectGroupRequest>();
            string[] _columns = { "AcademicYear", "Level", "Grade", "SubjectGroup", "Subject" };

            var predicate = PredicateBuilder.Create<MsTextbookSubjectGroup>(x => true);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.TextbookSubjectGroupDetails.Select(e => e.Subject.Grade.IdLevel).Contains(param.IdLevel));

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.TextbookSubjectGroupDetails.Select(e => e.Subject.IdGrade).Contains(param.IdGrade));

            //serach
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.SubjectGroupName.Contains(param.Search));

            var query = _dbContext.Entity<MsTextbookSubjectGroup>()
                .Include(e => e.TextbookSubjectGroupDetails).ThenInclude(e => e.Subject).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                .Include(e => e.AcademicYear)
                .Where(predicate)
               .Select(x => new
               {
                   Id = x.Id,
                   AcademicYear = x.AcademicYear.Description,
                   SubjectGroupName = x.SubjectGroupName,
                   IdLevel = x.TextbookSubjectGroupDetails.Select(e => e.Subject.Grade.IdLevel).FirstOrDefault(),
                   Level = x.TextbookSubjectGroupDetails.Select(e => e.Subject.Grade.Level.Code).FirstOrDefault(),
                   Detail = x.TextbookSubjectGroupDetails
                            .Select(e => new
                            {
                                IdGrade = e.Subject.IdGrade,
                                Grade = e.Subject.Grade.Description,
                                IdSubjet = e.IdSubject,
                                Subject = e.Subject.Description,
                            })
                            .OrderBy(e => e.Grade).ThenBy(e => e.Subject)
                            .ToList(),
                   IsDisabledEdit = x.Textbooks.Any(),
                   IsDisabledDelete = x.Textbooks.Any(),
                   OrderNumber = x.TextbookSubjectGroupDetails.Select(e => e.Subject.Grade.Level.OrderNumber).FirstOrDefault(),
               });

            //orderBy
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.OrderNumber)
                        : query.OrderBy(x => x.OrderNumber);
                    break;
                case "SubjectGroup":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SubjectGroupName)
                        : query.OrderBy(x => x.SubjectGroupName);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var GetSubjectGroup = await query
                    .ToListAsync(CancellationToken);

                items = GetSubjectGroup.Select(e => new GetTextbookPreparationSubjectGroupResult
                {
                    Id = e.Id,
                    AcademicYear = e.AcademicYear,
                    Level = e.Level,
                    SubjectGroup = e.SubjectGroupName,
                    Grade = ConvertString(e.Detail.Select(x => x.Grade).Distinct().ToList()),
                    Subject = ConvertString(e.Detail.Select(x => x.Grade + "." + x.Subject).Distinct().ToList()),
                    IsDisabledDelete = e.IsDisabledDelete,
                    IsDisabledEdit = e.IsDisabledEdit,
                }).ToList();
            }
            else
            {
                var GetSubjectGroup = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = GetSubjectGroup.Select(e => new GetTextbookPreparationSubjectGroupResult
                {
                    Id = e.Id,
                    AcademicYear = e.AcademicYear,
                    Level = e.Level,
                    SubjectGroup = e.SubjectGroupName,
                    Grade = ConvertString(e.Detail.Select(x => x.Grade).Distinct().ToList()),
                    Subject = ConvertString(e.Detail.Select(x => x.Grade + "." + x.Subject).Distinct().ToList()),
                    IsDisabledDelete = e.IsDisabledDelete,
                    IsDisabledEdit = e.IsDisabledEdit,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddTextbookPreparationSubjectGroupRequest, AddTextbookPreparationSubjectGroupValidator>();

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => e.Id == body.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);

            if (GetAcademicYear == null)
                throw new BadRequestException("Academic year with Id "+body.IdAcademicYear+" is not found");

            var GetSubject = await _dbContext.Entity<MsSubject>()
                                    .Where(e => body.IdSubject.Contains(e.Id) && e.Grade.Level.IdAcademicYear==body.IdAcademicYear)
                                    .ToListAsync(CancellationToken);

            if (!GetSubject.Any())
                throw new BadRequestException("Subject is not found");

            var GetTextbookSubjectGroupDetail = await _dbContext.Entity<MsTextbookSubjectGroupDetail>()
                                                .Include(e => e.TextbookSubjectGroup)
                                                .Include(e => e.Subject)
                                                .Where(e => e.TextbookSubjectGroup.IdAcademicYear == body.IdAcademicYear 
                                                        && GetSubject.Select(f=>f.IdGrade).ToList().Contains(e.Subject.IdGrade)
                                                        && e.TextbookSubjectGroup.SubjectGroupName==body.SubjectGroupName
                                                    )
                                                .ToListAsync(CancellationToken);

            if (GetTextbookSubjectGroupDetail.Any())
                throw new BadRequestException("Subject group name is exsis in subject group");

            var NewTextbookSubjectGroup = new MsTextbookSubjectGroup
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                SubjectGroupName = body.SubjectGroupName,
                
            };
            _dbContext.Entity<MsTextbookSubjectGroup>().Add(NewTextbookSubjectGroup);

            foreach (var item in GetSubject)
            {
                var NewTextbookSubjectGroupDetail = new MsTextbookSubjectGroupDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdSubject = item.Id,
                    IdTextbookSubjectGroup=NewTextbookSubjectGroup.Id
                };
                _dbContext.Entity<MsTextbookSubjectGroupDetail>().Add(NewTextbookSubjectGroupDetail);
            }
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateTextbookPreparationSubjectGroupRequest, UpdateTextbookPreparationSubjectGroupValidator>();

            var GetSubjectGroup = await _dbContext.Entity<MsTextbookSubjectGroup>()
                                    .Include(e=>e.TextbookSubjectGroupDetails)
                                    .Where(e => e.Id == body.Id)
                                    .FirstOrDefaultAsync(CancellationToken);

            if (GetSubjectGroup == null)
                throw new BadRequestException("Subject Group With Id:"+ body.Id +" is not found");

            var GetSubject = await _dbContext.Entity<MsSubject>()
                                    .Where(e => body.IdSubject.Contains(e.Id) && e.Grade.Level.IdAcademicYear == GetSubjectGroup.IdAcademicYear)
                                    .ToListAsync(CancellationToken);

            if (!GetSubject.Any())
                throw new BadRequestException("Subject is not found");

            GetSubjectGroup.SubjectGroupName = body.SubjectGroupName;
            _dbContext.Entity<MsTextbookSubjectGroup>().Update(GetSubjectGroup);

            var GetSubjectGroupDetail = GetSubjectGroup.TextbookSubjectGroupDetails.ToList();
            GetSubjectGroupDetail.ForEach(e =>e.IsActive = false);
            _dbContext.Entity<MsTextbookSubjectGroupDetail>().UpdateRange(GetSubjectGroupDetail);

            foreach (var item in GetSubject)
            {
                var NewTextbookSubjectGroupDetail = new MsTextbookSubjectGroupDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdSubject = item.Id,
                    IdTextbookSubjectGroup = GetSubjectGroup.Id
                };
                _dbContext.Entity<MsTextbookSubjectGroupDetail>().Add(NewTextbookSubjectGroupDetail);
            }
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        public static string ConvertString(List<string> GetData)
        {
            var Result = "";

            foreach (var item in GetData)
            {
                if (GetData.IndexOf(item) > 0)
                    Result += ", " + item;
                else
                    Result += item;
            }

            return Result;
        }
    }
}

