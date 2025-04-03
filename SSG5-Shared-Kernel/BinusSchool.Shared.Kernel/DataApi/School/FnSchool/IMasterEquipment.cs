using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentType;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IMasterEquipment : IFnSchool
    {
        [Get("/school/master-equipment/get-list-equipment-type")]
        Task<ApiErrorResult<IEnumerable<GetListEquipmentTypeResult>>> GetListEquipmentType(GetListEquipmentTypeRequest query);

        [Get("/school/master-equipment/get-detail-equipment-type")]
        Task<ApiErrorResult<GetDetailEquipmentTypeResult>> GetDetailEquipmentType(GetDetailEquipmentTypeRequest query);

        [Post("/school/master-equipment/save-equipment-type")]
        Task<ApiErrorResult> SaveEquipmentType([Body] SaveEquipmentTypeRequest body);

        [Post("/school/master-equipment/delete-equipment-type")]
        Task<ApiErrorResult> DeleteEquipmentType([Body] DeleteEquipmentTypeRequest body);

        [Get("/school/master-equipment/get-ddl-equipment-type")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> GetDDLEquipmentType(GetDDLEquipmentTypeRequest query);


        [Get("/school/master-equipment/get-list-equipment-details")]
        Task<ApiErrorResult<IEnumerable<GetListEquipmentDetailsResult>>> GetListEquipmentDetails(GetListEquipmentDetailsRequest query);

        [Get("/school/master-equipment/get-detail-equipment-details")]
        Task<ApiErrorResult<GetDetailEquipmentDetailsResult>> GetDetailEquipmentDetails(GetDetailEquipmentDetailsRequest query);

        [Post("/school/master-equipment/save-equipment-details")]
        Task<ApiErrorResult> SaveEquipmentDetails([Body] SaveEquipmentDetailsRequest body);

        [Post("/school/master-equipment/delete-equipment-details")]
        Task<ApiErrorResult> DeleteEquipmentDetails([Body] DeleteEquipmentDetailsRequest body);

    }
}
