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
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparation;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using BinusSchool.School.FnSchool.TextbookPreparation.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.School.FnSchool.TextbookPreparation
{
    public class TextbookPreparationHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public TextbookPreparationHandler(ISchoolDbContext schoolDbContext, IMachineDateTime Datetime)
        {
            _dbContext = schoolDbContext;
            _dateTime = Datetime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetTextbookAttachment = await _dbContext.Entity<TrTextbookAttachment>()
                                  .Where(e => ids.Contains(e.IdTextbook))
                                  .ToListAsync(CancellationToken);
            
            GetTextbookAttachment.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrTextbookAttachment>().UpdateRange(GetTextbookAttachment);

            var GetTextbook = await _dbContext.Entity<TrTextbook>()
                                  .Where(e => ids.Contains(e.Id))
                                  .ToListAsync(CancellationToken);
            if (!GetTextbook.Any())
                throw new BadRequestException("Textbook Attachment is not found");

            GetTextbook.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrTextbook>().UpdateRange(GetTextbook);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var item = await _dbContext.Entity<TrTextbook>()
                        .Include(e => e.Subject).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                        .Include(e => e.StaffCreate)
                        .Include(e => e.AcademicYear)
                        .Include(e => e.TextbookSubjectGroup)
                        .Include(e => e.Pathway)
                        .Include(e => e.StaffApproval1)
                        .Include(e => e.StaffApproval2)
                        .Include(e => e.StaffApproval3)
                        .Where(e => e.Id == id)
                        .Select(e => new DetailTextbookPreparationResult
                        {
                            Id = e.Id,
                            UserCreate = new NameValueVm
                            {
                                Id = e.StaffCreate.IdBinusian,
                                Name = (!string.IsNullOrEmpty(e.StaffCreate.FirstName) ? e.StaffCreate.FirstName : "")
                                        + (!string.IsNullOrEmpty(e.StaffCreate.LastName) ? " " + e.StaffCreate.LastName : "")
                            },
                            AcademicYear = new NameValueVm
                            {
                                Id = e.AcademicYear.Id,
                                Name = e.AcademicYear.Description,
                            },
                            Level = new NameValueVm
                            {
                                Id = e.Subject.Grade.Level.Id,
                                Name = e.Subject.Grade.Level.Description,
                            },
                            Grade = new NameValueVm
                            {
                                Id = e.Subject.Grade.Id,
                                Name = e.Subject.Grade.Description,
                            },
                            SubjectGroup = new NameValueVm
                            {
                                Id = e.TextbookSubjectGroup.Id,
                                Name = e.TextbookSubjectGroup.SubjectGroupName,
                            },
                            Subject = new NameValueVm
                            {
                                Id = e.Subject.Id,
                                Name = e.Subject.Description,
                            },
                            Streaming = new NameValueVm
                            {
                                Id = e.Pathway.Id,
                                Name = e.Pathway.Description,
                            },
                            BookType = e.BookType.GetDescription(),
                            ISBN = e.ISBN,
                            Title = e.Title,
                            Author = e.Author,
                            Publisher = e.Publisher,
                            Weight = e.Weight,
                            NoSKU = e.NoSKU,
                            IsRegion = e.IsRegion,
                            IsMandatory = e.IsMandatory,
                            IsCountinuity = e.IsCountinuity,
                            IsAvailableStatus = e.IsAvailableStatus,
                            Supplier = e.Supplier,
                            Location = e.Location,
                            LastModif = e.LastModif,
                            Vendor = e.Vendor,
                            OriginalPrice = e.OriginalPrice,
                            PriceAfterDiscount = e.PriceAfterDiscount,
                            Note = e.Note,
                            Status = e.Status.GetDescription(),
                            NoteApproval = e.IsApproval1 && e.IsApproval2 && e.IsApproval3
                                            ? e.ApprovalNote3
                                            : e.IsApproval1 && e.IsApproval2
                                                ? e.ApprovalNote2
                                                : e.IsApproval1
                                                    ? e.ApprovalNote1
                                                    : null,
                            LastApprovalName = e.IsApproval1 && e.IsApproval2 && e.IsApproval3
                                                ? (!string.IsNullOrEmpty(e.StaffApproval3.FirstName) ? e.StaffApproval3.FirstName : "")
                                                    + (!string.IsNullOrEmpty(e.StaffApproval3.LastName) ? " " + e.StaffApproval3.LastName : "")   
                                                : e.IsApproval1 && e.IsApproval2
                                                    ? (!string.IsNullOrEmpty(e.StaffApproval2.FirstName) ? e.StaffApproval2.FirstName : "")
                                                        + (!string.IsNullOrEmpty(e.StaffApproval2.LastName) ? " " + e.StaffApproval2.LastName : "")
                                                    : e.IsApproval1
                                                        ? (!string.IsNullOrEmpty(e.StaffApproval1.FirstName) ? e.StaffApproval1.FirstName : "")
                                                            + (!string.IsNullOrEmpty(e.StaffApproval1.LastName) ? " " + e.StaffApproval1.LastName : "")
                                                        : null,
                            LastUpdate = e.DateUp == null ? Convert.ToDateTime(e.DateIn) : Convert.ToDateTime(e.DateUp),
                            MinQty = e.MinQty,
                            MaxQty = e.MaxQty,
                            Attachments = e.TextbookAttachments.Select(x => new DetailTextbookAttachment
                            {
                                Id = x.Id,
                                Url = x.Url,
                                FileName = x.FileName,
                                FileNameOriginal = x.FileNameOriginal,
                                FileSize = x.FileSize,
                                FileType = x.FileType
                            }).ToList()
                        })
                        .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(item as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddTextbookPreparationRequest, AddTextbookPreparationValidator>();

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => e.Id == body.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);
            if (GetAcademicYear == null)
                throw new BadRequestException("Academic year with Id " + body.IdAcademicYear + " is not found");

            var GetSubjectGroup = await _dbContext.Entity<MsTextbookSubjectGroup>()
                                   .Where(e => e.Id == body.IdSubjectGroup)
                                   .FirstOrDefaultAsync(CancellationToken);
            if (GetSubjectGroup == null)
                throw new BadRequestException("Textbook preparation Subject group with Id " + body.IdSubjectGroup + " is not found");

            var GetSubject = await _dbContext.Entity<MsSubject>()
                                   .Where(e => e.Id == body.IdSubject)
                                   .FirstOrDefaultAsync(CancellationToken);
            if (GetSubject == null)
                throw new BadRequestException("Subject with Id " + body.IdSubject + " is not found");

            var GetPathway = await _dbContext.Entity<MsPathway>()
                                   .Where(e => e.Id == body.IdStreaming)
                                   .FirstOrDefaultAsync(CancellationToken);
            if (GetPathway == null)
                throw new BadRequestException("Pathway with Id " + body.IdStreaming + " is not found");

            var GetISBN = await _dbContext.Entity<TrTextbook>()
                                   .Where(e => e.ISBN == body.ISBN)
                                   .FirstOrDefaultAsync(CancellationToken);

            if (GetISBN != null)
                throw new BadRequestException("ISBN " + body.ISBN + " is already exist");

            var GetApprover = await _dbContext.Entity<MsTextbookSettingApproval>()
                                   .Where(e => e.IdSchool == GetAcademicYear.IdSchool)
                                   .ToListAsync(CancellationToken);

            var NewTextbook = new TrTextbook
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                IdBinusianCreated = body.IdUser,
                IdTextbookSubjectGroup = body.IdSubjectGroup,
                IdSubject = body.IdSubject,
                IdPathway = body.IdStreaming,
                BookType = body.BookType,
                ISBN = body.ISBN,
                Title = body.Title,
                Author = body.Author,
                Publisher = body.Publish,
                Weight = body.Weight,
                NoSKU = body.NoSku,
                IsRegion = body.IsRegion,
                IsMandatory = body.IsMandatory,
                IsCountinuity = body.IsContinuity,
                IsAvailableStatus = body.IsAvailableStatus,
                Supplier = body.Supplier,
                Location = body.Location,
                LastModif = body.LastModif,
                Vendor = body.Vendor,
                OriginalPrice = body.OriginalPrice,
                PriceAfterDiscount = body.PriceAfterDiscount,
                Note = body.Note,
                Status = body.IsDraft
                    ? TextbookPreparationStatus.Hold
                    : GetApprover.Any()
                        ? TextbookPreparationStatus.OnReview1
                        : TextbookPreparationStatus.Approved,
                MinQty = body.MinQty,
                MaxQty = body.MaxQty
                //IdUserApproval1 = GetApprover.Where(e => e.ApproverTo == 1).Select(e => e.IdUser).Any() ? GetApprover.Where(e => e.ApproverTo == 1).Select(e => e.IdUser).FirstOrDefault() : null,
                //IdUserApproval2 = GetApprover.Where(e => e.ApproverTo == 2).Select(e => e.IdUser).Any() ? GetApprover.Where(e => e.ApproverTo == 2).Select(e => e.IdUser).FirstOrDefault() : null,
                //IdUserApproval3 = GetApprover.Where(e => e.ApproverTo == 3).Select(e => e.IdUser).Any() ? GetApprover.Where(e => e.ApproverTo == 3).Select(e => e.IdUser).FirstOrDefault() : null,
            };
            _dbContext.Entity<TrTextbook>().Add(NewTextbook);

            foreach (var item in body.Attachments)
            {
                var NewTextbookAttachment = new TrTextbookAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdTextbook = NewTextbook.Id,
                    Url = item.Url,
                    FileName = item.FileName,
                    FileNameOriginal = item.FileNameOriginal,
                    FileSize = item.FileSize,
                    FileType = item.FileType
                };
                _dbContext.Entity<TrTextbookAttachment>().Add(NewTextbookAttachment);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region send email
            var GetPic = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                            .Where(e => e.TextbookUserPeriod.IdAcademicYear == body.IdAcademicYear && e.TextbookUserPeriod.AssignAs== TextBookPreparationUserPeriodAssignAs.TextbookPic)
                            .Select(e=>e.IdBinusian)
                            .ToListAsync(CancellationToken);

            //List<string> ids = new List<string>()
            //{
            //    "5170e328-0b39-46f2-9aeb-491d631ce7be",
            //    "5657e1aa-3239-4932-9cf1-7f01fb1ee74d",
            //    "f6fa9c82-f6aa-4323-871a-98fb619172ad"
            //};

            var GetUserApproval1 = await _dbContext.Entity<MsTextbookSettingApproval>()
                .Where(e => e.IdSchool == GetAcademicYear.IdSchool)
               .FirstOrDefaultAsync(CancellationToken);

            if (GetUserApproval1 == null)
                return Request.CreateApiResult2();

            var GetEmailTextbook = await _dbContext.Entity<TrTextbook>()
                                .Include(e=>e.Subject).ThenInclude(e=>e.Grade)
                                .Include(e=>e.StaffCreate)
                                .Where(e => e.Id == NewTextbook.Id)
                                //.Where(e => ids.Contains(e.Id))
                                .Select(e=> new
                                {
                                    Id = e.Id,
                                    IdUserApproval1 = GetUserApproval1.IdBinusian,
                                    NameCreated = NameUtil.GenerateFullName(e.StaffCreate.FirstName, e.StaffCreate.LastName),
                                    Subject = e.Subject.Description,
                                    Grade = e.Subject.Grade.Code,
                                    Author = e.Author,
                                    Title = e.Title,
                                    Isbn = e.ISBN,
                                    Note = e.Note,
                                    Status = e.Status.GetDescription(),
                                })
                                .ToListAsync(CancellationToken);

            if (GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.OnReview1.GetDescription()).Any())
            {
                var EmailTextbook = new GetEmailTextbookResult
                {
                    NameCreated = GetEmailTextbook.Select(e => e.NameCreated).FirstOrDefault(),
                    Textbooks = GetEmailTextbook.Where(e=>e.Status== TextbookPreparationStatus.OnReview1.GetDescription()).Select(e => new GetEmailTextbook
                    {
                        Id = e.Id,
                        Subject = e.Subject,
                        Grade = e.Grade,
                        Author = e.Author,
                        Title = e.Title,
                        Isbn = e.Isbn,
                        Note = e.Note,
                        Status = e.Status,
                        IdUserApproval = e.IdUserApproval1
                    }).ToList()
                };

                if (KeyValues.ContainsKey("EmailTextbook"))
                {
                    KeyValues.Remove("EmailTextbook");
                }
                KeyValues.Add("EmailTextbook", EmailTextbook);
                var Notification = TP6Notification(KeyValues, AuthInfo);
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
                    }).ToList()
                };

                if (KeyValues.ContainsKey("EmailTextbook"))
                {
                    KeyValues.Remove("EmailTextbook");
                }
                KeyValues.Add("EmailTextbook", EmailTextbook);
                var Notification = TP9Notification(KeyValues, AuthInfo);
            }

            #endregion
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateTextbookPreparationRequest, UpdateTextbookPreparationValidator>();

            var GetTextbook = await _dbContext.Entity<TrTextbook>()
                                    .Where(e => e.Id == body.Id)
                                    .FirstOrDefaultAsync(CancellationToken);
            if (GetTextbook == null)
                throw new BadRequestException("Textbook with Id " + body.Id + " is not found");

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => e.Id == GetTextbook.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);

            var GetSubjectGroup = await _dbContext.Entity<MsTextbookSubjectGroup>()
                                   .Where(e => e.Id == body.IdSubjectGroup)
                                   .FirstOrDefaultAsync(CancellationToken);
            if (GetSubjectGroup == null)
                throw new BadRequestException("Textbook preparation Subject group with Id " + body.IdSubjectGroup + " is not found");

            var GetSubject = await _dbContext.Entity<MsSubject>()
                                   .Where(e => e.Id == body.IdSubject)
                                   .FirstOrDefaultAsync(CancellationToken);
            if (GetSubject == null)
                throw new BadRequestException("Subject with Id " + body.IdSubject + " is not found");

            var GetPathway = await _dbContext.Entity<MsPathway>()
                                   .Where(e => e.Id == body.IdStreaming)
                                   .FirstOrDefaultAsync(CancellationToken);
            if (GetPathway == null)
                throw new BadRequestException("Pathway with Id " + body.IdStreaming + " is not found");

            var GetISBN = await _dbContext.Entity<TrTextbook>()
                                   .Where(e => e.ISBN == body.ISBN && e.Id != body.Id)
                                   .FirstOrDefaultAsync(CancellationToken);

            if (GetISBN != null)
                throw new BadRequestException("ISBN " + body.ISBN + " is already exist");

            var GetApprover = await _dbContext.Entity<MsTextbookSettingApproval>()
                                   .Where(e => e.IdSchool == GetAcademicYear.IdSchool)
                                   .ToListAsync(CancellationToken);

            GetTextbook.IdTextbookSubjectGroup = body.IdSubjectGroup;
            GetTextbook.IdSubject = body.IdSubject;
            GetTextbook.IdPathway = body.IdStreaming;
            GetTextbook.BookType = body.BookType;
            GetTextbook.ISBN = body.ISBN;
            GetTextbook.Title = body.Title;
            GetTextbook.Author = body.Author;
            GetTextbook.Publisher = body.Publish;
            GetTextbook.Weight = body.Weight;
            GetTextbook.NoSKU = body.NoSku;
            GetTextbook.IsRegion = body.IsRegion;
            GetTextbook.IsMandatory = body.IsMandatory;
            GetTextbook.IsCountinuity = body.IsContinuity;
            GetTextbook.IsAvailableStatus = body.IsAvailableStatus;
            GetTextbook.Supplier = body.Supplier;
            GetTextbook.Location = body.Location;
            GetTextbook.LastModif = body.LastModif;
            GetTextbook.Vendor = body.Vendor;
            GetTextbook.OriginalPrice = body.OriginalPrice;
            GetTextbook.PriceAfterDiscount = body.PriceAfterDiscount;
            GetTextbook.Note = body.Note;
            GetTextbook.Status = GetStatus(body.IsEditFromApprovalForm, GetTextbook.Status, body.IsDraft, GetApprover.Count());
            GetTextbook.MinQty = body.MinQty;
            GetTextbook.MaxQty = body.MaxQty;

            //GetTextbook.IdUserApproval1 = GetApprover.Where(e => e.ApproverTo == 1).Select(e => e.IdUser).Any() ? GetApprover.Where(e => e.ApproverTo == 1).Select(e => e.IdUser).FirstOrDefault() : null;
            //GetTextbook.IdUserApproval2 = GetApprover.Where(e => e.ApproverTo == 2).Select(e => e.IdUser).Any() ? GetApprover.Where(e => e.ApproverTo == 2).Select(e => e.IdUser).FirstOrDefault() : null;
            //GetTextbook.IdUserApproval3 = GetApprover.Where(e => e.ApproverTo == 3).Select(e => e.IdUser).Any() ? GetApprover.Where(e => e.ApproverTo == 3).Select(e => e.IdUser).FirstOrDefault() : null;
            _dbContext.Entity<TrTextbook>().Update(GetTextbook);

            var GetTextbookAttachment = await _dbContext.Entity<TrTextbookAttachment>()
                                            .Where(e => e.IdTextbook == GetTextbook.Id)
                                            .ToListAsync(CancellationToken);
            GetTextbookAttachment.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrTextbookAttachment>().UpdateRange(GetTextbookAttachment);

            foreach (var item in body.Attachments)
            {
                var NewTextbookAttachment = new TrTextbookAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdTextbook = GetTextbook.Id,
                    Url = item.Url,
                    FileName = item.FileName,
                    FileNameOriginal = item.FileNameOriginal,
                    FileSize = item.FileSize,
                    FileType = item.FileType
                };
                _dbContext.Entity<TrTextbookAttachment>().Add(NewTextbookAttachment);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region send email
            var GetPic = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                            .Where(e => e.TextbookUserPeriod.IdAcademicYear == GetTextbook.IdAcademicYear && e.TextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookPic)
                            .Select(e => e.IdBinusian)
                            .ToListAsync(CancellationToken);

            var NamaUser = await _dbContext.Entity<MsUser>()
                            .Where(e => e.Id==body.IdUser)
                            .Select(e => e.DisplayName)
                            .FirstOrDefaultAsync(CancellationToken);

            //List<string> ids = new List<string>()
            //{
            //    "5170e328-0b39-46f2-9aeb-491d631ce7be",
            //    "5657e1aa-3239-4932-9cf1-7f01fb1ee74d",
            //    "f6fa9c82-f6aa-4323-871a-98fb619172ad"
            //};

            var GetEmailTextbook = await _dbContext.Entity<TrTextbook>()
                                .Include(e => e.Subject).ThenInclude(e => e.Grade)
                                .Include(e => e.StaffCreate)
                                .Where(e => e.Id == GetTextbook.Id)
                                //.Where(e => ids.Contains(e.Id))
                                .Select(e => new
                                {
                                    Id = e.Id,
                                    IdUserApproval = e.IdBinusianApproval2,
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
                                    IdApproval = e.IdBinusianApproval1,
                                })
                                .ToListAsync(CancellationToken);

            if (!body.IsEditFromApprovalForm)
            {
                if (GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.OnReview1.GetDescription()).Any())
                {
                    var EmailTextbook = new GetEmailTextbookResult
                    {
                        NameCreated = GetEmailTextbook.Select(e => e.NameCreated).FirstOrDefault(),
                        Textbooks = GetEmailTextbook.Where(e => e.Status == TextbookPreparationStatus.OnReview1.GetDescription()).Select(e => new GetEmailTextbook
                        {
                            Id = e.Id,
                            Subject = e.Subject,
                            Grade = e.Grade,
                            Author = e.Author,
                            Title = e.Title,
                            Isbn = e.Isbn,
                            Note = e.Note,
                            Status = e.Status,
                            IdUserApproval = e.IdApproval
                        }).ToList()
                    };

                    if (KeyValues.ContainsKey("EmailTextbook"))
                    {
                        KeyValues.Remove("EmailTextbook");
                    }
                    KeyValues.Add("EmailTextbook", EmailTextbook);
                    var Notification = TP6Notification(KeyValues, AuthInfo);
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
                            IdUserApproval = e.IdApproval
                        }).ToList()
                    };

                    if (KeyValues.ContainsKey("EmailTextbook"))
                    {
                        KeyValues.Remove("EmailTextbook");
                    }
                    KeyValues.Add("EmailTextbook", EmailTextbook);
                    var Notification = TP9Notification(KeyValues, AuthInfo);
                }
            }
            else
            {
                var IsUserPIC = GetPic.Contains(body.IdUser);

                if (IsUserPIC)
                {
                    var EmailTextbook = new GetEmailTextbookResult
                    {
                        IdUserPic = GetPic,
                        NameCreated = GetEmailTextbook.Select(e => e.NameCreated).FirstOrDefault(),
                        NameUser = NamaUser,
                        IdUser = body.IdUser,
                        Textbooks = GetEmailTextbook.Select(e => new GetEmailTextbook
                        {
                            Id = e.Id,
                            Subject = e.Subject,
                            Grade = e.Grade,
                            Author = e.Author,
                            Title = e.Title,
                            Isbn = e.Isbn,
                            Note = e.Note,
                            Status = e.Status,
                            IdUserCreated = e.IdUserCreated
                        }).ToList()
                    };

                    if (KeyValues.ContainsKey("EmailTextbook"))
                    {
                        KeyValues.Remove("EmailTextbook");
                    }
                    KeyValues.Add("EmailTextbook", EmailTextbook);
                    var Notification = TP11Notification(KeyValues, AuthInfo);
                }
                else
                {
                    var EmailTextbook = new GetEmailTextbookResult
                    {
                        NameCreated = GetEmailTextbook.Select(e => e.NameCreated).FirstOrDefault(),
                        NameUser = NamaUser,
                        Textbooks = GetEmailTextbook.Select(e => new GetEmailTextbook
                        {
                            Id = e.Id,
                            Subject = e.Subject,
                            Grade = e.Grade,
                            Author = e.Author,
                            Title = e.Title,
                            Isbn = e.Isbn,
                            Note = e.Note,
                            Status = e.Status,
                            IdUserCreated = e.IdUserCreated
                        }).ToList()
                    };

                    if (KeyValues.ContainsKey("EmailTextbook"))
                    {
                        KeyValues.Remove("EmailTextbook");
                    }
                    KeyValues.Add("EmailTextbook", EmailTextbook);
                    var Notification = TP10Notification(KeyValues, AuthInfo);
                }
            }

           

            #endregion
            return Request.CreateApiResult2();
        }

        public static string TP6Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbook").Value;
            var EmailTextbook = JsonConvert.DeserializeObject<GetEmailTextbookResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP6")
                {
                    IdRecipients = EmailTextbook.Textbooks.Select(e=>e.IdUserApproval).Distinct().ToList(),
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

        public static string TP10Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbook").Value;
            var EmailTextbook = JsonConvert.DeserializeObject<GetEmailTextbookResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP10")
                {
                    IdRecipients = EmailTextbook.Textbooks.Select(e => e.IdUserCreated).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string TP11Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailTextbook").Value;
            var EmailTextbook = JsonConvert.DeserializeObject<GetEmailTextbookResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "TP11")
                {
                    IdRecipients = EmailTextbook.Textbooks.Select(e => e.IdUserCreated).Distinct().ToList(),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static TextbookPreparationStatus GetStatus(bool IsEditFromApprovalForm, TextbookPreparationStatus Status, bool IsDraft, int CountApprover)
        {
            TextbookPreparationStatus Value = default;

            if (IsEditFromApprovalForm)
            {
                Value = Status;
            }
            else
            {
                Value = IsDraft
                            ? TextbookPreparationStatus.Hold
                            : CountApprover > 0
                                ? TextbookPreparationStatus.OnReview1
                                : TextbookPreparationStatus.Approved;
            }

            return Value;
        }
    }
}
