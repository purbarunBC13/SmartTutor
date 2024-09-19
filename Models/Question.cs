using Microsoft.CodeAnalysis.Options;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTutor.Models
{
    public class Question
    {
        [BsonId] // Indicates that this is the primary key in the MongoDB collection
        [BsonElement("QuestionId")]
        public string QuestionId { get; set; }

        [ForeignKey("Exam")]
        public string ExamId { get; set; } // Foreign key to Exam

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public string[] Options { get; set; } = new string[4];

        [Range(0, 3, ErrorMessage = "Correct option must be between 0 and 3")]
        public int CorrectOptionId { get; set; }

        [NotMapped]
        public int? SelectedOption { get; set; }
    }

}
