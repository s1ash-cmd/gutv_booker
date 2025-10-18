using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace gutv_booker.Models
{
    public class EquipmentModel
    {
        public enum EquipmentCategory
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

        public enum EquipmentAccess
        {
            User,
            Osnova,
            Ronin
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EquipmentCategory Category { get; set; }
        public EquipmentAccess Access { get; set; } = EquipmentAccess.User;

        [JsonIgnore]
        public string AttributesJson { get; set; } = "{}";


        [NotMapped]
        public Dictionary<string, object> Attributes
        {
            get => string.IsNullOrEmpty(AttributesJson)
                ? new Dictionary<string, object>()
                : JsonSerializer.Deserialize<Dictionary<string, object>>(AttributesJson)!;
            set => AttributesJson = JsonSerializer.Serialize(value);
        }

        public List<EquipmentItem> EquipmentItems { get; set; } = new();
    }
}