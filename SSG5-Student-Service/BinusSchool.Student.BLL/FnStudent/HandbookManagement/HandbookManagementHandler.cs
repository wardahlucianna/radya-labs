

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.HandbookManagement;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.HandbookManagement.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.HandbookManagement
{
    public class HandbookManagementHandler : FunctionsHttpCrudHandler
    {
        private IStudentDbContext _dbContext;

        public HandbookManagementHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetHandbookManagement = await _dbContext.Entity<TrHandbook>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(CancellationToken);

            var GetHandbookManagementViewFor = await _dbContext.Entity<TrHandbookViewFor>()
           .Where(x => ids.Contains(x.IdTrHandbook))
           .ToListAsync(CancellationToken);

            var GetHandbookManagementLevel = await _dbContext.Entity<TrHandbookLevel>()
           .Where(x => ids.Contains(x.IdTrHandbook))
           .ToListAsync(CancellationToken);

            var GetHandbookManagementAttachment = await _dbContext.Entity<TrHandbookAttachment>()
            .Where(x => ids.Contains(x.IdTrHandbook))
            .ToListAsync(CancellationToken);

            GetHandbookManagement.ForEach(x => x.IsActive = false);

            GetHandbookManagementViewFor.ForEach(x => x.IsActive = false);

            GetHandbookManagementLevel.ForEach(x => x.IsActive = false);

            GetHandbookManagementAttachment.ForEach(x => x.IsActive = false);

            _dbContext.Entity<TrHandbook>().UpdateRange(GetHandbookManagement);

            _dbContext.Entity<TrHandbookViewFor>().UpdateRange(GetHandbookManagementViewFor);

            _dbContext.Entity<TrHandbookLevel>().UpdateRange(GetHandbookManagementLevel);

            _dbContext.Entity<TrHandbookAttachment>().UpdateRange(GetHandbookManagementAttachment);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<TrHandbook>()
            .Include(p => p.AcademicYear)
            .Include(p => p.HandbookViewLevel).ThenInclude(q => q.Level)
            .Where(x => x.Id == id)
            .Select(x => new GetHandbookManagementDetailResult
            {
                Id = x.Id,
                AcademicYear = new CodeWithIdVm
                {
                    Id = x.AcademicYear.Id,
                    Code = x.AcademicYear.Code,
                    Description = x.AcademicYear.Description
                },
                ViewFors = x.HandbookViewFors.Where(e => e.IdTrHandbook == id).Select(e => new ViewForHandbookManagement
                {
                    ViewFor = e.For.ToString()
                }).ToList(),
                Levels = x.HandbookViewLevel.Where(e => e.IdTrHandbook == id).Select(e => new CodeWithIdVm
                {
                    Id = e.IdLevel,
                    Code = e.Level.Code,
                    Description = e.Level.Description
                }).ToList(),
                Description = x.Description,
                Title = x.Title,
                Url = x.Url,
                Attachments = x.HandbookAttachment.Where(e => e.IdTrHandbook == id).Select(e => new AttachmentHandbookManagement
                {
                    Id = e.Id,
                    Url = e.Url,
                    OriginalFilename = e.OriginalFilename,
                    FileName = e.Filename,
                    FileSize = e.Filesize,
                    FileType = e.Filetype,
                }).ToList()
            }).FirstOrDefaultAsync(CancellationToken);

            foreach (var item in query.ViewFors)
            {
                item.Index = (int)Enum.Parse(typeof(HandbookFor), item.ViewFor);
            }

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetHandbookManagementRequest>();

            var columns = new[] { "AcademicYear", "ViewFors", "IdsLevel", "Title" };

            var query = _dbContext.Entity<TrHandbook>()
                       .Include(e => e.AcademicYear)
                       .Include(e => e.HandbookViewFors)
                       .Include(e => e.HandbookViewLevel).ThenInclude(e => e.Level)
                       .Where(e => e.IdAcademicYear == param.IdAcademicYear);

            //filter
            if (!string.IsNullOrEmpty(param.Idlevel))
                query = query.Where(x => x.HandbookViewLevel.Any(y => y.IdLevel == param.Idlevel)); // ini  untuk 
            if (!string.IsNullOrEmpty(param.ViewFor.ToString()))
                query = query.Where(x => x.HandbookViewFors.Any(y => param.ViewFor.Contains(y.For) )); // ini  untuk 
            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Title, param.SearchPattern()) ||
                EF.Functions.Like(x.Description, param.SearchPattern()));
            }

            if (!param.IsHandbookForm)
            {
                if (!string.IsNullOrEmpty(param.IdUser))
                    query = query.Where(x => x.UserIn == param.IdUser);
            }
            else
            {
                List<string> IdlevelTeacher = new List<string>();
                List<string> child = new List<string>();

                var GetHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                            .Where(e => e.IdBinusian == param.IdUser && e.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                            .ToListAsync(CancellationToken);

                IdlevelTeacher.AddRange(GetHomeroomTeacher.Select(e => e.Homeroom.Grade.IdLevel));

                var GetLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                            .Include(e => e.Lesson).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                            .Where(e => e.IdUser == param.IdUser && e.Lesson.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                            .ToListAsync(CancellationToken);

                IdlevelTeacher.AddRange(GetLessonTeacher.Select(e => e.Lesson.Grade.IdLevel));

                var GetUserRole = await _dbContext.Entity<MsUserRole>()
                            .Include(e => e.Role).ThenInclude(e => e.RoleGroup)
                            .Where(e => e.IdUser == param.IdUser)
                            .ToListAsync(CancellationToken);

                var GetStaff = GetUserRole.Where(e => e.Role.RoleGroup.Code == "STAFF");
                var GetParent = GetUserRole.Where(e => e.Role.RoleGroup.Code == "PARENT");

                var GetHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                        .Where(e => e.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                        .ToListAsync(CancellationToken);

                var GetStudent = GetHomeroomStudent
                                .Where(e => e.IdStudent == param.IdUser)
                                .FirstOrDefault();

                #region get student by parent
                if (param.IdUser.Substring(0, 1) == "P")
                {
                    var IdStudentByParent = param.IdUser.Substring(1);
                    var GetSibling = await _dbContext.Entity<MsSiblingGroup>()
                                    .Where(e => e.IdStudent == IdStudentByParent)
                                    .FirstOrDefaultAsync(CancellationToken);

                    var GetChild = await _dbContext.Entity<MsSiblingGroup>()
                                    .Where(e => e.Id == GetSibling.Id)
                                    .Select(e => e.IdStudent.Trim())
                                    .ToListAsync(CancellationToken);

                    child.AddRange(GetChild);
                }
                var IdLevelParent = GetHomeroomStudent.Where(e => child.Contains(e.IdStudent)).Select(e => e.Homeroom.Grade.IdLevel).Distinct().ToList();
                #endregion

                if (IdlevelTeacher.Any())
                    query = query.Where(x => x.HandbookViewLevel.Any(e => IdlevelTeacher.Contains(e.IdLevel)) && x.HandbookViewFors.Any(e => e.For == HandbookFor.Teacher));
                else if (GetStaff.Any())
                    query = query.Where(x => x.HandbookViewFors.Any(e => e.For == HandbookFor.Staff));
                else if (GetParent.Any())
                    query = query.Where(x => x.HandbookViewFors.Any(e => e.For == HandbookFor.Parent) && x.HandbookViewLevel.Any(e => IdLevelParent.Contains(e.IdLevel)));
                else if (GetStudent != null)
                    query = query.Where(x => x.HandbookViewFors.Any(e => e.For == HandbookFor.Student) && x.HandbookViewLevel.Any(e => e.IdLevel == GetStudent.Homeroom.Grade.IdLevel));
            }

            ////ordering
            query = query.OrderByDescending(x => x.DateIn);

            switch (param.OrderBy)
            {
                case "academicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "title":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Title)
                        : query.OrderBy(x => x.Title);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                var data = result.Select(x => new GetHandbookManagementQueryResult
                {
                    Id = x.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.AcademicYear.Id,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    ViewFors = x.HandbookViewFors.Select(e => new ViewForHandbookManagement
                    {
                        ViewFor = e.For.ToString()
                    }).ToList(),
                    Levels = x.HandbookViewLevel.Select(e => new CodeWithIdVm
                    {
                        Id = e.IdLevel,
                        Code = e.Level.Code,
                        Description = e.Level.Description
                    }).ToList(),
                    Description = x.Description,
                    Title = x.Title,
                    Date = x.DateIn.Value.Date.ToString("dd MMM yyyy")
                }).ToList();

                items = data.Select(x => new GetHandbookManagementResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    ViewFors = GetViewFors(x.ViewFors),
                    Levels = GetLevels(x.Levels),
                    Description = x.Description,
                    Title = x.Title,
                    Date = x.Date
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                var data = result.Select(x => new GetHandbookManagementQueryResult
                {
                    Id = x.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.AcademicYear.Id,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    ViewFors = x.HandbookViewFors.Select(e => new ViewForHandbookManagement
                    {
                        ViewFor = e.For.ToString()
                    }).ToList(),
                    Levels = x.HandbookViewLevel.Select(e => new CodeWithIdVm
                    {
                        Id = e.IdLevel,
                        Code = e.Level.Code,
                        Description = e.Level.Description
                    }).ToList(),
                    Description = x.Description,
                    Title = x.Title,
                    Date = x.DateIn.Value.Date.ToString("dd MMM yyyy")
                }).ToList();

                items = data.Select(x => new GetHandbookManagementResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    ViewFors = GetViewFors(x.ViewFors),
                    Levels = GetLevels(x.Levels),
                    Description = x.Description,
                    Title = x.Title,
                    Date = x.Date
                }).ToList();
            }
            var count = param.CanCountWithoutFetchDb(items.Count)
            ? items.Count
            : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));

        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddHandbookManagementRequest, AddHandbookManagementValidator>();

            var existsData = _dbContext.Entity<TrHandbook>()
                .Any(x => x.AcademicYear.Id == body.IdAcademicYear && x.Title == body.Title);

            if (existsData)
            {
                throw new BadRequestException($"Title { body.Title} already exists.");
            }

            if (!string.IsNullOrEmpty(body.Url))
            {
                if (body.Url.StartsWith("http://") == false && body.Url.StartsWith("https://") == false)
                {
                    body.Url = "http://" + body.Url;
                }
            }

            //var existsData2 = _dbContext.Entity<TrHandbook>()
            //    .Any(x => x.AcademicYear.Id == body.IdAcademicYear && x.Url == body.Url);

            //if (existsData2)
            //{
            //    throw new BadRequestException($"Url { body.Url} already exists.");
            //}


            var idHandbookManagement = Guid.NewGuid().ToString();

            var newHandbookManagement = new TrHandbook
            {
                Id = idHandbookManagement,
                IdAcademicYear = body.IdAcademicYear,
                Title = body.Title,
                Description = body.Description == null ? "" : body.Description,
                Url = body.Url == null ? "" : body.Url,
            };

            if (body.ViewFors != null)
            {
                foreach (var viewFor in body.ViewFors)
                {
                    var newHandbookManagementViewFor = new TrHandbookViewFor
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdTrHandbook = idHandbookManagement,
                        For = viewFor.ViewFor
                    };

                    _dbContext.Entity<TrHandbookViewFor>().Add(newHandbookManagementViewFor);
                }
            }

            if (body.IdsLevel != null)
            {
                if (body.ViewFors != null)
                {
                    var isInput = false;
                    foreach (var viewFor in body.ViewFors) //pastikan input level jika selain untuk staff
                    {
                        if (viewFor.ViewFor != HandbookFor.Staff)
                        {
                            isInput = true;
                            break;
                        }
                    }
                    if (isInput)
                    {
                        foreach (var levelFor in body.IdsLevel)
                        {
                            var newHandbookManagementLevel = new TrHandbookLevel
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdTrHandbook = idHandbookManagement,
                                IdLevel = levelFor.Id
                            };

                            _dbContext.Entity<TrHandbookLevel>().Add(newHandbookManagementLevel);
                        }
                    }
                }

            }

            if (body.Attachments != null)
            {
                foreach (var ItemAttachment in body.Attachments)
                {
                    var newHandbookManagementAttachment = new TrHandbookAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdTrHandbook = newHandbookManagement.Id,
                        OriginalFilename = ItemAttachment.OriginalFilename,
                        Url = ItemAttachment.Url,
                        Filename = ItemAttachment.FileName,
                        Filetype = ItemAttachment.FileType,
                        Filesize = ItemAttachment.FileSize,
                    };
                    _dbContext.Entity<TrHandbookAttachment>().Add(newHandbookManagementAttachment);

                }
            }

            _dbContext.Entity<TrHandbook>().Add(newHandbookManagement);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateHandbookManagementRequest, UpdateHandbookManagementValidator>();
            var GetHandbookManagement = await _dbContext.Entity<TrHandbook>().Where(e => e.Id == body.IdHandbookManagement).SingleOrDefaultAsync(CancellationToken);
            var GetHandbookManagementViewFor = await _dbContext.Entity<TrHandbookViewFor>().Where(e => e.IdTrHandbook == body.IdHandbookManagement).ToListAsync(CancellationToken);
            var GetHandbookManagementLevel = await _dbContext.Entity<TrHandbookLevel>().Where(e => e.IdTrHandbook == body.IdHandbookManagement).ToListAsync(CancellationToken);
            var GetHandbookManagementAttachment = await _dbContext.Entity<TrHandbookAttachment>().Where(e => e.IdTrHandbook == body.IdHandbookManagement).ToListAsync(CancellationToken);

            if (GetHandbookManagement is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Hanbook Management"], "Id", body.IdHandbookManagement));
            }

            //update data in TrPersonalWellBeing
            if (GetHandbookManagement.Title != body.Title)
            {
                var checkTitle = _dbContext.Entity<TrHandbook>().Where(x => x.AcademicYear.Id == GetHandbookManagement.IdAcademicYear && x.Title == body.Title).FirstOrDefault();

                if (checkTitle != null)
                {
                    throw new BadRequestException($"Title {body.Title} already exists");
                }

                GetHandbookManagement.Title = body.Title;
            }

            if (!string.IsNullOrEmpty(body.Url))
            {
                if (body.Url.StartsWith("http://") == false && body.Url.StartsWith("https://") == false)
                {
                    body.Url = "http://" + body.Url;
                }
            }
            //if (GetHandbookManagement.Url != body.Url)
            //{
            //    var checkLink = _dbContext.Entity<TrHandbook>().Where(x => x.AcademicYear.Id == GetHandbookManagement.IdAcademicYear && x.Url == body.Url).FirstOrDefault();

            //    if (checkLink != null)
            //    {
            //        throw new BadRequestException($"Url {body.Url} already exists");
            //    }
            //}

            //update data in TrHandbook
            GetHandbookManagement.Url = body.Url == null ? "" : body.Url;
            GetHandbookManagement.Description = body.Description == null ? "" : body.Description;
            _dbContext.Entity<TrHandbook>().Update(GetHandbookManagement);

            //update data in TrHandbookViewFor
            //remove View For
            foreach (var ItemViewFor in GetHandbookManagementViewFor)
            {
                var ExsisBodyViewForId = body.ViewFors.Any(e => e.ViewFor == ItemViewFor.For);

                if (!ExsisBodyViewForId)
                {
                    ItemViewFor.IsActive = false;
                    _dbContext.Entity<TrHandbookViewFor>().Update(ItemViewFor);
                }
            }
            ////Add View For
            if (body.ViewFors != null)
            {
                foreach (var viewFor in body.ViewFors)
                {
                    var ExsistdbId = GetHandbookManagementViewFor.Where(e => e.For == viewFor.ViewFor && e.IdTrHandbook == GetHandbookManagement.Id).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newGetHandbookManagementViewFor = new TrHandbookViewFor
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdTrHandbook = GetHandbookManagement.Id,
                            For = viewFor.ViewFor
                        };

                        _dbContext.Entity<TrHandbookViewFor>().Add(newGetHandbookManagementViewFor);
                    }
                }
            }

            //update data in TrHandbookLevel
            //remove level
            foreach (var ItemLevel in GetHandbookManagementLevel)
            {
                var ExsisBodyLevelId = body.IdsLevel.Any(e => e.Id == ItemLevel.IdLevel);

                if (!ExsisBodyLevelId)
                {
                    ItemLevel.IsActive = false;
                    _dbContext.Entity<TrHandbookLevel>().Update(ItemLevel);
                }
            }
            ////Add level
            ///
            if (body.IdsLevel != null)
            {
                if (body.ViewFors != null)
                {
                    var isInput = false;
                    foreach (var viewFor in body.ViewFors) //pastikan input level jika selain untuk staff
                    {
                        if (viewFor.ViewFor != HandbookFor.Staff)
                        {
                            isInput = true;
                            break;
                        }
                    }
                    if (isInput)
                    {
                        foreach (var Levelid in body.IdsLevel)
                        {
                            var ExsistdbId = GetHandbookManagementLevel.Where(e => e.IdLevel == Levelid.Id && e.IdTrHandbook == GetHandbookManagement.Id).SingleOrDefault();
                            if (ExsistdbId is null)
                            {
                                var newGetHandbookManagementLevel = new TrHandbookLevel
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdTrHandbook = GetHandbookManagement.Id,
                                    IdLevel = Levelid.Id
                                };

                                _dbContext.Entity<TrHandbookLevel>().Add(newGetHandbookManagementLevel);
                            }
                        }
                    }
                }

            }

            //update data in TrPersonalWellBeingAttachment
            //remove attachment
            foreach (var ItemAttachment in GetHandbookManagementAttachment)
            {
                var ExsisBodyAttachment = body.Attachments.Any(e => e.Id == ItemAttachment.Id);

                if (!ExsisBodyAttachment)
                {
                    ItemAttachment.IsActive = false;
                    _dbContext.Entity<TrHandbookAttachment>().Update(ItemAttachment);
                }
            }

            //Add attachment
            foreach (var ItemAttachment in body.Attachments.Where(e => e.Id == null || e.Id == "").ToList())
            {
                var newGetHandbookManagementAttachment = new TrHandbookAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdTrHandbook = GetHandbookManagement.Id,
                    OriginalFilename = ItemAttachment.OriginalFilename,
                    Url = ItemAttachment.Url,
                    Filename = ItemAttachment.FileName,
                    Filetype = ItemAttachment.FileType,
                    Filesize = ItemAttachment.FileSize,
                };
                _dbContext.Entity<TrHandbookAttachment>().Add(newGetHandbookManagementAttachment);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        public string GetViewFors(List<ViewForHandbookManagement> ViewForData)
        {
            var viewFors = "";

            foreach (var data in ViewForData)
            {
                if (!string.IsNullOrEmpty(data.ViewFor))
                    viewFors += viewFors == "" ? data.ViewFor : $", {data.ViewFor}";
            }

            return viewFors;
        }

        public string GetLevels(List<CodeWithIdVm> LevelData)
        {
            var Levels = "";

            foreach (var data in LevelData)
            {
                if (!string.IsNullOrEmpty(data.Code))
                    Levels += Levels == "" ? data.Code : $", {data.Code}";
            }

            return Levels;
        }
    }
}
