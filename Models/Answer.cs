using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Models
{
    public class Answer
    {
        [BsonId] // Indicates that this is the primary key in the MongoDB collection
        [BsonElement("AnswerId")]
        public string AnswerId { get; set; }

        [Required]
        [ForeignKey("user")]
        public string Id { get; set; }

        [ForeignKey("Exam")]
        public string ExamId { get; set; }

        [Required]
        public int Score { get; set; } = 0;
    }
}
