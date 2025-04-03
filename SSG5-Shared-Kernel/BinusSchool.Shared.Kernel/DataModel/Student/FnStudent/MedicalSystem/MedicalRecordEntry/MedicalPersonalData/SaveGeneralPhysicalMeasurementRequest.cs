using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class SaveGeneralPhysicalMeasurementRequest
    {
        public string Id { get; set; }
        public string Mode { get; set; }
        public string IdMedicalPhysicalMeasurement { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public string BloodPressure { get; set; }
        public decimal BodyTemperature { get; set; }
        public int? HeartRate { get; set; }
        public int? Saturation { get; set; }
        public int? RespiratoryRate { get; set; }
        public DateTime MeasurementDate { get; set; }
        public string MeasurementPIC { get; set; }
    }
}
