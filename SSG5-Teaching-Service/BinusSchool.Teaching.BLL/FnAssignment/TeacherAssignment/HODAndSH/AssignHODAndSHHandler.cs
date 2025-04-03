using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.School.FnSubject;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using BinusSchool.Data.Model.School.FnSubject.Department;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Teaching.FnAssignment.TeacherAssignment.HODAndSH.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.HODAndSH
{
	public class AssignHODAndSHHandler : FunctionsHttpCrudHandler
	{
		private readonly ITeachingDbContext _teachingDbContext;
		private readonly IStringLocalizer _localizer;
		private readonly IServiceProvider _provider;
        private List<string> _deletedIds;
		public AssignHODAndSHHandler(ITeachingDbContext teachingDbContext, IApiService<ISchool> schoolApi,
			IApiService<IUser> userApi, IStringLocalizer localizer,
			IApiService<IGrade> gradeApi, IApiService<IAcademicYear> academicYear,
			IApiService<IClassroomMap> classroomMap, IServiceProvider provider,
			IApiService<IDepartment> department, IApiService<ISubject> subject,
			IApiService<IMetadata> metaDataApi)
		{
			_teachingDbContext = teachingDbContext;
			_localizer = localizer;
			_provider = provider;
		}

		protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
		{
			var assignments = await _teachingDbContext.Entity<TrNonTeachingLoad>()
				.Where(x => ids.Contains(x.Id))
				.ToListAsync(CancellationToken);
			assignments.ForEach(x=>x.IsActive = false);
			_teachingDbContext.Entity<TrNonTeachingLoad>().UpdateRange(assignments);
			await _teachingDbContext.SaveChangesAsync(CancellationToken);
			return Request.CreateApiResult2();
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
			var model = await Request.ValidateBody<AddAssignHODAndSHRequest, AddAssignHODAndSHValidator>();
			Transaction = await _teachingDbContext.BeginTransactionAsync(CancellationToken);
			#region CheckAssingmentHOD

			var checkTeachingLoadInAcademicYear = await _teachingDbContext.Entity<MsNonTeachingLoad>()
					.Include(x => x.TeacherPosition)
						.ThenInclude(x => x.Position)
				.Where(x => new[] { PositionConstant.HeadOfDepartment, PositionConstant.SubjectHead , PositionConstant.SubjectHeadAssitant }.Contains(x.TeacherPosition.Position.Code))
				// .Where(x => x.TeacherPosition.IdSchool == model.IdSchool)
				.Where(x=>x.IdAcademicYear == model.IdSchoolAcadYear)
				.ToListAsync();

			if (checkTeachingLoadInAcademicYear.Where(x=>x.TeacherPosition.Position.Code == PositionConstant.HeadOfDepartment).FirstOrDefault() == null)
				    throw new Exception($"Position {PositionConstant.HeadOfDepartment} not exists in this academic year");

            if (checkTeachingLoadInAcademicYear.Where(x => x.TeacherPosition.Position.Code == PositionConstant.SubjectHead).FirstOrDefault() == null)
                    throw new Exception($"Position {PositionConstant.SubjectHead} not exists in this academic year");

            if (checkTeachingLoadInAcademicYear.Where(x => x.TeacherPosition.Position.Code == PositionConstant.SubjectHeadAssitant).FirstOrDefault() == null)
                    throw new Exception($"Position {PositionConstant.SubjectHeadAssitant} not exists in this academic year");

            if (checkTeachingLoadInAcademicYear
               .Where(x => x.Parameter != null)
                .Where(x => x.TeacherPosition.Position.Code == PositionConstant.HeadOfDepartment)
                .FirstOrDefault() == null)
                    throw new Exception($"Parameter data for position {PositionConstant.HeadOfDepartment} have not been set");

            if (checkTeachingLoadInAcademicYear
               .Where(x => x.Parameter != null)
               .Where(x => x.TeacherPosition.Position.Code == PositionConstant.SubjectHead)
               .FirstOrDefault() == null)
                    throw new Exception($"Parameter data for position {PositionConstant.SubjectHead} have not been set");

            if (checkTeachingLoadInAcademicYear
            .Where(x => x.Parameter != null)
               .Where(x => x.TeacherPosition.Position.Code == PositionConstant.SubjectHeadAssitant)
               .FirstOrDefault() == null)
                    throw new Exception($"Parameter data for position {PositionConstant.SubjectHeadAssitant} have not been set");

            var userHODInSchool = new TrNonTeachingLoad();
			var checkUserHODInSchool = await _teachingDbContext.Entity<TrNonTeachingLoad>()
				.Include(x => x.MsNonTeachingLoad)
					.ThenInclude(x => x.TeacherPosition)
						.ThenInclude(x => x.Position)
				.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.HeadOfDepartment)
				.Where(x => x.MsNonTeachingLoad.TeacherPosition.IdSchool == model.IdSchool)
				.ToListAsync();
			//check spesific HOD by department exist or not
			foreach (var hod in checkUserHODInSchool)
			{
				var data = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(hod.Data);
				if (data.TryGetValue("Department", out var department) && department.Id == model.IdSchoolDepartment)
					userHODInSchool = hod;
			}

			#endregion

			if (userHODInSchool.Id != null)
			{
				if (model.IdSchoolUserHeadOfDepartment != userHODInSchool.IdUser)
				{
					userHODInSchool.IdUser = model.IdSchoolUserHeadOfDepartment;
					userHODInSchool.Data = model.Data;
					userHODInSchool.Load = model.Load;
					userHODInSchool.IdMsNonTeachingLoad = model.IdSchoolNonTeachingLoadDepartment;
					_teachingDbContext.Entity<TrNonTeachingLoad>().Update(userHODInSchool);
				}
			}
			else
			{
				//insert data for Head Of Department
				var insertData = new TrNonTeachingLoad();
				insertData.Id = Guid.NewGuid().ToString();
				insertData.IdUser = model.IdSchoolUserHeadOfDepartment;
				insertData.IdMsNonTeachingLoad = model.IdSchoolNonTeachingLoadDepartment;
				insertData.Load = model.Load;
				insertData.Data = model.Data;
				_teachingDbContext.Entity<TrNonTeachingLoad>().Add(insertData);
			}

			foreach (var item in model.SubjectHeads)
			{
				var data = await _teachingDbContext.Entity<TrNonTeachingLoad>().Where(x => x.Id == item.Id).FirstOrDefaultAsync();
				if (data == null)
				{
					if (!string.IsNullOrEmpty(item.IdSchoolUser))
					{
						var insertData = new TrNonTeachingLoad();
						insertData.Id = Guid.NewGuid().ToString();
						insertData.IdUser = item.IdSchoolUser;
						insertData.IdMsNonTeachingLoad = item.IdSchoolNonTeachingLoad;
						insertData.Load = item.Load;
						insertData.Data = item.Data;
						_teachingDbContext.Entity<TrNonTeachingLoad>().Add(insertData);
					}
					var dataSubjectHeadAssistance = await _teachingDbContext.Entity<TrNonTeachingLoad>().Where(x => x.Id == item.SubjectHeadAssitance.Id).FirstOrDefaultAsync();
					if (dataSubjectHeadAssistance == null)
					{
						if (!string.IsNullOrEmpty(item.SubjectHeadAssitance.IdSchoolUser))
						{
							var insertDataSubjectHeadAssistance = new TrNonTeachingLoad();
							insertDataSubjectHeadAssistance.Id = Guid.NewGuid().ToString();
							insertDataSubjectHeadAssistance.IdUser = item.SubjectHeadAssitance.IdSchoolUser;
							insertDataSubjectHeadAssistance.IdMsNonTeachingLoad = item.SubjectHeadAssitance.IdSchoolNonTeachingLoad;
							insertDataSubjectHeadAssistance.Load = item.SubjectHeadAssitance.Load;
							insertDataSubjectHeadAssistance.Data = item.SubjectHeadAssitance.Data;
							_teachingDbContext.Entity<TrNonTeachingLoad>().Add(insertDataSubjectHeadAssistance);
						}

					}
					else
					{
						if (!string.IsNullOrEmpty(item.SubjectHeadAssitance.IdSchoolUser))
						{
							dataSubjectHeadAssistance.IdUser = item.SubjectHeadAssitance.IdSchoolUser;
							dataSubjectHeadAssistance.IdMsNonTeachingLoad = item.SubjectHeadAssitance.IdSchoolNonTeachingLoad;
							dataSubjectHeadAssistance.Load = item.SubjectHeadAssitance.Load;
							dataSubjectHeadAssistance.Data = item.SubjectHeadAssitance.Data;
							_teachingDbContext.Entity<TrNonTeachingLoad>().Update(dataSubjectHeadAssistance);
						}

					}
				}
				else
				{
					if (!string.IsNullOrEmpty(item.IdSchoolUser))
					{
						data.IdUser = item.IdSchoolUser;
						data.IdMsNonTeachingLoad = item.IdSchoolNonTeachingLoad;
						data.Load = item.Load;
						data.Data = item.Data;
						_teachingDbContext.Entity<TrNonTeachingLoad>().Update(data);
					}

					var dataSubjectHeadAssistance = await _teachingDbContext.Entity<TrNonTeachingLoad>().Where(x => x.Id == item.SubjectHeadAssitance.Id).FirstOrDefaultAsync();

					if (dataSubjectHeadAssistance == null)
					{
						if (!string.IsNullOrEmpty(item.SubjectHeadAssitance.IdSchoolUser))
						{
							var insertDataSubjectHeadAssistance = new TrNonTeachingLoad();
							insertDataSubjectHeadAssistance.Id = Guid.NewGuid().ToString();
							insertDataSubjectHeadAssistance.IdUser = item.SubjectHeadAssitance.IdSchoolUser;
							insertDataSubjectHeadAssistance.IdMsNonTeachingLoad = item.SubjectHeadAssitance.IdSchoolNonTeachingLoad;
							insertDataSubjectHeadAssistance.Load = item.SubjectHeadAssitance.Load;
							insertDataSubjectHeadAssistance.Data = item.SubjectHeadAssitance.Data;
							_teachingDbContext.Entity<TrNonTeachingLoad>().Add(insertDataSubjectHeadAssistance);
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(item.SubjectHeadAssitance.IdSchoolUser))
						{
							dataSubjectHeadAssistance.IdUser = item.SubjectHeadAssitance.IdSchoolUser;
							dataSubjectHeadAssistance.IdMsNonTeachingLoad = item.SubjectHeadAssitance.IdSchoolNonTeachingLoad;
							dataSubjectHeadAssistance.Load = item.SubjectHeadAssitance.Load;
							dataSubjectHeadAssistance.Data = item.SubjectHeadAssitance.Data;
							_teachingDbContext.Entity<TrNonTeachingLoad>().Update(dataSubjectHeadAssistance);
						}
					}
				}

			}
			await _teachingDbContext.SaveChangesAsync(CancellationToken);
			await Transaction.CommitAsync(CancellationToken);

			return Request.CreateApiResult2();
		}

		protected override Task<ApiErrorResult<object>> PutHandler()
		{
			throw new NotImplementedException();
		}

		// private string GetStatus(string departmentId, IEnumerable<CodeWithIdVm> subjects , string IdAcademicYear)
		// {
		// 	var isHODAssigned = false;
		// 	var isSHAssigned = false;
		// 	_deletedIds = new List<string>();
		// 	var teachingLoads = _teachingDbContext.Entity<TrNonTeachingLoad>()
		// 		.Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.Position)
		// 		.Where(x => new[] { PositionConstant.HeadOfDepartment, PositionConstant.SubjectHead,PositionConstant.SubjectHeadAssitant }.Contains(x.MsNonTeachingLoad.TeacherPosition.Position.Code))
		// 		.Where(x=>x.MsNonTeachingLoad.IdAcademicYear == IdAcademicYear)
		// 		.ToList();

		// 	foreach (var item in teachingLoads.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.HeadOfDepartment))
		// 	{
		// 		var data = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
		// 		if (data.TryGetValue("Department", out var department))
		// 			if (department.Id == departmentId)
		// 			{
		// 				if(department.Id == departmentId)
        //                 {
        //                     isHODAssigned = true;
        //                     _deletedIds.Add(item.Id);
		// 					continue;
        //                 }
        //                 else
        //                 {
        //                     isHODAssigned = false;
        //                 }
		// 			}
		// 	}
		// 	List<bool> statusSH = new List<bool>();
		// 	foreach (var datasubject in subjects)
		// 	{
		// 		if (!string.IsNullOrEmpty(datasubject.Id))
		// 		{
		// 			var teachingLoadSH = teachingLoads
		// 			.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.SubjectHead)
		// 			.Where(x=> 
		// 			{
		// 				bool matchDepartment = true, matchSubject = true;
		// 				var department = x.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Department", false);
		// 				var subject = x.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Subject", false);

		// 				matchDepartment = department.Id == departmentId;
		// 				matchSubject = subject.Id == datasubject.Id;
		// 				return matchDepartment && matchSubject;
		// 			}).FirstOrDefault();
		// 			if (teachingLoadSH != null)
		// 			{
		// 				statusSH.Add(true);
		// 				 _deletedIds.Add(teachingLoadSH.Id);
		// 			}
		// 			else
		// 			{
		// 				statusSH.Add(false);
		// 			}
		// 			var teachingLoadSHA = teachingLoads
		// 			.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.SubjectHead)
		// 			.Where(x=> 
		// 			{
		// 				bool matchDepartment = true, matchSubject = true;
		// 				var department = x.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Department", false);
		// 				var subject = x.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Subject", false);

		// 				matchDepartment = department.Id == departmentId;
		// 				matchSubject = subject.Id == datasubject.Id;
		// 				return matchDepartment && matchSubject;
		// 			}).FirstOrDefault();
		// 			if (teachingLoadSH != null)
		// 			{
		// 				 _deletedIds.Add(teachingLoadSHA.Id);
		// 			}
		// 		}
		// 	}
		// 	isSHAssigned = statusSH.All(x=>x);
		// 	var status = (isHODAssigned, isSHAssigned) switch
		// 	{
		// 		(true, true) => "All Assigned",
		// 		(false, true) => "SH Assigned",
		// 		(true, false) => "HOD Assigned",
		// 		_ => "HOD and SH Not Assigned"
		// 	};

		// 	return status;
		// }
	}
}
