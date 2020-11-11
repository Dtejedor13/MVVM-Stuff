namespace MapCreatorV2.Models
{
    public class Export 
    {
        public int ID { get; set; }
        public string ImageSource { get; set; }
        public bool IsPassable { get; set; }
        public int GoToMapID { get; set; }
        public Export(int id, string sorces, bool ispassable, int MapID)
        {
            this.ID = id;
            this.ImageSource = sorces;
            this.IsPassable = ispassable;
            this.GoToMapID = MapID;
        }
    }
}
