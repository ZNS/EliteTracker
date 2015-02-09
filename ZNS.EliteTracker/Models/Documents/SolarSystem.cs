using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public class SolarSystem : BaseDocument, ICommentable
    {
        public string Name { get; set; }
        public long Population { get; set; }
        public long PopulationPrev { get; set; }
        public SolarSystemSecurity Security { get; set; }
        public SolarSystemSecurity SecurityPrev { get; set; }
        public Coordinate Coordinates { get; set; }
        public List<Station> Stations { get; set; }
        public List<CommanderRef> ActiveCommanders { get; set; }
        public List<FactionRef> Factions { get; set; }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public double Distance { get; set; }
        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public FactionAttitude Attitude
        {
            get
            {
                var main = Stations.FirstOrDefault(x => x.Main);
                if (main != null && main.Faction != null)
                {
                    return main.Faction.Attitude;
                }
                return FactionAttitude.Neutral;
            }
        }
        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public List<StationEconomy> Economies
        {
            get
            {
                return Stations.SelectMany(x => x.Economy).Distinct().ToList();
            }
        }

        [Raven.Imports.Newtonsoft.Json.JsonIgnore]
        public string IdPrefix
        {
            get
            {
                return "SolarSystems";
            }
        }

        public SolarSystem()
        {
            Security = SolarSystemSecurity.Normal;
            Coordinates = new Coordinate();
            Stations = new List<Station>();
            ActiveCommanders = new List<CommanderRef>();
            Factions = new List<FactionRef>();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SolarSystem))
                return false;
            if (((SolarSystem)obj).Id == 0 && this.Id == 0)
                return base.Equals(obj);
            return ((SolarSystem)obj).Id == this.Id;
        }

        public override int GetHashCode()
        {
            if (Id != 0)
                return Id.GetHashCode();
            return base.GetHashCode();
        }
    }
}