using System.Diagnostics.CodeAnalysis;
using VolleyManagement.Domain.TournamentsAggregate;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Tournaments;
using VolleyManagement.UnitTests.Services.TournamentService;
using Xunit;

namespace VolleyManagement.UnitTests.Mvc.ViewModels
{
    /// <summary>
    ///     Tests for DomainToViewModel class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TournamentDomainToViewModel
    {
        /// <summary>
        ///     Test for Map() method.
        ///     The method should map tournament domain model to view model.
        /// </summary>
        [Fact]
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
            TournamentViewModelComparer.AssertAreEqual(expected, actual);
        }
    }
}