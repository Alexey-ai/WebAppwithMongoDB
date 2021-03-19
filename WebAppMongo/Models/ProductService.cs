using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace WebAppMongo.Models
{
    public class ProductService
    {
        IGridFSBucket gridFS;   // файловое хранилище
        IMongoCollection<Book> Books; // коллекция в базе данных
        public ProductService()
        {
            // строка подключения
            var client = new MongoClient("mongodb+srv://mongotest_user:1234567890@cluster0.schkg.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
            var database = client.GetDatabase("test");
            gridFS = new GridFSBucket(database);
            // обращаемся к коллекции Products
            Books = database.GetCollection<Book>("Books");
        }
        // получаем все документы, используя критерии фальтрации
        public async Task<IEnumerable<Book>> GetProducts(int? minPrice, int? maxPrice, string name)
        {
            // строитель фильтров
            var builder = new FilterDefinitionBuilder<Book>();
            var filter = builder.Empty; // фильтр для выборки всех документов
            // фильтр по имени
            if (!String.IsNullOrWhiteSpace(name))
            {
                filter = filter & builder.Regex("Name", new BsonRegularExpression(name));
            }
            if (minPrice.HasValue)  // фильтр по минимальной цене
            {
                filter = filter & builder.Gte("Price", minPrice.Value);
            }
            if (maxPrice.HasValue)  // фильтр по максимальной цене
           {
                filter = filter & builder.Lte("Price", maxPrice.Value);
            }
                return await Books.Find(filter).ToListAsync();

        }

        // получаем один документ по id
        public async Task<Book> GetProduct(string id)
        {
            return await Books.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }
        // добавление документа
        public async Task Create(Book p)
        {
            await Books.InsertOneAsync(p);
        }
        // обновление документа
        public async Task Update(Book p)
        {
            await Books.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(p.Id)), p);
        }
        // удаление документа
        public async Task Remove(string id)
        {
            await Books.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
        }
        // получение изображения
        public async Task<byte[]> GetImage(string id)
        {
            return await gridFS.DownloadAsBytesAsync(new ObjectId(id));
        }
        // сохранение изображения
        public async Task StoreImage(string id, Stream imageStream, string imageName)
        {
            Book p = await GetProduct(id);
            if (p.HasImage())
            {
                // если ранее уже была прикреплена картинка, удаляем ее
                await gridFS.DeleteAsync(new ObjectId(p.ImageId));
            }
            // сохраняем изображение
            ObjectId imageId = await gridFS.UploadFromStreamAsync(imageName, imageStream);
            // обновляем данные по документу
            p.ImageId = imageId.ToString();
            var filter = Builders<Book>.Filter.Eq("_id", new ObjectId(p.Id));
            var update = Builders<Book>.Update.Set("ImageId", p.ImageId);
            await Books.UpdateOneAsync(filter, update);
        }
    }
}
