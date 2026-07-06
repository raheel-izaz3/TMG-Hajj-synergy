namespace TMG.Models.FormModels
{
    public record AziziaRoomCountModel
    {
        public int RoomId { get; set; }
        public string RoomCode { get; set; }
        public string HotelName { get; set; }
        public float RoomCount { get; set; }
        public float RoomPax { get; set; }
        public int HotelId { get; set; }
        public float MaleRoomCount { get; set; }
        public float FemaleRoomCount { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

    }
}
