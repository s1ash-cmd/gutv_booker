using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

public enum Category
{
    Camera,
    Lens,
    Card,
    Battery,
    Charger,
    Sound,
    Stand,
    Light,
    Other
}

namespace gutv_booker.Models
{
    public class EquipmentType
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Category Category { get; set; }

        public string AttributesJson { get; set; } = "{}";

        [NotMapped]
        public Dictionary<string, object> Attributes
        {
            get => string.IsNullOrEmpty(AttributesJson)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(AttributesJson)!;
            set => AttributesJson = JsonSerializer.Serialize(value);
        }
    }
}