﻿namespace VolleyManagement.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VolleyManagement.Contracts;
    using VolleyManagement.Contracts.Authorization;
    using VolleyManagement.Contracts.Exceptions;
    using VolleyManagement.Crosscutting.Contracts.Providers;
    using VolleyManagement.Data.Contracts;
    using VolleyManagement.Data.Exceptions;
    using VolleyManagement.Data.Queries.Common;
    using VolleyManagement.Data.Queries.GameResult;
    using VolleyManagement.Data.Queries.Tournament;
    using VolleyManagement.Domain.GamesAggregate;
    using VolleyManagement.Domain.Properties;
    using VolleyManagement.Domain.RolesAggregate;
    using VolleyManagement.Domain.TournamentsAggregate;
    using GameResultConstants = VolleyManagement.Domain.Constants.GameResult;

    /// <summary>
    /// Defines an implementation of <see cref="IGameService"/> contract.
    /// </summary>
    public class GameService : IGameService
    {
        #region Fields

        private readonly IGameRepository _gameRepository;
        private readonly IAuthorizationService _authService;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentService _tournamentService;

        #endregion

        #region Query objects

        private readonly IQuery<GameResultDto, FindByIdCriteria> _getByIdQuery;
        private readonly IQuery<List<GameResultDto>, TournamentGameResultsCriteria> _tournamentGameResultsQuery;
        private readonly IQuery<TournamentScheduleDto, TournamentScheduleInfoCriteria> _tournamentScheduleDtoByIdQuery;
        private readonly IQuery<List<Game>, TournamentRoundsGameResultsCriteria> _gamesByTournamentIdRoundsNumberQuery;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/> class.
        /// </summary>
        /// <param name="gameRepository">Instance of class which implements <see cref="IGameRepository"/>.</param>
        /// <param name="getByIdQuery">Query which gets <see cref="GameResultDto"/> object by its identifier.</param>
        /// <param name="tournamentGameResultsQuery">Query which gets <see cref="GameResultDto"/> objects
        /// of the specified tournament.</param>
        /// <param name="getTournamentByIdQuery">Query which gets <see cref="Tournament"/> object by its identifier.</param>
        /// <param name="gamesByTournamentIdRoundsNumberQuery">Query which gets <see cref="Game"/> object by its identifier.</param>
        /// <param name="authService">Authorization service</param>
        /// <param name="tournamentRepository">Tournament repository</param>
        /// <param name="tournamentService">Tournament service </param>
        public GameService(
            IGameRepository gameRepository,
            IQuery<GameResultDto, FindByIdCriteria> getByIdQuery,
            IQuery<List<GameResultDto>, TournamentGameResultsCriteria> tournamentGameResultsQuery,
            IQuery<TournamentScheduleDto, TournamentScheduleInfoCriteria> getTournamentByIdQuery,
            IQuery<List<Game>, TournamentRoundsGameResultsCriteria> gamesByTournamentIdRoundsNumberQuery,
            IAuthorizationService authService,
            ITournamentRepository tournamentRepository,
            ITournamentService tournamentService)
        {
            _gameRepository = gameRepository;
            _getByIdQuery = getByIdQuery;
            _tournamentGameResultsQuery = tournamentGameResultsQuery;
            _tournamentScheduleDtoByIdQuery = getTournamentByIdQuery;
            _gamesByTournamentIdRoundsNumberQuery = gamesByTournamentIdRoundsNumberQuery;
            _authService = authService;
            _tournamentRepository = tournamentRepository;
            _tournamentService = tournamentService;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Creates a new game.
        /// </summary>
        /// <param name="game">Game to create.</param>
        public void Create(Game game)
        {
            _authService.CheckAccess(AuthOperations.Games.Create);

            if (game == null)
            {
                throw new ArgumentNullException("game");
            }

            if (game.Result != null)
            {
                ValidateResult(game.Result);
            }

            ValidateGame(game);
            UpdateTournamentLastTimeUpdated(game);
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
            return _tournamentGameResultsQuery
                .Execute(
                new TournamentGameResultsCriteria { TournamentId = tournamentId });
        }

        /// <summary>
        /// Edits specified instance of game.
        /// </summary>
        /// <param name="game">Game to update.</param>
        public void Edit(Game game)
        {
            _authService.CheckAccess(AuthOperations.Games.Edit);

            ValidateGame(game);

            try
            {
                _gameRepository.Update(game);
            }
            catch (ConcurrencyException ex)
            {
                throw new MissingEntityException(ServiceResources.ExceptionMessages.GameNotFound, ex);
            }

            UpdateTournamentLastTimeUpdated(game);
            _gameRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Edits result of specified instance of game.
        /// </summary>
        /// <param name="game">Game which result have to be to update.</param>
        public void EditGameResult(Game game)
        {
            _authService.CheckAccess(AuthOperations.Games.EditResult);

            ValidateResult(game.Result);

            try
            {
                _gameRepository.Update(game);
            }
            catch (ConcurrencyException ex)
            {
                throw new MissingEntityException(ServiceResources.ExceptionMessages.GameNotFound, ex);
            }

            UpdateTournamentLastTimeUpdated(game);
            _gameRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Deletes game by its identifier.
        /// </summary>
        /// <param name="id">Identifier of game.</param>
        public void Delete(int id)
        {
            _authService.CheckAccess(AuthOperations.Games.Delete);

            GameResultDto game = Get(id);

            if (game == null)
            {
                throw new ArgumentNullException("game");
            }

            ValidateGameInRoundOnDelete(game);
            _gameRepository.Remove(id);
            _gameRepository.UnitOfWork.Commit();
        }

        /// <summary>
        /// Method swap all games between two rounds.
        /// </summary>
        /// <param name="tournamentId">Identifier of tournament.</param>
        /// <param name="firstRoundNumber">Identifier of first round number.</param>
        /// <param name="secondRoundNumber">Identifier of second round number.</param>
        public void SwapRounds(int tournamentId, byte firstRoundNumber, byte secondRoundNumber)
        {
            _authService.CheckAccess(AuthOperations.Games.SwapRounds);

            List<Game> games = _gamesByTournamentIdRoundsNumberQuery.Execute(
                new TournamentRoundsGameResultsCriteria
                {
                    TournamentId = tournamentId,
                    FirstRoundNumber = firstRoundNumber,
                    SecondRoundNumber = secondRoundNumber
                });

            try
            {
                foreach (var game in games)
                {
                    game.Round = game.Round == firstRoundNumber ? secondRoundNumber : firstRoundNumber;
                    _gameRepository.Update(game);
                }
            }
            catch (ConcurrencyException ex)
            {
                throw new MissingEntityException(ServiceResources.ExceptionMessages.GameNotFound, ex);
            }

            _gameRepository.UnitOfWork.Commit();
        }

        #endregion

        #region Validation methods

        private void ValidateGame(Game game)
        {
            ValidateTeams(game.HomeTeamId, game.AwayTeamId);
            ValidateGameInTournament(game);
            if (game.Result == null)
            {
                game.Result = new Result();
                return;
            }
        }

        private void ValidateResult(Result result)
        {
            ValidateSetsScore(result.SetsScore, result.IsTechnicalDefeat);
            ValidateSetsScoreMatchesSetScores(result.SetsScore, result.SetScores);
            ValidateSetScoresValues(result.SetScores, result.IsTechnicalDefeat);
            ValidateSetScoresOrder(result.SetScores);
        }

        private void ValidateTeams(int homeTeamId, int? awayTeamId)
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

            for (int i = 0, setOrderNumber = 1; i < setScores.Count; i++, setOrderNumber++)
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
                    if (!ResultValidation.IsOptionalSetScoreValid(setScores[i], isTechnicalDefeat, setOrderNumber))
                    {
                        if (setOrderNumber == GameResultConstants.MAX_SETS_COUNT)
                        {
                            throw new ArgumentException(
                            string.Format(
                            Resources.GameResultFifthSetScoreInvalid,
                            GameResultConstants.FIFTH_SET_POINTS_MIN_VALUE_TO_WIN,
                            GameResultConstants.SET_POINTS_MIN_DELTA_TO_WIN));
                        }

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

        private void ValidateGameInTournament(Game game)
        {
            TournamentScheduleDto tournamentDto = _tournamentScheduleDtoByIdQuery
                .Execute(new TournamentScheduleInfoCriteria { TournamentId = game.TournamentId });

            if (tournamentDto == null)
            {
                throw new ArgumentException(Resources.NoSuchToruanment);
            }

            List<GameResultDto> allGames = this.GetTournamentResults(tournamentDto.Id);
            GameResultDto oldGameToUpdate = allGames.Where(gr => gr.Id == game.Id).SingleOrDefault();

            if (oldGameToUpdate != null)
            {
                allGames.Remove(oldGameToUpdate);
            }

            ValidateGameDate(tournamentDto, game);
            ValidateGameInRound(game, allGames);
            if (tournamentDto.Scheme == TournamentSchemeEnum.One)
            {
                ValidateGamesInTournamentSchemeOne(game, allGames);
            }
            else if (tournamentDto.Scheme == TournamentSchemeEnum.Two)
            {
                ValidateGamesInTournamentSchemeTwo(game, allGames);
            }
        }

        private void ValidateGameInRound(Game newGame, List<GameResultDto> games)
        {
            List<GameResultDto> gamesInRound = games
               .Where(gr => gr.Round == newGame.Round)
               .ToList();
            ValidateGameInRoundOnCreate(newGame, gamesInRound);
        }

        private void ValidateGameInRoundOnCreate(Game newGame, List<GameResultDto> gamesInRound)
        {
            // We are sure that newGame is been created, not edited
            foreach (GameResultDto game in gamesInRound)
            {
                if (GameValidation.AreSameTeamsInGames(game, newGame))
                {
                    if (GameValidation.IsFreeDayGame(newGame))
                    {
                        throw new ArgumentException(
                            Resources
                            .SameFreeDayGameInRound);
                    }
                    else
                    {
                        throw new ArgumentException(
                           string.Format(
                           Resources.SameGameInRound,
                           game.HomeTeamName,
                           game.AwayTeamName,
                           game.Round.ToString()));
                    }
                }
                else if (GameValidation.IsTheSameTeamInTwoGames(game, newGame))
                {
                    if (GameValidation.IsFreeDayGame(game))
                    {
                        throw new ArgumentException(
                            Resources
                            .SameFreeDayGameInRound);
                    }
                    else
                    {
                        string opositeTeam = string.Empty;

                        if (game.HomeTeamId == newGame.HomeTeamId
                            || game.HomeTeamId == newGame.AwayTeamId)
                        {
                            opositeTeam = game.HomeTeamName;
                        }
                        else
                        {
                            opositeTeam = game.AwayTeamName;
                        }

                        throw new ArgumentException(
                          string.Format(
                          Resources.SameTeamInRound,
                                 opositeTeam));
                    }
                }
            }
        }

        private void ValidateGameInRoundOnDelete(GameResultDto gameToDelete)
        {
            if (gameToDelete.HomeSetsScore != 0 || gameToDelete.AwaySetsScore != 0
                || gameToDelete.GameDate < TimeProvider.Current.UtcNow)
            {
                throw new ArgumentException(Resources.WrongDeletingGame);
            }
        }

        private void ValidateGamesInTournamentSchemeTwo(Game newGame, List<GameResultDto> games)
        {
            var tournamentGames = games
                .Where(gr => gr.Round != newGame.Round)
                .ToList();

            var duplicates = tournamentGames
                    .Where(x => GameValidation.AreSameOrderTeamsInGames(x, newGame))
                    .ToList();

            if (GameValidation.IsFreeDayGame(newGame))
            {
                if (duplicates.Count == GameValidation.MAX_DUPLICATE_GAMES_IN_SCHEMA_TWO)
                {
                    throw new ArgumentException(
                        string.Format(
                        Resources.SameGameInTournamentSchemeTwo,
                        duplicates.First().HomeTeamName,
                        duplicates.First().AwayTeamName));
                }
            }
            else
            {
                if (duplicates.Count > 0)
                {
                    SwitchTeamsOrder(newGame);

                    int switchedDuplicatesCount = tournamentGames
                            .Where(x => GameValidation.AreSameOrderTeamsInGames(x, newGame))
                            .Count();

                    if (switchedDuplicatesCount > 0)
                    {
                        throw new ArgumentException(
                        string.Format(
                        Resources.SameGameInTournamentSchemeTwo,
                        duplicates.First().HomeTeamName,
                        duplicates.First().AwayTeamName));
                    }
                }
            }
        }

        private void ValidateGamesInTournamentSchemeOne(Game newGame, List<GameResultDto> games)
        {
            List<GameResultDto> tournamentGames = games
                .Where(gr => gr.Round != newGame.Round)
                .ToList();

            var duplicates = tournamentGames
                .Where(x => GameValidation.AreSameTeamsInGames(x, newGame))
                .ToList();

            if (duplicates.Count > 0)
            {
                string awayTeamName = string.Empty;
                if (GameValidation.IsFreeDayGame(newGame))
                {
                    awayTeamName = GameResultConstants.FREE_DAY_TEAM_NAME;
                }
                else
                {
                    awayTeamName = duplicates.First().AwayTeamName;
                }

                throw new ArgumentException(
                    string.Format(
                    Resources.SameGameInTournamentSchemeOne,
                    duplicates.First().HomeTeamName,
                    awayTeamName));
            }
        }

        private void ValidateGameDate(TournamentScheduleDto tournament, Game game)
        {
            if (DateTime.Compare(tournament.StartDate, game.GameDate) > 0
                || DateTime.Compare(tournament.EndDate, game.GameDate) < 0)
            {
                throw new ArgumentException(Resources.WrongRoundDate);
            }
        }

        private void SwitchTeamsOrder(Game game)
        {
            if (!GameValidation.IsFreeDayGame(game))
            {
                int tempHomeId = game.HomeTeamId;
                game.HomeTeamId = game.AwayTeamId.Value;
                game.AwayTeamId = tempHomeId;
            }
        }
        #endregion

        #region private methods

        private void UpdateTournamentLastTimeUpdated(Game game)
        {
            var tournament = _tournamentService.Get(game.TournamentId);
            tournament.LastTimeUpdated = TimeProvider.Current.UtcNow;
            _tournamentRepository.Update(tournament);
        }

        #endregion
    }
}