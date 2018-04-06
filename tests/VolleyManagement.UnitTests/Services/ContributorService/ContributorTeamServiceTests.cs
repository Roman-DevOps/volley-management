using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using VolleyManagement.Data.Contracts;
using VolleyManagement.Domain.ContributorsAggregate;
using VolleyManagement.Services;
using Xunit;

namespace VolleyManagement.UnitTests.Services.ContributorService
{
    /// <summary>
    ///     Tests for ContributorTeamServiceTests class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ContributorTeamServiceTests
    {
        /// <summary>
        ///     Initializes test data.
        /// </summary>
        public ContributorTeamServiceTests()
        {
            _contributorTeamRepositoryMock = new Mock<IContributorTeamRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _contributorTeamRepositoryMock.Setup(tr => tr.UnitOfWork).Returns(_unitOfWorkMock.Object);
        }

        private readonly ContributorTeamServiceTestFixture _testFixture = new ContributorTeamServiceTestFixture();
        private readonly Mock<IContributorTeamRepository> _contributorTeamRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private ContributorTeamService BuildSUT()
        {
            return new ContributorTeamService(_contributorTeamRepositoryMock.Object);
        }

        private void MockRepositoryFindAll(List<ContributorTeam> testData)
        {
            _contributorTeamRepositoryMock.Setup(tr => tr.Find()).Returns(testData);
        }

        private bool PlayersAreEqual(ContributorTeam x, ContributorTeam y)
        {
            return new ContributorTeamComparer().Compare(x, y) == 0;
        }

        /// <summary>
        ///     Test for Get() method. The method should return existing contributors teams
        /// </summary>
        [Fact]
        public void GetAll_ContributorsTeamExist_ContributorsTeamReturned()
        {
            // Arrange
            var testData = _testFixture.TestContributors()
                .Build();

            MockRepositoryFindAll(testData);

            var sut = BuildSUT();
            var expected = new ContributorTeamServiceTestFixture()
                .TestContributors()
                .Build();


            // Act
            var actual = sut.Get();

            // Assert
            TestHelper.AreEqual(expected, actual, new ContributorTeamComparer());
        }
    }
}