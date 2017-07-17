﻿namespace VolleyManagement.Data.MsSql.Repositories
{
    using System.Data.Entity;
    using System.Linq;
    using Contracts;
    using Crosscutting.Contracts.Specifications;
    using Domain.TournamentsAggregate;
    using Entities;
    using Exceptions;
    using Mappers;
    using Specifications;

    /// <summary>
    /// Defines implementation of the ITournamentRepository contract.
    /// </summary>
    internal class TournamentRepository : ITournamentRepository
    {
        private readonly DbSet<TournamentEntity> _dalTournaments;
        private readonly DbSet<TeamEntity> _dalTeams;
        private readonly DbSet<DivisionEntity> _dalDivisions;
        private readonly DbSet<GroupEntity> _dalGroups;
        private readonly VolleyUnitOfWork _unitOfWork;
        private readonly ISpecification<TournamentEntity> _dbStorageSpecification = new TournamentsStorageSpecification();

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public TournamentRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = (VolleyUnitOfWork)unitOfWork;
            _dalTournaments = _unitOfWork.Context.Tournaments;
            _dalTeams = _unitOfWork.Context.Teams;
            _dalDivisions = _unitOfWork.Context.Divisions;
            _dalGroups = _unitOfWork.Context.Groups;
        }

        /// <summary>
        /// Gets unit of work.
        /// </summary>
        public IUnitOfWork UnitOfWork
        {
            get { return _unitOfWork; }
        }

        /// <summary>
        /// Adds new tournament.
        /// </summary>
        /// <param name="newEntity">The tournament for adding.</param>
        public void Add(Tournament newEntity)
        {
            var tournament = new TournamentEntity();
            DomainToDal.Map(tournament, newEntity);

            if (!_dbStorageSpecification.IsSatisfiedBy(tournament))
            {
                throw new InvalidEntityException();
            }

            _dalTournaments.Add(tournament);
            _unitOfWork.Commit();
            MapIdentifiers(newEntity, tournament);
        }

        /// <summary>
        /// Updates specified tournament.
        /// </summary>
        /// <param name="updatedEntity">Updated tournament.</param>
        public void Update(Tournament updatedEntity)
        {
            var tournamentToUpdate = _dalTournaments.Single(t => t.Id == updatedEntity.Id);
            updatedEntity.Divisions.Clear();
            DomainToDal.Map(tournamentToUpdate, updatedEntity);

            // ToDo: Check Do we really need this?
            //// _dalTournaments.Context.ObjectStateManager.ChangeObjectState(tournamentToUpdate, EntityState.Modified);
        }

        /// <summary>
        /// Removes tournament by id.
        /// </summary>
        /// <param name="id">The id of tournament to remove.</param>
        public void Remove(int id)
        {
            var dalToRemove = new TournamentEntity { Id = id };
            _dalTournaments.Attach(dalToRemove);
            _dalTournaments.Remove(dalToRemove);
        }

        /// <summary>
        /// Add team and group to the tournament
        /// </summary>
        /// <param name="teamId">Team id to add</param>
        /// <param name="tournamentId">Tournament id, where team is going to play</param>
        /// <param name="groupId">Group id to add</param>
        /// <param name="divisionId">Division id, where group is</param>
        public void AddTeamToTournament(int teamId, int tournamentId, int groupId, int divisionId)
        {
            var tournamentEntity = _dalTournaments.Find(tournamentId);
            var teamEntity = _dalTeams.Find(teamId);

            tournamentEntity.Divisions.Find(d => d.Id == divisionId).Groups.Find(g => g.Id == groupId).Teams.Add(teamEntity);
        }

        /// <summary>
        /// Removes team from the tournament
        /// </summary>
        /// <param name="teamId">Team to be removed id</param>
        /// <param name="tournamentId">Tournament id from which remove team</param>
        public void RemoveTeamFromTournament(int teamId, int tournamentId)
        {
            var tournamentEntity = _unitOfWork.Context.Tournaments.Find(tournamentId);
            var teamEntity = tournamentEntity?.Divisions
                                             .SelectMany(d => d.Groups)
                                             .SelectMany(g => g.Teams)
                                             .SingleOrDefault(t => t.Id == teamId);

            if (teamEntity == null)
            {
                throw new ConcurrencyException();
            }

            foreach (var group in teamEntity.Groups)
            {
                if (group.Division.TournamentId == tournamentId)
                {
                    group.Teams.Remove(teamEntity);
                }
            }
        }

        private void MapIdentifiers(Tournament to, TournamentEntity from)
        {
            to.Id = from.Id;

            foreach (DivisionEntity divisionEntity in from.Divisions)
            {
                Division divisionDomain = to.Divisions.Where(d => d.Name == divisionEntity.Name).First();
                divisionDomain.Id = divisionEntity.Id;
                divisionDomain.TournamentId = divisionEntity.TournamentId;

                foreach (GroupEntity groupEntity in divisionEntity.Groups)
                {
                    Group groupDomain = divisionDomain.Groups.Where(g => g.Name == groupEntity.Name).First();
                    groupDomain.Id = groupEntity.Id;
                    groupDomain.DivisionId = divisionEntity.Id;
                }
            }
        }
    }
}
