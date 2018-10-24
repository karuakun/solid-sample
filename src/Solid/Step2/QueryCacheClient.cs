using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Solid.Data;

namespace Solid.Step2
{
    public interface IQueryCacheClient
    {
        Task<IEnumerable<Data.Entities.Post>> GetCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate);
        Task SetCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate, List<Typetalk.Dto.Post> posts);
        Task<bool> HasCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate);
    }

    public class QueryCacheClient : IQueryCacheClient
    {
        private readonly TypetalkDataContext _typetalkDataContext;

        public QueryCacheClient(TypetalkDataContext typetalkDataContext)
        {
            _typetalkDataContext = typetalkDataContext;
        }

        public async Task<IEnumerable<Data.Entities.Post>> GetCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate)
        {
            var cache = await _typetalkDataContext
                .QueryCache
                .FirstAsync(q => q.SpaceKey == spaceKey
                                 && q.TopicId == topicId
                                 && q.FromDate == fromDate
                                 && q.ToDate == toDate);
            return await _typetalkDataContext
                .Post
                .Where(p => p.QueryCacheId == cache.Id)
                .Include(p => p.Account)
                .Include(p => p.Likes)
                .ToListAsync();
        }
        public async Task SetCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate, List<Typetalk.Dto.Post> posts)
        {

            var postsData = Mapper.Map<Collection<Data.Entities.Post>>(posts);
            var cache = new Data.Entities.QueryCache
            {
                Id = Guid.NewGuid().ToString(),
                SpaceKey = spaceKey,
                TopicId = topicId,
                FromDate = fromDate,
                ToDate = toDate
            };
            foreach (var p in postsData)
            {
                p.QueryCacheId = cache.Id;
                p.Id = $"{p.QueryCacheId}-{p.PostId}";
            }

            await _typetalkDataContext.QueryCache.AddAsync(cache);
            await _typetalkDataContext.Post.AddRangeAsync(postsData);
            _typetalkDataContext.SaveChanges();
        }

        public async Task<bool> HasCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate)
        {
            return await _typetalkDataContext.QueryCache
                .AnyAsync(p =>
                    p.SpaceKey == spaceKey 
                    && p.TopicId == topicId 
                    && p.FromDate == fromDate 
                    && p.ToDate == toDate);
        }

    }
}
