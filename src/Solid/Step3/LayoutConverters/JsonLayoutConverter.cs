using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Solid.Typetalk.Dto;

namespace Solid.Step3.LayoutConverters
{
    public class JsonLayoutConverter: ILayoutConverter
    {
        public string ConvertToText(IEnumerable<LikedSummary> data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}
