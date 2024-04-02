using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace BusinessObject.Models
{
    public class User : IdentityUser
    {
        [StringLength(200)]
        [DefaultValue("Chưa có bio")]
        public string? bio { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? updatedDate { get; set; }

    }
}
