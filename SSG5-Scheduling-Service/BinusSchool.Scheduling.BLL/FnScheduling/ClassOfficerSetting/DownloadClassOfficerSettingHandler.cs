using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.ClassOfficerSetting
{
    public class DownloadClassOfficerSettingHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetClassOfficerSettingRequest.IdAcademicYear),
            nameof(GetClassOfficerSettingRequest.Position),
        };
        private readonly ISchedulingDbContext _dbContext;
        private readonly ISchool _schoolService;
        private readonly IStorageManager _storageManager;

        public DownloadClassOfficerSettingHandler(ISchedulingDbContext dbContext, ISchool schoolService, IStorageManager storageManager)
        {
            _dbContext = dbContext;
            _schoolService = schoolService;
            _storageManager = storageManager;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<GetClassOfficerSettingRequest>(_requiredParams);

            List<ClassOfficerSettingHomeroom> listHomeroomPosition = new List<ClassOfficerSettingHomeroom>();

            #region HomeroomTeacher
            var queryHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
               .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdBinusian))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.IdBinusian == param.IdBinusian);

            if (!string.IsNullOrEmpty(param.IdLevel))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.IdGrade == param.IdGrade);

            if (!string.IsNullOrEmpty(param.IdHomeRoom))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Id == param.IdHomeRoom);

            var listHomeroomTeacher = await queryHomeroomTeacher
                                        .Select(e => new ClassOfficerSettingHomeroom
                                        {
                                            IdHomeroom = e.IdHomeroom,
                                            Position = PositionConstant.ClassAdvisor
                                        })
                                        .ToListAsync(CancellationToken);

            listHomeroomPosition.AddRange(listHomeroomTeacher);
            #endregion

            #region Level Head dan Coor
            List<string> position = new List<string>()
            {
                PositionConstant.AffectiveCoordinator, PositionConstant.LevelHead,
            };

            var queryTeacherNonTeaching = _dbContext.Entity<TrNonTeachingLoad>()
                                .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                 .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdAcademicYear && position.Contains(x.MsNonTeachingLoad.TeacherPosition.Position.Code));

            if (!string.IsNullOrEmpty(param.IdBinusian))
                queryTeacherNonTeaching = queryTeacherNonTeaching.Where(e => e.IdUser == param.IdBinusian);

            var listTeacherNonTeaching = await queryTeacherNonTeaching.ToListAsync(CancellationToken);

            if (listTeacherNonTeaching.Count() > 0)
            {
                            var listLesson = await _dbContext.Entity<MsLesson>()
                                .Include(e => e.Grade)
                                .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                                .Select(e => new
                                {
                                    IdLesson = e.Id,
                                    IdSubject = e.IdSubject,
                                    IdLevel = e.Grade.IdLevel
                                })
                                .ToListAsync(CancellationToken);

            var listEnroll = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Where(x => x.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                                .Select(e => new
                                {
                                    IdLesson = e.Id,
                                    IdSubject = e.IdSubject,
                                    IdHomeroom = e.HomeroomStudent.Homeroom.Id,
                                    IdLevel = e.HomeroomStudent.Homeroom.Grade.IdLevel,
                                    IdGrade = e.HomeroomStudent.Homeroom.IdGrade,
                                })
                                .ToListAsync(CancellationToken);

            var listHomeroom = await _dbContext.Entity<MsHomeroom>()
                                .Include(e => e.Grade).ThenInclude(e => e.Level)
                                .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                                .Select(e => new
                                {
                                    IdHomeroom = e.Id,
                                    IdLevel = e.Grade.IdLevel,
                                    IdGrade = e.IdGrade,
                                })
                                .ToListAsync(CancellationToken);

            var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                .Include(e => e.Level).ThenInclude(e => e.MsGrades)
                                 .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            foreach (var item in listTeacherNonTeaching)
            {
                var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
                if (_SubjectPosition == null && _GradePosition == null && _LevelPosition == null && _DepartemenPosition != null)
                {
                    var getDepartmentLevelbyIdLevel = listDepartmentLevel
                                                        .Where(e => e.IdDepartment == _DepartemenPosition.Id)
                                                        .Select(e => e.IdLevel)
                                                        .Distinct()
                                                        .ToList();

                    var listHomeroomByIdLevel = listHomeroom
                                           .Where(e => getDepartmentLevelbyIdLevel.Contains(e.IdLevel))
                                           .Select(e => new ClassOfficerSettingHomeroom
                                           {
                                               IdHomeroom = e.IdHomeroom,
                                               Position = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                                           })
                                           .Distinct().ToList();
                    listHomeroomPosition.AddRange(listHomeroomByIdLevel);
                }
                else if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null && _DepartemenPosition != null)
                {
                    var listHomeroomByIdSubject = listEnroll
                                                .Where(e => e.IdSubject == _SubjectPosition.Id)
                                                .Select(e => new ClassOfficerSettingHomeroom
                                                {
                                                    IdHomeroom = e.IdHomeroom,
                                                    Position = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                                                })
                                                .Distinct().ToList();
                    listHomeroomPosition.AddRange(listHomeroomByIdSubject);
                }
                else if (_SubjectPosition == null && _GradePosition != null && _LevelPosition != null)
                {
                    var listHomeroomByIdGrade = listHomeroom
                                            .Where(e => e.IdGrade == _GradePosition.Id)
                                            .Select(e => new ClassOfficerSettingHomeroom
                                            {
                                                IdHomeroom = e.IdHomeroom,
                                                Position = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                                            }).Distinct().ToList();

                    listHomeroomPosition.AddRange(listHomeroomByIdGrade);
                }
                else if (_SubjectPosition == null && _GradePosition == null && _LevelPosition != null)
                {
                    var listHomeroomByIdLevel = listHomeroom
                                            .Where(e => e.IdLevel == _LevelPosition.Id)
                                            .Select(e => new ClassOfficerSettingHomeroom
                                            {
                                                IdHomeroom = e.IdHomeroom,
                                                Position = item.MsNonTeachingLoad.TeacherPosition.Position.Code
                                            }).Distinct().ToList();

                    listHomeroomPosition.AddRange(listHomeroomByIdLevel);
                }
            }
            }
            #endregion

            var listIdHomeroom = new List<string>();
            if (listHomeroomPosition.Any())
                listIdHomeroom = listHomeroomPosition.Where(e => e.Position == param.Position).Select(e => e.IdHomeroom).Distinct().ToList();

            var query = from h in _dbContext.Entity<MsHomeroom>()
                        join ht in _dbContext.Entity<MsHomeroomTeacher>() on h.Id equals ht.IdHomeroom
                        join g in _dbContext.Entity<MsGrade>() on h.IdGrade equals g.Id
                        join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                        join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                        join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
                        join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                        join _hf in _dbContext.Entity<MsHomeroomOfficer>() on h.Id equals _hf.Id into _hfData
                        from hf in _hfData.DefaultIfEmpty()
                        join _uu in _dbContext.Entity<MsUser>() on hf.UserUp equals _uu.Id into _uuData
                        from uu in _uuData.DefaultIfEmpty()
                        join _uc in _dbContext.Entity<MsUser>() on hf.UserIn equals _uc.Id into _ucData
                        from uc in _ucData.DefaultIfEmpty()
                        where listIdHomeroom.Contains(h.Id)
                        select new DownloadClassOfficerSettingResult
                        {
                            Id = h.Id,
                            IdAcademicYear = new CodeWithIdVm
                            {
                                Id = ay.Id,
                                Code = ay.Code,
                                Description = ay.Description
                            },
                            Level = new CodeWithIdVm
                            {
                                Id = l.Id,
                                Code = l.Code,
                                Description = l.Description
                            },
                            Grade = new CodeWithIdVm
                            {
                                Id = g.Id,
                                Code = g.Code,
                                Description = g.Description
                            },
                            HomeRoom = new CodeWithIdVm
                            {
                                Id = h.Id,
                                Code = c.Code,
                                Description = c.Description
                            },
                            Semester = h.Semester,
                            LastModified = (hf != null)
                                                ? hf.DateUp == null
                                                    ? hf.DateIn : hf.DateUp
                                                : null,
                            UserModified = (hf != null)
                                                ? hf.UserUp == null
                                                    ? uc.DisplayName : uu.DisplayName
                                                : string.Empty,
                            Captain = new UserHomeroomCaptain
                            {
                                Id = hf.IdUserHomeroomCaptain,
                                Name = hf.HomeroomCaptain.DisplayName,
                                CaptainCanAssignClassDiary = hf.CaptainCanAssignClassDiary
                            },
                            ViceCaptain = new UserHomeroomViceCaptain
                            {
                                Id = hf.IdUserHomeroomViceCaptain,
                                Name = hf.HomeroomViceCaptain.DisplayName,
                                ViceCaptainCanAssignClassDiary = hf.ViceCaptainCanAssignClassDiary,
                            },
                            Secretary = new UserHomeroomSecretary
                            {
                                Id = hf.IdUserHomeroomSecretary,
                                Name = hf.HomeroomSecretary.DisplayName,
                                SecretaryCanAssignClassDiary = hf.SecretaryCanAssignClassDiary
                            },
                            Treasurer = new UserHomeroomTreasurer
                            {
                                Id = hf.IdUserHomeroomTreasurer,
                                Name = hf.HomeroomTreasurer.DisplayName,
                                TreasurerCanAssignClassDiary = hf.TreasurerCanAssignClassDiary
                            }
                        };

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
            {
                query = query.Where(x => x.IdAcademicYear.Id.Contains(param.IdAcademicYear));
            }
            if (!string.IsNullOrEmpty(param.IdGrade))
            {
                query = query.Where(x => x.Grade.Id.Contains(param.IdGrade));
            }
            if (!string.IsNullOrEmpty(param.IdLevel))
            {
                query = query.Where(x => x.Level.Id.Contains(param.IdLevel));
            }
            if (!string.IsNullOrEmpty(param.IdHomeRoom))
            {
                query = query.Where(x => x.HomeRoom.Id.Contains(param.IdHomeRoom));
            }
            if (!string.IsNullOrEmpty(param.Semester))
            {
                query = query.Where(x => x.Semester.ToString().Contains(param.Semester));
            }
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.HomeRoom.Description, param.SearchPattern()));
            }

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdAcademicYear.Description)
                        : query.OrderBy(x => x.IdAcademicYear.Description);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level.Description)
                        : query.OrderBy(x => x.Level.Description);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade.Description)
                        : query.OrderBy(x => x.Grade.Description);
                    break;
                case "HomeRoom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.HomeRoom.Description)
                        : query.OrderBy(x => x.HomeRoom.Description);
                    break;
                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "LastModified":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LastModified)
                        : query.OrderBy(x => x.LastModified);
                    break;
            };

            var queries = query.ToList();

            var groupItems = queries.GroupBy(item => item.Id,
                    (key, group) => new { Id = key, Items = group.ToList() })
                    .ToList();

            var items = groupItems
                .Select(x => new DownloadClassOfficerSettingResult
                {
                    Id = x.Id,
                    IdAcademicYear = new CodeWithIdVm
                    {
                        Id = x.Items.Select(y => y.IdAcademicYear.Id).First(),
                        Code = x.Items.Select(y => y.IdAcademicYear.Code).First(),
                        Description = x.Items.Select(y => y.IdAcademicYear.Description).First()
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = x.Items.Select(y => y.Level.Id).First(),
                        Code = x.Items.Select(y => y.Level.Code).First(),
                        Description = x.Items.Select(y => y.Level.Description).First()
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = x.Items.Select(y => y.Grade.Id).First(),
                        Code = x.Items.Select(y => y.Grade.Code).First(),
                        Description = x.Items.Select(y => y.Grade.Description).First()
                    },
                    HomeRoom = new CodeWithIdVm
                    {
                        Id = x.Items.Select(y => y.HomeRoom.Id).First(),
                        Code = x.Items.Select(y => y.HomeRoom.Code).First(),
                        Description = x.Items.Select(y => y.HomeRoom.Description).First()
                    },
                    Semester = x.Items.Select(y => y.Semester).First(),
                    LastModified = x.Items.Select(y => y.LastModified).First(),
                    UserModified = x.Items.Select(y => y.UserModified).First(),
                    Captain = new UserHomeroomCaptain
                    {
                        Id = x.Items.Select(y => y.Captain).First().Id,
                        Name = x.Items.Select(y => y.Captain).First().Name,
                        CaptainCanAssignClassDiary = x.Items.Select(y => y.Captain).First().CaptainCanAssignClassDiary
                    },
                    ViceCaptain = new UserHomeroomViceCaptain
                    {
                        Id = x.Items.Select(y => y.ViceCaptain).First().Id,
                        Name = x.Items.Select(y => y.ViceCaptain).First().Name,
                        ViceCaptainCanAssignClassDiary = x.Items.Select(y => y.ViceCaptain).First().ViceCaptainCanAssignClassDiary
                    },
                    Secretary = new UserHomeroomSecretary
                    {
                        Id = x.Items.Select(y => y.Secretary).First().Id,
                        Name = x.Items.Select(y => y.Secretary).First().Name,
                        SecretaryCanAssignClassDiary = x.Items.Select(y => y.Secretary).First().SecretaryCanAssignClassDiary
                    },
                    Treasurer = new UserHomeroomTreasurer
                    {
                        Id = x.Items.Select(y => y.Treasurer).First().Id,
                        Name = x.Items.Select(y => y.Treasurer).First().Name,
                        TreasurerCanAssignClassDiary = x.Items.Select(y => y.Treasurer).First().TreasurerCanAssignClassDiary
                    }
                }).ToList();

            if (items.Count() <= 0)
                throw new NotFoundException("Class officer setting data not found");

            var idSchool = await _dbContext.Entity<MsUserSchool>().
                Where(x => x.IdUser == param.IdBinusian)
                .Select(x => x.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);
            if (idSchool == null)
                throw new NotFoundException("ID School not found");

            var result = await _schoolService.GetSchoolDetail(idSchool);
            var schoolResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

            byte[] logo = default;
            if (!string.IsNullOrEmpty(schoolResult.LogoUrl))
            {
                var blobNameLogo = schoolResult.LogoUrl;
                var blobContainerLogo = await _storageManager.GetOrCreateBlobContainer("school-logo", ct: CancellationToken);
                var blobClientLogo = blobContainerLogo.GetBlobClient(blobNameLogo);

                // generate SAS uri with expire time in 10 minutes
                var sasUri = GenerateSasUri(blobClientLogo);

                using var client = new HttpClient();
                logo = await client.GetByteArrayAsync(sasUri.AbsoluteUri);
            }

            var excelClassOfficer = GenerateExcel(items, items.FirstOrDefault().IdAcademicYear.Description, logo);

            return new FileContentResult(excelClassOfficer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"ClassOfficerSetting.xlsx"
            };
        }

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.AddMonths(1);

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }

        private byte[] GenerateExcel(List<DownloadClassOfficerSettingResult> items, string ayDescription, byte[] logo)
        {
            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(fontBold);

            var titleFontStyle = workbook.CreateFont();
            titleFontStyle.IsBold = true;
            titleFontStyle.FontHeightInPoints = 18;

            var borderCellStyle = workbook.CreateCellStyle();
            borderCellStyle.BorderBottom = BorderStyle.Thin;
            borderCellStyle.BorderLeft = BorderStyle.Thin;
            borderCellStyle.BorderRight = BorderStyle.Thin;
            borderCellStyle.BorderTop = BorderStyle.Thin;

            var headerCellStyle = workbook.CreateCellStyle();
            headerCellStyle.CloneStyleFrom(borderCellStyle);
            headerCellStyle.SetFont(fontBold);

            var titleCellStyle = workbook.CreateCellStyle();
            titleCellStyle.SetFont(titleFontStyle);

            var itemCellStyle = workbook.CreateCellStyle();
            itemCellStyle.CloneStyleFrom(borderCellStyle);

            var sheet = workbook.CreateSheet();

            if (logo != null)
            {
                byte[] dataImg = logo;
                int pictureIndex = workbook.AddPicture(dataImg, NPOI.SS.UserModel.PictureType.PNG);
                ICreationHelper helper = workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row
                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(2, 5);
            }

            // Title
            var rowTitle = sheet.CreateRow(1);
            var cellTitle = rowTitle.CreateCell(3);
            cellTitle.SetCellValue("SUMMARY CLASS OFFICER");
            cellTitle.CellStyle = titleCellStyle;

            var rowAy = sheet.CreateRow(5);
            var cellAy = rowAy.CreateCell(0);
            cellAy.SetCellValue("Academic Year :");

            cellAy = rowAy.CreateCell(1);
            cellAy.SetCellValue($"{ayDescription}");

            // Header
            var rowHeader = sheet.CreateRow(7);
            for (int i = 0; i < 10; i++)
            {
                var cellHeader = rowHeader.CreateCell(i);
                switch (i)
                {
                    case 0:
                        cellHeader.SetCellValue("Academic Year");
                        break;
                    case 1:
                        cellHeader.SetCellValue("Level");
                        break;
                    case 2:
                        cellHeader.SetCellValue("Grade");
                        break;
                    case 3:
                        cellHeader.SetCellValue("Homeroom");
                        break;
                    case 4:
                        cellHeader.SetCellValue("Semester");
                        break;
                    case 5:
                        cellHeader.SetCellValue("Last Modified");
                        break;
                    case 6:
                        cellHeader.SetCellValue("Class Captain");
                        break;
                    case 7:
                        cellHeader.SetCellValue("Assistant Class Captain");
                        break;
                    case 8:
                        cellHeader.SetCellValue("Secretary");
                        break;
                    case 9:
                        cellHeader.SetCellValue("Treasurer");
                        break;
                    default:
                        break;
                }
                cellHeader.CellStyle = headerCellStyle;
            }

            // Items
            int itemRowNumber = 8;
            foreach (var item in items)
            {
                var rowItem = sheet.CreateRow(itemRowNumber++);
                for (int i = 0; i < 10; i++)
                {
                    var cellItem = rowItem.CreateCell(i);
                    switch (i)
                    {
                        case 0:
                            cellItem.SetCellValue(item.IdAcademicYear.Description);
                            break;
                        case 1:
                            cellItem.SetCellValue(item.Level.Code);
                            break;
                        case 2:
                            cellItem.SetCellValue(item.Grade.Code);
                            break;
                        case 3:
                            cellItem.SetCellValue(item.HomeRoom.Code);
                            break;
                        case 4:
                            cellItem.SetCellValue(item.Semester.ToString());
                            break;
                        case 5:
                            if (item.LastModified != null)
                                cellItem.SetCellValue($"{item.LastModified.Value.ToString("dd/MM/yy HH:mm:ss")} ({item.UserModified})");
                            else
                                cellItem.SetCellValue("-");
                            break;
                        case 6:
                            if (item.Captain == null)
                                cellItem.SetCellValue("-");
                            else
                                cellItem.SetCellValue($"{item.Captain.Name} - {item.Captain.Id}");
                            break;
                        case 7:
                            if (item.ViceCaptain == null)
                                cellItem.SetCellValue("-");
                            else
                                cellItem.SetCellValue($"{item.ViceCaptain.Name} - {item.ViceCaptain.Id}");
                            break;
                        case 8:
                            if (item.Secretary == null)
                                cellItem.SetCellValue("-");
                            else
                                cellItem.SetCellValue($"{item.Secretary.Name} - {item.Secretary.Id}");
                            break;
                        case 9:
                            if (item.Treasurer == null)
                                cellItem.SetCellValue("-");
                            else
                                cellItem.SetCellValue($"{item.Treasurer.Name} - {item.Treasurer.Id}");
                            break;
                        default:
                            break;
                    }
                    cellItem.CellStyle = itemCellStyle;
                }
            }

            using (var ms = new MemoryStream())
            {
                ms.Position = 0;
                workbook.Write(ms);

                return ms.ToArray();
            }
        }
    }
}
