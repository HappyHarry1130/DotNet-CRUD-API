using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Text.Json;
using MongoDB.Driver;

namespace MyWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public IActionResult Post([FromBody] UserData userData)
        {
            // Validate input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert UserData to BsonDocument
            BsonDocument userDocument = new BsonDocument
            {
                { "Name", userData.Name },
                { "Age", userData.Age },
                { "Gender", userData.Gender },
                { "Birthday", userData.Birthday }
            };
            InsertDocument(userDocument);
            string jsonString = userDocument.ToJson();
            // For demonstration purposes, let's just return the received data
            return Ok(jsonString);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            // Validate input
            if (!ObjectId.TryParse(id, out var objectId) || objectId == ObjectId.Empty)
            {
                return BadRequest("Invalid ObjectId.");
            }

            // Perform deletion based on ObjectId
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            var result = _collection.DeleteOne(filter);

            if (result.DeletedCount == 0)
            {
                return NotFound($"No user with ObjectId '{objectId}' found.");
            }

            return Ok($"User with ObjectId '{objectId}' deleted successfully.");
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

    // Define a class to represent the user data
    public class UserData
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public DateTime Birthday { get; set; }
    }
}
