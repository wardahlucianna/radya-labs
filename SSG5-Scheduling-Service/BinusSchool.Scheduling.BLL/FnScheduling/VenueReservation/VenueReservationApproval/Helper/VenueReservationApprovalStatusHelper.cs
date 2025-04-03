using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;
using System.ComponentModel;
using System.Reflection;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Helper
{
    public static class VenueReservationApprovalStatusHelper
    {
        public static string ApprovalStatus(int status)
        {
            if (!Enum.IsDefined(typeof(VenueApprovalStatus), status))
            {
                throw new ArgumentException("Invalid value for VenueApprovalStatus enum");
            }

            VenueApprovalStatus enumValue = (VenueApprovalStatus)status;
            FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
            }

            // Return the enum value as string if no description is found
            return "Default value";
        }
    }
}
