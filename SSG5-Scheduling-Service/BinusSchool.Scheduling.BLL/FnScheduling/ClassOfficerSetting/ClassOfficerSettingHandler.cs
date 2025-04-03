using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassOfficerSetting.Validator;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;

namespace BinusSchool.Scheduling.FnSchedule.ClassOfficerSetting
{
    public class ClassOfficerSettingHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public ClassOfficerSettingHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await (from h in _dbContext.Entity<MsHomeroom>()
                               join _hf in _dbContext.Entity<MsHomeroomOfficer>() on h.Id equals _hf.Id into _hfData
                               from hf in _hfData.DefaultIfEmpty()
                               join ht in _dbContext.Entity<MsHomeroomTeacher>() on h.Id equals ht.IdHomeroom
                               join g in _dbContext.Entity<MsGrade>() on h.IdGrade equals g.Id
                               join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                               join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                               join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
                               join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                               where h.Id == id
                               select new GetClassOfficerSettingDetailResult
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
                                       Id = c.Id,
                                       Code = c.Code,
                                       Description = c.Description
                                   },
                                   Semester = h.Semester,
                                   UserHomeroomCaptain = new UserHomeroomCaptain
                                   {
                                       Id = hf.IdUserHomeroomCaptain,
                                       Name = hf.HomeroomCaptain.DisplayName,
                                       CaptainCanAssignClassDiary = hf.CaptainCanAssignClassDiary
                                   },
                                   UserHomeroomViceCaptain = new UserHomeroomViceCaptain
                                   {
                                       Id = hf.IdUserHomeroomViceCaptain,
                                       Name = hf.HomeroomViceCaptain.DisplayName,
                                       ViceCaptainCanAssignClassDiary = hf.ViceCaptainCanAssignClassDiary,
                                   },
                                   UserHomeroomSecretary = new UserHomeroomSecretary
                                   {
                                       Id = hf.IdUserHomeroomSecretary,
                                       Name = hf.HomeroomSecretary.DisplayName,
                                       SecretaryCanAssignClassDiary = hf.SecretaryCanAssignClassDiary
                                   },
                                   UserHomeroomTreasurer = new UserHomeroomTreasurer
                                   {
                                       Id = hf.IdUserHomeroomTreasurer,
                                       Name = hf.HomeroomTreasurer.DisplayName,
                                       TreasurerCanAssignClassDiary = hf.TreasurerCanAssignClassDiary
                                   }
                               }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetClassOfficerSettingRequest>();

            var columns = new[] { "AcademicYear", "Level", "Grade", "HomeRoom", "Semester" };

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
                        select new GetClassOfficerSettingResult
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
                            UserIdModified = (hf != null)
                                                ? hf.UserUp == null
                                                    ? hf.UserIn : hf.UserUp
                                                : string.Empty,
                            UserModified = (hf != null)
                                                ? hf.UserUp == null
                                                    ? uc.DisplayName : uu.DisplayName
                                                : string.Empty,
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

            var querys = query.ToList();

            var groupItems = querys.GroupBy(item => item.Id,
                    (key, group) => new { Id = key, Items = group.ToList() })
                    .ToList();

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = groupItems
                    .Select(x => new ItemValueVm(x.Id, x.Items.Select(x => x.HomeRoom.Id).First()))
                    .ToList();
            }
            else
            {
                items = groupItems
                .SetPagination(param)
                .Select(x => new GetClassOfficerSettingResult
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
                    UserIdModified = x.Items.Select(y => y.UserIdModified).First(),
                    UserModified = x.Items.Select(y => y.UserModified).First(),
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : groupItems.Select(x => x.Id).Count(); return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateClassOfficerSettingRequest, UpdateClassOfficerSettingValidator>();

            var existsOfficer = _dbContext.Entity<MsHomeroomOfficer>().Where(x => x.Id == body.IdHomeRoom).FirstOrDefault();

            if (existsOfficer is null)
            {
                //add new officer
                await AddNewHomeRoomOfficer(body);
            }
            else
            {
                //update new officer
                await UpdateHomeRoomOfficer(existsOfficer, body);
            }
            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private async Task AddNewHomeRoomOfficer(UpdateClassOfficerSettingRequest data)
        {
            await _dbContext.Entity<MsHomeroomOfficer>().AddAsync(new MsHomeroomOfficer
            {
                Id = data.IdHomeRoom,
                IdUserHomeroomCaptain = data.IdUserHomeroomCaptain,
                IdUserHomeroomViceCaptain = data.IdUserHomeroomViceCaptain,
                IdUserHomeroomSecretary = data.IdUserHomeroomSecretary,
                IdUserHomeroomTreasurer = data.IdUserHomeroomTreasurer,
                CaptainCanAssignClassDiary = data.CapatainCanAssignClassDiary,
                ViceCaptainCanAssignClassDiary = data.ViceCapatainCanAssignClassDiary,
                SecretaryCanAssignClassDiary = data.SecretaryCanAssignClassDiary,
                TreasurerCanAssignClassDiary = data.TreasurerCanAssignClassDiary
            });
        }

        private async Task UpdateHomeRoomOfficer(MsHomeroomOfficer existsData, UpdateClassOfficerSettingRequest data)
        {
            if (existsData.IdUserHomeroomCaptain != data.IdUserHomeroomCaptain)
            {
                existsData.IdUserHomeroomCaptain = data.IdUserHomeroomCaptain;
            }
            if (existsData.IdUserHomeroomViceCaptain != data.IdUserHomeroomViceCaptain)
            {
                existsData.IdUserHomeroomViceCaptain = data.IdUserHomeroomViceCaptain;
            }
            if (existsData.IdUserHomeroomSecretary != data.IdUserHomeroomSecretary)
            {
                existsData.IdUserHomeroomSecretary = data.IdUserHomeroomSecretary;
            }
            if (existsData.IdUserHomeroomTreasurer != data.IdUserHomeroomTreasurer)
            {
                existsData.IdUserHomeroomTreasurer = data.IdUserHomeroomTreasurer;
            }
            if (existsData.CaptainCanAssignClassDiary != data.CapatainCanAssignClassDiary)
            {
                existsData.CaptainCanAssignClassDiary = data.CapatainCanAssignClassDiary;
            }
            if (existsData.ViceCaptainCanAssignClassDiary != data.ViceCapatainCanAssignClassDiary)
            {
                existsData.ViceCaptainCanAssignClassDiary = data.ViceCapatainCanAssignClassDiary;
            }
            if (existsData.SecretaryCanAssignClassDiary != data.SecretaryCanAssignClassDiary)
            {
                existsData.SecretaryCanAssignClassDiary = data.SecretaryCanAssignClassDiary;
            }
            if (existsData.TreasurerCanAssignClassDiary != data.TreasurerCanAssignClassDiary)
            {
                existsData.TreasurerCanAssignClassDiary = data.TreasurerCanAssignClassDiary;
            }

            _dbContext.Entity<MsHomeroomOfficer>().Update(existsData);
        }
    }
}
