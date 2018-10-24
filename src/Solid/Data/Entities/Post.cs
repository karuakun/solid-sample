using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Solid.Data.Entities
{
    public class Post
    {
        public string Id { get; set; }
        public string QueryCacheId { get; set; }
        public string PostId { get; set; }
        public string TopicId { get; set; }
        public Account Account { get; set; }
        public string Message { get; set; }

        [ForeignKey(nameof(Like.PostId))]
        public ICollection<Like> Likes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
