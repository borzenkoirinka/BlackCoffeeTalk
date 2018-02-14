using System;
using System.ComponentModel.DataAnnotations;

namespace BlackCoffeeTalk.Service.Models
{
    public class City
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(255)]
        public string Name { get; set; }
        [Required, MaxLength(12)]
        public string Zip { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreationTime { get; set; }
}
}
