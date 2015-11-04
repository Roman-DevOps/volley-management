﻿namespace VolleyManagement.Data.MsSql.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using VolleyManagement.Data.Contracts;
    using VolleyManagement.Data.MsSql.Entities;
    using VolleyManagement.Data.Queries.Common;
    using VolleyManagement.Data.Queries.Team;
    using VolleyManagement.Domain.PlayersAggregate;
    using VolleyManagement.Domain.TeamsAggregate;

    /// <summary>
    /// Provides Query Object implementation for Player entity
    /// </summary>
    public class TeamQueries : IQuery<Team, FindByIdCriteria>,
                               IQuery<List<Team>, GetAllCriteria>,
                               IQuery<Team, FindByCaptainIdCriteria>
    {
        #region Fields

        private readonly VolleyUnitOfWork _unitOfWork;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamQueries"/> class.
        /// </summary>
        /// <param name="unitOfWork"> The unit of work. </param>
        public TeamQueries(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (VolleyUnitOfWork)unitOfWork;
        }

        #endregion

        #region Implemenations

        /// <summary>
        /// Finds Team by given criteria
        /// </summary>
        /// <param name="criteria"> The criteria. </param>
        /// <returns> The <see cref="Player"/>. </returns>
        public Team Execute(FindByIdCriteria criteria)
        {
            return this._unitOfWork.Context.Teams
                .Where(t => t.Id == criteria.Id)
                .Select(GetTeamMapping())
                .SingleOrDefault();
        }

        /// <summary>
        /// Finds Teams by given criteria
        /// </summary>
        /// <param name="criteria"> The criteria. </param>
        /// <returns> The <see cref="Team"/>. </returns>
        public List<Team> Execute(GetAllCriteria criteria)
        {
            return _unitOfWork.Context.Teams.Select(GetTeamMapping()).ToList();
        }

        /// <summary>
        /// Finds Teams by given criteria
        /// </summary>
        /// <param name="criteria"> The criteria. </param>
        /// <returns> The <see cref="Team"/>. </returns>
        public Team Execute(FindByCaptainIdCriteria criteria)
        {
            return _unitOfWork.Context.Teams
                                      .Where(t => t.CaptainId == criteria.CaptainId)
                                      .Select(GetTeamMapping())
                                      .SingleOrDefault();
        }

        #endregion

        #region Mapping

        private static Expression<Func<TeamEntity, Team>> GetTeamMapping()
        {
            return t => new Team
                            {
                                Id = t.Id,
                                Name = t.Name,
                                Coach = t.Coach,
                                CaptainId = t.CaptainId,
                                Achievements = t.Achievements
                            };
        }

        #endregion
    }
}