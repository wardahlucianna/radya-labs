using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine
{
    public class SaveMedicalVaccineRequest
    {
        public string? IdMedicalVaccine { get; set; }
        public string MedicalVaccineName { get; set; }
        public string IdDosageType { get; set; }
        public int DosageAmount { get; set; }
        public string IdSchool { get; set; }
    }
}
