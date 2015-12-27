﻿namespace VolleyManagement.UI.Areas.Mvc.ViewModels.GameResults
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using VolleyManagement.Domain;
    using VolleyManagement.Domain.GameResultsAggregate;

    /// <summary>
    /// Represents a view model for game result.
    /// </summary>
    public class GameResultViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameResultViewModel"/> class.
        /// </summary>
        public GameResultViewModel()
        {
            SetsScore = new Score();
            SetScores = Enumerable.Repeat(new Score(), Constants.GameResult.MAX_SETS_COUNT).ToList();
            TeamsList = new List<SelectListItem>();
        }

        /// <summary>
        /// Gets or sets the identifier of game result.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the tournament where game result belongs.
        /// </summary>
        public int TournamentId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the home team which played the game.
        /// </summary>
        public int HomeTeamId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the away team which played the game.
        /// </summary>
        public int AwayTeamId { get; set; }

        /// <summary>
        /// Gets or sets the name of the home team which played the game.
        /// </summary>
        public string HomeTeamName { get; set; }

        /// <summary>
        /// Gets or sets the name of the away team which played the game.
        /// </summary>
        public string AwayTeamName { get; set; }

        /// <summary>
        /// Gets or sets the final score of the game.
        /// </summary>
        public Score SetsScore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the technical defeat has taken place.
        /// </summary>
        public bool IsTechnicalDefeat { get; set; }

        /// <summary>
        /// Gets or sets the set scores.
        /// </summary>
        public List<Score> SetScores { get; set; }

        /// <summary>
        /// Gets or sets the list of teams of a tournament where game result belongs.
        /// </summary>
        public List<SelectListItem> TeamsList { get; set; }

        /// <summary>
        /// Maps domain model of game result to view model of game result.
        /// </summary>
        /// <param name="gameResult">Domain model of game result.</param>
        /// <returns>View model of game result.</returns>
        public static GameResultViewModel Map(GameResultDto gameResult)
        {
            return new GameResultViewModel
            {
                Id = gameResult.Id,
                TournamentId = gameResult.TournamentId,
                HomeTeamId = gameResult.HomeTeamId,
                AwayTeamId = gameResult.AwayTeamId,
                HomeTeamName = gameResult.HomeTeamName,
                AwayTeamName = gameResult.AwayTeamName,
                SetsScore = new Score { Home = gameResult.HomeSetsScore, Away = gameResult.AwaySetsScore },
                IsTechnicalDefeat = gameResult.IsTechnicalDefeat,
                SetScores = new List<Score>
                {
                    new Score { Home = gameResult.HomeSet1Score, Away = gameResult.AwaySet1Score },
                    new Score { Home = gameResult.HomeSet2Score, Away = gameResult.AwaySet2Score },
                    new Score { Home = gameResult.HomeSet3Score, Away = gameResult.AwaySet3Score },
                    new Score { Home = gameResult.HomeSet4Score, Away = gameResult.AwaySet4Score },
                    new Score { Home = gameResult.HomeSet5Score, Away = gameResult.AwaySet5Score }
                }
            };
        }

        /// <summary>
        /// Maps view model of game result to domain model of game result.
        /// </summary>
        /// <returns>Domain model of game result.</returns>
        public GameResult ToDomain()
        {
            return new GameResult
            {
                Id = this.Id,
                TournamentId = this.TournamentId,
                HomeTeamId = this.HomeTeamId,
                AwayTeamId = this.AwayTeamId,
                SetsScore = this.SetsScore,
                IsTechnicalDefeat = this.IsTechnicalDefeat,
                SetScores = this.SetScores
            };
        }
    }
}