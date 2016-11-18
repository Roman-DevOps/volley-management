namespace VolleyManagement.Data.MsSql.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// DAL user model
    /// </summary>
    public class UserEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserEntity"/> class.
        /// </summary>
        public UserEntity()
        {
            LoginProviders = new List<LoginInfoEntity>();
        }

        /// <summary>
        /// Gets or sets id of user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets login of user
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets email of user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets full name of user
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets telephone of user
        /// </summary>
        public string CellPhone { get; set; }

        /// <summary>
        /// Gets or sets boolean value pointing is user account blocked .
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Login providers
        /// </summary>
        public virtual ICollection<LoginInfoEntity> LoginProviders { get; set; }

        /// <summary>
        /// Roles assigned to current user
        /// </summary>
        public virtual ICollection<RoleEntity> Roles { get; set; }
    }
}