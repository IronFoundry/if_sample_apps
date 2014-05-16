using System.ComponentModel.DataAnnotations;

namespace SoftballStatsViewer.Models
{
    public class Player
    {
        protected bool Equals(Player other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Player) obj);
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public int ID { get; set; }
        public string PlayerName { get; set; }
        public string Position { get; set; }
        public int TeamId { get; set; }

        public virtual Team Team { get; set; }
    }
}