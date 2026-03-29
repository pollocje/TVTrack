namespace TVTrack.Models
{
    public class CustomListItem
    {
        public int Id { get; set; }
        public int CustomListId { get; set; }
        public int ShowId { get; set; }

        public CustomList CustomList { get; set; } = null!;
        public TVShow Show { get; set; } = null!;
    }
}
