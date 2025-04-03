using System;
using System.Collections.Generic;
using System.IO;
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
using BinusSchool.School.FnSchool.TextbookPreparation.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;

namespace BinusSchool.School.FnSchool.TextbookPreparation
{
    public class UploadTextbookPreparationHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public UploadTextbookPreparationHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                throw new BadRequestException("Excel file not provided");

            var fileInfo = new FileInfo(file.FileName);
            if (fileInfo.Extension != ".xlsx")
                throw new BadRequestException("File extension not supported");

            using var fs = file.OpenReadStream();
            var workbook = new XSSFWorkbook(fs);
            var sheet = workbook.GetSheetAt(0);

            var SecondRowVal = sheet.GetRow(4);
            var IdAcademicYear = SecondRowVal.GetCell(1) == null ? "" : SecondRowVal.GetCell(1).ToString();

            if (sheet.LastRowNum == 0)
                throw new BadRequestException("No data is imported");

            var DataExcel = new List<GetUploadTextbookPreparationRequest>();
            for (var row = 6; row <= sheet.LastRowNum; row++)
            {
                var rowVal = sheet.GetRow(row);
                var Weight = rowVal.GetCell(9).ToString().Replace(",", "");
                var OriginalPrice = rowVal.GetCell(19).ToString().Replace(".", "").Replace(",", "");
                var PriceAfterDiscount = rowVal.GetCell(20).ToString().Replace(".", "").Replace(",", "");

                DataExcel.Add(new GetUploadTextbookPreparationRequest
                {
                    AcademicYear = IdAcademicYear,
                    Level = rowVal.GetCell(0) == null ? "" : rowVal.GetCell(0).ToString(),
                    Grade = rowVal.GetCell(1) == null ? "" : rowVal.GetCell(1).ToString(),
                    SubjectGroup = rowVal.GetCell(2) == null ? "" : rowVal.GetCell(2).ToString(),
                    Subject = rowVal.GetCell(3) == null ? "" : rowVal.GetCell(3).ToString(),
                    Streaming = rowVal.GetCell(4) == null ? "" : rowVal.GetCell(4).ToString(),
                    Isbn = rowVal.GetCell(5) == null ? "" : rowVal.GetCell(5).ToString(),
                    Title = rowVal.GetCell(6) == null ? "" : rowVal.GetCell(6).ToString(),
                    Author = rowVal.GetCell(7) == null ? "" : rowVal.GetCell(7).ToString(),
                    Publish = rowVal.GetCell(8) == null ? "" : rowVal.GetCell(8).ToString(),
                    Weight = rowVal.GetCell(9) == null ? 0 : Convert.ToDecimal(Weight),
                    IsMandator = rowVal.GetCell(10) == null ? false : rowVal.GetCell(10).ToString().ToLower() == "yes" ? true : false,
                    MinQty = rowVal.GetCell(11) == null ? 0 : Convert.ToInt32(rowVal.GetCell(11).ToString()),
                    MaxQty = rowVal.GetCell(12) == null ? 0 : Convert.ToInt32(rowVal.GetCell(12).ToString()),
                    IsContinuity = rowVal.GetCell(13) == null ? false : rowVal.GetCell(13).ToString().ToLower() == "yes" ? true : false,
                    IsAvailableStatus = rowVal.GetCell(14) == null ? false : rowVal.GetCell(14).ToString().ToLower() == "yes" ? true : false,
                    Supplier = rowVal.GetCell(15) == null ? "" : rowVal.GetCell(15).ToString(),
                    Location = rowVal.GetCell(16) == null ? "" : rowVal.GetCell(16).ToString(),
                    LastModif = rowVal.GetCell(17) == null ? "" : rowVal.GetCell(17).ToString(),
                    Vendor = rowVal.GetCell(18) == null ? "" : rowVal.GetCell(18).ToString(),
                    OriginalPrice = rowVal.GetCell(19) == null ? 0 : Convert.ToInt32(OriginalPrice),
                    PriceAfterDiscount = rowVal.GetCell(20) == null ? 0 : Convert.ToInt32(PriceAfterDiscount),
                    IsRelagion = rowVal.GetCell(21) == null ? false : rowVal.GetCell(21).ToString().ToLower() == "yes" ? true : false,
                    NoSku = rowVal.GetCell(22) == null ? "" : rowVal.GetCell(22).ToString(),
                    BookType = rowVal.GetCell(23) == null ? "" : rowVal.GetCell(23).ToString(),
                    Note = rowVal.GetCell(24) == null ? "" : rowVal.GetCell(24).ToString(),
                    UrlImage = rowVal.GetCell(25) == null ? "" : rowVal.GetCell(25).ToString(),
                });
            }

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                            .Where(e => e.Id == IdAcademicYear)
                            .FirstOrDefaultAsync(CancellationToken);

            if (GetAcademicYear == null)
                throw new BadRequestException("Academic year with Id: " + IdAcademicYear + " is not found");

            var GetTextbookExisting = await _dbContext.Entity<TrTextbook>()
                            .Where(e => e.IdAcademicYear == IdAcademicYear)
                            .ToListAsync(CancellationToken);

            var GetLevel = await _dbContext.Entity<MsLevel>()
                            .Where(e => DataExcel.Select(f => f.Level.ToLower()).ToList().Contains(e.Code.ToLower()) && e.IdAcademicYear == IdAcademicYear)
                            .ToListAsync(CancellationToken);

            var GetGrade = await _dbContext.Entity<MsGrade>()
                            .Include(e => e.Level)
                            .Where(e => DataExcel.Select(f => f.Grade.ToLower()).ToList().Contains(e.Code.ToLower()) && e.Level.IdAcademicYear == IdAcademicYear)
                            .ToListAsync(CancellationToken);

            var GetStreaming = await _dbContext.Entity<MsPathway>()
                            .Where(e => DataExcel.Select(f => f.Streaming.ToLower()).ToList().Contains(e.Description.ToLower())
                                        && e.IdAcademicYear == IdAcademicYear)
                            .ToListAsync(CancellationToken);

            var GetSubject = await _dbContext.Entity<MsTextbookSubjectGroupDetail>()
                               .Include(e => e.TextbookSubjectGroup)
                               .Include(e => e.Subject).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                               .Where(e => DataExcel.Select(f => f.Subject.ToLower()).ToList().Contains(e.Subject.Description.ToLower())
                                       && e.TextbookSubjectGroup.IdAcademicYear == IdAcademicYear
                                       && GetLevel.Select(f => f.Id).ToList().Contains(e.Subject.Grade.IdLevel)
                                       && GetGrade.Select(f => f.Id).ToList().Contains(e.Subject.IdGrade)
                                       && e.Subject.Grade.Level.IdAcademicYear == IdAcademicYear)
                               .ToListAsync(CancellationToken);

            var GetTextbookPreparation = new List<GetUploadTextbookPreparation>();

            foreach (var item in DataExcel)
            {
                bool IsWarningAcademicYear = false;
                bool IsWarningLevel = false;
                bool IsWarningGrade = false;
                bool IsWarningSubjectGroup = false;
                bool IsWarningSubject = false;
                bool IsWarningStreaming = false;
                bool IsWarningIsbn = false;
                bool IsWarningTitle = false;
                bool IsWarningAuthor = false;
                bool IsWarningBookType = false;
                TextbookPreparationBookType BookType = default;


                if (GetAcademicYear == null || string.IsNullOrEmpty(item.AcademicYear))
                    IsWarningAcademicYear = true;

                var GetLevelByDescription = GetLevel.Where(e => e.Code.ToLower() == item.Level.ToLower()).FirstOrDefault();
                if (GetLevelByDescription == null || string.IsNullOrEmpty(item.Level))
                    IsWarningLevel = true;

                var GetGradeByDescription = GetGrade.Where(e => e.Code.ToLower() == item.Grade.ToLower()).FirstOrDefault();
                if (GetGradeByDescription == null || string.IsNullOrEmpty(item.Grade))
                    IsWarningGrade = true;

                var GetsubjectGroupBygrade = GetSubject
                                        .Where(e => e.TextbookSubjectGroup.SubjectGroupName.ToLower() == item.SubjectGroup.ToLower()
                                        && e.Subject.IdGrade == (GetGradeByDescription == null ? "" : GetGradeByDescription.Id)
                                        && e.Subject.Grade.IdLevel == (GetLevelByDescription == null ? "" : GetLevelByDescription.Id)
                                        && e.Subject.Grade.Level.IdAcademicYear == IdAcademicYear).FirstOrDefault();

                if (GetsubjectGroupBygrade == null || string.IsNullOrEmpty(item.SubjectGroup))
                    IsWarningSubjectGroup = true;

                var GetsubjectBygrade = GetSubject
                                        .Where(e => e.Subject.Description.ToLower() == item.Subject.ToLower()
                                        && e.Subject.IdGrade == (GetGradeByDescription == null ? "" : GetGradeByDescription.Id)
                                        && e.Subject.Grade.IdLevel == (GetLevelByDescription == null ? "" : GetLevelByDescription.Id)
                                        && e.Subject.Grade.Level.IdAcademicYear == IdAcademicYear).FirstOrDefault();
                if (GetsubjectBygrade == null || string.IsNullOrEmpty(item.Subject))
                    IsWarningSubject = true;

                var GetStreamingByAcademicYear = GetStreaming
                                                    .Where(e => e.Description.ToLower() == item.Streaming.ToLower()
                                                    && e.IdAcademicYear == IdAcademicYear).FirstOrDefault();
                if (GetStreamingByAcademicYear == null && !string.IsNullOrEmpty(item.Subject))
                    IsWarningStreaming = true;

                if (string.IsNullOrEmpty(item.Isbn))
                {
                    IsWarningIsbn = true;
                }
                else //add validation duplicate isbn
                {
                    if (DataExcel.Where(x => x.Isbn == item.Isbn).Count() > 1)
                    {
                        IsWarningIsbn = true;
                    }
                    if (IsWarningIsbn == false)
                    {
                        if(GetTextbookExisting.Any(x=> x.ISBN == item.Isbn))
                        {
                            IsWarningIsbn = true;
                        }
                    }
                }

                if (string.IsNullOrEmpty(item.Title))
                    IsWarningTitle = true;

                if (string.IsNullOrEmpty(item.Author))
                    IsWarningAuthor = true;

                if (string.IsNullOrEmpty(item.BookType))
                {
                    IsWarningAuthor = true;
                }
                else
                {
                    if (item.BookType.ToLower() == "textbook")
                    {
                        BookType = TextbookPreparationBookType.Texbook;
                        IsWarningAuthor = false;
                    }
                    else if (item.BookType.ToLower() == "e-book")
                    {
                        BookType = TextbookPreparationBookType.Ebook;
                        IsWarningAuthor = false;
                    }
                    else
                    {
                        IsWarningAuthor = true;
                    }
                }

                GetTextbookPreparation.Add(new GetUploadTextbookPreparation
                {
                    IdAcademicYear = GetAcademicYear == null ? "" : GetAcademicYear.Id,
                    IsWarningAcademicYear = IsWarningAcademicYear,
                    AcademicYear = GetAcademicYear == null ? "" : GetAcademicYear.Description,

                    IdLevel = GetLevelByDescription == null ? "" : GetLevelByDescription.Id,
                    IsWarningLevel = IsWarningLevel,
                    Level = item.Level,

                    IdGrade = GetGradeByDescription == null ? "" : GetGradeByDescription.Id,
                    IsWarningGrade = IsWarningGrade,
                    Grade = item.Grade,

                    IdSubjectGroup = GetsubjectGroupBygrade == null ? "" : GetsubjectGroupBygrade.TextbookSubjectGroup.Id,
                    IsWarningSubjectGroup = IsWarningSubjectGroup,
                    SubjectGroup = item.SubjectGroup,

                    IdSubject = GetsubjectBygrade == null ? "" : GetsubjectBygrade.IdSubject,
                    IsWarningSubject = IsWarningSubject,
                    Subject = item.Subject,

                    IdStreaming = GetStreamingByAcademicYear == null ? "" : GetStreamingByAcademicYear.Id,
                    IsWarningStreaming = IsWarningStreaming,
                    Streaming = item.Streaming,

                    Isbn = item.Isbn,
                    IsWarningIsbn = IsWarningIsbn,

                    Title = item.Title,
                    IsWarningTitle = IsWarningTitle,

                    Author = item.Author,
                    IsWarningAuthor = IsWarningAuthor,

                    IdBookType = BookType,
                    BookType = item.BookType,
                    IsWarningBookType = IsWarningBookType,

                    Publish = item.Publish,
                    Weight = item.Weight,
                    IsMandator = item.IsMandator,
                    MinQty = item.MinQty,
                    MaxQty = item.MaxQty,
                    IsContinuity = item.IsContinuity,
                    IsAvailableStatus = item.IsAvailableStatus,
                    Supplier = item.Supplier,
                    Location = item.Location,
                    LastModif = item.LastModif,
                    Vendor = item.Vendor,
                    OriginalPrice = item.OriginalPrice,
                    PriceAfterDiscount = item.PriceAfterDiscount,
                    IsRelagion = item.IsRelagion,
                    NoSku = item.NoSku,
                    Note = item.Note,
                    UrlImage = item.UrlImage,
                });
            }

            var items = new GetUploadTextbookPreparationResult
            {
                Filed = GetTextbookPreparation
                        .Where(e => e.IsWarningAcademicYear == true
                                || e.IsWarningLevel == true
                                || e.IsWarningGrade == true
                                || e.IsWarningSubjectGroup == true
                                || e.IsWarningSubject == true
                                || e.IsWarningStreaming == true
                                || e.IsWarningBookType == true
                                || e.IsWarningIsbn == true
                                || e.IsWarningTitle == true
                                || e.IsWarningAuthor == true)
                        .ToList(),

                Success = (List<GetUploadTextbookPreparationSuccess>)GetTextbookPreparation
                        .Where(e => e.IsWarningAcademicYear == false
                                && e.IsWarningLevel == false
                                && e.IsWarningGrade == false
                                && e.IsWarningSubjectGroup == false
                                && e.IsWarningSubject == false
                                && e.IsWarningStreaming == false
                                && e.IsWarningBookType == false
                                && e.IsWarningIsbn == false
                                && e.IsWarningTitle == false
                                && e.IsWarningAuthor == false)
                        .Select(item => new GetUploadTextbookPreparationSuccess
                        {
                            IdAcademicYear = item.IdAcademicYear,
                            AcademicYear = item.AcademicYear,
                            IdLevel = item.IdLevel,
                            Level = item.Level,
                            IdGrade = item.IdGrade,
                            Grade = item.Grade,
                            IdSubjectGroup = item.IdSubjectGroup,
                            SubjectGroup = item.SubjectGroup,
                            IdSubject = item.IdSubject,
                            Subject = item.Subject,
                            IdStreaming = item.IdStreaming,
                            Streaming = item.Streaming,
                            Isbn = item.Isbn,
                            Title = item.Title,
                            Author = item.Author,
                            IdBookType = item.IdBookType,
                            BookType = item.BookType,
                            Publish = item.Publish,
                            Weight = item.Weight,
                            IsMandator = item.IsMandator,
                            MinQty = item.MinQty,
                            MaxQty = item.MaxQty,
                            IsContinuity = item.IsContinuity,
                            IsAvailableStatus = item.IsAvailableStatus,
                            Supplier = item.Supplier,
                            Location = item.Location,
                            LastModif = item.LastModif,
                            Vendor = item.Vendor,
                            OriginalPrice = item.OriginalPrice,
                            PriceAfterDiscount = item.PriceAfterDiscount,
                            IsRelagion = item.IsRelagion,
                            NoSku = item.NoSku,
                            Note = item.Note,
                            UrlImage = item.UrlImage,
                            Attachments = string.IsNullOrEmpty(item.UrlImage)
                                ? new List<AddTextbookAttachment>()
                                : new List<AddTextbookAttachment>()
                                    {
                                        new AddTextbookAttachment
                                        {
                                            FileName = item.Title+item.UrlImage.Substring(item.UrlImage.LastIndexOf(".")),
                                            FileNameOriginal = item.Title+item.UrlImage.Substring(item.UrlImage.LastIndexOf(".")),
                                            FileSize = 0,
                                            FileType = item.UrlImage.Substring(item.UrlImage.LastIndexOf(".")),
                                            Url = item.UrlImage
                                        }
                                    },
                        })
                        .ToList(),
            };

            return Request.CreateApiResult2(items as object);
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<AddUploadTextbookPreparationRequest, AddUploadTextbookPreparationValidator>();
            List<string> Ids = new List<string>();

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => body.TextbookPeparations.Select(x => x.IdAcademicYear).ToList().Contains(e.Id))
                                    .ToListAsync(CancellationToken);

            var GetSubjectGroup = await _dbContext.Entity<MsTextbookSubjectGroup>()
                                    .Where(e => body.TextbookPeparations.Select(x => x.IdSubjectGroup).ToList().Contains(e.Id))
                                   .ToListAsync(CancellationToken);

            var GetSubject = await _dbContext.Entity<MsSubject>()
                                    .Where(e => body.TextbookPeparations.Select(x => x.IdSubject).ToList().Contains(e.Id))
                                   .ToListAsync(CancellationToken);

            var GetPathway = await _dbContext.Entity<MsPathway>()
                                    .Where(e => body.TextbookPeparations.Select(x => x.IdStreaming).ToList().Contains(e.Id))
                                   .FirstOrDefaultAsync(CancellationToken);

            var GetApprover = await _dbContext.Entity<MsTextbookSettingApproval>()
                                    .Where(e => GetAcademicYear.Select(x => x.IdSchool).ToList().Contains(e.IdSchool))
                                   .ToListAsync(CancellationToken);

            foreach (var item in body.TextbookPeparations)
            {
                var GetAcademicYearById = await _dbContext.Entity<MsAcademicYear>()
                                    .Where(e => e.Id == item.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);
                if (GetAcademicYear == null)
                    throw new BadRequestException("Academic year with Id " + item.IdAcademicYear + " is not found");

                var GetSubjectGroupById = await _dbContext.Entity<MsTextbookSubjectGroup>()
                                       .Where(e => e.Id == item.IdSubjectGroup)
                                       .FirstOrDefaultAsync(CancellationToken);
                if (GetSubjectGroup == null)
                    throw new BadRequestException("Textbook preparation Subject group with Id " + item.IdSubjectGroup + " is not found");

                var GetSubjectById = await _dbContext.Entity<MsSubject>()
                                       .Where(e => e.Id == item.IdSubject)
                                       .FirstOrDefaultAsync(CancellationToken);
                if (GetSubject == null)
                    throw new BadRequestException("Subject with Id " + item.IdSubject + " is not found");

                var GetPathwayById = await _dbContext.Entity<MsPathway>()
                                       .Where(e => e.Id == item.IdStreaming)
                                       .FirstOrDefaultAsync(CancellationToken);
                if (GetPathway == null)
                    throw new BadRequestException("Pathway with Id " + item.IdStreaming + " is not found");

                var NewTextbook = new TrTextbook
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = item.IdAcademicYear,
                    IdBinusianCreated = item.IdUser,
                    IdTextbookSubjectGroup = item.IdSubjectGroup,
                    IdSubject = item.IdSubject,
                    IdPathway = item.IdStreaming,
                    BookType = item.BookType,
                    ISBN = item.ISBN,
                    Title = item.Title,
                    Author = item.Author,
                    Publisher = item.Publish,
                    Weight = item.Weight,
                    NoSKU = item.NoSku,
                    IsRegion = item.IsRegion,
                    IsMandatory = item.IsMandatory,
                    MinQty = item.MinQty,
                    MaxQty = item.MaxQty,
                    IsCountinuity = item.IsContinuity,
                    IsAvailableStatus = item.IsAvailableStatus,
                    Supplier = item.Supplier,
                    Location = item.Location,
                    LastModif = item.LastModif,
                    Vendor = item.Vendor,
                    OriginalPrice = item.OriginalPrice,
                    PriceAfterDiscount = item.PriceAfterDiscount,
                    Note = item.Note,
                    Status = item.IsDraft
                    ? TextbookPreparationStatus.Hold
                    : GetApprover.Any()
                        ? TextbookPreparationStatus.OnReview1
                        : TextbookPreparationStatus.Approved,
                    IdBinusianApproval1 = GetApprover.Where(e => e.ApproverTo == 1).Select(e => e.IdBinusian).Any() ? GetApprover.Where(e => e.ApproverTo == 1).Select(e => e.IdBinusian).FirstOrDefault() : null,
                    IdBinusianApproval2 = GetApprover.Where(e => e.ApproverTo == 2).Select(e => e.IdBinusian).Any() ? GetApprover.Where(e => e.ApproverTo == 2).Select(e => e.IdBinusian).FirstOrDefault() : null,
                    IdBinusianApproval3 = GetApprover.Where(e => e.ApproverTo == 3).Select(e => e.IdBinusian).Any() ? GetApprover.Where(e => e.ApproverTo == 3).Select(e => e.IdBinusian).FirstOrDefault() : null,
                };
                _dbContext.Entity<TrTextbook>().Add(NewTextbook);

                Ids.Add(NewTextbook.Id);

                foreach (var itemAttachment in item.Attachments)
                {
                    var NewTextbookAttachment = new TrTextbookAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdTextbook = NewTextbook.Id,
                        Url = itemAttachment.Url,
                        FileName = itemAttachment.FileName,
                        FileNameOriginal = itemAttachment.FileNameOriginal,
                        FileSize = itemAttachment.FileSize,
                        FileType = itemAttachment.FileType
                    };
                    _dbContext.Entity<TrTextbookAttachment>().Add(NewTextbookAttachment);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region send email
            var GetPic = await _dbContext.Entity<MsTextbookUserPeriodDetail>()
                            .Where(e => e.TextbookUserPeriod.IdAcademicYear == body.TextbookPeparations.Select(e => e.IdAcademicYear).FirstOrDefault() && e.TextbookUserPeriod.AssignAs == TextBookPreparationUserPeriodAssignAs.TextbookPic)
                            .Select(e => e.IdBinusian)
                            .ToListAsync(CancellationToken);

            //string[] Id = {
            //    "5170e328-0b39-46f2-9aeb-491d631ce7be",
            //    "5657e1aa-3239-4932-9cf1-7f01fb1ee74d",
            //    "f6fa9c82-f6aa-4323-871a-98fb619172ad"
            //};

            //Ids.AddRange(Id);

            var GetEmailTextbook = await _dbContext.Entity<TrTextbook>()
                                .Include(e => e.Subject).ThenInclude(e => e.Grade)
                                .Include(e => e.StaffCreate)
                                .Where(e => Ids.Contains(e.Id))
                                .Select(e => new
                                {
                                    Id = e.Id,
                                    IdUser = e.IdBinusianApproval2,
                                    NameCreated = (!string.IsNullOrEmpty(e.StaffCreate.FirstName) ? e.StaffCreate.FirstName : "")
                                                    + (!string.IsNullOrEmpty(e.StaffCreate.LastName) ? " " + e.StaffCreate.LastName : ""),
                                    Subject = e.Subject.Description,
                                    Grade = e.Subject.Grade.Code,
                                    Author = e.Author,
                                    Title = e.Title,
                                    Isbn = e.ISBN,
                                    Note = e.Note,
                                    Status = e.Status.GetDescription(),
                                    IdApproval = e.IdBinusianApproval1
                                })
                                .ToListAsync(CancellationToken);

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
                    IdRecipients = EmailTextbook.Textbooks.Select(e => e.IdUserApproval).Distinct().ToList(),
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
