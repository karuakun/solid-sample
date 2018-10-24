using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Solid.Typetalk.Dto;

namespace Solid.Step3.LayoutConverters
{
    public class CsvLayoutConverter: ILayoutConverter
    {
        public string ConvertToText(IEnumerable<LikedSummary> data)
        {
            var result = new List<string>();
            result.Add($"#{nameof(LikedSummary.AccountName)},{nameof(LikedSummary.LikedCount)}");
            result.AddRange(data.Select(l => $"{l.AccountName},{l.LikedCount}"));
            return string.Join(Environment.NewLine, result);
        }
    }
}
