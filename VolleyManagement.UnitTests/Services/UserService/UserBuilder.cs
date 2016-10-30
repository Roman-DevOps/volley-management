﻿namespace VolleyManagement.UnitTests.Services.UserManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Contracts.Authentication.Models;

    using VolleyManagement.Domain.UsersAggregate;
    /// <summary>
    /// Builder for test users
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class UserBuilder
    {
        /// <summary>
        /// Holds test user instance
        /// </summary>
        private User _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserModelBuilder"/> class
        /// </summary>
        public UserBuilder()
        {
            this._user = new User
            {
                Id = 1,
                UserName = "Player",
                Email = "example@i.ua",
                PersonName = "Eugene",
                PhoneNumber = "068-11-22-333"
            };
        }

        /// <summary>
        /// Sets id of test user
        /// </summary>
        /// <param name="id">Id for test user</param>
        /// <returns>user builder object</returns>
        public UserBuilder WithId(int id)
        {
            this._user.Id = id;
            return this;
        }

        /// <summary>
        /// Sets username of test user
        /// </summary>
        /// <param name="username">Username for test user</param>
        /// <returns>User builder object</returns>
        public UserBuilder WithUserName(string username)
        {
            this._user.UserName = username;
            return this;
        }

        /// <summary>
        /// Sets email of test user
        /// </summary>
        /// <param name="email">email for test user</param>
        /// <returns>User builder object</returns>
        public UserBuilder WithEmail(string email)
        {
            this._user.Email = email;
            return this;
        }

        /// <summary>
        /// Sets phone number of test user
        /// </summary>
        /// <param name="phoneNumber">Phone Number for test user</param>
        /// <returns>User builder object</returns>
        public UserBuilder WithPhoneNumber(string phoneNumber)
        {
            this._user.PhoneNumber = phoneNumber;
            return this;
        }

        /// <summary>
        /// Sets person name of test user
        /// </summary>
        /// <param name="personName">Person name for test user</param>
        /// <returns>User builder object</returns>
        public UserBuilder WithFullName(string personName)
        {
            this._user.PersonName = personName;
            return this;
        }

        /// <summary>
        /// Sets login providers of test user
        /// </summary>
        /// <param name="providers">Login providers for test user</param>
        /// <returns>User builder object</returns>
        public UserBuilder WithLoginProviders(List<LoginProviderInfo> providers)
        {
            _user.LoginProviders = providers;
            return this;
        }

        /// <summary>
        /// Builds test user
        /// </summary>
        /// <returns>Test user</returns>
        public User Build()
        {
            return this._user;
        }
    }
}
