using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparation;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.TextbookPreparation
{
    public class GetTextBookPreparationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetTextBookPreparationHandler(ISchoolDbContext DbContext, IMachineDateTime DateTime)
        {
            _dbContext = DbContext;
            _dateTime = DateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTextbookPreparationRequest>();
            string[] _columns = { "AcademicYear", "Subject", "Level", "Grade", "Title", "Author", "Isbn", "BookType", "Note", "Status" };

            var predicate = PredicateBuilder.Create<TrTextbook>(x => x.IdBinusianCreated == param.IdUser);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Subject.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Subject.IdGrade == param.IdGrade);

            //serach
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Subject.Description.Contains(param.Search) || x.Title.Contains(param.Search));

            var DateTime = _dateTime.ServerTime.Date;

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                .Where(e => e.Id == param.IdAcademicYear)
               .FirstOrDefaultAsync(CancellationToken);

            var GetUserEntry = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                .Include(e => e.TextbookUserPeriod).ThenInclude(e => e.AcademicYear)
                .Where(e => e.IdBinusian == param.IdUser)
               .ToListAsync(CancellationToken);

            var ExsisUserEntry = GetUserEntry
                                    .Where(e => (DateTime >= Convert.ToDateTime(e.TextbookUserPeriod.StartDate).Date && DateTime <= Convert.ToDateTime(e.TextbookUserPeriod.EndDate).Date) && e.TextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry)
                                    .Any();

            var GetTextbookUserPeriod = await _dbContext.Entity<TrTextbook>()
                .Include(e => e.AcademicYear)
                .Include(e => e.Subject)
                .Where(predicate)
               .Select(x => new
               {
                   Id = x.Id,
                   AcademicYear = x.AcademicYear.Description,
                   Subject = x.Subject.Description,
                   Level = x.Subject.Grade.Level.Code,
                   Grade = x.Subject.Grade.Code,
                   Title = x.Title,
                   Author = x.Author,
                   Isbn = x.ISBN,
                   BookType = x.BookType.GetDescription(),
                   Note = x.IsApproval1 && x.IsApproval2 && x.IsApproval3
                            ? x.ApprovalNote3
                            : x.IsApproval1 && x.IsApproval2
                                ? x.ApprovalNote2
                                : x.IsApproval1
                                    ? x.ApprovalNote1
                                    : null,
                   Status = x.Status.GetDescription(),
                   IsDisabledEdit = x.Status == TextbookPreparationStatus.Declined || x.Status == TextbookPreparationStatus.Hold ? false : true,
                   IsDisabledDelete = x.Status == TextbookPreparationStatus.Declined || x.Status == TextbookPreparationStatus.Hold ? false : true,
               }).ToListAsync(CancellationToken);

            var query = GetTextbookUserPeriod.Distinct();

            if (!string.IsNullOrEmpty(param.Status))
                query = query.Where(x => x.Status == param.Status);
            //orderBy
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "Subject":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Subject)
                        : query.OrderBy(x => x.Subject);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level)
                        : query.OrderBy(x => x.Level);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade)
                        : query.OrderBy(x => x.Grade);
                    break;
                case "Title":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Title)
                        : query.OrderBy(x => x.Title);
                    break;
                case "Author":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Author)
                        : query.OrderBy(x => x.Author);
                    break;
                case "Isbn":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Isbn)
                        : query.OrderBy(x => x.Isbn);
                    break;
                case "BookType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BookType)
                        : query.OrderBy(x => x.BookType);
                    break;
                case "Note":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Note)
                        : query.OrderBy(x => x.Note);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
            };

            GetTextbookPreparationResult items;
            if (param.Return == CollectionType.Lov)
                items = new GetTextbookPreparationResult
                {
                    IsEnableButtonAdd = ExsisUserEntry,
                    TextbokPreparations = query
                                           .Select(x => new GetTextbookPreparation
                                           {
                                               Id = x.Id,
                                               AcademicYear = x.AcademicYear,
                                               Subject = x.Subject,
                                               Level = x.Level,
                                               Grade = x.Grade,
                                               Title = x.Title,
                                               Author = x.Author,
                                               ISBN = x.Isbn,
                                               BookType = x.BookType,
                                               Note = x.Note,
                                               Status = x.Status,
                                               IsDisabledDelete = x.IsDisabledDelete,
                                               IsDisabledEdit = x.IsDisabledEdit
                                           })
                                            .ToList()
                };
            else
                items = new GetTextbookPreparationResult
                {
                    IsEnableButtonAdd = ExsisUserEntry,
                    TextbokPreparations = query
                        .SetPagination(param)
                        .Select(x => new GetTextbookPreparation
                        {
                            Id = x.Id,
                            AcademicYear = x.AcademicYear,
                            Subject = x.Subject,
                            Level = x.Level,
                            Grade = x.Grade,
                            Title = x.Title,
                            Author = x.Author,
                            ISBN = x.Isbn,
                            BookType = x.BookType,
                            Note = x.Note,
                            Status = x.Status,
                            IsDisabledDelete = x.IsDisabledDelete,
                            IsDisabledEdit = x.IsDisabledEdit
                        })
                        .ToList()
                };

            var count = param.CanCountWithoutFetchDb(items.TextbokPreparations.Count)
                ? items.TextbokPreparations.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
