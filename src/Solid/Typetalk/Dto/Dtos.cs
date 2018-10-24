using System;
using System.Collections.Generic;

namespace Solid.Typetalk.Dto
{
    public class LikedSummary
    {
        public string AccountName { get; set; }
        public int LikedCount { get; set; }
    }
    public class GetTopicRequest : TypetalkApiRequest
    {
        public override string ApiName { get; set; } = "api/v1/topics/{0}";

        [TypetalkRequestParameter(Name = "count")]
        public string Count { get; set; } = "200";

        [TypetalkRequestParameter(Name = "direction")]
        public string Direction { get; set; } = "backward";

        [TypetalkRequestParameter(Name = "from")]
        public string FromPostId { get; set; }
    }


    public class SearchRequest : TypetalkApiRequest
    {
        public override string ApiName { get; set; } = "api/v2/search/posts";
        [TypetalkRequestParameter(Name = "q")] public string Query { get; set; }

        [TypetalkRequestParameter(Name = "spaceKey")]
        public string SpaceKey { get; set; }

        [TypetalkRequestParameter(Name = "topicIds")]
        public string TopickId { get; set; }

        [TypetalkRequestParameter(Name = "from")]
        public string FromDate { get; set; }

        [TypetalkRequestParameter(Name = "to")]
        public string ToDate { get; set; }

    }
    
    public class Account
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }
    public class Like
    {
        public string PostId { get; set; }
        public string Id { get; set; }
        public Account Account { get; set; }
    }
   
    public class Post
    {
        public string Id { get; set; }
        public string TopicId { get; set; }
        public Account Account { get; set; }
        public string Message { get; set; }

        public ICollection<Like> Likes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PostResponse
    {
        public Post[] Posts { get; set; }
    }
}
