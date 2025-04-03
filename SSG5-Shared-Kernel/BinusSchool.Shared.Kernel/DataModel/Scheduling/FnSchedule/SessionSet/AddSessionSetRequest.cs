using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SessionSet
{
    public class AddSessionSetRequest 
    {
        public string IdSchool { get; set; }
        //optional unutk mekanisme copy pas create 
        public string IdSessionFrom { get; set; }
        public string Name { get; set; }
    }
}
