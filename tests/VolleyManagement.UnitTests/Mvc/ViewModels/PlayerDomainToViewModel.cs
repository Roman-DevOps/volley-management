﻿using VolleyManagement.UI.Areas.Mvc.ViewModels.Players;
using VolleyManagement.UnitTests.Services.PlayerService;
using Xunit;

namespace VolleyManagement.UnitTests.Mvc.ViewModels
{
    /// <summary>
    ///     View model player class test
    /// </summary>
    public class PlayerDomainToViewModel
    {
        /// <summary>
        ///     Map() method test.
        ///     Does correct a player domain model mapped to a view model.
        /// </summary>
        [Fact]
        public void Map_DomainPlayerAsParam_MappedToViewModel()
        {
            // Arrange
            var testViewModel = new PlayerMvcViewModelBuilder()
                .WithId(1)
                .WithFirstName("FirstName")
                .WithLastName("LastName")
                .WithBirthYear(1983)
                .WithHeight(186)
                .WithWeight(95)
                .Build();

            var testDomainModel = new PlayerBuilder()
                .WithId(1)
                .WithFirstName("FirstName")
                .WithLastName("LastName")
                .WithBirthYear(1983)
                .WithHeight(186)
                .WithWeight(95)
                .Build();

            // Act
            var actual = PlayerViewModel.Map(testDomainModel);

            // Assert
            TestHelper.AreEqual(testViewModel, actual, new PlayerViewModelComparer());
        }
    }
}