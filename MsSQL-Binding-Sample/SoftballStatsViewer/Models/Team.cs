using System.Collections.Generic;

namespace SoftballStatsViewer.Models
{
    public class Team
    {
        public Team()
        {
            Players = new List<Player>();
        }

        public int ID { get; set; }
        public string SchoolName { get; set; }
        public string Nickname { get; set; }

        public virtual ICollection<Player> Players { get; set; }
    }
}