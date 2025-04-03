using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnGuidanceCounseling.CounselorData.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselorData
{
    public class CounselorDataHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public CounselorDataHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var datas = await _dbContext.Entity<MsCounselor>()
                .Include(x => x.CounselingServicesEntry)
                .Include(x => x.CounselorGrade)
                .Include(x => x.CounselorPhoto)
                .Include(x => x.User)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                // don't set inactive when row have to-many relation
                if (data.CounselingServicesEntry.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.User.DisplayName));
                }
                else
                {
                    data.IsActive = false;
                    data.CounselorGrade.ToList().ForEach(x => x.IsActive = false);
                    data.CounselorPhoto.ToList().ForEach(x => x.IsActive = false);
                    _dbContext.Entity<MsCounselor>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            //source
            var query = await _dbContext.Entity<MsCounselor>()
                .Include(x => x.CounselorPhoto)
                .Include(x => x.CounselorGrade).ThenInclude(x => x.Grade).ThenInclude(e=>e.MsLevel)
                .Include(x => x.User)
                .Include(x => x.AcademicYear)
                .Include(x => x.Role)
                .Include(x => x.Position)
                .FirstOrDefaultAsync(x => x.Id == id);

            var queryCounselor = await _dbContext.Entity<MsStaff>().FirstOrDefaultAsync(x => x.IdBinusian == query.IdUser);

            //destination
            GetDetailCounselorDataResult data = new GetDetailCounselorDataResult()
            {
                Id = query.Id,
                IdAcademicYear = query.IdAcademicYear,
                AcademicYearCode = query.AcademicYear.Code,
                GCPhoto = query.CounselorPhoto.FirstOrDefault(x => x.IsActive == true)?.Url,
                IdUser = query.IdUser,
                CounselorName = $"{queryCounselor.FirstName} {queryCounselor.LastName}",
                CounselorEmail = query.User.Email,
                OfficerLocation = query.OfficerLocation,
                ExtensionNumber = query.ExtensionNumber,
                OtherInformation = query.OtherInformation,
                Role =  new ItemValueVm
                {
                    Id = query.Role.Id,
                    Description = query.Role.Description
                },
                Position = new ItemValueVm
                {
                    Id = query.Position.Id,
                    Description = query.Position.Description
                },
                AcademicYear = new AcademicYearObject()
                {
                    Id = query.AcademicYear.Id,
                    Code = query.AcademicYear.Code,
                    Description = query.AcademicYear.Description
                },
                ListDetailGradeCounselorData = query.CounselorGrade
                                            .OrderBy(x => x.Grade.OrderNumber)
                                            .Select(e => new ItemValueVm
                                            {
                                                Id = e.Grade.Id,
                                                Description = e.Grade.Code,
                                            })
                                            .ToList(),
                ListDetailLevelCounselorData = query.CounselorGrade
                                            .GroupBy(e => new
                                            {
                                                Id = e.Grade.MsLevel.Id,
                                                Code = e.Grade.MsLevel.Code,
                                            })
                                            .Select(e => new ItemValueVm
                                            {
                                                Id = e.Key.Id,
                                                Description = e.Key.Code,
                                            })
                                            .ToList(),
                ListAttachmentCounselorData = query.CounselorPhoto
                                                .Select(x=> new AttachmentCounselorData
                                                {
                                                    Id = x.Id,
                                                    OriginalFilename = x.OriginalName,
                                                    Url = x.Url,
                                                    FileName = x.FileName,
                                                    FileType = x.FileType,
                                                    FileSize = x.FileSize
                                                })
                                                .ToList()

            };

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetCounselorDataRequest>();
            string[] columns = { "AcademicYearCode", "Grades", "GCPhoto", "CounselorName", "OfficeLocation", "ExtentionNumber", "ConselorEmail", "OtherInformation" };

            // var query = _dbContext.Entity<MsCounselor>()
            //     .Include(x => x.CounselorGrade).ThenInclude(x => x.Grade)
            //     .Include(x => x.CounselorPhoto)
            //     .Include(x => x.User)
            //     .Include(x => x.AcademicYear).AsQueryable();
            //.Where(predicate);

            var queryJoin = await (from c in _dbContext.Entity<MsCounselor>()
                        join cg in _dbContext.Entity<MsCounselorGrade>() on c.Id equals cg.IdCounselor
                        join g in _dbContext.Entity<MsGrade>() on cg.IdGrade equals g.Id
                        join cp in _dbContext.Entity<MsCounselorPhoto>() on c.Id equals cp.IdCounselor
                        join u in _dbContext.Entity<MsUser>() on c.IdUser equals u.Id
                        join ay in _dbContext.Entity<MsAcademicYear>() on c.IdAcademicYear equals ay.Id
                        join s in _dbContext.Entity<MsStaff>() on u.Id equals s.IdBinusian
                        where c.IdAcademicYear == param.IdAcadyear
                        select new {
                            Id = c.Id,
                            IdAcademicYear = c.IdAcademicYear,
                            AcademicYearCode = ay.Code,
                            Grades = g.Code,
                            GCPhoto = cp.Url,
                            CounselorName = s.FirstName + " " + s.LastName,
                            OfficerLocation = c.OfficerLocation,
                            ExtensionNumber = c.ExtensionNumber,
                            CounselorEmail = u.Email,
                            OtherInformation = c.OtherInformation,
                            AcademicYear = new AcademicYearObject()
                            {
                                Id = ay.Id,
                                Code = ay.Code,
                                Description = ay.Description
                            }
                        }).ToListAsync(CancellationToken);

            //filter
            // if (!string.IsNullOrEmpty(param.IdAcadyear))
            //     queryJoin = queryJoin.Where(x => x.IdAcademicYear == param.IdAcadyear);
            // if (!string.IsNullOrEmpty(param.Search))
            // {
            //     queryJoin = queryJoin.Where(x => EF.Functions.Like(x.CounselorName, param.SearchPattern()));
            // }

            IReadOnlyList<IItemValueVm> items;

            if (param.Return == CollectionType.Lov)
                items = queryJoin
                    .Select(x => new GetCounselorDataResult()
                    {
                        Id = x.Id,
                        AcademicYearCode = x.AcademicYearCode,
                        Grades = x.Grades,
                        GCPhoto = x.GCPhoto,
                        CounselorName = x.CounselorName,
                        OfficerLocation = x.OfficerLocation,
                        ExtensionNumber = x.ExtensionNumber,
                        CounselorEmail = x.CounselorEmail,
                        OtherInformation = x.OtherInformation,
                        AcademicYear = x.AcademicYear
                    })
                    // .OrderByDynamic(param)
                    .ToList();
            else
                items = queryJoin
                    .SetPagination(param)
                    .Select(x => new GetCounselorDataResult()
                    {
                        Id = x.Id,
                        AcademicYearCode = x.AcademicYearCode,
                        Grades = x.Grades,
                        GCPhoto = x.GCPhoto,
                        CounselorName = x.CounselorName,
                        OfficerLocation = x.OfficerLocation,
                        ExtensionNumber = x.ExtensionNumber,
                        CounselorEmail = x.CounselorEmail,
                        OtherInformation = x.OtherInformation,
                        AcademicYear = x.AcademicYear
                    })
                    // .OrderByDynamic(param)
                    .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : queryJoin.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddCounselorDataRequest, AddCounselorDataValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var isExist = await _dbContext.Entity<MsCounselor>()
                .Include(x => x.AcademicYear)
                .Include(x => x.User)
                .Where(x => x.IdAcademicYear.ToLower() == body.IdAcademicYear.ToLower() &&
                        x.IdUser.ToLower() == body.IdUser.ToLower())
                .FirstOrDefaultAsync();

            if (isExist != null)
                throw new BadRequestException($"{isExist.AcademicYear.Code} , {isExist.User.DisplayName} already exists");

            var param = new MsCounselor
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = body.IdAcademicYear,
                IdUser = body.IdUser,
                OfficerLocation = body.OfficerLocation,
                ExtensionNumber = body.ExtensionNumber,
                OtherInformation = body.OtherInformation,
                IdPosition = body.IdPosition,
                IdRole = body.IdRole,
            };

            List<MsCounselorGrade> listCounselorGrade = new List<MsCounselorGrade>();
            foreach (var grade in body.ListGradeCounselorData)
            {
                listCounselorGrade.Add(new MsCounselorGrade()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGrade = grade.IdGrade,
                    UserIn = AuthInfo.UserId
                });
            }
            param.CounselorGrade = listCounselorGrade;

            List<MsCounselorPhoto> listCounselorPhoto = new List<MsCounselorPhoto>();
            foreach (var attachment in body.ListAttachmentCounselorData)
            {
                listCounselorPhoto.Add(new MsCounselorPhoto()
                {
                    Id = Guid.NewGuid().ToString(),
                    OriginalName = attachment.OriginalFilename,
                    Url = attachment.Url,
                    FileName = attachment.FileName,
                    FileType = attachment.FileType,
                    FileSize = attachment.FileSize,
                    UserIn = AuthInfo.UserId,
                    IsActive = true
                });
            }
            param.CounselorPhoto = listCounselorPhoto;

            _dbContext.Entity<MsCounselor>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateCounselorDataRequest, UpdateCounselorDataValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsCounselor>()
                .Include(x => x.CounselorGrade)
                .Include(x => x.CounselorPhoto)
                .FirstOrDefaultAsync(x => x.Id == body.Id, CancellationToken);

            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Counselor Data"], "Id", body.Id));

            var isExist = await _dbContext.Entity<MsCounselor>()
                .Where(x => x.Id != body.Id && x.IdAcademicYear.ToLower() == body.IdUser.ToLower())
                .Include(x => x.AcademicYear)
                .Include(x => x.User)
                .FirstOrDefaultAsync();

            if (isExist != null)
                throw new BadRequestException($"{isExist.AcademicYear.Code}, {isExist.User.DisplayName} already exists");

            data.IdAcademicYear = body.IdAcademicYear;
            data.IdUser = body.IdUser;
            data.OfficerLocation = body.OfficerLocation;
            data.ExtensionNumber = body.ExtensionNumber;
            data.OtherInformation = body.OtherInformation;
            data.UserUp = AuthInfo.UserId;

            //not found grade in database
            var idsGrade = body.ListGradeCounselorData.Where(x => !string.IsNullOrEmpty(x.Id)).Select(x => x.Id);
            var notExistGrade = data.CounselorGrade.Except(data.CounselorGrade.Where(x => idsGrade.Contains(x.Id)));
            notExistGrade.ToList().ForEach(x => x.IsActive = false);
            //add grade to database
            foreach (var grade in body.ListGradeCounselorData.Where(x => string.IsNullOrEmpty(x.Id)))
            {
                data.CounselorGrade.Add(new MsCounselorGrade()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGrade = grade.IdGrade
                });
            }
            //not found photo in database
            var idsPhoto = body.ListAttachmentCounselorData.Where(x => !string.IsNullOrEmpty(x.Id)).Select(x => x.Id);
            var notExistPhoto = data.CounselorPhoto.Except(data.CounselorPhoto.Where(x => idsPhoto.Contains(x.Id)));
            notExistPhoto.ToList().ForEach(x => x.IsActive = false);
            //add grade to database
            foreach (var attachment in body.ListAttachmentCounselorData.Where(x => string.IsNullOrEmpty(x.Id)))
            {
                data.CounselorPhoto.Add(new MsCounselorPhoto()
                {
                    Id = Guid.NewGuid().ToString(),
                    OriginalName = attachment.OriginalFilename,
                    Url = attachment.Url,
                    FileName = attachment.FileName,
                    FileType = attachment.FileType,
                    FileSize = attachment.FileSize,
                    UserIn = AuthInfo.UserId,
                    IsActive = true
                });
            }

            _dbContext.Entity<MsCounselor>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
