using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Solid.Data.Entities
{
    public class QueryCache
    {
        public string Id { get; set; }
        public string SpaceKey { get; set; }
        public string TopicId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        [ForeignKey(nameof(Post.QueryCacheId))]
        public ICollection<Post> Posts { get; set; }
    }
}
