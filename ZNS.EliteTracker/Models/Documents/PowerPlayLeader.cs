using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum PowerPlayLeader
    {
        None = 1,
        [Display(Name = "Arissa Lavigny-Duval")]
        ArissaLavignyDuval = 2,
        [Display(Name = "Aisling Duval")]
        AislingDuval = 3,
        [Display(Name = "Archon Delaine")]
        ArchonDelaine = 4,
        [Display(Name = "Denton Patreus")]
        DentonPatreus = 5,
        [Display(Name = "Edmund Mahon")]
        EdmundMahon = 6,
        [Display(Name = "Felicia Winters")]
        FeliciaWinters = 7,
        [Display(Name = "Li Yong-Rui")]
        LiYongRui = 8,
        [Display(Name = "Pranav Antal")]
        PranavAntal = 9,
        [Display(Name = "Zachary Hudson")]
        ZacharyHudson = 10,
        [Display(Name = "Zemina Torval")]
        ZeminaTorval = 11,
        [Display(Name = "Valentina Tereshkova")]
        ValentinaTereshkova = 12
    }
}