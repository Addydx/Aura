
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