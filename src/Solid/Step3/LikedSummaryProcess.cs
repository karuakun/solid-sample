using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Solid.Configs;
using Solid.Data;

namespace Solid.Step3
{
    /// <summary>
    /// typetalkから特定期間中の投稿を対象に利用者ごとのいいねの件数を集計する。
    /// </summary>
    public class LikedSummaryProcess: ILikedSummaryProcess
    {
        private readonly Step2.PostLoader _postLoader;
        private readonly Step2.QueryCacheClient _queryCacheClient;
        private readonly IPostAggregater _postAggregater;
        private readonly LayoutConverters.LayoutConveterRepository _layoutConveterRepository;

        public LikedSummaryProcess(
            IOptions<TypetalkConfig> typetalkConfig, 
            TypetalkDataContext typetalkDataContext, 
            IPostAggregater postAggregater,
            LayoutConverters.LayoutConveterRepository layoutConveterRepository)
        {
            _postLoader = new Step2.PostLoader(typetalkConfig);
            _queryCacheClient = new Step2.QueryCacheClient(typetalkDataContext);
            _postAggregater = postAggregater;
            _layoutConveterRepository = layoutConveterRepository;
        }

        public async Task<IEnumerable<Typetalk.Dto.LikedSummary>> Run(string spaceKey, string topicId, DateTime fromDate, DateTime toDate, string layoutName)
        {
            List<Typetalk.Dto.Post> posts;
            // キャッシュがあるか確認し
            if (await _queryCacheClient.HasCacheAsync(spaceKey, topicId, fromDate, toDate))
            {
                // ある場合はキャッシュを取得する。
                var _ = await _queryCacheClient.GetCacheAsync(spaceKey, topicId, fromDate, toDate);
                posts = Mapper.Map<List<Typetalk.Dto.Post>>(_);
            }
            else
            {
                // なければTypetalkから対象期間の情報を検索し
                posts = (await _postLoader.LoadPosts(spaceKey, topicId, fromDate, toDate)).ToList();
                if (!posts.Any())
                {
                    WriteInformationLog($"対象期間のPostが存在しません。{fromDate}～{toDate}");
                    return null;
                }

                // 結果をデータベースにキャッシュする。
                await _queryCacheClient.SetCacheAsync(spaceKey, topicId, fromDate, toDate, posts);
            }
            // 取得したデータを集計し、利用者に返却する。
            var summary = _postAggregater.Aggregate(posts).ToList();
            DumpSummaryLog(summary, layoutName);
            return summary;
        }

        private void DumpSummaryLog(IEnumerable<Typetalk.Dto.LikedSummary> entity, string layoutName)
        {
            var layoutConverter = _layoutConveterRepository.Get(layoutName);
            var text = layoutConverter.ConvertToText(entity);
            WriteInformationLog(text);
        }

        private void WriteInformationLog(string message)
        {
            Console.WriteLine(message);
        }
    }
}