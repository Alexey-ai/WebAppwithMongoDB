using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace WebAppMongo.Models
{
    public class Book
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [Display(Name = "Название")]
        public string Name { get; set; }
        [Display(Name = "Автор")]
        public string Author { get; set; }

        [Display(Name = "Цена")]
        public int Price { get; set; }

        public string ImageId { get; set; }

        public bool HasImage()
        {
            return !String.IsNullOrWhiteSpace(ImageId);
        }
    }
}
