using System.ComponentModel.DataAnnotations;

namespace BlackCoffeeTalk.Service.Models
{
    public class Coffee
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
