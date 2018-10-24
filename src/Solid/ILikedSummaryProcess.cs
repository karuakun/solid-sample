using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Solid
{
    public interface ILikedSummaryProcess
    {
        Task<IEnumerable<Typetalk.Dto.LikedSummary>> Run(string spaceKey, string topicId, DateTime fromDate, DateTime toDate, string layoutName);
    }
}
