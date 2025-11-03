using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CommentService.Models
{
    public class Comment
    {
        [BsonId]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string ImageId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}