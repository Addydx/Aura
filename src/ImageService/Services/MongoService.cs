

using MongoDB.Driver;

namespace ImageService.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<Models.Image> _images;
        public MongoService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase("Aura");
            _images = database.GetCollection<Models.Image>("Images");
        }

        public async Task CreateImageAsync(Models.Image image) =>
            await _images.InsertOneAsync(image);

        public async Task<Models.Image?> GetImageByIdAsync(string id) =>
            await _images.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task DeleteImageAsync(string id) =>
            await _images.DeleteOneAsync(x => x.Id == id);
    }
}