﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tournament.cs" company="SoftServe">
//   Copyright (c) SoftServe. All rights reserved.
// </copyright>
// <summary>
//   Defines the domain Tournament type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftServe.VolleyManagement.Domain.Tournaments
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Tournament domain class.
    /// </summary>
    public class Tournament
    {
        /// <summary>
        /// Gets or sets a value indicating where Id.
        /// </summary>
        /// <value>Id of tournament.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating where Name.
        /// </summary>
        /// <value>Name of tournament.</value>
        [Required(ErrorMessageResourceName = "NameRequired",
            ErrorMessageResourceType = typeof(Resources))]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating where Description.
        /// </summary>
        /// <value>Description of tournament.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating where Season.
        /// </summary>
        /// <value>Season of tournament.</value>
        [Required(ErrorMessageResourceName = "SeasonRequired",
            ErrorMessageResourceType = typeof(Resources))]
        public string Season { get; set; }

        /// <summary>
        /// Gets or sets a value indicating where Scheme.
        /// </summary>
        /// <value>Scheme of tournament.</value>
        [Required(ErrorMessageResourceName = "SchemeRequired",
            ErrorMessageResourceType = typeof(Resources))]
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating where LinkToReglament.
        /// </summary>
        /// <value>LinkToReglament of tournament.</value>
        public string LinkToReglament { get; set; }
    }
}
