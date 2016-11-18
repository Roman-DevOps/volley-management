﻿namespace VolleyManagement.Data.MsSql.Repositories
{
    using System.Data.Entity;
    using System.Linq;
    using VolleyManagement.Data.Contracts;
    using VolleyManagement.Data.Exceptions;
    using VolleyManagement.Data.MsSql.Entities;
    using VolleyManagement.Data.MsSql.Mappers;
    using VolleyManagement.Domain.TournamentRequestAggregate;

    /// <summary>
    /// Defines implementation of the ITournamentRequestRepository contract.
    /// </summary>
    internal class TournamentRequestRepository : ITournamentRequestRepository
    {
        private readonly VolleyUnitOfWork _unitOfWork;
        private readonly DbSet<TournamentRequestEntity> _dalRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentRequestRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">Instance of class which implements <see cref="IUnitOfWork"/>.</param>
        public TournamentRequestRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = (VolleyUnitOfWork)unitOfWork;
            _dalRequest = _unitOfWork.Context.TournamentRequests;
        }

        /// <summary>
        /// Gets unit of work.
        /// </summary>
        public IUnitOfWork UnitOfWork
        {
            get
            {
                return _unitOfWork;
            }
        }

        /// <summary>
        /// Adds new tournament request.
        /// </summary>
        /// <param name="newRequest">Tournament request to add.</param>
        public void Add(TournamentRequest newRequest)
        {
            var newRequestEntity = new TournamentRequestEntity();

            DomainToDal.Map(newRequestEntity, newRequest);
            _dalRequest.Add(newRequestEntity);
            _unitOfWork.Commit();
            newRequest.Id = newRequestEntity.Id;
        }

        /// <summary>
        /// Updates specified tournament request.
        /// </summary>
        /// <param name="updatedEntity">Updated request.</param>
        public void Update(TournamentRequest updatedEntity)
        {
            var requestToUpdate = _dalRequest.SingleOrDefault(t => t.Id == updatedEntity.Id);

            if (requestToUpdate == null)
            {
                throw new ConcurrencyException();
            }

            DomainToDal.Map(requestToUpdate, updatedEntity);
        }

        /// <summary>
        /// Removes tournament request by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the request.</param>
        public void Remove(int id)
        {
            var dalToRemove = new TournamentRequestEntity { Id = id };

            _dalRequest.Attach(dalToRemove);
            _dalRequest.Remove(dalToRemove);
        }
    }
}
