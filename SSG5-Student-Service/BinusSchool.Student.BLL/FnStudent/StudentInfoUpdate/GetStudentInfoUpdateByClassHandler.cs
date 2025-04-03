using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentInfoUpdate
{
    public class GetStudentInfoUpdateByClassHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStudentHomeroomDetail _homeroomService;
        private readonly IClassroomMap _classroomMapService;

        public GetStudentInfoUpdateByClassHandler(IStudentDbContext studentDbContext
            , IStudentHomeroomDetail homeroomService
            , IClassroomMap classroomMapService)
        {
            _dbContext = studentDbContext;
            _homeroomService = homeroomService;
            _classroomMapService = classroomMapService;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMappingClassRequest>(nameof(GetMappingClassRequest.IdSchool));
            var predicate = PredicateBuilder.Create<TrStudentInfoUpdate>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IdUser, $"%{param.Search}%"));

            var idStudents = await _dbContext.Entity<TrStudentInfoUpdate>()
                .Select(x => x.Constraint3Value)
                .Distinct()
                .ToListAsync();

            var listStudentbyUpdate = await _dbContext.Entity<TrStudentInfoUpdate>()
                .Select(x => new
                {
                    IdStudent = x.Constraint3Value,
                    IsParentUpdate = x.IsParentUpdate
                })
                .Distinct()
                .ToListAsync();

            /*var lastGrades = await _dbContext.Entity<MsStudentGrade>()
                .Where(x => idStudents.Contains(x.IdStudent))
                .OrderByDescending(x => x.DateIn).ThenByDescending(x => x.DateUp)
                .ToListAsync(CancellationToken);*/

            var lastGrades = await _classroomMapService.GetClassroomMappedsByLevel(new GetMappingClassByLevelRequest
            {
                IdSchool = param.IdSchool,
                IdAcadyear = param.IdAcadyear,
                IdLevel = param.IdLevel,
                Search = param.Search
            });

            var homerooms = Enumerable.Empty<GetGradeAndClassByStudentResult>();
            var homeroomsParam = new GetHomeroomByStudentRequest
            {
                Ids = idStudents,
                IdGrades = lastGrades.Payload.Select(x => x.Grade.Id).Distinct()
            };
            var homeroomsResult = await _homeroomService.GetGradeAndClassByStudents(homeroomsParam);
            homerooms = homeroomsResult.Payload;

            var crMapResult = await _classroomMapService.GetClassroomMappedsByGrade(new GetListGradePathwayClassRoomRequest
            {
                Ids = lastGrades.Payload.Select(x => x.Grade.Id).Distinct()
            });

            var templatesiswa = homeroomsResult.Payload.Select(x => new
            {
                IdGrade = x.Grade.Id,
                Grade = x.Grade.Description,
                IdKelas = x.Classroom.Id,
                kelas = x.Classroom.Code,
                siswa = x.IdStudent
            }).ToList();

            var listStudentUpdate = templatesiswa
                .Join(listStudentbyUpdate,
                templatesiswa => templatesiswa.siswa,
                listStudentbyUpdate => listStudentbyUpdate.IdStudent,
                (templatesiswa, listStudentbyUpdate) => new
                {
                    Grade = templatesiswa.Grade,
                    kelas = templatesiswa.kelas,
                    IdStudent = templatesiswa.siswa,
                    ParentUpdate = listStudentbyUpdate.IsParentUpdate == 1 ? 1 : 0,
                    StaffUpdate = listStudentbyUpdate.IsParentUpdate == 0 ? 1 : 0,
                }).ToList();

            var templateClass = crMapResult.Payload.Select(x => new
            {
                Grade = x.Grade.Description,
                ClassRoom = x.Class.Code,
                ParentUpdatelist = listStudentUpdate.Where(y => y.Grade == x.Grade.Description && y.kelas == x.Class.Code && y.ParentUpdate == 1).Select(x => x.IdStudent).ToList(),
                ParentUpdate = listStudentUpdate.Where( y => y.Grade == x.Grade.Description && y.kelas == x.Class.Code && y.ParentUpdate == 1).Count(),
                StaffUpdatelist = listStudentUpdate.Where(y => y.Grade == x.Grade.Description && y.kelas == x.Class.Code && y.StaffUpdate == 1).Select(x => x.IdStudent).ToList(),
                StaffUpdate = listStudentUpdate.Where(y => y.Grade == x.Grade.Description && y.kelas == x.Class.Code && y.StaffUpdate == 1).Count()
            }).ToList();
            return Request.CreateApiResult2(templateClass as object);
            //throw new NotImplementedException();

            /*IReadOnlyList<GetStudentInfoUpdateResult> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetStudentInfoUpdateResult
                    {
                        IdUser = x.IdUser,
                        TableName = x.TableName,
                        FieldName = x.FieldName,
                        OldFieldValue = x.OldFieldValue,
                        CurrentFieldValue = x.CurrentFieldValue,
                        Constraint1 = x.Constraint1,
                        Constraint1Value = x.Constraint1Value,
                        Constraint2 = x.Constraint2,
                        Constraint2Value = x.Constraint2Value,
                        Constraint3 = x.Constraint3,
                        Constraint3Value = x.Constraint3Value,
                        IsParentUpdate = x.IsParentUpdate
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetStudentInfoUpdateResult
                    {
                        IdUser = x.IdUser,
                        TableName = x.TableName,
                        FieldName = x.FieldName,
                        OldFieldValue = x.OldFieldValue,
                        CurrentFieldValue = x.CurrentFieldValue,
                        Constraint1 = x.Constraint1,
                        Constraint1Value = x.Constraint1Value,
                        Constraint2 = x.Constraint2,
                        Constraint2Value = x.Constraint2Value,
                        Constraint3 = x.Constraint3,
                        Constraint3Value = x.Constraint3Value,
                        IsParentUpdate = x.IsParentUpdate
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdUser).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));

            //var studentHomeroomDetail = await _homeroomService.GetHomeroomByStudents(paramForStudentEnrollment);

            /*IReadOnlyList<GetStudentInfoUpdateResult> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetStudentInfoUpdateResult
                    {
                        IdUser = x.IdUser,
                        TableName = x.TableName,
                        FieldName = x.FieldName,
                        OldFieldValue = x.OldFieldValue,
                        CurrentFieldValue = x.CurrentFieldValue,
                        Constraint1 = x.Constraint1,
                        Constraint1Value = x.Constraint1Value,
                        Constraint2 = x.Constraint2,
                        Constraint2Value = x.Constraint2Value,
                        Constraint3 = x.Constraint3,
                        Constraint3Value = x.Constraint3Value,
                        IsParentUpdate = x.IsParentUpdate
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetStudentInfoUpdateResult
                    {
                        IdUser = x.IdUser,
                        TableName = x.TableName,
                        FieldName = x.FieldName,
                        OldFieldValue = x.OldFieldValue,
                        CurrentFieldValue = x.CurrentFieldValue,
                        Constraint1 = x.Constraint1,
                        Constraint1Value = x.Constraint1Value,
                        Constraint2 = x.Constraint2,
                        Constraint2Value = x.Constraint2Value,
                        Constraint3 = x.Constraint3,
                        Constraint3Value = x.Constraint3Value,
                        IsParentUpdate = x.IsParentUpdate
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdUser).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));*/
        }
    }
}
