using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebExample.Models.Data
{
    public class DbName
    {
        private static readonly string[] AllDefinedDbName =
        {
            "BetaBetradar",
            "BetaMainDb",
            "DevBetradar",
            "DevMainDb"
        };

        public static readonly string BetaBetradar = AllDefinedDbName[0];
        public static readonly string BetaMainDb = AllDefinedDbName[1];
        public static readonly string DevBetradar = AllDefinedDbName[2];
        public static readonly string DevMainDb = AllDefinedDbName[3];
    }
}