using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.CertificationType;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface ICertificationType : IFnStaff
    {
        [Get("/staff/certification-type")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetCertificationTypes(CollectionRequest query);
    }
}