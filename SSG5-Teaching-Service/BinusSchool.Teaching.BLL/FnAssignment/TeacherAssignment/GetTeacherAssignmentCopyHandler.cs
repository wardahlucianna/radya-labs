using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Common.Model.Enums;
using Newtonsoft.Json;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.Data;
using FluentEmail.Core;
using System.Globalization;
using System.Security.Cryptography.Xml;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment
{
    public class GetTeacherAssignmentCopyHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;

        public GetTeacherAssignmentCopyHandler(ITeachingDbContext teachingDbContext)
        {
            _teachingDbContext = teachingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherAssignmentCopyRequest>();

            GetTeacherAssignmentCopyResult item = new GetTeacherAssignmentCopyResult();

            List<string> IdAcademicYear = new List<string>()
            {
                param.IdAcademicYearFrom,
                param.IdAcademicYearTo
            };

            #region Data
            #region Data Grade
            var listGrade = await _teachingDbContext.Entity<MsGrade>()
                                    .Include(e => e.Level)
                                    .Where(e => IdAcademicYear.Contains(e.Level.IdAcademicYear))
                                    .ToListAsync(CancellationToken);

            var listGradeFrom = listGrade.Where(e => e.Level.IdAcademicYear == param.IdAcademicYearFrom).ToList();
            var listGradeTo = listGrade.Where(e => e.Level.IdAcademicYear == param.IdAcademicYearTo).ToList();
            #endregion

            #region Data Department
            var listDepartment = await _teachingDbContext.Entity<MsDepartment>()
                                    .Where(e => IdAcademicYear.Contains(e.IdAcademicYear))
                                    .ToListAsync(CancellationToken);

            var listDepartmentFrom = listDepartment.Where(e => e.IdAcademicYear == param.IdAcademicYearFrom).ToList();
            var listDepartmentTo = listDepartment.Where(e => e.IdAcademicYear == param.IdAcademicYearTo).ToList();
            #endregion

            #region Data Subject
            var listSubject = await _teachingDbContext.Entity<MsSubject>()
                                    .Where(e => IdAcademicYear.Contains(e.Grade.Level.IdAcademicYear))
                                    .ToListAsync(CancellationToken);

            var listSubjectFrom = listSubject.Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYearFrom).ToList();
            var listSubjectTo = listSubject.Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYearTo).ToList();
            #endregion

            #region Data Grade Pathway (Streaming) 
            var listStreaming = await _teachingDbContext.Entity<MsGradePathwayDetail>()
                                    .Include(e => e.GradePathway).ThenInclude(e => e.Grade)
                                    .Include(e => e.Pathway)
                                    .Where(e => IdAcademicYear.Contains(e.GradePathway.Grade.Level.IdAcademicYear))
                                    .ToListAsync(CancellationToken);

            var listStreamingFrom = listStreaming.Where(e => e.GradePathway.Grade.Level.IdAcademicYear == param.IdAcademicYearFrom).ToList();
            var listStreamingTo = listStreaming.Where(e => e.GradePathway.Grade.Level.IdAcademicYear == param.IdAcademicYearTo).ToList();
            #endregion

            #region Data Grade Pathway Classroom (Classroom) 
            var listClassroom = await _teachingDbContext.Entity<MsGradePathwayClassroom>()
                                    .Include(e => e.Classroom)
                                    .Where(e => IdAcademicYear.Contains(e.GradePathway.Grade.Level.IdAcademicYear))
                                    .ToListAsync(CancellationToken);

            var listClassroomFrom = listClassroom.Where(e => e.GradePathway.Grade.Level.IdAcademicYear == param.IdAcademicYearFrom).ToList();
            var listClassroomTo = listClassroom.Where(e => e.GradePathway.Grade.Level.IdAcademicYear == param.IdAcademicYearTo).ToList();
            #endregion

            #region MsNonTeachingLoad
            var listMsNonTeachingLoad = _teachingDbContext.Entity<MsNonTeachingLoad>()
                                     .Include(x => x.TeacherPosition)
                                     .Where(x => IdAcademicYear.Contains(x.IdAcademicYear))
                                     .ToList();

            var listMsNonTeachingLoadFrom = listMsNonTeachingLoad
                                            .Where(e => e.IdAcademicYear == param.IdAcademicYearFrom)
                                            .Select(x => new ListNonTeachingAcademic
                                            {
                                                Id = x.Id,
                                                Name = x.TeacherPosition.Code,
                                                Description = x.TeacherPosition.Description,
                                                Data = x.Parameter,
                                                Load = x.Load,
                                                IsHaveMasterForNextAy = true,
                                                Category = x.Category
                                            })
                                            .ToList();
            var listMsNonTeachingLoadTo = listMsNonTeachingLoad.Where(e => e.IdAcademicYear == param.IdAcademicYearTo).ToList();
            #endregion

            #region TrNonTeachingLoad
            var listTrNonTeachingLoad = _teachingDbContext.Entity<TrNonTeachingLoad>()
                                      .Include(x => x.MsNonTeachingLoad)
                                        .ThenInclude(x => x.TeacherPosition)
                                      .Where(x => x.IdUser == param.IdUser)
                                      .Where(x => IdAcademicYear.Contains(x.MsNonTeachingLoad.IdAcademicYear))
                                      .ToList();

            var listTrNonTeachingLoadFrom = listTrNonTeachingLoad
                                            .Where(e => e.MsNonTeachingLoad.IdAcademicYear == param.IdAcademicYearFrom)
                                            .ToList();
            var listTrNonTeachingLoadTo = listTrNonTeachingLoad.Where(e => e.MsNonTeachingLoad.IdAcademicYear == param.IdAcademicYearTo).ToList();

            List<NonTeachingLoadData> listNonTeachingLoadDataTo = new List<NonTeachingLoadData>();
            foreach (var nonTeachingLoadTo in listTrNonTeachingLoadTo)
            {
                NonTeachingLoadData newNonTeachingLoadDataTo = new NonTeachingLoadData();
                newNonTeachingLoadDataTo.IdMsNonTeachingLoad = nonTeachingLoadTo.IdMsNonTeachingLoad;
                newNonTeachingLoadDataTo.No = listTrNonTeachingLoadTo.IndexOf(nonTeachingLoadTo);

                if (nonTeachingLoadTo.Data == null)
                {
                    listNonTeachingLoadDataTo.Add(newNonTeachingLoadDataTo);
                    continue;
                }

                var _dataPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(nonTeachingLoadTo.Data);
                _dataPosition.TryGetValue("Level", out var _LevelPosition);
                _dataPosition.TryGetValue("Department", out var _DepartemenPosition);
                _dataPosition.TryGetValue("Grade", out var _GradePosition);
                _dataPosition.TryGetValue("Subject", out var _SubjectPosition);
                _dataPosition.TryGetValue("Streaming", out var _StreamingPosition);
                _dataPosition.TryGetValue("Classroom", out var _ClassroomPosition);

                if (_LevelPosition != null)
                {
                    newNonTeachingLoadDataTo.Level = listGradeTo
                        .Where(e=>e.IdLevel==_LevelPosition.Id)
                        .Select(e=>new CodeWithIdVm
                        {
                            Id = e.IdLevel,
                            Code = e.Level.Code,
                            Description = e.Level.Description,
                        }).FirstOrDefault();
                }
                
                if (_DepartemenPosition != null)
                {
                    newNonTeachingLoadDataTo.Department = listDepartmentTo
                        .Where(e => e.Id == _DepartemenPosition.Id)
                        .Select(e => new CodeWithIdVm
                        {
                            Id = e.Id,
                            Code = e.Code,
                            Description = e.Description,
                        }).FirstOrDefault();
                }

                if (_GradePosition != null)
                {
                    newNonTeachingLoadDataTo.Grade = listGradeTo
                        .Where(e => e.Id == _GradePosition.Id)
                        .Select(e => new CodeWithIdVm
                        {
                            Id = e.Id,
                            Code = e.Code,
                            Description = e.Description,
                        }).FirstOrDefault();
                }

                if (_SubjectPosition != null)
                {
                    newNonTeachingLoadDataTo.Subject = listSubjectTo
                        .Where(e => e.Id == _SubjectPosition.Id)
                        .Select(e => new CodeWithIdVm
                        {
                            Id = e.Id,
                            Code = e.Code,
                            Description = e.Description,
                        }).FirstOrDefault();
                }

                if (_StreamingPosition != null)
                {
                    newNonTeachingLoadDataTo.Streaming = listStreamingTo
                        .Where(e => e.IdGradePathway == _StreamingPosition.Id)
                        .Select(e => new CodeWithIdVm
                        {
                            Id = e.IdGradePathway,
                            Code = e.Pathway.Code,
                            Description = e.Pathway.Description,
                        }).FirstOrDefault();
                }

                if (_ClassroomPosition != null)
                {
                    newNonTeachingLoadDataTo.Classroom = listClassroomTo
                        .Where(e => e.Id == _ClassroomPosition.Id)
                        .Select(e => new CodeWithIdVm
                        {
                            Id = e.Id,
                            Code = e.Classroom.Code,
                            Description = e.Classroom.Description,
                        }).FirstOrDefault();
                }

                listNonTeachingLoadDataTo.Add(newNonTeachingLoadDataTo);
            }

            #endregion
            #endregion

            List<NonTeacingLoadFromAndTo> listNonTeacingLoad = new List<NonTeacingLoadFromAndTo>();
            foreach (var NonTeachingLoadAcademic in listTrNonTeachingLoadFrom)
            {
                var IdTeacherPositionFrom = NonTeachingLoadAcademic.MsNonTeachingLoad.IdTeacherPosition;

                var getMsNonTeachingLoadTo = listMsNonTeachingLoadTo
                                                .Where(e => e.IdTeacherPosition == IdTeacherPositionFrom)
                                                .FirstOrDefault();


                if (getMsNonTeachingLoadTo == null)
                {
                    listMsNonTeachingLoadFrom.Where(e => e.Id == NonTeachingLoadAcademic.IdMsNonTeachingLoad).ForEach(e => e.IsHaveMasterForNextAy = false);
                }

                var listNonTeachingLoadDataToByIdMsNonTeaching = listNonTeachingLoadDataTo.Where(e => e.IdMsNonTeachingLoad == getMsNonTeachingLoadTo.Id).ToList();
                bool isDisabled = false;
                string data = null;
                List<string> MasterError = new List<string>();
                if (NonTeachingLoadAcademic.Data != null)
                {
                    GetDataNonTeachingLoadResult dataObject = new GetDataNonTeachingLoadResult();
                    DataNonTeachingLoadRequest paramData = new DataNonTeachingLoadRequest
                    {
                        Data = NonTeachingLoadAcademic.Data,
                        GradeFrom = listGradeFrom,
                        GradeTo = listGradeTo,
                        DepartmentFrom = listDepartmentFrom,
                        DepartmentTo = listDepartmentTo,
                        SubjectFrom = listSubjectFrom,
                        SubjectTo = listSubjectTo,
                        StreamingFrom = listStreamingFrom,
                        StreamingTo = listStreamingTo,
                        ClassroomFrom = listClassroomFrom,
                        ClassroomTo = listClassroomTo,
                        NonTeachingLoadDataTo = listNonTeachingLoadDataToByIdMsNonTeaching,
                    };

                    dataObject = GetDataNonTeachingLoad(paramData);

                    if (dataObject == null)
                        continue;

                    isDisabled = dataObject.IsExsis;
                    data = dataObject.Data;
                    MasterError = dataObject.MasterError;
                }
                else
                {
                    isDisabled = listNonTeachingLoadDataToByIdMsNonTeaching.Any();
                    data = null;
                    MasterError = null;
                }

                listNonTeacingLoad.Add(new NonTeacingLoadFromAndTo
                {
                    Category = NonTeachingLoadAcademic.MsNonTeachingLoad.Category,
                    IsDisabled = isDisabled,
                    MasterError = MasterError,
                    NonTeachingLoadFrom = new NonTeacingLoad
                    {
                        Id = NonTeachingLoadAcademic.Id,
                        IdUser = NonTeachingLoadAcademic.IdUser,
                        IdMsNonTeachingLoad = NonTeachingLoadAcademic.IdMsNonTeachingLoad,
                        Load = NonTeachingLoadAcademic.Load,
                        Data = NonTeachingLoadAcademic.Data,
                    },
                    NonTeachingLoadTo = new NonTeacingLoad
                    {
                        IdUser = NonTeachingLoadAcademic.IdUser,
                        IdMsNonTeachingLoad = getMsNonTeachingLoadTo==null?null:getMsNonTeachingLoadTo.Id,
                        Load = getMsNonTeachingLoadTo == null ? 0 : getMsNonTeachingLoadTo.Load,
                        Data = data,
                    },
                });
            }

            #region Academic
            item.ListNonTeachingAcademics = listMsNonTeachingLoadFrom
                                            .Where(e=>e.Category== AcademicType.Academic)
                                            .Select(x=>new ListNonTeachingAcademic
                                            {
                                                Id = x.Id,
                                                Name = x.Name,
                                                Description = x.Description,
                                                Data = x.Data,
                                                Load = x.Load,
                                                IsHaveMasterForNextAy = x.IsHaveMasterForNextAy
                                            })
                                            .ToList();

            item.NonTeachingAssignmentAcademic = listNonTeacingLoad
                                      .Where(x => x.Category == AcademicType.Academic)
                                      .ToList();
            #endregion

            #region Non Academic
            item.ListNonTeachingNonAcademics = listMsNonTeachingLoadFrom
                                            .Where(e => e.Category == AcademicType.NonAcademic)
                                            .Select(x => new ListNonTeachingAcademic
                                            {
                                                Id = x.Id,
                                                Name = x.Name,
                                                Description = x.Description,
                                                Data = x.Data,
                                                Load = x.Load,
                                                IsHaveMasterForNextAy = x.IsHaveMasterForNextAy
                                            })
                                            .ToList();

            item.NonTeachingAssignmentNonAcademic = listNonTeacingLoad
                                      .Where(x => x.Category == AcademicType.NonAcademic)
                                      .ToList();
            #endregion



            return Request.CreateApiResult2(item as object);
        }


        public GetDataNonTeachingLoadResult GetDataNonTeachingLoad(DataNonTeachingLoadRequest param)
        {
            dynamic dataNonTeachingLoadTo = new System.Dynamic.ExpandoObject();
           

            var _dataPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(param.Data);
            _dataPosition.TryGetValue("Level", out var _LevelPosition);
            _dataPosition.TryGetValue("Department", out var _DepartemenPosition);
            _dataPosition.TryGetValue("Grade", out var _GradePosition);
            _dataPosition.TryGetValue("Subject", out var _SubjectPosition);
            _dataPosition.TryGetValue("Streaming", out var _StreamingPosition);
            _dataPosition.TryGetValue("Classroom", out var _ClassroomPosition);

            var query = param.NonTeachingLoadDataTo.Distinct();
            List<string> listErrorMaster = new List<string>();
            if (_LevelPosition != null)
            {
                var idLevelFrom = _LevelPosition.Id;
                var levelCodeFrom = param.GradeFrom.Where(e => e.IdLevel == idLevelFrom).Select(e => e.Level.Code).Distinct().FirstOrDefault();

                dataNonTeachingLoadTo.Level = param.GradeTo
                            .Where(e => e.Level.Code == levelCodeFrom)
                            .Select(e => new ItemValueVm
                            {
                                Id = e.IdLevel,
                                Description = e.Level.Description,
                            })
                            .Distinct()
                            .FirstOrDefault();

                if (dataNonTeachingLoadTo.Level == null)
                {
                    listErrorMaster.Add("Level");
                    //return default;
                }

                query = query.Where(e => e.Level.Description == dataNonTeachingLoadTo.Level.Description);
            }

            if (_DepartemenPosition != null)
            {
                var idDepartemenFrom = _DepartemenPosition.Id;
                var DepartemenCodeFrom = param.DepartmentFrom.Where(e => e.Id == idDepartemenFrom).Select(e => e.Code).Distinct().FirstOrDefault();

                dataNonTeachingLoadTo.Department = param.DepartmentTo
                                    .Where(e => e.Code == DepartemenCodeFrom)
                                    .Select(e => new ItemValueVm
                                    {
                                        Id = e.Id,
                                        Description = e.Description,
                                    })
                                    .Distinct()
                                    .FirstOrDefault();

                if (dataNonTeachingLoadTo.Department == null)
                {
                    listErrorMaster.Add("Department");
                    //return default;
                }

                query = query.Where(e => e.Department.Description == dataNonTeachingLoadTo.Department.Description);
            }

            if (_GradePosition != null)
            {
                var idGradeFrom = _GradePosition.Id;
                var GradeCodeFrom = param.GradeFrom.Where(e => e.Id == idGradeFrom).Select(e => e.Code).Distinct().FirstOrDefault();

                dataNonTeachingLoadTo.Grade = param.GradeTo
                            .Where(e => e.Code == GradeCodeFrom)
                            .Select(e => new ItemValueVm
                            {
                                Id = e.Id,
                                Description = e.Description,
                            })
                            .Distinct()
                            .FirstOrDefault();

                if (dataNonTeachingLoadTo.Grade == null)
                {
                    listErrorMaster.Add("Grade");
                    //return default;
                }

                query = query.Where(e => e.Grade.Description == dataNonTeachingLoadTo.Grade.Description);
            }

            if (_SubjectPosition != null)
            {
                var idSubjectFrom = _SubjectPosition.Id;
                var SubjectCodeFrom = param.SubjectFrom.Where(e => e.Id == idSubjectFrom).Select(e => e.Code).Distinct().FirstOrDefault();

                if (dataNonTeachingLoadTo.Department == null || dataNonTeachingLoadTo.Grade == null)
                    return default;

                dataNonTeachingLoadTo.Subject = param.SubjectTo
                            .Where(e => e.Code == SubjectCodeFrom && e.IdDepartment == dataNonTeachingLoadTo.Department.Id && e.IdGrade == dataNonTeachingLoadTo.Grade.Id)
                            .Select(e => new ItemValueVm
                            {
                                Id = e.Id,
                                Description = e.Description,
                            })
                            .Distinct()
                            .FirstOrDefault();

                if (dataNonTeachingLoadTo.Subject == null)
                {
                    listErrorMaster.Add("Subject");
                    //return default;
                }

                query = query.Where(e => e.Subject != null && e.Subject.Description == dataNonTeachingLoadTo.Subject.Description);
            }

            if (_StreamingPosition != null)
            {
                var idStreamingFrom = _StreamingPosition.Id;
                var StreamingFrom = _StreamingPosition.Description;

                var StreamingCodeFrom = param.StreamingFrom
                                    .Where(e => e.IdGradePathway == idStreamingFrom && e.Pathway.Description == StreamingFrom)
                                    .Select(e => e.Pathway.Code)
                                    .Distinct().FirstOrDefault();

                dataNonTeachingLoadTo.Streaming = param.StreamingTo
                                .Where(e => e.Pathway.Code == StreamingCodeFrom && e.GradePathway.Grade.Id == dataNonTeachingLoadTo.Grade.Id)
                                .Select(e => new ItemValueVm
                                {
                                    Id = e.IdGradePathway,
                                    Description = e.Pathway.Description
                                }).Distinct().FirstOrDefault();

                if (dataNonTeachingLoadTo.Streaming == null)
                {
                    listErrorMaster.Add("Streaming");
                    //return default;
                }

                query = query.Where(e => e.Streaming.Description == dataNonTeachingLoadTo.Streaming.Description);
            }

            if (_ClassroomPosition != null)
            {
                var idClassroomFrom = _ClassroomPosition.Id;
                var ClassroomCodeFrom = param.ClassroomFrom
                                    .Where(e => e.Id == idClassroomFrom)
                                    .Select(e => e.Classroom.Code)
                                    .Distinct().FirstOrDefault();

                dataNonTeachingLoadTo.Classroom = param.ClassroomTo
                                .Where(e => e.Classroom.Code == ClassroomCodeFrom && e.GradePathway.Grade.Id == dataNonTeachingLoadTo.Grade.Id)
                                .Select(e => new ItemValueVm
                                {
                                    Id = e.Id,
                                    Description = e.Classroom.Description
                                }).Distinct().FirstOrDefault();

                if (dataNonTeachingLoadTo.Classroom == null)
                {
                    listErrorMaster.Add("Classroom");
                    //return default;
                }

                query = query.Where(e => e.Classroom.Description == dataNonTeachingLoadTo.Classroom.Description);
            }

            var exsisTeachingLoadDataTo = query.Any();

            string jsonString = JsonConvert.SerializeObject(dataNonTeachingLoadTo);

            GetDataNonTeachingLoadResult item = new GetDataNonTeachingLoadResult
            {
                Data = jsonString,
                IsExsis = exsisTeachingLoadDataTo,
                MasterError = listErrorMaster.Distinct().ToList()
            };
            return item;
        }
    }

    public class NonTeachingLoadData
    {
        public int No {get; set; }
        public string IdMsNonTeachingLoad {get; set; }
        public CodeWithIdVm Level {get; set; }
        public CodeWithIdVm Department { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Subject { get; set; }
        public CodeWithIdVm Streaming { get; set; }
        public CodeWithIdVm Classroom { get; set; }
    }

    public class GetDataNonTeachingLoadResult
    {
        public string Data { get; set; }
        public List<string> MasterError { get; set; }
        public bool IsExsis { get; set; }
    }

    public class DataNonTeachingLoadRequest
    {
        public string IdAcademicYearFrom { get; set; }
        public string IdAcademicYearTo { get; set; }
        public string Data { get; set; }
        public List<MsGrade> GradeFrom { get; set; }
        public List<MsGrade> GradeTo { get; set; }
        public List<MsDepartment> DepartmentFrom { get; set; }
        public List<MsDepartment> DepartmentTo { get; set; }
        public List<MsSubject> SubjectFrom { get; set; }
        public List<MsSubject> SubjectTo { get; set; }
        public List<MsGradePathwayDetail> StreamingFrom { get; set; }
        public List<MsGradePathwayDetail> StreamingTo { get; set; }
        public List<MsGradePathwayClassroom> ClassroomFrom { get; set; }
        public List<MsGradePathwayClassroom> ClassroomTo { get; set; }
        public List<NonTeachingLoadData> NonTeachingLoadDataTo { get; set; }
    }
   
}
