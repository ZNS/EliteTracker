using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ZNS.EliteTracker.Models.Documents
{
    public enum FactionGovernment
    {
		Anarchy = 1,
		Democracy = 2,
		Dictatorship = 3,
		Corporate = 4,
		Confederacy = 5,
		Patronage = 6,
        [Display(Name = "Religious Cult")]
		ReligiousCult = 7,
		Theocracy = 8,
		Autocracy = 9,
		Communism = 10,
		Feudal = 11,
        [Display(Name = "Imperial Colony")]
		ImperialColony = 12,
        [Display(Name = "Imperial Protectorate")]
		ImperialProtectorate = 13,
        [Display(Name = "Prison Colony")]
		PrisonColony = 14,
        Colony = 15,
        Cooperative = 16,
        Imperial = 17
    }
}