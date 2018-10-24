using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Solid.Configs;
using Solid.Step2;
using Solid.Typetalk;

namespace Solid.Step4
{
    public class PostLoader : IPostLoader
    {
        private readonly TypetalkConfig _typetalkConfig;
        private readonly IHttpClientFactory _httpClientFactory;

        public PostLoader(IOptions<TypetalkConfig> typetalkConfig, IHttpClientFactory httpClientFactory)
        {
            _typetalkConfig = typetalkConfig.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<Typetalk.Dto.Post>> LoadPosts(string spaceKey, string topicId, DateTime fromDate, DateTime toDate)
        {
            var typeTalkConnection = TypeTalkConnection.Create(_httpClientFactory.CreateClient(_typetalkConfig.TypetalkApiUrl), _typetalkConfig.ClientId, _typetalkConfig.ClientSecret);
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
