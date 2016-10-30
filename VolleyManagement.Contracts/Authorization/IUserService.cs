﻿using VolleyManagement.Domain.RolesAggregate;

namespace VolleyManagement.Contracts
{
    using System.Collections.Generic;
    using Domain.UsersAggregate;

    /// <summary>
    /// Provides specified information about user.
    /// </summary>
    public interface IUserService
    {

        /// <summary>
        /// Get all registered users.
        /// </summary>
        /// <returns>List of users.</returns>
        List<User> GetAllUsers();

        /// <summary>
        /// Get user instance by Id.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <returns>User instance.</returns>
        User GetUser(int userId);


        /// <summary>
        /// Get user's roles by user's Id.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <returns>List of all roles for user.</returns>
        List<Role> GetUserRoles(int userId);

        /// <summary>
        /// Get login providers info by user's Id.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <returns>List of all auth providers for user.</returns>
        List<LoginProviderInfo> GetAuthProviders(int userId);
    }
}
