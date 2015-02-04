﻿namespace VolleyManagement.WebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.OData;
    using System.Web.Http.OData.Routing;
    using Contracts;
    using Domain.Tournaments;
    using VolleyManagement.WebApi.Mappers;
    using VolleyManagement.WebApi.ViewModels.Tournaments;

    /// <summary>
    /// Defines TournamentsController
    /// </summary>
    public class TournamentsController : ODataController
    {
        /// <summary>
        /// Holds TournamentService instance
        /// </summary>
        private readonly ITournamentService _tournamentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentsController"/> class
        /// </summary>
        /// <param name="tournamentService">The tournament service</param>
        public TournamentsController(ITournamentService tournamentService)
        {
            _tournamentService = tournamentService;
        }

        /// <summary>
        /// Gets all tournaments from TournamentService
        /// </summary>
        /// <returns>All tournaments</returns>
        public IQueryable<TournamentViewModel> Get()
        {
            try
            {
                var tounaments = _tournamentService.GetAll().ToList();
                var viewModelTournament = new List<TournamentViewModel>();
                foreach (var t in tounaments)
                {
                    viewModelTournament.Add(DomainToViewModel.Map(t));
                }

                return viewModelTournament.AsQueryable<TournamentViewModel>();
            }
            catch (Exception)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }

        /// <summary>
        /// Gets specific tournament from TournamentService
        /// </summary>
        /// <param name="key">Tournament id</param>
        /// <returns>Response with specific tournament</returns>
        public HttpResponseMessage Get([FromODataUri]int key)
        {
            try
            {
                var tournament = _tournamentService.FindById(key);
                return Request.CreateResponse(HttpStatusCode.OK, DomainToViewModel.Map(tournament));
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Creates new Tournament.
        /// </summary>
        /// <param name="viewModel">Tournament to create.</param>
        /// <returns>HttpResponse with created tournament.</returns>
        [HttpPost]
        public HttpResponseMessage Post(TournamentViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var response = new HttpResponseMessage();
            try
            {
                var tournamentToCreate = ViewModelToDomain.Map(viewModel);
                _tournamentService.Create(tournamentToCreate);
                viewModel.Id = tournamentToCreate.Id;
                response = Request.CreateResponse<TournamentViewModel>(HttpStatusCode.Created, viewModel);
                return response;
            }
            catch (Exception)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError);
                return response;
            }
        }

        /// <summary>
        /// Updates tournament.
        /// </summary>
        /// <param name="key">Tournament id</param>
        /// <param name="viewModel">Tournament to update.</param>
        /// <returns>HttpResponse successful updated.</returns>
        [HttpPut]
        public HttpResponseMessage Put([FromODataUri]int key, [FromBody]TournamentViewModel viewModel)
        {
            if (!ModelState.IsValid || viewModel.Id != key)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            try
            {
                var tournamentToUpdate = ViewModelToDomain.Map(viewModel);
                _tournamentService.Edit(tournamentToUpdate);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (ArgumentException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Deletes tournament from TournamentService
        /// </summary>
        /// <param name="key">key to delete</param>
        /// <returns>All tournaments</returns>
        [HttpDelete]
        public HttpResponseMessage Delete(int key)
        {
            try
            {
                _tournamentService.Delete(key);
                return Request.CreateResponse(HttpStatusCode.Accepted);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }
    }
}