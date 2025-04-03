using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Employee.FnStaff.Teacher
{
    public class GetTeacherForAscTimetableHandler : FunctionsHttpSingleHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        private readonly IApiService<IUser> _serviceUser;
        public GetTeacherForAscTimetableHandler(IEmployeeDbContext dbContext,
            IApiService<IUser> serviceUser)
        {
            _dbContext = dbContext;
            _serviceUser = serviceUser;
        }


        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();
            //_serviceUser.SetConfigurationFrom(ApiConfiguration);

            var param = await Request.GetBody<CheckTeacherForAscTimetableRequest>();

            var getteacher = await _dbContext.Entity<MsStaff>()
                                      .Where(p => (param.ShortName.Any(x => x == p.ShortName) ||
                                                  param.ShortName.Any(x => x == p.IdBinusian) ||
                                                  param.ShortName.Any(x => x == p.InitialName))
                                                  && (!string.IsNullOrEmpty(param.IdSchool) ? p.IdSchool == param.IdSchool : true))
                                      .Select(p => new CheckTeacherForAscTimetableResult
                                      {
                                          IdTeacher = p.IdBinusian,
                                          TeacherBinusianId = p.IdBinusian,
                                          TeacherName = p.FirstName,
                                          TeacherShortName = p.ShortName,
                                          TeacherInitialName = p.InitialName,
                                      }).ToListAsync();

            //var user = new List<GetUserResult>();

            //if (getteacher != null)
            //{
            //    var getUserFromModuleUser = await _serviceUser.Execute.GetUsers(new GetUserRequest
            //    {
            //        GetAll = true,
            //        Ids = getteacher.Count > 0 ? getteacher.Select(p => p.IdTeacher).ToList() : new List<string>(){"dummy"},
            //        IdSchool =!string.IsNullOrWhiteSpace(param.IdSchool)? new List<string> { param.IdSchool }:new List<string>(),
            //    });
            //    user = getUserFromModuleUser.Payload.ToList();
            //}
            //var query = from teacher in getteacher
            //            join us in user on teacher?.IdTeacher equals us.Id // into pk
            //            select new { tchr = teacher,users= us };

            return Request.CreateApiResult2(getteacher as object);
        }
    }
}
