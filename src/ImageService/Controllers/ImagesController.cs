using Microsoft.AspNetCore.Mvc;
using ImageService.Models;
using ImageService.Services;

namespace ImageService.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly CloudinaryService _cloudinary;
        private readonly MongoService _mongo;
        private readonly RabbitMQService _rabbit;

        public ImagesController(CloudinaryService cloudinary, MongoService mongo, RabbitMQService rabbit)
        {
            _cloudinary = cloudinary;
            _mongo = mongo;
            _rabbit = rabbit;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] FileUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("No file uploaded.");

            // Validar tipo de archivo
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(request.File.ContentType.ToLower()))
                return BadRequest("Invalid file type. Only JPEG, PNG, and GIF are allowed.");

            try
            {
                using var stream = request.File.OpenReadStream();
                var url = await _cloudinary.UploadImageAsync(stream, request.File.FileName);

                var image = new Image
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = url,
                    UserId = "test-user", // TODO: Get from authentication
                    CreatedAt = DateTime.UtcNow
                };

                await _mongo.CreateImageAsync(image);
                await _rabbit.PublishAsync(new { ImageId = image.Id, Url = image.Url });

                return Ok(image);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading image: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetImage(string id)
        {
            var image = await _mongo.GetImageByIdAsync(id);
            if (image == null)
                return NotFound();

            return Ok(image);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Image ID is required.");

            try
            {
                // Verificar si la imagen existe antes de eliminar
                var existingImage = await _mongo.GetImageByIdAsync(id);
                if (existingImage == null)
                    return NotFound("Image not found.");

                await _mongo.DeleteImageAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting image: {ex.Message}");
            }
        }
    }
}
