
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ImageService.Models
{
    public class Image
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}