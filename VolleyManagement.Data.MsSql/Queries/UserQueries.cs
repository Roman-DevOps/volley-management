﻿namespace VolleyManagement.Data.MsSql.Queries
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using VolleyManagement.Data.Contracts;
    using VolleyManagement.Data.MsSql.Entities;
    using VolleyManagement.Data.Queries.Common;
    using VolleyManagement.Data.Queries.User;
    using VolleyManagement.Domain.UsersAggregate;

    /// <summary>
    /// Provides Object Query implementation for Users
    /// </summary>
    public class UserQueries : IQueryAsync<User, FindByIdCriteria>,
                             IQueryAsync<User, FindByNameCriteria>,
                             IQueryAsync<User, FindByEmailCriteria>,
                             IQueryAsync<User, FindByLoginInfoCriteria>
    {
        #region Fields

        private readonly VolleyUnitOfWork _unitOfWork;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UserQueries"/> class.
        /// </summary>
        /// <param name="unitOfWork"> The unit of work. </param>
        public UserQueries(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (VolleyUnitOfWork)unitOfWork;
        }

        #endregion

        #region Implemenations

        /// <summary>
        /// Finds User by given criteria
        /// </summary>
        /// <param name="criteria"> The criteria. </param>
        /// <returns> The <see cref="User"/>. </returns>
        public Task<User> ExecuteAsync(FindByIdCriteria criteria)
        {
            var query = _unitOfWork.Context.Users.Where(u => u.Id == criteria.Id);

            // ToDo: Use Automapper to substitute Select clause
            return query.Select(GetUserMapping()).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds User by given criteria
        /// </summary>
        /// <param name="criteria"> The criteria. </param>
        /// <returns> The <see cref="User"/>. </returns>
        public Task<User> ExecuteAsync(FindByNameCriteria criteria)
        {
            var query = _unitOfWork.Context.Users.Where(u => u.UserName == criteria.Name);

            // ToDo: Use Automapper to substitute Select clause
            return query.Select(GetUserMapping()).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds User by given criteria
        /// </summary>
        /// <param name="criteria"> The criteria. </param>
        /// <returns> The <see cref="User"/>. </returns>
        public Task<User> ExecuteAsync(FindByEmailCriteria criteria)
        {
            var query = _unitOfWork.Context.Users.Where(u => u.Email == criteria.Email);

            // ToDo: Use Automapper to substitute Select clause
            return query.Select(GetUserMapping()).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds User by given criteria
        /// </summary>
        /// <param name="criteria"> The criteria. </param>
        /// <returns> The <see cref="User"/>. </returns>
        public Task<User> ExecuteAsync(FindByLoginInfoCriteria criteria)
        {
            var query = _unitOfWork.Context.LoginProviders
                                           .Where(l => l.ProviderKey == criteria.ProviderKey
                                               && l.LoginProvider == criteria.LoginProvider)
                                           .Select(l => l.User);

            // ToDo: Use Automapper to substitute Select clause
            return query.Select(GetUserMapping()).FirstOrDefaultAsync();
        }

        #endregion

        #region Mapping

        private static Expression<Func<UserEntity, User>> GetUserMapping()
        {
            return
                t =>
                new User
                {
                    Id = t.Id,
                    UserName = t.UserName,
                    Email = t.Email,
                    PersonName = t.FullName,
                    PhoneNumber = t.CellPhone,
                    LoginProviders = t.LoginProviders.Select(
                                            l => new LoginProviderInfo
                                                     {
                                                         ProviderKey = l.ProviderKey,
                                                         LoginProvider = l.LoginProvider
                                                     })
                };
        }

        #endregion
    }
}