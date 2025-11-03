using Microsoft.AspNetCore.Mvc;
using CommentService.Models;
using CommentService.Services;
using Shared.Contracts.Events;

namespace CommentService.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly CommentRepository _commentRepository;
        private readonly RabbitMQPublisher _rabbitPublisher;
        
        public CommentController(CommentRepository commentRepository, RabbitMQPublisher rabbitPublisher)
        {
            _commentRepository = commentRepository;
            _rabbitPublisher = rabbitPublisher;
        }

        [HttpGet("image/{imageId}")]
        public async Task<IActionResult> GetByImageId(string imageId)
        {
            var comments = await _commentRepository.GetCommentsByImageIdAsync(imageId);
            return Ok(comments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                return NotFound();
            return Ok(comment);
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
        {
            if (string.IsNullOrEmpty(comment.Content))
                return BadRequest("Comment content is required.");

            try
            {
                var createdComment = await _commentRepository.CreateAsync(comment);
                
                // Publicar evento a RabbitMQ
                var commentEvent = new CommentCreatedEvent
                {
                    CommentId = createdComment.Id,
                    ImageId = createdComment.ImageId,
                    UserId = createdComment.UserId,
                    Content = createdComment.Content,
                    CreatedAt = createdComment.CreatedAt
                };

                await _rabbitPublisher.PublishCommentCreatedAsync(commentEvent);

                return CreatedAtAction(nameof(GetById), new { id = createdComment.Id }, createdComment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating comment: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Comment ID is required.");

            var existingComment = await _commentRepository.GetByIdAsync(id);
            if (existingComment == null)
                return NotFound("Comment not found.");

            await _commentRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}