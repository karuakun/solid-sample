using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Solid.Configs;
using Solid.Data;
using Solid.Typetalk;

namespace Solid.Step1
{
    /// <summary>
    /// typetalkから特定期間中の投稿を対象に利用者ごとのいいねの件数を集計する。
    /// </summary>
    public class LikedSummaryProcess: ILikedSummaryProcess
    {

        private readonly TypetalkConfig _typetalkConfig;
        private readonly TypetalkDataContext _typetalkDataContext;

        public LikedSummaryProcess(IOptions<TypetalkConfig> typetalkConfig, TypetalkDataContext typetalkDataContext)
        {
            _typetalkConfig = typetalkConfig.Value;
            _typetalkDataContext = typetalkDataContext;
        }

        public async Task<IEnumerable<Typetalk.Dto.LikedSummary>> Run(string spaceKey, string topicId, DateTime fromDate, DateTime toDate, string layoutName)
        {
            List<Typetalk.Dto.Post> posts;
            // キャッシュがあるか確認し
            if (await HasCacheAsync(spaceKey, topicId, fromDate, toDate))
            {
                // ある場合はキャッシュを取得する。
                var _ = await GetCacheAsync(spaceKey, topicId, fromDate, toDate);
                posts = Mapper.Map<List<Typetalk.Dto.Post>>(_);
            }
            else
            {
                // なければTypetalkから対象期間の情報を検索し
                posts = (await SearchPosts(spaceKey, topicId, fromDate, toDate)).ToList();
                if (!posts.Any())
                {
                    WriteInformationLog($"対象期間のPostが存在しません。{fromDate}～{toDate}");
                    return null;
                }

                // 結果をデータベースにキャッシュする。
                await SetCacheAsync(spaceKey, topicId, fromDate, toDate, posts);
            }

            // 取得したデータを集計し、利用者に返却する。
            var summary = posts.GroupBy(p => new { p.Account.Id, p.Account.Name }, p => p.Likes.Count)
                .Select(g => new Typetalk.Dto.LikedSummary { AccountName = g.Key.Name, LikedCount = g.Sum(_ => _)})
                    .OrderByDescending(s => s.LikedCount).ToList();
            DumpSummaryLog(summary, layoutName);
            return summary;
        }

        private async Task<IEnumerable<Data.Entities.Post>> GetCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate)
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
        private async Task SetCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate, List<Typetalk.Dto.Post> posts)
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

        private async Task<bool> HasCacheAsync(string spaceKey, string topicId, DateTime fromDate, DateTime toDate)
        {
            return await _typetalkDataContext.QueryCache
                .AnyAsync(p =>
                    p.SpaceKey == spaceKey
                    && p.TopicId == topicId
                    && p.FromDate == fromDate
                    && p.ToDate == toDate);
        }

        private async Task<IEnumerable<Typetalk.Dto.Post>> SearchPosts(string spaceKey, string topicId, DateTime fromDate, DateTime toDate)
        {
            var typeTalkConnection = TypeTalkConnection.Create(_typetalkConfig.TypetalkApiUrl, _typetalkConfig.ClientId, _typetalkConfig.ClientSecret);
            await typeTalkConnection.Login();

            var foundPosts = await typeTalkConnection.GetAsync<Typetalk.Dto.SearchRequest, Typetalk.Dto.PostResponse>(
                new Typetalk.Dto.SearchRequest
                {
                    Query = string.Empty,
                    SpaceKey = spaceKey,
                    TopickId = topicId,
                    FromDate = fromDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ToDate = toDate.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                });
            var newestPost = foundPosts.Posts.FirstOrDefault(p => p.Id == foundPosts.Posts.Max(_ => _.Id));
            if (newestPost == null)
                return null;

            // 検索の始点から Forward して、toDateまで取得する
            var foundsPost = new List<Typetalk.Dto.Post>();
            {
                var fromPostId = newestPost.Id;
                while (true)
                {
                    var posts = await typeTalkConnection.GetAsync<Typetalk.Dto.GetTopicRequest, Typetalk.Dto.PostResponse>(
                        new Typetalk.Dto.GetTopicRequest
                        {
                            ApiName = $"api/v1/topics/{topicId}",
                            FromPostId = fromPostId
                        });


                    var rangePosts = posts.Posts.Where(p => fromDate <= p.CreatedAt && p.CreatedAt < toDate).ToList();
                    if (!rangePosts.Any())
                        break;
                    foundsPost.AddRange(rangePosts);
                    fromPostId = rangePosts.Min(p => p.Id);
                }
            }
            return foundsPost;
        }

        private void DumpSummaryLog(IEnumerable<Typetalk.Dto.LikedSummary> entity, string layoutName)
        {
            var json =JsonConvert.SerializeObject(entity);
            WriteInformationLog(json);
        }

        private void WriteInformationLog(string message)
        {
            Console.WriteLine(message);
        }
    }
}