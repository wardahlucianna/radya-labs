using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Teaching.FnAssignment.TeacherAssignment.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Common.Constants;
using Newtonsoft.Json;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment
{
    public class TeacherAssignmentHandler : FunctionsHttpCrudHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        private readonly IStringLocalizer _localizer;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;

        public TeacherAssignmentHandler(ITeachingDbContext teachingDbContext,
            IStringLocalizer localizer, IMessage messageService, IEventSchool eventSchool)
        {
            _teachingDbContext = teachingDbContext;
            _localizer = localizer;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _teachingDbContext.BeginTransactionAsync(CancellationToken);

            var teacher = await _teachingDbContext.Entity<TrNonTeachingLoad>()
                .Include(e=>e.MsNonTeachingLoad).ThenInclude(e=>e.AcademicYear)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(teacher.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(_localizer["ExNotFound"], x));

            // find already used ids
            foreach (var posision in teacher)
            {
                posision.IsActive = false;
                _teachingDbContext.Entity<TrNonTeachingLoad>().Update(posision);
            }

            await _teachingDbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);
            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<TeacherAssignmentGetListRequest>(nameof(TeacherAssignmentGetListRequest.IdSchool), nameof(TeacherAssignmentGetListRequest.IdAcadyear));
            if (!string.IsNullOrWhiteSpace(param.Status) && param.Status != "above" && param.Status != "standard")
                throw new BadRequestException(string.Format(_localizer["ExValueNotExpected"], "Status", string.Join(", ", new[] { "above", "standard" })));

            #region "Build Column"
            var columns = new List<ColumListTeacherAssignment>()
            {
                new ColumListTeacherAssignment()
                {
                    Id="",
                    ColumName = "Action"
                },
                new ColumListTeacherAssignment()
                {
                    Id="",
                    ColumName = "Teacher's Name"
                },
                new ColumListTeacherAssignment()
                {
                    Id="",
                    ColumName="Academic Years",
                },
                new ColumListTeacherAssignment()
                {
                    Id="",
                    GroupName="Teaching Assignment",
                    ColumName="Teaching Assignment",
                }
            };
            var appendcolumnsAcademic = await _teachingDbContext.Entity<MsNonTeachingLoad>()
                .Include(p => p.TeacherPosition)
                .Where(x => param.IdSchool.Contains(x.TeacherPosition.IdSchool) &&
                       x.IsActive &&
                       x.Category == AcademicType.Academic &&
                       x.IdAcademicYear == param.IdAcadyear)
                .Select(p => new ColumListTeacherAssignment()
                {
                    Id = p.Id,
                    GroupName = "Non-Teaching Assignment(Academic)",
                    ColumName = p.TeacherPosition.Code,
                })
                .ToListAsync(CancellationToken);

            //add colum setting data dari nontacing load academic
            foreach (var item in appendcolumnsAcademic)
            {
                columns.Add(item);
            }

            var appendColumnsNonAcademic = await _teachingDbContext.Entity<MsNonTeachingLoad>()
               .Include(p => p.TeacherPosition)
               .Where(x => param.IdSchool.Contains(x.TeacherPosition.IdSchool) &&
                      x.IsActive &&
                      x.Category == AcademicType.NonAcademic &&
                      x.IdAcademicYear == param.IdAcadyear)
               .Select(p => new ColumListTeacherAssignment()
               {
                   Id = p.Id,
                   GroupName = "Non-Teaching Assignment(Academic)",
                   ColumName = p.TeacherPosition.Code,
               })
               .ToListAsync(CancellationToken);

            foreach (var item in appendColumnsNonAcademic)
            {
                columns.Add(item);
            }

            columns.Add(new ColumListTeacherAssignment()
            {
                Id = "",
                ColumName = "Total Load"
            });

            columns.Add(new ColumListTeacherAssignment()
            {
                Id = "",
                ColumName = "Status"
            });

            #endregion

            IReadOnlyList<IItemValueVm> items = default;
            var count = 0;
            //var dataUserInSchool = _teachingDbContext.Entity<MsUserRole>()
            //    .Include(x => x.Role)
            //    .Include(x => x.User)
            //    .Where(x => param.IdSchool.Contains(x.Role.IdSchool) && x.IdRole == "TCH")
            //    .Select(x => new GetUserResult
            //    {
            //        Id = x.User.Id,
            //        Code = (_teachingDbContext.Entity<MsStaff>().Where(e => e.IdBinusian == x.IdUser).FirstOrDefault().ShortName),
            //        Description = x.User.DisplayName,
            //        DisplayName = x.User.DisplayName,
            //    })
            //    .ToList();

            var dataUserInSchool = await _teachingDbContext.Entity<MsUserSchool>()
                .Include(x => x.User)
                    .ThenInclude(x => x.UserRoles)
                        .ThenInclude(x => x.Role)
                            .ThenInclude(x => x.RoleGroup)
                .Where(x => param.IdSchool.Contains(x.IdSchool))
                .Where(x => x.User.UserRoles.Any(y => y.Role.RoleGroup.Code == RoleConstant.Teacher))
                .Select(x => new GetUserResult
                {
                    Id = x.User.Id,
                    Code = (_teachingDbContext.Entity<MsStaff>().Where(e => e.IdBinusian == x.IdUser).FirstOrDefault().ShortName),
                    Description = x.User.DisplayName,
                    DisplayName = x.User.DisplayName,
                })
                .ToListAsync();

            var listIdUser = dataUserInSchool.Select(x => x.Id).ToList();
            var dataUserProfile = _teachingDbContext.Entity<MsStaff>().Where(x => listIdUser.Contains(x.IdBinusian))
                .Select(x => new CheckTeacherForAscTimetableResult
                {
                    IdTeacher = x.IdBinusian,
                    TeacherBinusianId = x.IdBinusian,
                    TeacherName = string.IsNullOrEmpty(x.FirstName) ? x.LastName : x.FirstName,
                    TeacherShortName = x.ShortName,
                })
                .ToList();

            int assignedCount = 0, unassignedCount = 0, aboveStandardCount = 0;
            if (param.Return == CollectionType.Lov)
            {
                items = (
                    from _dataUser in dataUserInSchool
                    join _dataUserProfile in dataUserProfile on _dataUser.Id equals _dataUserProfile.TeacherBinusianId
                    where !string.IsNullOrEmpty(param.Search) ? _dataUser.DisplayName.Contains(param.Search, StringComparison.InvariantCultureIgnoreCase) : true
                    select new ItemValueVm
                    {
                        Id = _dataUser.Id,
                        Description = _dataUserProfile.TeacherName
                    })
                    .ToList();
            }
            else
            {
                var dataMaxTeacherLoad = 0;
                var maxTeacherLoadByAcademicYear = _teachingDbContext.Entity<MsMaxTeacherLoad>().Where(x => x.IdAcademicYear == param.IdAcadyear).FirstOrDefault();
                if (maxTeacherLoadByAcademicYear != null)
                    dataMaxTeacherLoad = maxTeacherLoadByAcademicYear.Max;

                var teachers =
                (
                    from _dataUser in dataUserInSchool
                    join _dataUserProfile in dataUserProfile on _dataUser.Id equals _dataUserProfile.TeacherBinusianId
                    where !string.IsNullOrEmpty(param.Search) ? _dataUser.DisplayName.Contains(param.Search, StringComparison.InvariantCultureIgnoreCase) : true
                    select new
                    {
                        IsAssigned = false,
                        IsAboveStandard = false,
                        TotalLoad = 0,
                        NonTeachingLoad = (_teachingDbContext.Entity<TrNonTeachingLoad>().Include(x => x.MsNonTeachingLoad).Where(x => x.IdUser == _dataUser.Id && x.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear).Sum(x => x.Load)),
                        Teacher = _dataUserProfile
                    }
                )
                .ToList();

                var dataItems = teachers
                    .Select(x => new TeacherAssignmentGetListResult
                    {
                        Id = x.Teacher.TeacherBinusianId,
                        TeacherName = x.Teacher.TeacherName,
                        Acadyear = "",
                        IdAcadyear = param.IdAcadyear,
                        TeacingAssignment = _teachingDbContext.Entity<TrTeachingLoad>()
                            .Include(x => x.TimetablePrefDetail).ThenInclude(x => x.TimetablePrefHeader)
                            .Where(p => p.IdUser == x.Teacher.TeacherBinusianId)
                            .Select(x => new TeachingAssignmentVm { Id = x.TimetablePrefDetail.IdTimetablePrefHeader, Load = x.Load, IdUser = x.IdUser })
                            .ToList(),
                        NonTeachingAssignmentAcademic = _teachingDbContext.Entity<TrNonTeachingLoad>()
                            .Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition)
                            .Where(x => x.MsNonTeachingLoad.Category == AcademicType.Academic)
                            .Where(p => p.IdUser == x.Teacher.TeacherBinusianId)
                            .Where(p => p.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear)
                            .Select(x => new NonTeachingAssignmentAcademicVm
                            {
                                IdSchoolNonTeachingLoad = x.IdMsNonTeachingLoad,
                                Data = x.Data,
                                Load = x.Load
                            })
                            .ToList(),
                        NonTeachingAssignmentNonAcademic = _teachingDbContext.Entity<TrNonTeachingLoad>()
                            .Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition)
                            .Where(x => x.MsNonTeachingLoad.Category == AcademicType.NonAcademic)
                            .Where(p => p.IdUser == x.Teacher.TeacherBinusianId)
                            .Where(p => p.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear)
                            .Select(x => new NonTeachingAssignmentNonAcademicVm
                            {
                                IdSchoolNonTeachingLoad = x.IdMsNonTeachingLoad,
                                Data = x.Data,
                                Load = x.Load
                            })
                            .ToList(),
                        NonTeachingLoad = x.NonTeachingLoad,
                        TotalLoad = x.TotalLoad,
                    })
                    .ToList();

                var idSubjectCombination = new List<string>();
                foreach (var item in dataItems)
                {
                    foreach (var ta in item.TeacingAssignment)
                    {
                        if (!idSubjectCombination.Contains(ta.Id))
                            idSubjectCombination.Add(ta.Id);
                    }
                }

                var listIds = idSubjectCombination.Count > 0 ? idSubjectCombination : new List<string> { "dummY" };

                var metaDataSubjectCombination = await _teachingDbContext.Entity<MsSubjectCombination>()
                .Include(x => x.Subject)
                .Include(x => x.Subject)
                    .ThenInclude(x => x.Department)
                .Include(x => x.Subject)
                    .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                .Include(x => x.GradePathwayClassroom)
                    .ThenInclude(x => x.Classroom)
                .Include(x => x.GradePathwayClassroom)
                    .ThenInclude(x => x.GradePathway)
                        .ThenInclude(x => x.Grade)
                .Where(x => listIds.Contains(x.Id))
                .Select(x => new GetListSubjectCombinationTimetableResult
                {
                    Id = x.Id,
                    AcadYear = new CodeView
                    {
                        Id = x.Subject.Grade.Level.IdAcademicYear,
                        Code = x.Subject.Grade.Level.AcademicYear.Code,
                        Description = x.Subject.Grade.Level.AcademicYear.Description,
                        IdMapping = x.GradePathwayClassroom.GradePathway.Grade.Level.AcademicYear.Id
                    },
                    Level = new CodeView
                    {
                        Id = x.Subject.Grade.IdLevel,
                        Code = x.Subject.Grade.Level.Code,
                        Description = x.Subject.Grade.Level.Description
                    },
                    Grade = new CodeView
                    {
                        Id = x.Subject.IdGrade,
                        Code = x.Subject.Grade.Code,
                        Description = x.Subject.Grade.Description
                    },
                    Class = new CodeView
                    {
                        Id = x.GradePathwayClassroom.Id,
                        Code = x.GradePathwayClassroom.Classroom.Code,
                        Description = x.GradePathwayClassroom.Classroom.Description
                    },
                    Subject = new SubjectVm
                    {
                        Id = x.Subject.Id,
                        SubjectId = x.Subject.SubjectID,
                        SubjectName = x.Subject.Code,
                        Description = x.Subject.Description,
                        MaxSession = x.Subject.MaxSession
                    },
                    Department = new CodeView
                    {
                        Id = x.Subject.Department.Id,
                        Code = x.Subject.Department.Code,
                        Description = x.Subject.Department.Description,
                    },
                    Streaming = x.Subject.SubjectPathways.Select(sp => new CodeView
                    {
                        Id = sp.GradePathwayDetail.Pathway.Id,
                        Code = sp.GradePathwayDetail.Pathway.Code,
                        Description = sp.GradePathwayDetail.Pathway.Description,
                    }).ToList(),
                    TotalSession = x.Subject.MaxSession
                }).ToListAsync();

                var dataSubjectCombination = metaDataSubjectCombination
                .Where(x => x.AcadYear.Id == (!string.IsNullOrEmpty(param.IdAcadyear) ? param.IdAcadyear : x.AcadYear.Id))
                .Where(x => x.Level.Id == (!string.IsNullOrEmpty(param.IdLevel) ? param.IdLevel : x.Level.Id))
                .Where(x => x.Grade.Id == (!string.IsNullOrEmpty(param.IdGrade) ? param.IdGrade : x.Grade.Id))
                .Where(x => x.Class.Id == (!string.IsNullOrEmpty(param.IdClass) ? param.IdClass : x.Class.Id))
                .ToList();

                var ListAcadyears = dataItems.Count > 0 ? dataItems.Select(x => x.IdAcadyear).ToList() : new List<string>() { "dummy" };
                var reqDataAcadYear1 = _teachingDbContext.Entity<MsAcademicYear>()
                  .ToList();

                var reqDataAcadYear = _teachingDbContext.Entity<MsAcademicYear>()
                    .Where(x => ListAcadyears.Contains(x.Id))
                    .ToList();

                var dataAcadYear = reqDataAcadYear;
                var realData =
                    (
                        from _dataItem in dataItems
                        join _dataAcadYear in dataAcadYear on _dataItem.IdAcadyear equals _dataAcadYear.Id
                        where
                           string.IsNullOrEmpty(param.Status) || (param.Status == "above" ? _dataItem.IsAboveStandard : !_dataItem.IsAboveStandard)
                           && _dataAcadYear.Id == param.IdAcadyear
                        select new TeacherAssignmentGetListResult
                        {
                            Id = _dataItem.Id,
                            IdAcadyear = _dataAcadYear.Id,
                            Acadyear = _dataAcadYear.Description,
                            Description = "",
                            TeacingAssignment =
                                (
                                    from _dataTeachingLoad in _dataItem.TeacingAssignment
                                    join _dataSubjectCombination in dataSubjectCombination on _dataTeachingLoad.Id equals _dataSubjectCombination.Id
                                    where
                                    _dataTeachingLoad.IdUser == _dataItem.Id
                                    select new TeachingAssignmentVm
                                    {
                                        Id = _dataTeachingLoad.Id,
                                        Grade = new CodeWithIdVm
                                        {
                                            Id = _dataSubjectCombination.Grade.Id,
                                            Code = _dataSubjectCombination.Grade.Code,
                                            Description = _dataSubjectCombination.Grade.Description
                                        },
                                        Subject = _dataSubjectCombination.Subject.SubjectName,
                                        ClassAndSession = new CodeWithIdVm
                                        {
                                            Id = _dataSubjectCombination.Class.Id,
                                            Code = _dataSubjectCombination.Class.Code,
                                            Description = _dataSubjectCombination.Class.Description
                                        },
                                        Load = _dataTeachingLoad.Load,
                                        IdUser = _dataTeachingLoad.IdUser,
                                        AcademicYear = new CodeWithIdVm
                                        {
                                            Id = _dataSubjectCombination.AcadYear.Id,
                                            Code = _dataSubjectCombination.AcadYear.Code,
                                            Description = _dataSubjectCombination.AcadYear.Description
                                        },
                                    }
                                )
                                .Where(x => x.IdUser == _dataItem.Id)
                                .ToList(),
                            NonTeachingAssignmentAcademic = _dataItem.NonTeachingAssignmentAcademic,
                            NonTeachingAssignmentNonAcademic = _dataItem.NonTeachingAssignmentNonAcademic,
                            Status = _dataItem.Status,
                            TeacherName = _dataItem.TeacherName,
                            TotalLoad = _dataItem.NonTeachingLoad + (
                                    from _dataTeachingLoad in _dataItem.TeacingAssignment
                                    join _dataSubjectCombination in dataSubjectCombination on _dataTeachingLoad.Id equals _dataSubjectCombination.Id
                                    where
                                    _dataSubjectCombination.AcadYear.Id == param.IdAcadyear
                                    && _dataTeachingLoad.IdUser == _dataItem.Id
                                    group _dataTeachingLoad by _dataTeachingLoad.IdUser into g
                                    select new
                                    {
                                        Load = g.Sum(x => x.Load)// + _dataItem.NonTeachingLoad
                                    }
                                )
                                .FirstOrDefault()?.Load ?? _dataItem.NonTeachingLoad,
                            IsAboveStandard = _dataItem.NonTeachingLoad + ((
                                    from _dataTeachingLoad in _dataItem.TeacingAssignment
                                    join _dataSubjectCombination in dataSubjectCombination on _dataTeachingLoad.Id equals _dataSubjectCombination.Id
                                    where
                                    _dataSubjectCombination.AcadYear.Id == param.IdAcadyear
                                    && _dataTeachingLoad.IdUser == _dataItem.Id
                                    group _dataTeachingLoad by _dataTeachingLoad.IdUser into g
                                    select new
                                    {
                                        Load = g.Sum(x => x.Load) //+ _dataItem.NonTeachingLoad
                                    }
                                )
                                .FirstOrDefault()?.Load) > dataMaxTeacherLoad ? true : false,
                            IsAssigned = (
                                //(
                                //    from _dataTeachingLoad in _dataItem.TeacingAssignment
                                //    join _dataSubjectCombination in dataSubjectCombination on _dataTeachingLoad.Id equals _dataSubjectCombination.Id
                                //    where
                                //    _dataSubjectCombination.AcadYear.Id == param.IdAcadyear
                                //    && _dataTeachingLoad.IdUser == _dataItem.Id
                                //    group _dataTeachingLoad by _dataTeachingLoad.IdUser into g
                                //    select new
                                //    {
                                //        g.Key
                                //    }
                                //)
                                _dataItem.NonTeachingAssignmentAcademic.Count() > 0) ? true : false
                        }
                    )
                    .ToList();
                
                if (!string.IsNullOrEmpty(param.IdLevel))
                {
                    if (!string.IsNullOrEmpty(param.IdGrade))
                    {
                        realData = realData.Where(x=> x.NonTeachingAssignmentAcademic.Any(y=> y.Data.Contains(param.IdGrade))).ToList();
                    }
                    else
                    {
                        realData = realData.Where(x => x.NonTeachingAssignmentAcademic.Any(y => y.Data.Contains(param.IdLevel))).ToList();
                    }
                }

               count = param.CanCountWithoutFetchDb(realData.Count)
               ? realData.Count
               : realData.Select(x => x.Id).Count();

                var newRealData = realData.SetPagination(param).ToList(); // realData.Where(x => x.TeacingAssignment.Count() > 0).ToList();
                assignedCount = realData.Count(x => x.IsAssigned);
                unassignedCount = teachers.Count() - assignedCount;
                aboveStandardCount = realData.Count(x => x.IsAboveStandard);

                foreach (var item in newRealData)
                {
                    item.assignedCount = assignedCount;
                    item.unassignedCount = unassignedCount;
                    item.aboveStandardCount = aboveStandardCount;
                    item.totalTeacher = teachers.Count();
                }
                items = newRealData;
            }
            //var count = param.CanCountWithoutFetchDb(realData.Count)
            //    ? items.Count
            //    : items.Select(x => x.Id).Count();

            var results = param.CreatePaginationProperty(count)
                .AddColumnProperty(columns)
                .AddProperty(
                    KeyValuePair.Create("Assigned", assignedCount as object),
                    KeyValuePair.Create("Unassigned", unassignedCount as object),
                    KeyValuePair.Create("AboveStandard", aboveStandardCount as object));

            return Request.CreateApiResult2(items, results);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var model = await Request.ValidateBody<AddTeacherAssignmentRequest, AddTeacherAssignmentValidator>();
            Transaction = await _teachingDbContext.BeginTransactionAsync(CancellationToken);
            foreach (var item in model.NonteacingLoadAcademic)
            {
                var datas = await _teachingDbContext.Entity<TrNonTeachingLoad>()
                .Include(x => x.MsNonTeachingLoad)
                    .ThenInclude(x => x.TeacherPosition)
                        .ThenInclude(x => x.Position)
                .Where(p => p.IdMsNonTeachingLoad == item.IdSchoolNonTeachingLoad).ToListAsync();
                //ambil data sesuai teaching load
                var position = await _teachingDbContext.Entity<MsNonTeachingLoad>()
                    .Include(x => x.TeacherPosition)
                        .ThenInclude(x => x.Position)
                .Where(p => p.Id == item.IdSchoolNonTeachingLoad)
                .Select(x => new
                {
                    x1 = x.TeacherPosition.Code,
                    x2 = x.TeacherPosition.Position.Code
                })
                .FirstOrDefaultAsync();

                var getData = await _teachingDbContext.Entity<TrNonTeachingLoad>().Where(p => p.Id == item.Id).FirstOrDefaultAsync();
                if (getData == null)
                {
                    var insertData = new TrNonTeachingLoad
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUser = item.IdSchoolUser,
                        IdMsNonTeachingLoad = item.IdSchoolNonTeachingLoad,
                        Load = item.Load,
                        Data = item.Data
                    };
                    _teachingDbContext.Entity<TrNonTeachingLoad>().Add(insertData);
                }
                else
                {
                    getData.IdUser = item.IdSchoolUser;
                    getData.IdMsNonTeachingLoad = item.IdSchoolNonTeachingLoad;
                    getData.Load = item.Load;
                    getData.Data = item.Data;
                    _teachingDbContext.Entity<TrNonTeachingLoad>().Update(getData);
                }
            }

            foreach (var item in model.NonteacingLoadNonAcademic)
            {
                var getData = await _teachingDbContext.Entity<TrNonTeachingLoad>().Where(p => p.Id == item.Id).FirstOrDefaultAsync();
                if (getData == null)
                {
                    var insertData = new TrNonTeachingLoad
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUser = item.IdSchoolUser,
                        IdMsNonTeachingLoad = item.IdSchoolNonTeachingLoad,
                        Load = item.Load,
                        Data = item.Data
                    };
                    _teachingDbContext.Entity<TrNonTeachingLoad>().Add(insertData);
                }
                else
                {
                    getData.IdUser = item.IdSchoolUser;
                    getData.IdMsNonTeachingLoad = item.IdSchoolNonTeachingLoad;
                    getData.Load = item.Load;
                    getData.Data = item.Data;
                    _teachingDbContext.Entity<TrNonTeachingLoad>().Update(getData);
                }
            }
            await _teachingDbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            var idSchool = await _teachingDbContext.Entity<MsAcademicYear>()
                .Where(x => x.Id == model.IdAcademicYear)
                .Select(e => e.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = idSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = idSchool
            });

            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
