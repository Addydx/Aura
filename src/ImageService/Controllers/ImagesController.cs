using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.DTOs;
using Shared.Contracts.Events;
using ImageService.Services;

[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly CloudinaryService _cloudinary;
    private readonly RabbitMQService _rabbit;

    public ImageController(CloudinaryService cloudinary, RabbitMQService rabbit)
    {
        _cloudinary = cloudinary;
        _rabbit = rabbit;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {
        var url = await _cloudinary.UploadImageAsync(file);

        var image = new ImageDto { Id = Guid.NewGuid().ToString(), Url = url };
        _rabbit.Publish(new ImageUploadedEvent { ImageId = image.Id, Url = image.Url });

        return Ok(image);
    }

    [HttpGet("{id}")]
    public IActionResult GetImage(string id)
    {
        // Aquí deberías recuperar la imagen de la base de datos o almacenamiento.
        return Ok(new { Id = id, Url = "https://example.com/image.png" });
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteImage(string id)
    {
        // Elimina la imagen.
        return NoContent();
    }
}
