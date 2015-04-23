﻿namespace VolleyManagement.Services
{
    using System;
    using System.Linq;
    using VolleyManagement.Contracts;
    using VolleyManagement.Contracts.Exceptions;
    using VolleyManagement.Dal.Contracts;
    using VolleyManagement.Dal.Exceptions;
    using VolleyManagement.Domain.Players;
    using VolleyManagement.Domain.Teams;

    using DAL = VolleyManagement.Dal.Contracts;

    /// <summary>
    /// Defines PlayerService
    /// </summary>
    public class PlayerService : IPlayerService
    {
        /// <summary>
        /// Holds PlayerRepository instance.
        /// </summary>
        private readonly IPlayerRepository _playerRepository;

        private readonly ITeamRepository _teamRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerService"/> class.
        /// </summary>
        /// <param name="playerRepository">The player repository</param>
        /// <param name="teamRepository">The team repository</param>
        public PlayerService(IPlayerRepository playerRepository, ITeamRepository teamRepository)
        {
            _playerRepository = playerRepository;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// Method to get all players.
        /// </summary>
        /// <returns>All players.</returns>
        public IQueryable<Player> Get()
        {
            return _playerRepository.Find();
        }

        /// <summary>
        /// Create a new player.
        /// </summary>
        /// <param name="playerToCreate">A Player to create.</param>
        public void Create(Player playerToCreate)
        {
            _playerRepository.Add(playerToCreate);
            _playerRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Finds a Player by id.
        /// </summary>
        /// <param name="id">id for search.</param>
        /// <returns>A found Player.</returns>
        public Player Get(int id)
        {
            Player player;
            try
            {
                player = _playerRepository.FindWhere(t => t.Id == id).Single();
            }
            catch (InvalidOperationException ex)
            {
                throw new MissingEntityException("Player with specified Id can not be found", ex);
            }

            return player;
        }

        /// <summary>
        /// Edit player.
        /// </summary>
        /// <param name="playerToEdit">Player to edit.</param>
        public void Edit(Player playerToEdit)
        {
            Team playerTeam = GetPlayerLeadedTeam(playerToEdit.Id);

            // Check if player is captain of team and teamId is null or changed
            if (playerTeam != null &&
                (playerToEdit.TeamId == null || playerTeam.Id != playerToEdit.TeamId))
            {
                string message = string.Format("Player is captain of the team {0}", playerTeam.Name);
                throw new InvalidOperationException(message);
            }
            else if (playerToEdit.TeamId != null)
            {
                // Check if new team id isn't exist
                try
                {
                    playerTeam = GetTeamWhere(t => t.Id == playerToEdit.TeamId);
                }
                catch (InvalidOperationException ex)
                {
                    throw new MissingEntityException("Team with specified Id can not be found", ex);
                }
            }

            try
            {
                _playerRepository.Update(playerToEdit);
            }
            catch (InvalidKeyValueException ex)
            {
                throw new MissingEntityException("Player with specified Id can not be found", ex);
            }

            _playerRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Delete player by id.
        /// </summary>
        /// <param name="id">The id of player to delete.</param>
        public void Delete(int id)
        {
            Team playerTeam = GetPlayerLeadedTeam(id);
            if (playerTeam != null)
            {
                string message = string.Format("Player is captain of the team {0}", playerTeam.Name);
                throw new InvalidOperationException(message);
            }

            try
            {
                _playerRepository.Remove(id);
                _playerRepository.UnitOfWork.Commit();
            }
            catch (InvalidKeyValueException ex)
            {
                var serviceException = new MissingEntityException("Player with specified Id can not be found", ex);
                throw serviceException;
            }
        }

        /// <summary>
        /// Find team of specified player
        /// </summary>
        /// <param name="player">Player which team should be found</param>
        /// <returns>Player's team</returns>
        public Team GetPlayerTeam(Player player)
        {
            if (player.TeamId == null)
            {
                return null;
            }

            try
            {
                return GetTeamWhere(t => t.Id == player.TeamId);
            }
            catch (InvalidOperationException ex)
            {
                throw new MissingEntityException("Team with specified Id can not be found", ex);
            }
        }

        /// <summary>
        /// Update player team
        /// </summary>
        /// <param name="playerId">Player which team should update</param>
        /// <param name="teamId">Team which should be set to player</param>
        public void UpdatePlayerTeam(Player player, Team team)
        {
            if (team != null)
            {
                // Check if new team isn't exist
                try
                {
                    GetTeamWhere(t => t.Id == team.Id);
                }
                catch (InvalidOperationException ex)
                {
                    throw new MissingEntityException("Team with specified Id can not be found", team.Id, ex); ;
                };
            }

            // Check case if player was a captain and team changed

            // Get player
            Player playerToUpdate;
            try
            {
                playerToUpdate = Get(player.Id);
            }
            catch (InvalidOperationException ex)
            {
                throw new MissingEntityException("Player with specified Id can not be found", player.Id, ex);
            };

            // Update team
            if (team == null)
            {
                playerToUpdate.TeamId = null;
            }
            else
            {
                playerToUpdate.TeamId = team.Id;
            }

            _playerRepository.Update(playerToUpdate);
        }

        private Team GetPlayerLeadedTeam(int playerId)
        {
            Team team;
            try
            {
                team = GetTeamWhere(t => t.CaptainId == playerId);
            }
            catch (InvalidOperationException)
            {
                team = null;
            };

            return team;
        }

        private Team GetTeamWhere(System.Linq.Expressions.Expression<Func<Team, bool>> predicate)
        {
            return _teamRepository.FindWhere(predicate).Single();
        }
    }
}
