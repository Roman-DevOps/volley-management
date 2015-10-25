﻿namespace VolleyManagement.Data.MsSql.Entities
{
    /// <summary>
    /// Database validation constraints
    /// </summary>
    internal static class ValidationConstants
    {
        public const int EMPTY_DATABASE_ID_VALUE = 0;

        /// <summary>
        /// Tournament entity validation constants
        /// </summary>
        public static class Tournament
        {
            public const short SCHEMA_STORAGE_OFFSET = 1900;

            public const int MAX_NAME_LENGTH = 60;

            public const int MAX_DESCRIPTION_LENGTH = 300;

            public const int MAX_URL_LENGTH = 255;
        }

        /// <summary>
        /// Player entity validation constants
        /// </summary>
        public static class Player
        {
            public const int MAX_FIRST_NAME_LENGTH = 60;

            public const int MAX_LAST_NAME_LENGTH = 60;
        }

        /// <summary>
        /// Team entity validation constants
        /// </summary>
        public static class Team
        {
            public const int MAX_NAME_LENGTH = 60;

            public const int MAX_COACH_NAME_LENGTH = 60;

            public const int MAX_ACHIEVEMENTS_LENGTH = 4000;
        }

        /// <summary>
        /// Team entity validation constants
        /// </summary>
        public static class Contributor
        {
            public const int MAX_TEAM_NAME_LENGTH = 20;

            public const int MAX_COURSE_NAME_LENGTH = 20;

            public const int MAX_NAME_LENGTH = 30;
        }

        /// <summary>
        /// User entity validation constants
        /// </summary>
        public static class User
        {
            public const int MAX_USER_NAME_LENGTH = 256;

            public const int MAX_EMAIL_LENGTH = 128;

            public const int MAX_FULL_NAME_LENGTH = 128;

            public const int MAX_PHONE_LENGTH = 15;

            public const int MAX_LOGIN_PROVIDER_LENGTH = 128;

            public const int MAX_PROVIDER_KEY_LENGTH = 128;
        }
    }
}