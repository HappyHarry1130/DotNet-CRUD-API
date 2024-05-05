using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text.Json;

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

        [HttpGet]
        public IActionResult GetUsers()
        {
            // Perform a query to get all documents from the collection
            var documents = GetDocuments();

            // Convert BsonDocuments to a list of dictionaries for easier serialization
            var users = new List<Dictionary<string, object>>();
            foreach (var document in documents)
            {
                var user = new Dictionary<string, object>();
                foreach (var element in document.Elements)
                {
                    user.Add(element.Name, BsonTypeMapper.MapToDotNetValue(element.Value));
                }
                users.Add(user);
            }

            return Ok(users);
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
            string jsonString = document.ToJson();

            return Ok(jsonString);

            
        }

        private void InsertDocument(BsonDocument document)
        {
            _collection.InsertOne(document);
            Console.WriteLine("Document inserted successfully.");
        }


        private IEnumerable<BsonDocument> GetDocuments()
        {
            // Perform a query to get all documents from the collection
            return _collection.Find(new BsonDocument()).ToList();
        }
    }
}
