﻿namespace VolleyManagement.UI.Areas.Mvc.ViewModels.Players
{
    using System.ComponentModel.DataAnnotations;

    using Domain.PlayersAggregate;
    using Resources.UI;

    /// <summary>
    /// Represents player view model
    /// </summary>
    public class PlayerNameViewModel
    {
        /// <summary>
        /// Gets or sets the player Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the player firstname
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the player lastname
        /// </summary>
        public string LastName { get; set; }

        #region Factory Methods

        /// <summary>
        /// Maps domain entity to presentation
        /// </summary>
        /// <param name="player"> Domain object </param>
        /// <returns> View model object </returns>
        public static PlayerNameViewModel Map(Player player)
        {
            var playerNameViewModel = new PlayerNameViewModel {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName
            };
            return playerNameViewModel;
        }

        /// <summary>
        /// Maps domain freePlayerDto to presentation
        /// </summary>
        /// <param name="player"> Domain object </param>
        /// <returns> View model object </returns>
        public static PlayerNameViewModel Map(FreePlayerDto player)
        {
            var playerNameViewModel = new PlayerNameViewModel {
                Id = player.PlayerId.Id,
                FirstName = player.FirstName,
                LastName = player.LastName
            };
            return playerNameViewModel;
        }

        /// <summary>
        /// Maps PlayerViewModel to PlayerNameViewModel
        /// </summary>
        /// <param name="player"> Domain object </param>
        /// <returns> View model object </returns>
        public static PlayerNameViewModel Map(PlayerViewModel player)
        {
            var playerNameViewModel = new PlayerNameViewModel {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName
            };
            return playerNameViewModel;
        }

        /// <summary>
        /// Maps presentation entity to domain
        /// </summary>
        /// <returns> Domain object </returns>
        public Player ToDomain()
        {
            return new Player(Id, FirstName, LastName);
        }

        /// <summary>
        /// Maps presentation entity to createPlayerDto
        /// </summary>
        /// <returns> CreatePlayerDto object </returns>
        public CreatePlayerDto ToCreatePlayerDto()
        {
            return new CreatePlayerDto {
                FirstName = FirstName,
                LastName = LastName,
                BirthYear = null,
                Height = null,
                Weight = null
            };
        }

        #endregion
    }
}