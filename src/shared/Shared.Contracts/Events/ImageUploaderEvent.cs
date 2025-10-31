
namespace Shared.Contracts.Events
{
    public class ImageUploaderEvent
    {
        //image id
        public string ImageId { get; set; } = string.Empty;
        //Owner id
        public string OwnerId { get; set; } = string.Empty;
        //url
        public string Url { get; set; } = string.Empty;
    }
}
/*
codigo de deepseek
namespace Shared.Contracts.Events
{
    public class ImageUploadedEvent
    {
        public string ImageId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
*/