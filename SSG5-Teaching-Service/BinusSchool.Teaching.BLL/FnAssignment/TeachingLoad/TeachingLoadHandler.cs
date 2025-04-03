using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeachingLoad;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TeachingLoad
{
    public class TeachingLoadHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetTeacherLoadRequest.IdSchool), nameof(GetTeacherLoadRequest.IdAcademicYear) };

        private readonly ITeachingDbContext _teachingDbContext;

        public TeachingLoadHandler(ITeachingDbContext dbContext)
        {
            _teachingDbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherLoadRequest>(_requiredParams);

            var timeTable = await _teachingDbContext.Entity<TrTimeTablePrefHeader>()
               .Where(x
                   => x.IdParent == null
                   && x.SubjectCombination.Subject.Grade.Level.IdAcademicYear == param.IdAcademicYear)
               .SelectMany(x => x.TimetablePrefDetails.SelectMany(y => y.TeachingLoads.Select(z => new { z.IdUser, z.Load })))
               .ToListAsync(CancellationToken);
            IReadOnlyList<IItemValueVm> items;
            var predicateUser = PredicateBuilder.Create<MsUser>(x
                => x.UserSchools.Any(y => y.IdSchool == param.IdSchool.FirstOrDefault())
                && x.UserRoles.Any(y => y.Role.RoleGroup.Code == RoleConstant.Teacher));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicateUser = predicateUser.And(x
                    => EF.Functions.Like(x.Id, param.SearchPattern())
                    || EF.Functions.Like(x.DisplayName, param.SearchPattern()));
            var query = _teachingDbContext.Entity<MsUser>()
                .Where(predicateUser)
                ;


            var dataUserInSchool = await query.SetPagination(param).Select(x => new { x.Id, x.DisplayName }).ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(dataUserInSchool.Count)
            ? dataUserInSchool.Count
            : await query.Select(x => x.Id).CountAsync(CancellationToken);

            var ListShortName = dataUserInSchool.Select(x => x.Id).ToList();

            var dataDetailUser = _teachingDbContext.Entity<MsStaff>()
                .Where(x => ListShortName.Contains(x.IdBinusian))
                .Select(p => new CheckTeacherForAscTimetableResult
                {
                    TeacherBinusianId = p.IdBinusian,
                })
                .ToList();

            var data = (from _dataUser in dataUserInSchool
                        join _dataDetailUser in dataDetailUser on _dataUser.Id equals _dataDetailUser.TeacherBinusianId
                        join _teachingLoad in
                            (
                                timeTable
                                    .GroupBy(x => x.IdUser)
                                    .Select(x => new { IdUser = x.Key, Load = x.Sum(y => y.Load) })
                            ) on _dataUser.Id equals _teachingLoad.IdUser into joinedTeaching
                        from _teachingLoad in joinedTeaching.DefaultIfEmpty(new
                        {
                            IdUser = "",
                            Load = 0
                        })
                        join _nonTeachingLoad in
                            (
                                from a in _teachingDbContext.Entity<TrNonTeachingLoad>()
                                join b in _teachingDbContext.Entity<MsNonTeachingLoad>() on a.IdMsNonTeachingLoad equals b.Id
                                where b.IdAcademicYear == param.IdAcademicYear
                                group a by a.IdUser into g
                                select new
                                {
                                    IdUser = g.Key,
                                    Load = g.Sum(x => x.Load)
                                }
                            ) on _dataUser.Id equals _nonTeachingLoad.IdUser into joinedNonTeaching
                        from _nonTeachingLoad in joinedNonTeaching.DefaultIfEmpty(new
                        {
                            IdUser = "",
                            Load = 0
                        })
                        join _assignToAnotherClass in
                            (
                                from _assign in _teachingDbContext.Entity<TrNonTeachingLoad>()
                                join _msTeachingLoad in _teachingDbContext.Entity<MsNonTeachingLoad>() on _assign.IdMsNonTeachingLoad equals _msTeachingLoad.Id
                                join _msTeacherPosition in _teachingDbContext.Entity<MsTeacherPosition>() on _msTeachingLoad.IdTeacherPosition equals _msTeacherPosition.Id
                                join _ltPosition in _teachingDbContext.Entity<LtPosition>() on _msTeacherPosition.IdPosition equals _ltPosition.Id
                                where
                                _ltPosition.Code == PositionConstant.ClassAdvisor
                                group _assign by _assign.IdUser into g
                                select new
                                {
                                    IdUser = g.Key
                                }
                            ) on _dataUser.Id equals _assignToAnotherClass.IdUser into joinedAssignCA
                        from _assignToAnotherClass in joinedAssignCA.DefaultIfEmpty(new
                        {
                            IdUser = ""
                        })
                        select new GetTeacherLoadResult
                        {
                            Id = _dataUser.Id,
                            BinusianId = _dataUser.Id,
                            NonTeachingLoad = _nonTeachingLoad.Load,
                            TeachingLoad = _teachingLoad.Load,
                            Code = _dataDetailUser.TeacherShortName,
                            Description = string.Format("{0}-{1}-{2} - {3}/{4}/{5}",
                                _dataUser.DisplayName,
                                _dataDetailUser.TeacherShortName,
                                _dataDetailUser.TeacherBinusianId,
                                _teachingLoad.Load,
                                _nonTeachingLoad.Load,
                                _teachingLoad.Load + _nonTeachingLoad.Load
                            ),
                            AssignCA = !string.IsNullOrWhiteSpace(_assignToAnotherClass?.IdUser)
                        }).ToList();

            return Request.CreateApiResult2(data as object,  param.CreatePaginationProperty(count).AddColumnProperty(null));
        }
    }
}
