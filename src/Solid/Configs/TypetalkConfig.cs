using System;
using System.Collections.Generic;
using System.Text;

namespace Solid.Configs
{
    public class TypetalkConfig
    {
        public static string SectionName = "typetalk";
        public string TypetalkApiUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
