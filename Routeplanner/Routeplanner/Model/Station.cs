using SQLite;

namespace Routeplanner.Model
{
    public class Station
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
