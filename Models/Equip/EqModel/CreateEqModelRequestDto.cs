namespace gutv_booker.Models
{
    public class CreateEqModelRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EquipmentModel.EquipmentCategory Category { get; set; }
        public bool Osnova {get; set;}
        public Dictionary<string, object>? Attributes { get; set; } = new();
    }
}