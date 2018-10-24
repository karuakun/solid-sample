using System.Collections.Generic;

namespace Solid.Step3.LayoutConverters
{
    public interface ILayoutConverter
    {
        string ConvertToText(IEnumerable<Typetalk.Dto.LikedSummary> data);
    }
}
