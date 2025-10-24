namespace gutv_booker.Models
{
    public class EqModelWithItemsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EquipmentModel.EquipmentCategory Category { get; set; }
        public EquipmentModel.EquipmentAccess Access { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();
        public List<EqItemResponseDto> Items { get; set; } = new();
    }
}