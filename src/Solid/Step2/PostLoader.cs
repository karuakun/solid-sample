using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Solid.Configs;
using Solid.Typetalk;

namespace Solid.Step2
{
    public interface IPostLoader
    {
        Task<IEnumerable<Typetalk.Dto.Post>> LoadPosts(string spaceKey, string topicId, DateTime fromDate, DateTime toDate);
    }

    public class PostLoader : IPostLoader
    {
        private readonly TypetalkConfig _typetalkConfig;

        public PostLoader(IOptions<TypetalkConfig> typetalkConfig)
        {
            _typetalkConfig = typetalkConfig.Value;
        }

        public async Task<IEnumerable<Typetalk.Dto.Post>> LoadPosts(string spaceKey, string topicId, DateTime fromDate, DateTime toDate)
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
    }
}
