using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Solid.Typetalk.Dto;

namespace Solid.Step3
{
    public interface IPostAggregater
    {
        IOrderedEnumerable<LikedSummary> Aggregate(List<Solid.Typetalk.Dto.Post> posts);
    }

    public class PostAggregater: IPostAggregater
    {
        public IOrderedEnumerable<LikedSummary> Aggregate(List<Solid.Typetalk.Dto.Post> posts)
        {
            return posts.GroupBy(p => new { p.Account.Id, p.Account.Name }, p => p.Likes.Count)
                .Select(g => new Typetalk.Dto.LikedSummary { AccountName = g.Key.Name, LikedCount = g.Sum(_ => _) })
                .OrderByDescending(s => s.LikedCount);
        }
    }
}
