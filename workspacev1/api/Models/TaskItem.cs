using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace api.Models
{
    public class TaskItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("shortTitle")]
        public string ShortTitle { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("files")]
        public List<string> Files { get; set; } = new List<string>();

        [BsonElement("dueDate")]
        public DateTime DueDate { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } // ToDo, InProgress, Done

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [BsonElement("criticality")]
        public string Criticality { get; set; } // Low, Medium, High


    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("assignedUsers")]
    public List<string> AssignedUsers { get; set; } = new List<string>();

        [BsonElement("tenantId")]
        public string TenantId { get; set; }

        [BsonElement("publicShareId")]
        public string PublicShareId { get; set; } // null if not shared

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdBy")]
    public string CreatedBy { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedBy")]
    public string UpdatedBy { get; set; }

    [BsonElement("completedBy")]
    public string CompletedBy { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }
    }
}
