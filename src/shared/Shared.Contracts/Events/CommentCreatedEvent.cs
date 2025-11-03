namespace Shared.Contracts.Events
{
    public class CommentCreatedEvent
    {
        public string CommentId { get; set; } = string.Empty;
        public string ImageId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}