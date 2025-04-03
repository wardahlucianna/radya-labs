using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.TextbookPreparationApproval.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.School.FnSchool.TextbookPreparationApproval
{
    public class TextbookPreparationApprovalHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public TextbookPreparationApprovalHandler(ISchoolDbContext schoolDbContext, IMachineDateTime DateTime)
        {
            _dbContext = schoolDbContext;
            _dateTime = DateTime;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetTextbookPreparationApprovalRequest>();
            string[] _columns = { "AcademicYear", "Subject", "Level", "Grade", "Title", "Author", "Isbn", "BookType", "Note", "Status" };
            var DateTime = _dateTime.ServerTime.Date;

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                .Where(e => e.Id == param.IdAcademicYear)
               .FirstOrDefaultAsync(CancellationToken);

            if (GetAcademicYear == null)
                throw new BadRequestException($"Academic year with Id: {param.IdAcademicYear} is not found");

            var GetApproval = await _dbContext.Entity<MsTextbookSettingApproval>()
                .Where(e => e.IdSchool == GetAcademicYear.IdSchool)
               .ToListAsync(CancellationToken);

            int CountApproval = GetApproval.Count();

            var GetUserApproval = GetApproval
                .Where(e => e.IdBinusian == param.IdUser)
               .FirstOrDefault();

            var ApproverTo = GetUserApproval == null ? 0 : GetUserApproval.ApproverTo;
            var isEdit = GetUserApproval == null ? false : GetUserApproval.IsEdit;
            var isDelete = GetUserApproval == null ? false : GetUserApproval.IsEdit;

            var IsPic = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
               .Include(e => e.TextbookUserPeriod).ThenInclude(e => e.AcademicYear)
               .Where(e => e.IdBinusian == param.IdUser
                   && e.TextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookPic
                   )
              .AnyAsync(CancellationToken);

            var predicate = PredicateBuilder.Create<TrTextbook>(x => x.Status != TextbookPreparationStatus.Hold);
            if (IsPic)
            {

            }
            else if (GetUserApproval.ApproverTo == 1)
            {
                predicate = predicate.And(x => x.IdBinusianApproval1 == param.IdUser || x.Status== TextbookPreparationStatus.OnReview1);
            }
            else if (GetUserApproval.ApproverTo == 2)
            {
                predicate = predicate.And(x => x.IdBinusianApproval2 == param.IdUser || x.Status == TextbookPreparationStatus.OnReview2);
            }
            else if (GetUserApproval.ApproverTo == 3)
            {
                predicate = predicate.And(x => x.IdBinusianApproval3 == param.IdUser || x.Status == TextbookPreparationStatus.OnReview3);
            }

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Subject.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Subject.IdGrade == param.IdGrade);

            //serach
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Subject.Description.Contains(param.Search) || x.Title.Contains(param.Search));

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
                   Note = x.Note,
                   Status = x.Status.GetDescription(),
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

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = query
                        .Select(x => new GetTextbookPreparationApprovalResult
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
                            IsEnableEdit = DisabledButton(x.Status, ApproverTo, IsPic, isEdit, CountApproval),
                            IsEnableDelete = DisabledButton(x.Status, ApproverTo, IsPic, isDelete, CountApproval),
                            IsEnableApproval = ApproverTo == 1 && x.Status == TextbookPreparationStatus.OnReview1.GetDescription()
                                ? true
                                : ApproverTo == 2 && x.Status == TextbookPreparationStatus.OnReview2.GetDescription()
                                    ? true
                                    : ApproverTo == 3 && x.Status == TextbookPreparationStatus.OnReview3.GetDescription()
                                        ? true
                                        : false,
                            IsShowEdit = IsPic 
                                            ? true 
                                            : GetApproval.Count()== ApproverTo
                                                ? isEdit
                                                : false,
                            IsShowDelete = IsPic
                                            ? true
                                            : GetApproval.Count() == ApproverTo
                                                ? isDelete
                                                : false,
                        })
                        .ToList();
            }
            else
            {
                items = query
                        .SetPagination(param)
                       .Select(x => new GetTextbookPreparationApprovalResult
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
                           IsEnableEdit = DisabledButton(x.Status, ApproverTo, IsPic, isEdit, CountApproval),
                           IsEnableDelete = DisabledButton(x.Status, ApproverTo, IsPic, isDelete, CountApproval),
                           IsEnableApproval = ApproverTo == 1 && x.Status == TextbookPreparationStatus.OnReview1.GetDescription()
                                ? true
                                : ApproverTo == 2 && x.Status == TextbookPreparationStatus.OnReview2.GetDescription()
                                    ? true
                                    : ApproverTo == 3 && x.Status == TextbookPreparationStatus.OnReview3.GetDescription()
                                        ? true
                                        : false,
                           IsShowEdit = IsPic
                                            ? true
                                            : GetApproval.Count() == ApproverTo
                                                ? isEdit
                                                : false,
                           IsShowDelete = IsPic
                                            ? true
                                            : GetApproval.Count() == ApproverTo
                                                ? isDelete
                                                : false,
                       })
                       .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<TextbookPreparationApprovalRequest, TextbookPreparationApprovalValidator>();

            var GetTextbook = await _dbContext.Entity<TrTextbook>()
                                    .Include(e => e.AcademicYear)
                                    .Where(e => body.Ids.Contains(e.Id))
                                    .ToListAsync(CancellationToken);
            if (!GetTextbook.Any())
                throw new BadRequestException("Textbook is not found");

            var IdSchool = GetTextbook.Select(e => e.AcademicYear.IdSchool).FirstOrDefault();
            var GetUserApproval = await _dbContext.Entity<MsTextbookSettingApproval>()
                                    .Where(e => e.IdSchool == IdSchool)
                                    .ToListAsync(CancellationToken);
            var idUserApproval1 = GetUserApproval.Where(e => e.ApproverTo == 1).Select(e => e.IdBinusian).FirstOrDefault();
            var idUserApproval2 = GetUserApproval.Where(e => e.ApproverTo == 2).Select(e => e.IdBinusian).FirstOrDefault();
            var idUserApproval3 = GetUserApproval.Where(e => e.ApproverTo == 3).Select(e => e.IdBinusian).FirstOrDefault();

            var MaxApproval = GetUserApproval.Select(e => e.ApproverTo).Max();

            var GetUserApprovalByUser = GetUserApproval
                                        .Where(e => e.IdBinusian == body.IdUser
                                                    && e.IdSchool == IdSchool)
                                        .FirstOrDefault();

            if (GetUserApprovalByUser == null)
                throw new BadRequestException($"You are not approver ");

            foreach (var item in GetTextbook)
            {
                if (body.IsApproved)
                {
                    if (GetUserApprovalByUser.ApproverTo == 1)
                    {
                        item.IdBinusianApproval1 = body.IdUser;
                        item.ApprovalNote1 = body.Note;
                        item.IsApproval1 = true;
                        
                        item.Status = MaxApproval == GetUserApprovalByUser.ApproverTo
                                                ? TextbookPreparationStatus.Approved
                                                : TextbookPreparationStatus.OnReview2;
                    }
                    if (GetUserApprovalByUser.ApproverTo == 2)
                    {
                        item.IdBinusianApproval2 = body.IdUser;
                        item.ApprovalNote2 = body.Note;
                        item.IsApproval2 = true;
                        item.IdBinusianApproval3 = GetUserApproval.Where(e => e.ApproverTo == 3).Select(e => e.IdBinusian).FirstOrDefault();
                        item.Status = MaxApproval == GetUserApprovalByUser.ApproverTo
                                                ? TextbookPreparationStatus.Approved
                                                : TextbookPreparationStatus.OnReview3;
                    }
                    if (GetUserApprovalByUser.ApproverTo == 3)
                    {
                        item.IdBinusianApproval3 = body.IdUser;
                        item.ApprovalNote3 = body.Note;
                        item.IsApproval3 = true;
                        item.Status = TextbookPreparationStatus.Approved;
                    }
                }
                else
                {
                    if (GetUserApprovalByUser.ApproverTo == 1)
                    {
                        item.IdBinusianApproval1 = body.IdUser;
                        item.ApprovalNote1 = body.Note;
                        item.IsApproval1 = true;
                        item.Status = TextbookPreparationStatus.Declined;
                    }
                    if (GetUserApprovalByUser.ApproverTo == 2)
                    {
                        item.IdBinusianApproval2 = body.IdUser;
                        item.ApprovalNote2 = body.Note;
                        item.IsApproval2 = true;
                        item.Status = TextbookPreparationStatus.Declined;
                    }
                    if (GetUserApprovalByUser.ApproverTo == 3)
                    {
                        item.IdBinusianApproval3 = body.IdUser;
                        item.ApprovalNote3 = body.Note;
                        item.IsApproval3 = true;
                        item.Status = TextbookPreparationStatus.Declined;
                    }
                }

                _dbContext.Entity<TrTextbook>().Update(item);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region send email
            var GetPic = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                            .Where(e => e.TextbookUserPeriod.IdAcademicYear == GetTextbook.Select(e => e.IdAcademicYear).FirstOrDefault() && e.TextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookPic)
                            .Select(e => e.IdBinusian)
                            .ToListAsync(CancellationToken);

            //List<string> ids = new List<string>()
            //{
            //    "5170e328-0b39-46f2-9aeb-491d631ce7be",
            //    "5657e1aa-3239-4932-9cf1-7f01fb1ee74d",
            //    "f6fa9c82-f6aa-4323-871a-98fb619172ad"
            //};

            var GetEmailTextbook = await _dbContext.Entity<TrTextbook>()
                                .Include(e => e.Subject).ThenInclude(e => e.Grade)
                                .Include(e => e.StaffCreate)
                                .Where(e => GetTextbook.Select(e => e.Id).ToList().Contains(e.Id))
                                //.Where(e => ids.Contains(e.Id))
                                .Select(e => new
                                {
                                    Id = e.Id,
                                    IdUser = e.IdBinusianApproval2,
                                    NameCreated = (!string.IsNullOrEmpty(e.StaffCreate.FirstName) ? e.StaffCreate.FirstName : "")
                                                    + (!string.IsNullOrEmpty(e.StaffCreate.LastName) ? " " + e.StaffCreate.LastName : ""),
                                    IdUserCreated = e.IdBinusianCreated,
                                    Subject = e.Subject.Description,
                                    Grade = e.Subject.Grade.Code,
                                    Author = e.Author,
                                    Title = e.Title,
                                    Isbn = e.ISBN,
                                    Note = e.Note,
                                    Status = e.Status.GetDescription(),
                                    IdApproval2 = e.IdBinusianApproval2,
                                    IdApproval3 = e.IdBinusianApproval3,
                                    NamaApproval2 = (!string.IsNullOrEmpty(e.StaffApproval2.FirstName) ? e.StaffApproval2.FirstName : "")
                                                    + (!string.IsNullOrEmpty(e.StaffApproval2.LastName) ? " " + e.StaffApproval2.LastName : ""),
                                    NamaApproval3 = (!string.IsNullOrEmpty(e.StaffApproval3.FirstName) ? e.StaffApproval3.FirstName : "")
                                                    + (!string.IsNullOrEmpty(e.StaffApproval3.LastName) ? " " + e.StaffApproval3.LastName : "")
                                })
                                .ToListAsync(CancellationToken);

            if (GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.OnReview2.GetDescription()).Any())
            {
                var EmailTextbookApproval = new GetEmailTextbookResult
                {
                    NameCreated = GetEmailTextbook.Select(e => e.NameCreated).FirstOrDefault(),
                    Textbooks = GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.OnReview2.GetDescription()).Select(e => new GetEmailTextbook
                    {
                        Id = e.Id,
                        Subject = e.Subject,
                        Grade = e.Grade,
                        Author = e.Author,
                        Title = e.Title,
                        Isbn = e.Isbn,
                        Note = e.Note,
                        Status = e.Status,
                        IdUserApprover = e.Status==TextbookPreparationStatus.OnReview2.GetDescription()
                                        ? idUserApproval2
                                        : e.Status == TextbookPreparationStatus.OnReview3.GetDescription()
                                            ? idUserApproval3
                                            : null
                    }).ToList()
                };

                if (KeyValues.ContainsKey("EmailTextbookApproval"))
                {
                    KeyValues.Remove("EmailTextbookApproval");
                }
                KeyValues.Add("EmailTextbookApproval", EmailTextbookApproval);
                var Notification = TP7Notification(KeyValues, AuthInfo);
            }

            if (GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.OnReview3.GetDescription()).Any())
            {
                var EmailTextbookApproval = new GetEmailTextbookResult
                {
                    NameCreated = GetEmailTextbook.Select(e => e.NameCreated).FirstOrDefault(),
                    Textbooks = GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.OnReview3.GetDescription()).Select(e => new GetEmailTextbook
                    {
                        Id = e.Id,
                        Subject = e.Subject,
                        Grade = e.Grade,
                        Author = e.Author,
                        Title = e.Title,
                        Isbn = e.Isbn,
                        Note = e.Note,
                        Status = e.Status,
                        IdUserApprover = e.Status == TextbookPreparationStatus.OnReview2.GetDescription()
                                        ? e.IdApproval2
                                        : e.Status == TextbookPreparationStatus.OnReview3.GetDescription()
                                            ? e.IdApproval3
                                            : null,
                        NamaApprover = e.Status == TextbookPreparationStatus.OnReview2.GetDescription()
                                        ? e.NamaApproval2
                                        : e.Status == TextbookPreparationStatus.OnReview3.GetDescription()
                                            ? e.NamaApproval3
                                            : null
                    }).ToList()
                };

                if (KeyValues.ContainsKey("EmailTextbookApproval"))
                {
                    KeyValues.Remove("EmailTextbookApproval");
                }
                KeyValues.Add("EmailTextbookApproval", EmailTextbookApproval);
                var Notification = TP7Notification(KeyValues, AuthInfo);
            }

            if (GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.Approved.GetDescription()).Any())
            {
                var EmailTextbook = new GetEmailTextbookResult
                {
                    IdUserPic = GetPic,
                    NameCreated = GetEmailTextbook.Select(e => e.NameCreated).FirstOrDefault(),
                    Textbooks = GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.Approved.GetDescription()).Select(e => new GetEmailTextbook
                    {
                        Id = e.Id,
                        Subject = e.Subject,
                        Grade = e.Grade,
                        Author = e.Author,
                        Title = e.Title,
                        Isbn = e.Isbn,
                        Note = e.Note,
                        Status = e.Status,
                        IdUserApprover = e.Status == TextbookPreparationStatus.OnReview2.GetDescription()
                                        ? idUserApproval2
                                        : e.Status == TextbookPreparationStatus.OnReview3.GetDescription()
                                            ? idUserApproval3
                                            : null
                    }).ToList()
                };

                if (KeyValues.ContainsKey("EmailTextbook"))
                {
                    KeyValues.Remove("EmailTextbook");
                }
                KeyValues.Add("EmailTextbook", EmailTextbook);
                var Notification = TP9Notification(KeyValues, AuthInfo);
            }

            if (GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.Declined.GetDescription()).Any())
            {
                var EmailTextbookApproval = new GetEmailTextbookApprovalResult
                {
                    IdUserPic = GetPic,
                    NameCreated = GetEmailTextbook.Select(e => e.NameCreated).FirstOrDefault(),
                    Textbooks = GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.Declined.GetDescription()).Select(e => new GetEmailTextbook
                    {
                        Id = e.Id,
                        Subject = e.Subject,
                        Grade = e.Grade,
                        Author = e.Author,
                        Title = e.Title,
                        Isbn = e.Isbn,
                        Note = e.Note,
                        Status = e.Status,
                        IdUserCreated = e.IdUserCreated,
                    }).ToList()
                };

                if (KeyValues.ContainsKey("EmailTextbookApproval"))
                {
                    KeyValues.Remove("EmailTextbookApproval");
                }
                KeyValues.Add("EmailTextbookApproval", EmailTextbookApproval);
                var Notification = TP8Notification(KeyValues, AuthInfo);
            }
            #endregion

            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }

        public static bool DisabledButton(string Status, int ApproverTo, bool IsPic, bool IsEnableButton, int CountApproval)
        {
            var IsEnable = false;

            if (IsPic)
            {
                IsEnable = true;
            }
            else if(CountApproval == 1 && Status == TextbookPreparationStatus.OnReview1.GetDescription())
            {
                IsEnable = true;
            }
            else
            {
                if (ApproverTo == 1 && Status == TextbookPreparationStatus.OnReview1.GetDescription())
                {
                    IsEnable = IsEnableButton;
                }
                if (ApproverTo == 2 && Status == TextbookPreparationStatus.OnReview2.GetDescription())
                {
                    IsEnable = IsEnableButton;
                }
                if (ApproverTo == 3 && Status == TextbookPreparationStatus.OnReview3.GetDescription())
                {
                    IsEnable = IsEnableButton;
                }
            }

            return IsEnable;
        }

        public static string TP7Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookApproval").Value;
            var EmailTextbookApproval = JsonConvert.DeserializeObject<GetEmailTextbookResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP7")
                {
                    IdRecipients = EmailTextbookApproval.Textbooks.Select(e => e.IdUserApprover).ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string TP8Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookApproval").Value;
            var EmailTextbookApproval = JsonConvert.DeserializeObject<GetEmailTextbookApprovalResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP8")
                {
                    IdRecipients = EmailTextbookApproval.Textbooks.Select(e => e.IdUserCreated).ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string TP9Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbook").Value;
            var EmailTextbook = JsonConvert.DeserializeObject<GetEmailTextbookResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP9")
                {
                    IdRecipients = EmailTextbook.IdUserPic,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
