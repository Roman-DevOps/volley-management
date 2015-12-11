﻿namespace VolleyManagement.UI.Areas.Mvc.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Contracts;
    using Contracts.Exceptions;
    using ViewModels.GameResults;

    /// <summary>
    /// Defines GameResultsController
    /// </summary>
    public class GameResultsController : Controller
    {
        /// <summary>
        /// Holds TournamentService instance
        /// </summary>
        private readonly ITeamService _teamService;

        /// <summary>
        /// Holds TournamentService instance
        /// </summary>
        private readonly IGameResultService _gameResultsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameResultsController"/> class
        /// </summary>
        /// <param name="teamService">The team service</param>
        /// <param name="gameResultsService">The game result service</param>
        public GameResultsController(ITeamService teamService, IGameResultService gameResultsService)
        {
            _teamService = teamService;
            _gameResultsService = gameResultsService;
        }

        /// <summary>
        /// Details method.
        /// </summary>
        /// <param name="id">Id of game result</param>
        /// <returns>Details view</returns>
        public ActionResult Details(int id)
        {
            var gameResult = GameResultViewModel.Map(_gameResultsService.Get(id));
            gameResult.HomeTeamName = _teamService.Get(gameResult.HomeTeamId).Name;
            gameResult.AwayTeamName = _teamService.Get(gameResult.AwayTeamId).Name;
            return View(gameResult);
        }

        /// <summary>
        /// Create GET method.
        /// </summary>
        /// <param name="Id">Id of tournament</param>
        /// <returns>Create view.</returns>
        public ActionResult Create(int Id)
        {
            ViewBag.Teams = GetTeams();
            GameResultViewModel gameResultViewModel = new GameResultViewModel() { TournamentId = Id };
            return View(gameResultViewModel);
        }

        /// <summary>
        /// Create POST method
        /// </summary>
        /// <param name="gameResultViewModel">Game result view model</param>
        /// <returns>View of tournament details</returns>
        [HttpPost]
        public ActionResult Create(GameResultViewModel gameResultViewModel)
        {
            var gameResult = gameResultViewModel.ToDomain();
            try
            {
                _gameResultsService.Create(gameResult);
                return RedirectToAction("Details", "Tournaments", new { id = gameResult.TournamentId });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("ValidationError", ex.Message);
                var teams = _teamService.Get().Select(team => new SelectListItem() { Value = team.Id.ToString(), Text = team.Name }).ToList();
                ViewBag.Teams = teams;
                return View(gameResultViewModel);
            }
        }

        /// <summary>
        /// Edit game results action (GET)
        /// </summary>
        /// <param name="id">Game results id</param>
        /// <returns>View to edit specific game results</returns>
        public ActionResult Edit(int id)
        {
            return GetGameResultsView(id);
        }

        /// <summary>
        /// Edit game results action (POST)
        /// </summary>
        /// <param name="gameResultViewModel">Game results after editing</param>
        /// <returns>Index view if game results was valid, otherwise - edit view</returns>
        [HttpPost]
        public ActionResult Edit(GameResultViewModel gameResultViewModel)
        {
            try
            {
                if (this.ModelState.IsValid)
                {
                    var gameResult = gameResultViewModel.ToDomain();
                    this._gameResultsService.Edit(gameResult);
                    return this.RedirectToAction(
                                        "TournamentResults", 
                                        new { id = gameResult.TournamentId });
                }
            }
            catch (MissingEntityException)
            {
                this.ModelState.AddModelError(
                                string.Empty,
                                Resources.GameResultsController.GameResultsWasDeleted);
            }

            ViewBag.Teams = GetTeams();
            return this.View(gameResultViewModel);
        }

        /// <summary>
        /// Delete tournament action (GET)
        /// </summary>
        /// <param name="id">Tournament id</param>
        /// <returns>View to delete specific tournament</returns>
        public ActionResult Delete(int id)
        {
            return GetGameResultsView(id);
        }

        /// <summary>
        /// Delete game result action (POST)
        /// </summary>
        /// <param name="id">Game result id</param>
        /// <returns>Index view</returns>
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var gameResult = _gameResultsService.Get(id);

            if (gameResult == null)
            {
                return HttpNotFound();
            }

            this._gameResultsService.Delete(id);
            return RedirectToAction(
                                    "TournamentResults",
                                    new { id = gameResult.TournamentId });
        }

        /// <summary>
        /// Represents all game results of specified tournament
        /// </summary>
        /// <param name="id">Id of tournament</param>
        /// <returns>View represents results list</returns>
        public ActionResult TournamentResults(int id)
        {
            var gameResults = _gameResultsService.Get().Where(
                                                            gr =>
                                                            gr.TournamentId == id)
                                                            .Select(
                gr =>
                {
                    var gameResult = GameResultViewModel.Map(gr);
                    gameResult.HomeTeamName = _teamService.Get(gameResult.HomeTeamId).Name;
                    gameResult.AwayTeamName = _teamService.Get(gameResult.AwayTeamId).Name;
                    return gameResult;
                })
                   .ToList();

            return View(gameResults);
        }

        private ActionResult GetGameResultsView(int id)
        {
            var gameResults = _gameResultsService.Get(id);

            if (gameResults == null)
            {
                return HttpNotFound();
            }

            ViewBag.Teams = GetTeams();
            var gameResultsViewModel = GameResultViewModel.Map(gameResults);
            gameResultsViewModel.HomeTeamName = _teamService.Get(gameResults.HomeTeamId).Name;
            gameResultsViewModel.AwayTeamName = _teamService.Get(gameResults.AwayTeamId).Name;
            return View(gameResultsViewModel);
        }

        private List<SelectListItem> GetTeams()
        {
            return _teamService.Get().Select(team => new SelectListItem() { Value = team.Id.ToString(), Text = team.Name }).ToList();
        }
    }
}