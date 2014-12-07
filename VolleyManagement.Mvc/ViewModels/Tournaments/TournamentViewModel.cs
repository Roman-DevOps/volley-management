﻿namespace VolleyManagement.Mvc.ViewModels.Tournaments
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using VolleyManagement.Domain.Tournaments;

    /// <summary>
    /// TournamentViewModel for Create and Edit actions
    /// </summary>
    public class TournamentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentViewModel"/> class.
        /// </summary>
        public TournamentViewModel()
        {
            Tournament = new Tournament();
            //           List<SelectListItem> items = new List<SelectListItem>();
             //items.Add(new SelectListItem { Text = "Action", Value = "0"});
             //items.Add(new SelectListItem { Text = "Drama", Value = "1" });
             //items.Add(new SelectListItem { Text = "Comedy", Value = "2", Selected = true });
            SchemeList = new List<string> { "1", "2", "2.5" };
            SeasonsList = new List<string>();
            int currentYear = DateTime.Now.Year;

            for (int i = 0; i < 9; i++)
            {
                this.SeasonsList.Add((currentYear - 3 + i).ToString() + "/"
                    + (currentYear - 2 + i).ToString());
            }
        }

        /// <summary>
        /// Gets or sets the list of seasons.
        /// </summary>
        /// <value>The list of seasons.</value>
        public List<string> SeasonsList { get; set; }

        /// <summary>
        /// Gets or sets the list of schemes.
        /// </summary>
        /// <value>The list of schemes.</value>
        public List<string> SchemeList { get; set; }

        /// <summary>
        /// Gets or sets the tournament.
        /// </summary>
        /// <value>The tournament.</value>
        public Tournament Tournament { get; set; }
    }
}