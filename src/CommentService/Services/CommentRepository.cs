using MongoDB.Driver;
using CommentService.Models;

namespace CommentService.Services
{
    public class CommentRepository
    {
        private readonly IMongoCollection<Comment> _comments;

        public CommentRepository(IConfiguration config)
        {
            var client = new MongoClient(config["ConnectionStrings:MongoDB"]);
            var database = client.GetDatabase(config["DatabaseName"] ?? "AuraDB");
            _comments = database.GetCollection<Comment>("Comments");
        }

        public async Task<List<Comment>> GetCommentsByImageIdAsync(string imageId)
        {
            return await _comments.Find(x => x.ImageId == imageId).ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(string id)
        {
            return await _comments.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            await _comments.InsertOneAsync(comment);
            return comment;
        }

        public async Task DeleteAsync(string id)
        {
            await _comments.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<List<Comment>> GetAllAsync()
        {
            return await _comments.Find(_ => true).ToListAsync();
        }
    }
}