using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApprovalSetting;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationUserPeriod;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using BinusSchool.School.FnSchool.TextbookPreparationUserPeriod.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.School.FnSchool.TextbookPreparationUserPeriod
{
    public class TextbookPreparationUserPeriodHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public TextbookPreparationUserPeriodHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetTextbookUserPeriodDetail = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                                  .Where(e => ids.Contains(e.IdTextbookUserPeriod))
                                  .ToListAsync(CancellationToken);
            if (!GetTextbookUserPeriodDetail.Any())
                throw new BadRequestException("Textbook user period is not found");

            GetTextbookUserPeriodDetail.ForEach(e => e.IsActive = false);
            _dbContext.Entity<MsTextbookUserPeriodDetail>().UpdateRange(GetTextbookUserPeriodDetail);

            var GetTextbookUserPeriod = await _dbContext.Entity<MsTextbookUserPeriod>()
                                  .Where(e => ids.Contains(e.Id))
                                  .ToListAsync(CancellationToken);
            if (!GetTextbookUserPeriod.Any())
                throw new BadRequestException("Textbook user period is not found");

            GetTextbookUserPeriod.ForEach(e => e.IsActive = false);
            _dbContext.Entity<MsTextbookUserPeriod>().UpdateRange(GetTextbookUserPeriod);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var item = await _dbContext.Entity<MsTextbookUserPeriod>()
                        .Include(e=> e.TextbookUserPeriodDetails).ThenInclude(e=>e.Role)
                        .Include(e=> e.TextbookUserPeriodDetails).ThenInclude(e=>e.TeacherPosition)
                        .Include(e=> e.AcademicYear)
                        .Include(e=> e.AcademicYear)
                        .Where(e=>e.Id==id)
                        .Select(e => new DetailTextbookPreparationUserPeriodResult
                        {
                            Id = e.Id,
                            AcademicYear = new NameValueVm
                            {
                                Id = e.IdAcademicYear,
                                Name = e.AcademicYear.Description
                            },
                            GroupName = e.GroupName,
                            AssignAs = e.AssignAs.GetDescription(),
                            OpenDate = e.StartDate,
                            CloseDate = e.EndDate,
                            Users = e.TextbookUserPeriodDetails.Select(f=>new TextbookPreparationUser
                            {
                                IdBinusian = f.IdBinusian,
                                StaffName = (!string.IsNullOrEmpty(f.Staff.FirstName) ? f.Staff.FirstName : "") 
                                            + (!string.IsNullOrEmpty(f.Staff.LastName) ? " " + f.Staff.LastName : ""),
                                IdRole = f.IdRole,
                                Role = f.Role.Description,
                                IdPosition = f.IdTeacherPosition,
                                Position = f.TeacherPosition.Description
                            }).ToList()
                        })
                        .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(item as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetTextbookPreparationUserPeriodRequest>();
            string[] _columns = { "AcademicYear", "GroupName", "AssignAs", "OpeningDate", "ClosedDate" };

            var predicate = PredicateBuilder.Create<MsTextbookUserPeriod>(x => true);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            //serach
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.GroupName.Contains(param.Search));

            var GetTextbookUserPeriod = await _dbContext.Entity<MsTextbookUserPeriod>()
                .Include(e => e.TextbookUserPeriodDetails)
                .Include(e => e.AcademicYear)
                .Where(predicate)
               .Select(x => new
               {
                   Id = x.Id,
                   AcademicYear = x.AcademicYear.Description,
                   GroupName = x.GroupName,
                   AssignAs = x.AssignAs.GetDescription(),
                   OpeningDate = x.StartDate,
                   ClosedDate = x.EndDate,
               }).ToListAsync(CancellationToken);

            var query = GetTextbookUserPeriod.Distinct();

            if (!string.IsNullOrEmpty(param.AssignAs))
                query = query.Where(x => x.AssignAs == param.AssignAs);

            //orderBy
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "GroupName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.GroupName)
                        : query.OrderBy(x => x.GroupName);
                    break;
                case "AssignAs":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AssignAs)
                        : query.OrderBy(x => x.AssignAs);
                    break;
                case "OpeningDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.OpeningDate)
                        : query.OrderBy(x => x.OpeningDate);
                    break;
                case "ClosedDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClosedDate)
                        : query.OrderBy(x => x.ClosedDate);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = query
                    .Select(x => new GetTextbookPreparationUserPeriodResult
                    {
                        Id = x.Id,
                        AcademicYear = x.AcademicYear,
                        GroupName = x.GroupName,
                        AssignAs = x.AssignAs,
                        OpeningDate = x.OpeningDate == null ? "-" : Convert.ToDateTime(x.OpeningDate).ToString("dd MMM yyyy"),
                        ClosedDate = x.ClosedDate == null ? "-" : Convert.ToDateTime(x.ClosedDate).ToString("dd MMM yyyy"),
                    })
                    .ToList();
            else
                items = query
                    .SetPagination(param)
                   .Select(x => new GetTextbookPreparationUserPeriodResult
                   {
                       Id = x.Id,
                       AcademicYear = x.AcademicYear,
                       GroupName = x.GroupName,
                       AssignAs = x.AssignAs,
                       OpeningDate = x.OpeningDate == null ? "-" : Convert.ToDateTime(x.OpeningDate).ToString("dd MMM yyyy"),
                       ClosedDate = x.ClosedDate == null ? "-" : Convert.ToDateTime(x.ClosedDate).ToString("dd MMM yyyy"),
                   })
                    .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddTextbookPreparationUserPeriodRequest, AddTextbookPreparationUserPeriodValidator>();

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => e.Id == body.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);
            if (GetAcademicYear == null)
                throw new BadRequestException("Academic year with Id " + body.IdAcademicYear + " is not found");

            if(body.AssignAs== TextBookPreparationUserPeriodAssignAs.TextbookPic)
            {
                var ExsisTextbookPic = await _dbContext.Entity<MsTextbookUserPeriod>()
                                    .Where(e => e.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookPic && e.AcademicYear.IdSchool== GetAcademicYear.IdSchool)
                                    .AnyAsync(CancellationToken);

                if (ExsisTextbookPic)
                    throw new BadRequestException("Textbook PIC is exsis");
            }

            var GetRole = await _dbContext.Entity<LtRole>()
                            .Where(e => body.UserStaff.Select(x => x.IdRole).ToList().Contains(e.Id))
                            .ToListAsync(CancellationToken);

            var GetTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                        .Where(e => body.UserStaff.Select(x => x.IdPosition).ToList().Contains(e.Id))
                                        .ToListAsync(CancellationToken);

            var GetUser = await _dbContext.Entity<MsUser>()
                                       .Where(e => body.UserStaff.Select(x => x.IdUser).ToList().Contains(e.Id))
                                       .ToListAsync(CancellationToken);

            var NewTextbookUserPeriod = new MsTextbookUserPeriod
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                GroupName = body.GroupName,
                AssignAs = body.AssignAs,
                StartDate = body.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry ? body.StartDate : (DateTime?)null,
                EndDate = body.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry ? body.EndDate : (DateTime?)null,
            };
            _dbContext.Entity<MsTextbookUserPeriod>().Add(NewTextbookUserPeriod);

            foreach (var item in body.UserStaff)
            {
                var GetRoleById = GetRole.Where(e => e.Id == item.IdRole).FirstOrDefault();
                if (GetRoleById == null)
                    continue;

                var GetTeacherPositionById = GetTeacherPosition.Where(e => e.Id == item.IdPosition).FirstOrDefault();
                if (GetTeacherPositionById == null)
                    continue;

                var GetUserById = GetUser.Where(e => e.Id == item.IdUser).FirstOrDefault();
                if (GetUserById == null)
                    continue;

                var CheckUserTextbookPeriod = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                                                .Include(e => e.TextbookUserPeriod)
                                                .Where(e => e.IdTextbookUserPeriod != NewTextbookUserPeriod.Id 
                                                    && e.IdBinusian == item.IdUser 
                                                    && e.TextbookUserPeriod.AssignAs== TextBookPreparationUserPeriodAssignAs.TextbookEntry)
                                                .FirstOrDefaultAsync(CancellationToken);
                if (CheckUserTextbookPeriod != null)
                    throw new BadRequestException(string.Format("User ID Binusian {0} is exist on other subject group", CheckUserTextbookPeriod.IdBinusian));

                var CheckUserRolePositionTextbookPeriod = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                                                .Include(e => e.TextbookUserPeriod)
                                                .Where(e => e.IdTextbookUserPeriod == NewTextbookUserPeriod.Id && e.IdBinusian == item.IdUser && e.IdRole == item.IdRole)
                                                .FirstOrDefaultAsync(CancellationToken);
                if (CheckUserRolePositionTextbookPeriod != null)
                    throw new BadRequestException(string.Format("User ID Binusian {0} is exist or cannot multiple role", CheckUserTextbookPeriod.IdBinusian));

                var NewTextbookUserPeriodDetail = new MsTextbookUserPeriodDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdRole = item.IdRole,
                    IdTeacherPosition = item.IdPosition,
                    IdBinusian = item.IdUser,
                    IdTextbookUserPeriod = NewTextbookUserPeriod.Id
                };
                _dbContext.Entity<MsTextbookUserPeriodDetail>().Add(NewTextbookUserPeriodDetail);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region send email
            var EmailTextbookUserPeriod = await (from TextbookUserPeriod in _dbContext.Entity<MsTextbookUserPeriod>()
                                                 join User in _dbContext.Entity<MsUser>() on TextbookUserPeriod.UserIn equals User.Id
                                                 where TextbookUserPeriod.Id == NewTextbookUserPeriod.Id
                                                 //where TextbookUserPeriod.Id == "1d59fd29-2bfc-449f-b410-30b348150a1f"
                                                 select new EmailAddTextbookUserPeriodResult
                                                 {
                                                     Id = TextbookUserPeriod.Id,
                                                     NamaAdministrator = User.DisplayName,
                                                     IdUserEntry = TextbookUserPeriod.TextbookUserPeriodDetails.Select(e => e.IdBinusian).ToList(),
                                                     AssignAs = TextbookUserPeriod.AssignAs
                                                 }).FirstOrDefaultAsync(CancellationToken);

            if(EmailTextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry)
            {
                if (KeyValues.ContainsKey("EmailTextbookUserPeriod"))
                {
                    KeyValues.Remove("EmailTextbookUserPeriod");
                }

                KeyValues.Add("EmailTextbookUserPeriod", EmailTextbookUserPeriod);
                var Notification = TP1Notification(KeyValues, AuthInfo);
            }
            else if (EmailTextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookPic)
            {
                if (KeyValues.ContainsKey("EmailTextbookUserPeriod"))
                {
                    KeyValues.Remove("EmailTextbookUserPeriod");
                }

                KeyValues.Add("EmailTextbookUserPeriod", EmailTextbookUserPeriod);
                var Notification = TP2Notification(KeyValues, AuthInfo);
            }
            #endregion

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateTextbookPreparationUserPeriodRequest, UpdateTextbookPreparationUserPeriodValidator>();
            List<string> UserNew = new List<string>();
            List<string> UserRemove = new List<string>();
            var UpdateDate = false;
            DateTime StartDate = default(DateTime);
            DateTime EndDate = default(DateTime);

            var GetTextbookUserPeriod = await _dbContext.Entity<MsTextbookUserPeriod>()
                                    .Include(e=>e.TextbookUserPeriodDetails)
                                    .Where(e => e.Id == body.Id)
                                    .FirstOrDefaultAsync(CancellationToken);
            if (GetTextbookUserPeriod == null)
                throw new BadRequestException("Textbook user period with Id "+body.Id+" is not found");

            //update date email
            if (GetTextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry)
            {
                UpdateDate = Convert.ToDateTime(GetTextbookUserPeriod.StartDate).Date == Convert.ToDateTime(body.StartDate).Date
                                && Convert.ToDateTime(GetTextbookUserPeriod.EndDate).Date == Convert.ToDateTime(body.EndDate).Date
                                ? false
                                : true;

                StartDate = Convert.ToDateTime(GetTextbookUserPeriod.StartDate);
                EndDate = Convert.ToDateTime(GetTextbookUserPeriod.EndDate);
            }

            GetTextbookUserPeriod.GroupName = body.GroupName;
            GetTextbookUserPeriod.StartDate = GetTextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry ? body.StartDate : (DateTime?)null;
            GetTextbookUserPeriod.EndDate = GetTextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry ? body.EndDate : (DateTime?)null;
            _dbContext.Entity<MsTextbookUserPeriod>().Update(GetTextbookUserPeriod);

            var GetTextbookUserPeriodDetail = GetTextbookUserPeriod.TextbookUserPeriodDetails.ToList();
            GetTextbookUserPeriodDetail.ForEach(e => e.IsActive = false);
            _dbContext.Entity<MsTextbookUserPeriodDetail>().UpdateRange(GetTextbookUserPeriodDetail);

            var GetRole = await _dbContext.Entity<LtRole>()
                            .Where(e => body.UserStaff.Select(x => x.IdRole).ToList().Contains(e.Id))
                            .ToListAsync(CancellationToken);

            var GetTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                        .Where(e => body.UserStaff.Select(x => x.IdPosition).ToList().Contains(e.Id))
                                        .ToListAsync(CancellationToken);

            var GetUser = await _dbContext.Entity<MsUser>()
                                       .Where(e => body.UserStaff.Select(x => x.IdUser).ToList().Contains(e.Id))
                                       .ToListAsync(CancellationToken);

            //Delete User email
            foreach(var item in GetTextbookUserPeriodDetail)
            {
                var ExsisUser = body.UserStaff.Where(e => e.IdUser == item.IdBinusian).Any();

                if (!ExsisUser)
                    UserRemove.Add(item.IdBinusian);
            }

           

            foreach (var item in body.UserStaff)
            {
                //New User email
                var ExsisUser = GetTextbookUserPeriodDetail.Where(e => e.IdBinusian == item.IdUser).Any();
                if (!ExsisUser)
                    UserNew.Add(item.IdUser);

                var GetRoleById = GetRole.Where(e => e.Id == item.IdRole).FirstOrDefault();
                if (GetRoleById == null)
                    continue;

                var GetTeacherPositionById = GetTeacherPosition.Where(e => e.Id == item.IdPosition).FirstOrDefault();
                if (GetTeacherPositionById == null)
                    continue;

                var GetUserById = GetUser.Where(e => e.Id == item.IdUser).FirstOrDefault();
                if (GetUserById == null)
                    continue;

                var CheckUserTextbookPeriod = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                                                .Include(e => e.TextbookUserPeriod)
                                                .Where(e => e.IdTextbookUserPeriod != GetTextbookUserPeriod.Id 
                                                    && e.IdBinusian == item.IdUser 
                                                    && e.TextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry)
                                                .FirstOrDefaultAsync(CancellationToken);
                if (CheckUserTextbookPeriod != null)
                    throw new BadRequestException(string.Format("User ID Binusian {0} is exist on other subject group", CheckUserTextbookPeriod.IdBinusian));

                var NewTextbookUserPeriodDetail = new MsTextbookUserPeriodDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdRole = item.IdRole,
                    IdTeacherPosition = item.IdPosition,
                    IdBinusian = item.IdUser,
                    IdTextbookUserPeriod = GetTextbookUserPeriod.Id
                };
                _dbContext.Entity<MsTextbookUserPeriodDetail>().Add(NewTextbookUserPeriodDetail);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region send email
            var EmailTextbookUserPeriod = await (from TextbookUserPeriod in _dbContext.Entity<MsTextbookUserPeriod>()
                                                 join User in _dbContext.Entity<MsUser>() on TextbookUserPeriod.UserIn equals User.Id
                                                 where TextbookUserPeriod.Id == GetTextbookUserPeriod.Id
                                                 //where TextbookUserPeriod.Id == "1d59fd29-2bfc-449f-b410-30b348150a1f"
                                                 select new EmailUpdateTextbookUserPeriodResult
                                                 {
                                                     Id = TextbookUserPeriod.Id,
                                                     NamaAdministrator = User.DisplayName,
                                                     IdUserEntry = TextbookUserPeriod.TextbookUserPeriodDetails.Select(e => e.IdBinusian).ToList(),
                                                     AssignAsString = TextbookUserPeriod.AssignAs.GetDescription(),
                                                     AssignAs = TextbookUserPeriod.AssignAs,
                                                     DateStartOld = StartDate.ToString("dd MMM yyyy"),
                                                     DateEndOld = EndDate.ToString("dd MMM yyyy"),
                                                     DateStartNew = Convert.ToDateTime(TextbookUserPeriod.StartDate).ToString("dd MMM yyyy"),
                                                     DateEndNew = Convert.ToDateTime(TextbookUserPeriod.EndDate).ToString("dd MMM yyyy"),
                                                 }).FirstOrDefaultAsync(CancellationToken);

            if (UpdateDate)
            {
                if (KeyValues.ContainsKey("EmailTextbookUserPeriod"))
                {
                    KeyValues.Remove("EmailTextbookUserPeriod");
                }

                KeyValues.Add("EmailTextbookUserPeriod", EmailTextbookUserPeriod);
                var Notification = TP3Notification(KeyValues, AuthInfo);
            }

            if (UserRemove.Any())
            {
                EmailTextbookUserPeriod.IdUserEntry = UserRemove;

                if (KeyValues.ContainsKey("EmailTextbookUserPeriod"))
                {
                    KeyValues.Remove("EmailTextbookUserPeriod");
                }

                KeyValues.Add("EmailTextbookUserPeriod", EmailTextbookUserPeriod);
                var Notification = TP4Notification(KeyValues, AuthInfo);
            }

            if (UserNew.Any())
            {
                EmailTextbookUserPeriod.IdUserEntry = UserNew;

                if (EmailTextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookEntry)
                {
                    if (KeyValues.ContainsKey("EmailTextbookUserPeriod"))
                    {
                        KeyValues.Remove("EmailTextbookUserPeriod");
                    }

                    KeyValues.Add("EmailTextbookUserPeriod", EmailTextbookUserPeriod);
                    var Notification = TP1Notification(KeyValues, AuthInfo);
                }
                else if (EmailTextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookPic)
                {
                    if (KeyValues.ContainsKey("EmailTextbookUserPeriod"))
                    {
                        KeyValues.Remove("EmailTextbookUserPeriod");
                    }

                    KeyValues.Add("EmailTextbookUserPeriod", EmailTextbookUserPeriod);
                    var Notification = TP2Notification(KeyValues, AuthInfo);
                }
            }
            #endregion
            return Request.CreateApiResult2();
        }

        public static string TP1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookUserPeriod").Value;
            var EmailTextbookUserPeriod = JsonConvert.DeserializeObject<EmailAddTextbookUserPeriodResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP1")
                {
                    IdRecipients = EmailTextbookUserPeriod.IdUserEntry,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string TP2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookUserPeriod").Value;
            var EmailTextbookUserPeriod = JsonConvert.DeserializeObject<EmailAddTextbookUserPeriodResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP2")
                {
                    IdRecipients = EmailTextbookUserPeriod.IdUserEntry,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string TP3Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookUserPeriod").Value;
            var EmailTextbookUserPeriod = JsonConvert.DeserializeObject<EmailUpdateTextbookUserPeriodResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP3")
                {
                    IdRecipients = EmailTextbookUserPeriod.IdUserEntry,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string TP4Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbookUserPeriod").Value;
            var EmailTextbookUserPeriod = JsonConvert.DeserializeObject<EmailUpdateTextbookUserPeriodResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP4")
                {
                    IdRecipients = EmailTextbookUserPeriod.IdUserEntry,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
