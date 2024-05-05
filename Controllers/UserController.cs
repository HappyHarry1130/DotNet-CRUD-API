using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace MyWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public UserController()
        {
            string connectionString = "mongodb://localhost:27017";
            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase database = client.GetDatabase("mydatabase");
            _collection = database.GetCollection<BsonDocument>("add");
        }

        [HttpPost]
        public IActionResult InsertUser()
        {
            // Create a document to be inserted
            var document = new BsonDocument
            {
                { "name", "John Doe" },
                { "age", 30 },
                { "city", "New York" }
            };

            // Insert the document into the collection
            InsertDocument(document);

            return Ok("Document inserted successfully.");
        }

        private void InsertDocument(BsonDocument document)
        {
            _collection.InsertOne(document);
            Console.WriteLine("Document inserted successfully.");
        }
    }
}
