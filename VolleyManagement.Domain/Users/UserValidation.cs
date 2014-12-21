﻿namespace VolleyManagement.Domain.Users
{
    using System;
    using System.Linq;
    using VolleyManagement.Domain.Tournaments;

    /// <summary>
    /// User validation class.
    /// </summary>
    public static class UserValidation
    {
        /// <summary>
        /// Validates email.
        /// </summary>
        /// <param name="email">Email for validation</param>
        /// <returns>Validity of email</returns>
        public static bool ValidateEmail(string email)
        {//not empty
            //valid
            //uniq
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates telephone.
        /// </summary>
        /// <param name="cellPhone">Telephone for validation</param>
        /// <returns>Validity of Telephone</returns>
        public static bool ValidateCellPhone(string cellPhone)
        {//valid
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates user name.
        /// </summary>
        /// <param name="userName">User name for validation</param>
        /// <returns>Validity of User nam</returns>
        public static bool ValidateUserName(string userName)
        {//not empty
            //uniq
            //60
            //letters of any languages
            return string.IsNullOrEmpty(userName) && userName.All(Char.IsLetter) && userName.Length > Constants.MaxNameLength;
        }

        /// <summary>
        /// Validates password.
        /// </summary>
        /// <param name="password">Password for validation</param>
        /// <returns>Validity of Password</returns>
        public static bool ValidatePassword(string password)
        {
            return string.IsNullOrEmpty(password);
        }

        /// <summary>
        /// Validates Full name.
        /// </summary>
        /// <param name="fullName">Full name for validation</param>
        /// <returns>Validity of Full name</returns>
        public static bool ValidateFullName(string fullName)
        {//60
            //letters of any languages
            return fullName.All(Char.IsLetter);
        }
    }
}