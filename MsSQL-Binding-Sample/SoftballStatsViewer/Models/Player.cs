using System.ComponentModel.DataAnnotations;

namespace SoftballStatsViewer.Models
{
    public class Player
    {
        public int ID { get; set; }
        public string PlayerName { get; set; }
        public string Position { get; set; }
        public int TeamId { get; set; }

        public virtual Team Team { get; set; }
    }
}