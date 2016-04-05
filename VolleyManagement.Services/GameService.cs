﻿namespace VolleyManagement.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VolleyManagement.Contracts;
    using VolleyManagement.Contracts.Exceptions;
    using VolleyManagement.Data.Contracts;
    using VolleyManagement.Data.Exceptions;
    using VolleyManagement.Data.Queries.Common;
    using VolleyManagement.Data.Queries.GameResult;
    using VolleyManagement.Domain.GamesAggregate;
    using VolleyManagement.Domain.Properties;
    using GameResultConstants = VolleyManagement.Domain.Constants.GameResult;

    /// <summary>
    /// Defines an implementation of <see cref="IGameService"/> contract.
    /// </summary>
    public class GameService : IGameService
    {
        #region Fields

        private readonly IGameRepository _gameRepository;

        #endregion

        #region Query objects

        private readonly IQuery<GameResultDto, FindByIdCriteria> _getByIdQuery;
        private readonly IQuery<List<GameResultDto>, TournamentGameResultsCriteria> _tournamentGameResultsQuery;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/> class.
        /// </summary>
        /// <param name="gameRepository">Instance of class which implements <see cref="IGameRepository"/>.</param>
        /// <param name="getByIdQuery">Query which gets <see cref="GameResultDto"/> object by its identifier.</param>
        /// <param name="tournamentGameResultsQuery">Query which gets <see cref="GameResultDto"/> objects
        /// of the specified tournament.</param>
        public GameService(
            IGameRepository gameRepository,
            IQuery<GameResultDto, FindByIdCriteria> getByIdQuery,
            IQuery<List<GameResultDto>, TournamentGameResultsCriteria> tournamentGameResultsQuery)
        {
            _gameRepository = gameRepository;
            _getByIdQuery = getByIdQuery;
            _tournamentGameResultsQuery = tournamentGameResultsQuery;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Creates a new game.
        /// </summary>
        /// <param name="game">Game to create.</param>
        public void Create(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException("game");
            }

            ValidateGame(game);

            _gameRepository.Add(game);
            _gameRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Gets game result by its identifier.
        /// </summary>
        /// <param name="id">Identifier of game result.</param>
        /// <returns>Instance of <see cref="GameResultDto"/> or null if nothing is obtained.</returns>
        public GameResultDto Get(int id)
        {
            return _getByIdQuery.Execute(new FindByIdCriteria { Id = id });
        }

        /// <summary>
        /// Gets game results of the tournament specified by its identifier.
        /// </summary>
        /// <param name="tournamentId">Identifier of the tournament.</param>
        /// <returns>List of game results of specified tournament.</returns>
        public List<GameResultDto> GetTournamentResults(int tournamentId)
        {
            return _tournamentGameResultsQuery.Execute(new TournamentGameResultsCriteria { TournamentId = tournamentId });
        }

        /// <summary>
        /// Edits specified instance of game.
        /// </summary>
        /// <param name="game">Game to update.</param>
        public void Edit(Game game)
        {
            try
            {
                _gameRepository.Update(game);
            }
            catch (ConcurrencyException ex)
            {
                throw new MissingEntityException(ServiceResources.ExceptionMessages.GameNotFound, ex);
            }

            _gameRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Deletes game by its identifier.
        /// </summary>
        /// <param name="id">Identifier of game.</param>
        public void Delete(int id)
        {
            _gameRepository.Remove(id);
            _gameRepository.UnitOfWork.Commit();
        }

        #endregion

        #region Validation methods

        private void ValidateGame(Game game)
        {
            ValidateTeams(game.HomeTeamId, game.AwayTeamId);
            ValidateSetsScore(game.Result.SetsScore, game.Result.IsTechnicalDefeat);
            ValidateSetsScoreMatchesSetScores(game.Result.SetsScore, game.Result.SetScores);
            ValidateSetScoresValues(game.Result.SetScores, game.Result.IsTechnicalDefeat);
            ValidateSetScoresOrder(game.Result.SetScores);
        }

        private void ValidateTeams(int homeTeamId, int awayTeamId)
        {
            if (GameValidation.AreTheSameTeams(homeTeamId, awayTeamId))
            {
                throw new ArgumentException(Resources.GameResultSameTeam);
            }
        }

        private void ValidateSetsScore(Score setsScore, bool isTechnicalDefeat)
        {
            if (!ResultValidation.IsSetsScoreValid(setsScore, isTechnicalDefeat))
            {
                throw new ArgumentException(
                    string.Format(
                    Resources.GameResultSetsScoreInvalid,
                    GameResultConstants.TECHNICAL_DEFEAT_SETS_WINNER_SCORE,
                    GameResultConstants.TECHNICAL_DEFEAT_SETS_LOSER_SCORE));
            }
        }

        private void ValidateSetsScoreMatchesSetScores(Score setsScore, IList<Score> setScores)
        {
            if (!ResultValidation.AreSetScoresMatched(setsScore, setScores))
            {
                throw new ArgumentException(Resources.GameResultSetsScoreNoMatchSetScores);
            }
        }

        private void ValidateSetScoresValues(IList<Score> setScores, bool isTechnicalDefeat)
        {
            bool isPreviousOptionalSetUnplayed = false;

            for (int i = 0; i < setScores.Count; i++)
            {
                if (i < GameResultConstants.SETS_COUNT_TO_WIN)
                {
                    if (!ResultValidation.IsRequiredSetScoreValid(setScores[i], isTechnicalDefeat))
                    {
                        throw new ArgumentException(
                            string.Format(
                            Resources.GameResultRequiredSetScores,
                            GameResultConstants.SET_POINTS_MIN_VALUE_TO_WIN,
                            GameResultConstants.SET_POINTS_MIN_DELTA_TO_WIN,
                            GameResultConstants.TECHNICAL_DEFEAT_SET_WINNER_SCORE,
                            GameResultConstants.TECHNICAL_DEFEAT_SET_LOSER_SCORE));
                    }
                }
                else
                {
                    if (!ResultValidation.IsOptionalSetScoreValid(setScores[i], isTechnicalDefeat))
                    {
                        throw new ArgumentException(
                            string.Format(
                            Resources.GameResultOptionalSetScores,
                            GameResultConstants.SET_POINTS_MIN_VALUE_TO_WIN,
                            GameResultConstants.SET_POINTS_MIN_DELTA_TO_WIN,
                            GameResultConstants.UNPLAYED_SET_HOME_SCORE,
                            GameResultConstants.UNPLAYED_SET_AWAY_SCORE));
                    }

                    if (isPreviousOptionalSetUnplayed)
                    {
                        if (!ResultValidation.IsSetUnplayed(setScores[i]))
                        {
                            throw new ArgumentException(Resources.GameResultPreviousOptionalSetUnplayed);
                        }
                    }

                    isPreviousOptionalSetUnplayed = ResultValidation.IsSetUnplayed(setScores[i]);
                }
            }
        }

        private void ValidateSetScoresOrder(IList<Score> setScores)
        {
            if (!ResultValidation.AreSetScoresOrdered(setScores))
            {
                throw new ArgumentException(Resources.GameResultSetScoresNotOrdered);
            }
        }

        #endregion
    }
}
