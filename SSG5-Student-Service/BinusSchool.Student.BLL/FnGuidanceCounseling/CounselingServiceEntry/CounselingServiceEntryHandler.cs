using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class CounselingServiceEntryHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IStudentDbContext _dbContext;

        public CounselingServiceEntryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var counselingServiceEntry = await _dbContext.Entity<TrCounselingServicesEntry>()
                .Where(x => ids.Contains(x.Id)).ToListAsync(CancellationToken);

            var counselingServiceEntryConcern = await _dbContext.Entity<TrCounselingServicesEntryConcern>()
                .Where(x => ids.Contains(x.IdCounselingServicesEntry)).ToListAsync(CancellationToken);

            var counselingServiceEntryAttachment = await _dbContext.Entity<TrCounselingServicesEntryAttachment>()
                .Where(x => ids.Contains(x.IdCounselingServicesEntry)).ToListAsync(CancellationToken);

            counselingServiceEntry.ForEach(x => x.IsActive = false);

            counselingServiceEntryConcern.ForEach(x => x.IsActive = false);

            counselingServiceEntryAttachment.ForEach(x => x.IsActive = false);

            _dbContext.Entity<TrCounselingServicesEntry>().UpdateRange(counselingServiceEntry);

            _dbContext.Entity<TrCounselingServicesEntryConcern>().UpdateRange(counselingServiceEntryConcern);

            _dbContext.Entity<TrCounselingServicesEntryAttachment>().UpdateRange(counselingServiceEntryAttachment);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected async override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<TrCounselingServicesEntry>()
                        .Include(f => f.CounselingServicesEntryConcern)
                        .Include(f => f.CounselingServicesEntryConcern).ThenInclude(f => f.ConcernCategory)
                        .Include(f => f.CounselingServicesEntryAttachment)
                        .Include(f => f.CounselingCategory)
                        .Include(f => f.Counselor)
                        .Include(f => f.Counselor).ThenInclude(f => f.User)
                        .Include(f => f.AcademicYear)
                        .Include(f => f.Student)
                        .Include(f => f.Student).ThenInclude(f => f.StudentParents).ThenInclude(f => f.Parent)
                        .Include(f => f.Student).ThenInclude(f => f.MsHomeroomStudents).ThenInclude(f => f.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom)
                        .Include(f => f.Student).ThenInclude(f => f.MsHomeroomStudents).ThenInclude(f => f.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel)
                        .Where(x => x.Id == id)
                        .Select(x => new GetCounselingServiceEntryDetailResult
                        {
                            Id = x.Id,
                            AcademicYear = new CodeWithIdVm
                            {
                                Id = x.AcademicYear.Id,
                                Code = x.AcademicYear.Code,
                                Description = x.AcademicYear.Description
                            },
                            Counselor = new NameValueVm
                            {
                                Id = x.Counselor.Id,
                                Name = x.Counselor.User.DisplayName
                            },
                            CounselingCategory = new NameValueVm
                            {
                                Id = x.CounselingCategory.Id,
                                Name = x.CounselingCategory.CounselingCategoryName
                            },
                            CounselingDate = x.DateTime,
                            StudentName = string.IsNullOrEmpty(x.Student.LastName) ? $"{x.Student.FirstName}{x.Student.MiddleName}" : $"{x.Student.FirstName}{x.Student.LastName}",
                            StudentId = x.Student.Id,
                            PictureStudent = string.Empty,
                            ReferredBy = x.ReferredBy,
                            BriefReport = x.BriefReport,
                            FollowUp = x.FollowUp,
                            CounselingWith = x.CounselingWith,
                            Level = new CodeWithIdVm
                            {
                                Id = x.Student.MsHomeroomStudents.FirstOrDefault().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id,
                                Code = x.Student.MsHomeroomStudents.FirstOrDefault().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Code,
                                Description = x.Student.MsHomeroomStudents.FirstOrDefault().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Description,
                            },
                            Grade = new CodeWithIdVm
                            {
                                Id = x.Student.MsHomeroomStudents.FirstOrDefault().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id,
                                Code = x.Student.MsHomeroomStudents.FirstOrDefault().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code,
                                Description = x.Student.MsHomeroomStudents.FirstOrDefault().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Description,
                            },
                            HomeRoom = new ItemValueVm
                            {
                                Id = x.Student.MsHomeroomStudents.FirstOrDefault().IdHomeroom,
                                Description = $"{x.Student.MsHomeroomStudents.FirstOrDefault().Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code}{x.Student.MsHomeroomStudents.FirstOrDefault().Homeroom.MsGradePathwayClassroom.Classroom.Code}"
                            },
                            ConcernCategory = x.CounselingServicesEntryConcern.Select(a => a.ConcernCategory).Select(b => new NameValueVm
                            {
                                Id = b.Id,
                                Name = b.ConcernCategoryName
                            }).ToList(),
                            Parents = x.Student.StudentParents.Select(y => y.Parent).Select(z => new StudentParent
                            {
                                IdParent = z.Id,
                                IdBinusian = z.IdBinusian,
                                ParentName = string.IsNullOrEmpty(z.LastName) ? $"{z.FirstName} {z.MiddleName}" : $"{z.FirstName} {z.LastName}"
                            }).ToList(),
                            Attachments = x.CounselingServicesEntryAttachment.Select(y => new Attachment
                            {
                                Url = y.Url,
                                FileName = y.FileName,
                                FileSize = y.FileSize,
                                FileType = y.FileType,
                                OriginalFilename = y.OriginalName
                            }).ToList()
                        }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetCounselingServiceEntryRequest>();

            var columns = new[] { "academicYear", "studentName", "idBinusian", "level", "grade", "homeRoom", "counselingCategory", "counselorName", "date" };

            var query = _dbContext.Entity<TrCounselingServicesEntry>()
                        .Include(f => f.CounselingServicesEntryConcern)
                        .Include(f => f.CounselingServicesEntryConcern).ThenInclude(f => f.ConcernCategory)
                        .Include(f => f.CounselingServicesEntryAttachment)
                        .Include(f => f.CounselingCategory)
                        .Include(f => f.Counselor)
                        .Include(f => f.Counselor).ThenInclude(f => f.User)
                        .Include(f => f.AcademicYear)
                        .Include(f => f.Student)
                        .Include(f => f.Student).ThenInclude(f => f.StudentParents).ThenInclude(f => f.Parent)
                        .Include(f => f.Student).ThenInclude(f => f.MsHomeroomStudents).ThenInclude(f => f.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom)
                        .Include(f => f.Student).ThenInclude(f => f.MsHomeroomStudents).ThenInclude(f => f.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel)
                        .Where(f => f.AcademicYear.Id == param.IdAcademicYear)
                        .Select(x => new
                        {
                            Id = x.Id,
                            Semester = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.Semester).FirstOrDefault(),
                            AcademicYear = new CodeWithIdVm
                            {
                                Id = x.AcademicYear.Id,
                                Code = x.AcademicYear.Code,
                                Description = x.AcademicYear.Description
                            },
                            CounselorName = x.Counselor.User.DisplayName,
                            StudentName = (x.Student.FirstName == null ? "" : x.Student.FirstName) + (x.Student.MiddleName == null ? "" : " " + x.Student.MiddleName) + (x.Student.LastName == null ? "" : " " + x.Student.LastName)/*NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)*/,
                            IdBinusian = x.Student.IdBinusian,
                            Level = new CodeWithIdVm
                            {
                                Id = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id).FirstOrDefault(),
                                Code = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Code).FirstOrDefault(),
                                Description = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Description).FirstOrDefault(),
                            },
                            Grade = new CodeWithIdVm
                            {
                                Id = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id).FirstOrDefault(),
                                Code = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code).FirstOrDefault(),
                                Description = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Description).FirstOrDefault(),
                            },
                            HomeRoom = new
                            {
                                Id = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.Id).FirstOrDefault(),
                                GradeOrder = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.OrderNumber).FirstOrDefault(),
                                Grade = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code).FirstOrDefault(),
                                ClassRoom = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.Classroom.Code).FirstOrDefault()
                            },
                            CounselingCategory = new NameValueVm
                            {
                                Id = x.CounselingCategory.Id,
                                Name = x.CounselingCategory.CounselingCategoryName
                            },
                            Date = x.DateTime
                        }).AsQueryable();

            //filter
            //if (!string.IsNullOrEmpty(param.IdAcademicYear))
            //{
            //    query = query.Where(x => EF.Functions.Like(x.AcademicYear.Id, param.IdAcademicYear));
            //}
            if (param.Semester.HasValue)
            {
                query = query.Where(x => x.Semester == param.Semester.Value);
            }
            if (!string.IsNullOrEmpty(param.IdLevel))
            {
                query = query.Where(x => EF.Functions.Like(x.Level.Id, param.IdLevel));
            }
            if (!string.IsNullOrEmpty(param.IdGrade))
            {
                query = query.Where(x => EF.Functions.Like(x.Grade.Id, param.IdGrade));
            }
            if (!string.IsNullOrEmpty(param.IdHomeRoom))
            {
                query = query.Where(x => EF.Functions.Like(x.HomeRoom.Id, param.IdHomeRoom));
            }
            if (!string.IsNullOrEmpty(param.IdCounselingCategory))
            {
                query = query.Where(x => EF.Functions.Like(x.CounselingCategory.Id, param.IdCounselingCategory));
            }
            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where
                    (x => x.StudentName.Contains(param.Search)
                    || x.IdBinusian.Contains(param.Search)
                    || x.CounselingCategory.Name.Contains(param.Search));
            }

            //ordering
            switch (param.OrderBy)
            {
                case "academicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear.Description)
                        : query.OrderBy(x => x.AcademicYear.Description);
                    break;
                case "studentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "idBinusian":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdBinusian)
                        : query.OrderBy(x => x.IdBinusian);
                    break;
                case "level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level.Description)
                        : query.OrderBy(x => x.Level.Description);
                    break;
                case "grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade.Description)
                        : query.OrderBy(x => x.Grade.Description);
                    break;
                case "homeRoom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.HomeRoom.GradeOrder).ThenByDescending(x => x.HomeRoom.ClassRoom)
                        : query.OrderBy(x => x.HomeRoom.GradeOrder).ThenBy(x => x.HomeRoom.ClassRoom);
                    break;
                case "counselingCategory":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CounselingCategory.Name)
                        : query.OrderBy(x => x.CounselingCategory.Name);
                    break;
                case "counselorName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.CounselorName)
                        : query.OrderBy(x => x.CounselorName);
                    break;
                case "date":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Date)
                        : query.OrderBy(x => x.Date);
                    break;
            }

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = query
                    .Select(x => new ItemValueVm(x.Id, x.Id))
                    .ToList();
            }
            else
            {
                var result = await query
                .SetPagination(param).ToListAsync(CancellationToken);

                items = result
                .Select(x => new GetCounselingServiceEntryResult
                {
                    Id = x.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.AcademicYear.Id,
                        Code = x.AcademicYear.Code,
                        Description = x.AcademicYear.Description
                    },
                    StudentName = x.StudentName,
                    IdBinusian = x.IdBinusian,
                    Level = new CodeWithIdVm
                    {
                        Id = x.Level.Id,
                        Code = x.Level.Code,
                        Description = x.Level.Description
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = x.Grade.Id,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description
                    },
                    HomeRoom = new ItemValueVm
                    {
                        Id = x.HomeRoom.Id,
                        Description = $"{x.HomeRoom.Grade}{x.HomeRoom.ClassRoom}"
                    },
                    CounselingCategory = new NameValueVm
                    {
                        Id = x.CounselingCategory.Id,
                        Name = x.CounselingCategory.Name
                    },
                    CounselorName = x.CounselorName,
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
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<AddCounselingServiceEntryRequest, AddCounselingServiceEntryValidator>();

            var idCounselingServiceEntry = Guid.NewGuid().ToString();

            var data = new TrCounselingServicesEntry
            {
                Id = idCounselingServiceEntry,
                IdAcademicYear = body.IdAcademicYear,
                IdCounselingCategory = body.IdCounselingCategory,
                IdStudent = body.IdStudent,
                DateTime = DateTime.Parse(body.Date + " " + body.Time),
                ReferredBy = body.ReferredBy,
                BriefReport = body.BriefReport,
                FollowUp = body.FollowUp
            };

            #region get id counselor by user id

            var studentGrade = await _dbContext.Entity<MsHomeroom>()
                                               .Include(x => x.Grade).ThenInclude(x => x.MsLevel)
                                               .Where(x => x.HomeroomStudents.Any(y => y.IdStudent == body.IdStudent) && x.Grade.MsLevel.IdAcademicYear == body.IdAcademicYear)
                                               .Select(x => x.Grade)
                                               .FirstOrDefaultAsync(CancellationToken);
            if (studentGrade is null)
                throw new BadRequestException("Student grade is not found");

            var idCounselor = await _dbContext.Entity<MsCounselor>()
                                              .Where(x => x.IdAcademicYear == body.IdAcademicYear
                                                          && x.IdUser == body.IdCounselor
                                                          && x.CounselorGrade.Any(y => y.IdGrade == studentGrade.Id))
                                              .Select(x => x.Id)
                                              .FirstOrDefaultAsync(CancellationToken);

            if (idCounselor == null)
            {
                throw new BadRequestException("Only counselor can entry counseling service");
                #region Get Position User
                //List<string> avaiablePosition = new List<string>();
                //var positionUser = await _dbContext.Entity<TrNonTeachingLoad>().Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.Position)
                //    .Where(x => x.IdUser == body.IdCounselor
                //                && x.MsNonTeachingLoad.IdAcademicYear == body.IdAcademicYear)
                //    .Select(x => new
                //    {
                //        x.Data,
                //        x.MsNonTeachingLoad.TeacherPosition.Position.Code
                //    }).ToListAsync(CancellationToken);
                //if (positionUser.Count == 0)
                //    throw new BadRequestException($"You're not counselor or head of gc for this student");

                //foreach (var pu in positionUser)
                //{
                //    avaiablePosition.Add(pu.Code);
                //}
                //if (avaiablePosition.Where(x => x == PositionConstant.VicePrincipal).Count() == 0)
                //    throw new BadRequestException($"You're not counselor or head of gc for this student");

                //var levelIdPositions = new List<string>();
                //if (positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).ToList() != null && positionUser.Where(y => y.Code == PositionConstant.VicePrincipal).Count() > 0)
                //{
                //    var Principal = positionUser.Where(x => x.Code == PositionConstant.VicePrincipal).ToList();

                //    foreach (var item in Principal)
                //    {
                //        var _dataNewLH = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                //        _dataNewLH.TryGetValue("Level", out var _levelLH);
                //        levelIdPositions.Add(_levelLH.Id);
                //    }

                //}
                //if (!levelIdPositions.Contains(studentGrade.IdLevel))
                //    throw new BadRequestException($"You're not counselor or head of gc for this student");
                #endregion
            }

            data.IdCounselor = idCounselor;

            #endregion

            #region conseling with
            //get parent
            var getParent = (from student in _dbContext.Entity<MsStudent>()
                             join studentParent in _dbContext.Entity<MsStudentParent>() on student.Id equals studentParent.IdStudent
                             join parent in _dbContext.Entity<MsParent>() on studentParent.IdParent equals parent.Id
                             where student.Id == body.IdStudent
                             select new
                             {
                                 parent
                             });

            if (body.CounselingWith == CounselingWith.BothParent.ToString())
            {
                data.CounselingWith = CounselingWith.BothParent;

                foreach (var item in getParent)
                {
                    if (item.parent.Gender == Gender.Male)
                    {
                        data.FatherIdParent = item.parent.Id;
                    }
                    if (item.parent.Gender == Gender.Female)
                    {
                        data.MotherIdParent = item.parent.Id;
                    }
                }
            }

            if (body.CounselingWith == CounselingWith.Father.ToString())
            {
                data.CounselingWith = CounselingWith.Father;

                foreach (var item in getParent)
                {
                    if (item.parent.Gender == Gender.Male)
                    {
                        data.FatherIdParent = item.parent.Id;
                    }
                }
            }

            if (body.CounselingWith == CounselingWith.Mother.ToString())
            {
                data.CounselingWith = CounselingWith.Mother;

                foreach (var item in getParent)
                {
                    if (item.parent.Gender == Gender.Female)
                    {
                        data.MotherIdParent = item.parent.Id;
                    }
                }
            }

            if (body.CounselingWith == CounselingWith.Student.ToString())
            {
                data.CounselingWith = CounselingWith.Student;
            }
            #endregion

            #region concern category
            if (body.ConcernCategory != null)
            {
                foreach (var item in body.ConcernCategory)
                {
                    var dataConcern = new TrCounselingServicesEntryConcern
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdCounselingServicesEntry = idCounselingServiceEntry,
                        IdConcernCategory = item
                    };

                    _dbContext.Entity<TrCounselingServicesEntryConcern>().Add(dataConcern);
                }
            }
            #endregion

            #region attachments
            if (body.Attachements != null)
            {
                foreach (var item in body.Attachements)
                {
                    var dataAttachment = new TrCounselingServicesEntryAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdCounselingServicesEntry = idCounselingServiceEntry,
                        OriginalName = item.OriginalFilename,
                        FileName = item.FileName,
                        FileSize = item.FileSize,
                        Url = item.Url,
                        FileType = item.FileType
                    };

                    _dbContext.Entity<TrCounselingServicesEntryAttachment>().Add(dataAttachment);
                }
            }
            #endregion

            _dbContext.Entity<TrCounselingServicesEntry>().Add(data);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateCounselingServiceEntryRequest, UpdateCounselingServiceEntryValidator>();

            var existData = await _dbContext.Entity<TrCounselingServicesEntry>()
                .Where(x => x.Id == body.Id).FirstOrDefaultAsync(CancellationToken);

            if (existData is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Counseling Service Entry"], "Id", body.Id));
            }

            var newDateTime = DateTime.Parse(body.Date + " " + body.Time);

            if (newDateTime != existData.DateTime)
            {
                existData.DateTime = newDateTime;
            }
            if (body.IdCounselingCategory != existData.IdCounselingCategory)
            {
                existData.IdCounselingCategory = body.IdCounselingCategory;
            }
            if (body.ReferredBy != existData.ReferredBy)
            {
                existData.ReferredBy = body.ReferredBy;
            }
            if (body.BriefReport != existData.BriefReport)
            {
                existData.BriefReport = body.BriefReport;
            }
            if (body.FollowUp != existData.FollowUp)
            {
                existData.FollowUp = body.FollowUp;
            }

            #region update parent
            //get parent
            var getParent = (from student in _dbContext.Entity<MsStudent>()
                             join studentParent in _dbContext.Entity<MsStudentParent>() on student.Id equals studentParent.IdStudent
                             join parent in _dbContext.Entity<MsParent>() on studentParent.IdParent equals parent.Id
                             where student.Id == body.IdStudent
                             select new
                             {
                                 parent
                             });

            if (body.CounselingWith != existData.CounselingWith.ToString())
            {
                if (body.CounselingWith == CounselingWith.BothParent.ToString())
                {
                    existData.CounselingWith = CounselingWith.BothParent;

                    foreach (var item in getParent)
                    {
                        if (item.parent.Gender == Gender.Male)
                        {
                            existData.FatherIdParent = item.parent.Id;
                        }
                        if (item.parent.Gender == Gender.Female)
                        {
                            existData.MotherIdParent = item.parent.Id;
                        }
                    }
                }

                if (body.CounselingWith == CounselingWith.Father.ToString())
                {
                    existData.CounselingWith = CounselingWith.Father;

                    foreach (var item in getParent)
                    {
                        if (item.parent.Gender == Gender.Male)
                        {
                            existData.FatherIdParent = item.parent.Id;

                            existData.MotherIdParent = null;
                        }
                    }
                }

                if (body.CounselingWith == CounselingWith.Mother.ToString())
                {
                    existData.CounselingWith = CounselingWith.Mother;

                    foreach (var item in getParent)
                    {
                        if (item.parent.Gender == Gender.Female)
                        {
                            existData.MotherIdParent = item.parent.Id;

                            existData.FatherIdParent = null;
                        }
                    }
                }

                if (body.CounselingWith == CounselingWith.Student.ToString())
                {
                    existData.CounselingWith = CounselingWith.Student;
                }
            }
            #endregion

            await UpdateCounselingCategory(body.Id, body.ConcernCategory);

            await UpdateAttachment(body.Id, body.Attachements);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        public async Task UpdateCounselingCategory(string id, List<string> ConcernCategory)
        {
            var tempData = await _dbContext.Entity<TrCounselingServicesEntryConcern>()
                .Where(x => x.IdCounselingServicesEntry == id).ToListAsync(CancellationToken);

            //delete data
            var counselingDeleted = tempData.Where(x => ConcernCategory.All(a => a != x.IdConcernCategory)).ToList();
            if (counselingDeleted.Any())
            {
                counselingDeleted.ForEach(e => e.IsActive = false);
            }

            //insert new data
            var newCounselingDatas = ConcernCategory.Where(x => tempData.All(a => a.IdConcernCategory != x)).ToList();
            if (newCounselingDatas.Any())
            {
                foreach (var newCounselingData in newCounselingDatas)
                {
                    await _dbContext.Entity<TrCounselingServicesEntryConcern>().AddAsync(new TrCounselingServicesEntryConcern
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdCounselingServicesEntry = id,
                        IdConcernCategory = newCounselingData
                    });
                }
            }
        }

        public async Task UpdateAttachment(string id, List<UpdateAttachment> attachment)
        {
            var tempData = await _dbContext.Entity<TrCounselingServicesEntryAttachment>()
               .Where(x => x.IdCounselingServicesEntry == id).ToListAsync(CancellationToken);

            //delete data
            var attachmentDeleted = tempData.Where(x => attachment.All(a => a.FileName != x.FileName)).ToList();
            if (attachmentDeleted.Any())
            {
                attachmentDeleted.ForEach(e => e.IsActive = false);
            }

            //insert new data
            var newAttahcments = attachment.Where(x => tempData.All(a => a.FileName != x.FileName)).ToList();
            if (newAttahcments.Any())
            {
                foreach (var newAttahcment in newAttahcments)
                {
                    await _dbContext.Entity<TrCounselingServicesEntryAttachment>().AddAsync(new TrCounselingServicesEntryAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdCounselingServicesEntry = id,
                        OriginalName = newAttahcment.OriginalFilename,
                        FileName = newAttahcment.FileName,
                        FileSize = newAttahcment.FileSize,
                        Url = newAttahcment.Url,
                        FileType = newAttahcment.FileType
                    });
                }
            }
        }

    }
}
