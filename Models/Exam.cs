using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SmartTutor.Models
{
    public class Exam
    {
        [BsonId] // Indicates that this is the primary key in the MongoDB collection
        [BsonElement("ExamId")] // Maps the property to MongoDB's _id field
        public string ExamId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public TimeOnly Duration { get; set; }
    }
}

