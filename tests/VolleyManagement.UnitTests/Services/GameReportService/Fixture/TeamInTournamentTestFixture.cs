using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VolleyManagement.Domain.TeamsAggregate;

namespace VolleyManagement.UnitTests.Services.GameReportService
{
    [ExcludeFromCodeCoverage]
    public class TeamInTournamentTestFixture
    {
        private readonly List<TeamTournamentDto> _teams = new List<TeamTournamentDto>();

        public TeamInTournamentTestFixture WithTeamsInSingleDivisionSingleGroup()
        {
            AddTeam(1, "A");
            AddTeam(2, "B");
            AddTeam(3, "C");
            return this;
        }

        public TeamInTournamentTestFixture WithTeamsInTwoDivisionTwoGroups()
        {
            // Division 1
            // Group 1
            AddTeam(1, "A", 1, divisionName: "DivisionNameA", groupId: 1);
            AddTeam(2, "B", 1, divisionName: "DivisionNameA", groupId: 1);
            AddTeam(3, "C", 1, divisionName: "DivisionNameA", groupId: 1);

            // Group 2
            AddTeam(4, "D", 1, divisionName: "DivisionNameA", groupId: 2);
            AddTeam(5, "E", 1, divisionName: "DivisionNameA", groupId: 2);
            AddTeam(6, "F", 1, divisionName: "DivisionNameA", groupId: 2);

            // Division 2
            // Group 1
            AddTeam(7, "G", 2, divisionName: "DivisionNameB", groupId: 3);
            AddTeam(8, "H", 2, divisionName: "DivisionNameB", groupId: 3);
            AddTeam(9, "I", 2, divisionName: "DivisionNameB", groupId: 3);

            // Group 2
            AddTeam(10, "J", 2, divisionName: "DivisionNameB", groupId: 4);
            AddTeam(11, "K", 2, divisionName: "DivisionNameB", groupId: 4);
            AddTeam(12, "L", 2, divisionName: "DivisionNameB", groupId: 4);

            return this;
        }

        public TeamInTournamentTestFixture WithUnorderedTeams()
        {
            AddTeam(4, "D", 1, divisionName: "DivisionNameA", groupId: 1, groupName: "GroupNameC");
            AddTeam(5, "E", 1, divisionName: "DivisionNameA", groupId: 2, groupName: "GroupNameA");
            AddTeam(6, "F", 1, divisionName: "DivisionNameA", groupId: 3, groupName: "GroupNameD");

            AddTeam(11, "J", 3, divisionName: "DivisionNameC", groupId: 7, groupName: "GroupNameC");
            AddTeam(12, "K", 3, divisionName: "DivisionNameC", groupId: 8, groupName: "GroupNameB");
            AddTeam(13, "L", 3, divisionName: "DivisionNameC", groupId: 9, groupName: "GroupNameA");

            AddTeam(7, "G", 2, divisionName: "DivisionNameB", groupId: 4, groupName: "GroupNameA");
            AddTeam(8, "H", 2, divisionName: "DivisionNameB", groupId: 5, groupName: "GroupNameC");
            AddTeam(9, "I", 2, divisionName: "DivisionNameB", groupId: 6, groupName: "GroupNameB");
            AddTeam(10, "Z", 2, divisionName: "DivisionNameB", groupId: 5, groupName: "GroupNameC");

            return this;
        }

        public List<TeamTournamentDto> Build()
        {
            return _teams;
        }

        private void AddTeam(int teamId, string teamName, int divisionId = 1, int groupId = 1,
            string divisionName = "DivisionNameA", string groupName = null)
        {
            _teams.Add(new TeamTournamentDto {
                TeamId = teamId,
                TeamName = $"TeamName{teamName}",
                DivisionId = divisionId,
                GroupId = groupId,
                DivisionName = divisionName,
                GroupName = groupName
            });
        }

        public TeamInTournamentTestFixture With8TeamsPlayoff()
        {
            AddTeam(1, "A");
            AddTeam(2, "B");
            AddTeam(3, "C");
            AddTeam(4, "D");
            AddTeam(5, "E");
            AddTeam(6, "F");
            AddTeam(7, "G");
            AddTeam(8, "H");
            return this;
        }
    }
}