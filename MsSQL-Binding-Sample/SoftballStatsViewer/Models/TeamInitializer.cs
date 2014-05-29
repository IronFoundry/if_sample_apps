using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftballStatsViewer.Models
{
    public class TeamInitializer : System.Data.Entity.CreateDatabaseIfNotExists<TeamContext>
    {
        protected override void Seed(TeamContext context)
        {
            var team = new Team {SchoolName = "DePauw", Nickname = "Tigers", ID = 5};
            var players = new List<Player>
            {
                new Player{ ID = 5, PlayerName = "Linsey", TeamId = 5},
                new Player{ ID = 6, PlayerName = "Jane", TeamId = 5}
            };

            players.ForEach(p => context.Players.Add(p));
            //context.SaveChanges();

            var teams = new List<Team>
            {
                team
            };

            teams.ForEach(t => context.Teams.Add(t));
            context.SaveChanges();
        }
    }
}
