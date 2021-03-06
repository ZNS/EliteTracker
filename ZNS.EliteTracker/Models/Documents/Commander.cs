﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class Commander : BaseDocument
    {
        public string Name { get; set; }
        public string PlayerName { get; set; }
        public Country Country { get; set; }
        public List<Ship> Ships { get; set; }
        public string Story { get; set; }
        public bool Enabled { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public List<string> Roles { get; set; }
        public List<int> SolarSystemGroups { get; set; }
        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string ImageUrl {
            get {
                return "/commander/image/" + Id;
            }
        }

        public Commander()
        {
            Enabled = true;
            Ships = new List<Ship>();
            Roles = new List<string>();
            SolarSystemGroups = new List<int>();
        }
    }
}