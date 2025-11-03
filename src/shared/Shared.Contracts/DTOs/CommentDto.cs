namespace Shared.Contracts.DTOs
{
    public class CommentDto
    {
        public string Id { get; set; } = string.Empty;
        public string ImageId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}