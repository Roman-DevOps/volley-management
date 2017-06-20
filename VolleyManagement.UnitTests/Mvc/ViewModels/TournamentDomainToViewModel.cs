﻿namespace VolleyManagement.UnitTests.Mvc.ViewModels
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VolleyManagement.Domain.TournamentsAggregate;
    using VolleyManagement.UI.Areas.Mvc.ViewModels.Tournaments;
    using VolleyManagement.UnitTests.Services.TournamentService;

    /// <summary>
    /// Tests for DomainToViewModel class.
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TournamentDomainToViewModel
    {
        /// <summary>
        /// Test for Map() method.
        /// The method should map tournament domain model to view model.
        /// </summary>
        [TestMethod]
        public void Map_TournamentAsParam_MappedToViewModel()
        {
            // Arrange
            var tournament = new TournamentBuilder()
                                        .WithId(1)
                                        .WithName("test")
                                        .WithDescription("Volley")
                                        .WithScheme(TournamentSchemeEnum.Two)
                                        .WithSeason(2016)
                                        .WithRegulationsLink("volley.dp.ua")
                                        .Build();
            var expected = new TournamentMvcViewModelBuilder()
                                        .WithId(1)
                                        .WithName("test")
                                        .WithDescription("Volley")
                                        .WithScheme(TournamentSchemeEnum.Two)
                                        .WithSeason(2016)
                                        .WithRegulationsLink("volley.dp.ua")
                                        .Build();

            // Act
            var actual = TournamentViewModel.Map(tournament);

            // Assert
            TestHelper.AreEqual<TournamentViewModel>(expected, actual, new TournamentViewModelComparer());
        }
    }
}