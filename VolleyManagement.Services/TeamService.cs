﻿namespace VolleyManagement.Services
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using VolleyManagement.Contracts;
    using VolleyManagement.Contracts.Exceptions;
    using VolleyManagement.Data.Contracts;
    using VolleyManagement.Data.Exceptions;
    using VolleyManagement.Data.Queries.Common;
    using VolleyManagement.Data.Queries.Player;
    using VolleyManagement.Data.Queries.Team;
    using VolleyManagement.Domain.PlayersAggregate;
    using VolleyManagement.Domain.TeamsAggregate;

    /// <summary>
    /// Defines TeamService
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;

        private readonly IPlayerRepository _playerRepository;

        private readonly IQuery<Team, FindByIdCriteria> _getTeamByIdQuery;

        private readonly IQuery<Player, FindByIdCriteria> _getPlayerByIdQuery;

        private readonly IQuery<Team, FindByCaptainIdCriteria> _getTeamByCaptainQuery;

        private readonly IQuery<List<Team>, GetAllCriteria> _getAllTeamsQuery;

        private readonly IQuery<List<Player>, TeamPlayersCriteria> _getTeamRosterQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamService"/> class.
        /// </summary>
        /// <param name="teamRepository">The team repository</param>
        /// <param name="playerRepository">The player repository</param>
        /// <param name="getTeamByIdQuery"> Get By ID query for Teams</param>
        /// <param name="getPlayerByIdQuery"> Get By ID query for Players</param>
        /// <param name="getTeamByCaptainQuery"> Get By Captain ID query for Teams</param>
        /// <param name="getAllTeamsQuery"> Get All teams query</param>
        /// <param name="getTeamRosterQuery"> Get players for team query</param>
        public TeamService(
            ITeamRepository teamRepository,
            IPlayerRepository playerRepository,
            IQuery<Team, FindByIdCriteria> getTeamByIdQuery,
            IQuery<Player, FindByIdCriteria> getPlayerByIdQuery,
            IQuery<Team, FindByCaptainIdCriteria> getTeamByCaptainQuery,
            IQuery<List<Team>, GetAllCriteria> getAllTeamsQuery,
            IQuery<List<Player>, TeamPlayersCriteria> getTeamRosterQuery)
        {
            _teamRepository = teamRepository;
            _playerRepository = playerRepository;
            _getTeamByIdQuery = getTeamByIdQuery;
            _getPlayerByIdQuery = getPlayerByIdQuery;
            _getTeamByCaptainQuery = getTeamByCaptainQuery;
            this._getAllTeamsQuery = getAllTeamsQuery;
            this._getTeamRosterQuery = getTeamRosterQuery;
        }

        /// <summary>
        /// Method to get all teams.
        /// </summary>
        /// <returns>All teams.</returns>
        public List<Team> Get()
        {
            return _getAllTeamsQuery.Execute(new GetAllCriteria());
        }

        /// <summary>
        /// Create a new team.
        /// </summary>
        /// <param name="teamToCreate">A Team to create.</param>
        public void Create(Team teamToCreate)
        {
            Player captain = this.GetPlayerById(teamToCreate.CaptainId);
            if (captain == null)
            {
                // ToDo: Revisit this case
                throw new MissingEntityException(ServiceResources.ExceptionMessages.PlayerNotFound, teamToCreate.CaptainId);
            }

            // Check if captain in teamToCreate is captain of another team
            if (captain.TeamId != null)
            {
                var existTeam = GetPlayerLedTeam(captain.Id);
                VerifyExistingTeamOrThrow(existTeam);
            }

            _teamRepository.Add(teamToCreate);

            captain.TeamId = teamToCreate.Id;
            _playerRepository.Update(captain);
            _playerRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Finds a Team by id.
        /// </summary>
        /// <param name="id">id for search.</param>
        /// <returns>founded Team.</returns>
        public Team Get(int id)
        {
            return _getTeamByIdQuery.Execute(new FindByIdCriteria { Id = id });
        }

        /// <summary>
        /// Delete team by id.
        /// </summary>
        /// <param name="teamId">The id of team to delete.</param>
        public void Delete(int teamId)
        {
            try
            {
                _teamRepository.Remove(teamId);
            }
            catch (InvalidKeyValueException ex)
            {
                throw new MissingEntityException(ServiceResources.ExceptionMessages.TeamNotFound, teamId, ex);
            }

            IEnumerable<Player> roster = GetTeamRoster(teamId);
            foreach (var player in roster)
            {
                player.TeamId = null;
                _playerRepository.Update(player);
            }

            _teamRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Find captain of specified team
        /// </summary>
        /// <param name="team">Team which captain should be found</param>
        /// <returns>Team's captain</returns>
        public Player GetTeamCaptain(Team team)
        {
            return this.GetPlayerById(team.CaptainId);
        }

        /// <summary>
        /// Find players of specified team
        /// </summary>
        /// <param name="teamId">Id of team which players should be found</param>
        /// <returns>Collection of team's players</returns>
        public List<Player> GetTeamRoster(int teamId)
        {
            var criteria = new TeamPlayersCriteria { TeamId = teamId };
            return _getTeamRosterQuery.Execute(criteria);
        }

        /// <summary>
        /// Sets team to player
        /// </summary>
        /// <param name="playerId">Id of player to set the team</param>
        /// <param name="teamId">Id of team which should be set to player</param>
        public void UpdatePlayerTeam(int playerId, int teamId)
        {
            Player player = this.GetPlayerById(playerId);
            if (player == null)
            {
                throw new MissingEntityException(ServiceResources.ExceptionMessages.PlayerNotFound, playerId);
            }

            // Check if player is captain of another team
            if (player.TeamId != null)
            {
                var existingTeam = GetPlayerLedTeam(player.Id);
                if (existingTeam != null && teamId != existingTeam.Id)
                {
                    var ex = new ValidationException(ServiceResources.ExceptionMessages.PlayerIsCaptainOfAnotherTeam);
                    ex.Data[Domain.Constants.ExceptionManagement.ENTITY_ID_KEY] = existingTeam.Id;
                    throw ex;
                }
            }

            Team team = _getTeamByIdQuery.Execute(new FindByIdCriteria { Id = teamId });
            if (team == null)
            {
                throw new MissingEntityException(ServiceResources.ExceptionMessages.TeamNotFound, teamId);
            }

            player.TeamId = teamId;
            _playerRepository.Update(player);
            _playerRepository.UnitOfWork.Commit();
        }

        private static void VerifyExistingTeamOrThrow(Team existTeam)
        {
            if (existTeam != null)
            {
                var ex = new ValidationException(ServiceResources.ExceptionMessages.PlayerIsCaptainOfAnotherTeam);
                ex.Data[Domain.Constants.ExceptionManagement.ENTITY_ID_KEY] = existTeam.Id;
                throw ex;
            }
        }

        private Team GetPlayerLedTeam(int playerId)
        {
            var criteria = new FindByCaptainIdCriteria { Id = playerId };
            return _getTeamByCaptainQuery.Execute(criteria);
        }

        private Player GetPlayerById(int id)
        {
            var criteria = new FindByIdCriteria { Id = id };
            return _getPlayerByIdQuery.Execute(criteria);
        }
    }
}
