﻿namespace VolleyManagement.Contracts
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
        /// <returns>User entity.</returns>
        User GetUser(int userId);

        /// <summary>
        /// It blocks an account of unwished user.
        /// </summary>
        /// <param name="userId">User's Id.</param>
        void SetUserBlocked(int userId);

        /// <summary>
        /// It unblocks an account of pointed user.
        /// </summary>
        /// <param name="userId">User's Id.</param>
        void SetUserUnblocked(int userId);

        /// <summary>
        /// Get user instance by Id.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <returns>User instance.</returns>
        User GetUserDetails(int userId);

        /// <summary>
        /// Get all active users.
        /// </summary>
        /// <returns>List of active users.</returns>
        List<User> GetAllActiveUsers();

        /// <summary>
        /// Gets list of users which role is Admin.
        /// </summary>
        /// <returns>List of User instances.</returns>
        List<User> GetAdminsList();
    }
}
