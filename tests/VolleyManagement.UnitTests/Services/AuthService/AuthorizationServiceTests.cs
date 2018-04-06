﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Moq;
using VolleyManagement.Contracts.Authorization;
using VolleyManagement.Contracts.Exceptions;
using VolleyManagement.Data.Contracts;
using VolleyManagement.Data.Queries.Common;
using VolleyManagement.Domain.RolesAggregate;
using VolleyManagement.Services.Authorization;
using Xunit;

namespace VolleyManagement.UnitTests.Services.AuthService
{
    /// <summary>
    ///     Tests <see cref="IAuthorizationService" /> implementation
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AuthorizationServiceTests
    {
        public AuthorizationServiceTests()
        {
            _getByIdQueryMock = new Mock<IQuery<ICollection<AuthOperation>, FindByUserIdCriteria>>();
            _currentUserService = new Mock<ICurrentUserService>();
        }

        private const byte AREA_1_ID = 1;
        private const byte AREA_2_ID = 2;
        private const int OPERATION_1_ID = 1;
        private const int OPERATION_2_ID = 2;
        private const int OPERATION_3_ID = 3;
        private const int OPERATION_4_ID = 4;

        private const byte BYTE_SIZE_SHIFT = 8;

        private readonly Mock<IQuery<ICollection<AuthOperation>, FindByUserIdCriteria>> _getByIdQueryMock;
        private readonly Mock<ICurrentUserService> _currentUserService;
        private readonly Type[] _authOperationsAreas = typeof(AuthOperations).GetNestedTypes();

        private AuthorizationService BuildSUT()
        {
            return new AuthorizationService(_currentUserService.Object, _getByIdQueryMock.Object);
        }

        private void MockAllowedOperations(List<AuthOperation> operations)
        {
            _getByIdQueryMock.Setup(q => q.Execute(It.IsAny<FindByUserIdCriteria>())).Returns(operations);
        }

        [Fact]
        public void AuthOperation_DifferenAreaIdSameOperationId_OperationsNotEquals()
        {
            // Arrange
            AuthOperation operation1 = Tuple.Create(AREA_1_ID, OPERATION_1_ID);
            AuthOperation operation2 = Tuple.Create(AREA_2_ID, OPERATION_1_ID);

            // Act
            var result = operation1 == operation2;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthOperation_SameAreaIdDifferentOperationId_OperationsNotEquals()
        {
            // Arrange
            AuthOperation operation1 = Tuple.Create(AREA_1_ID, OPERATION_1_ID);
            AuthOperation operation2 = Tuple.Create(AREA_1_ID, OPERATION_2_ID);

            // Act
            var result = operation1 == operation2;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AuthOperation_SameAreaIdOperationId_OperationsEquals()
        {
            // Arrange
            AuthOperation operation1 = Tuple.Create(AREA_1_ID, OPERATION_1_ID);
            AuthOperation operation2 = Tuple.Create(AREA_1_ID, OPERATION_1_ID);

            // Act
            var result = operation1 == operation2;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void AuthOperations_AllAreas_AllAreasHasDifferentId()
        {
            // Arrange
            var processedAreas = new Dictionary<byte, string>();
            var errors = string.Empty;

            // Act
            foreach (var area in _authOperationsAreas)
            {
                var areaOperations = area.GetFields();
                var areaName = area.Name;
                foreach (var operation in areaOperations)
                {
                    short operationId = (AuthOperation) operation.GetValue(null);
                    var areaId = (byte) (operationId >> BYTE_SIZE_SHIFT);
                    if (processedAreas.ContainsKey(areaId))
                    {
                        if (processedAreas[areaId] != areaName)
                        {
                            errors += string.Format("{0} → {1}; \n", areaName, operation.Name);
                        }
                    }
                    else
                    {
                        processedAreas.Add(areaId, areaName);
                    }
                }
            }

            // Assert
            errors.Should().BeNullOrEmpty(errors);
        }

        [Fact]
        public void AuthOperations_AllAreas_AllOperationsInAreaHasDifferentId()
        {
            // Arrange
            var errors = string.Empty;

            // Act
            foreach (var area in _authOperationsAreas)
            {
                var areaOperations = area.GetFields();
                var areaName = area.Name;
                var processed = new List<int>();
                foreach (var operation in areaOperations)
                {
                    var operationId = (AuthOperation) operation.GetValue(null);
                    var operationKeyId = (byte) ((operationId << BYTE_SIZE_SHIFT) >> BYTE_SIZE_SHIFT);

                    if (processed.Contains(operationKeyId))
                    {
                        errors += string.Format("{0} → {1}; \n", areaName, operation.Name);
                    }
                    else
                    {
                        processed.Add(operationKeyId);
                    }
                }
            }

            // Assert
            errors.Should().BeNullOrEmpty(errors);
        }

        [Fact]
        public void AuthOperations_AllAreas_AllOperationsInAreaHasSameAreaId()
        {
            // Arrange
            var errors = string.Empty;

            // Act
            foreach (var area in _authOperationsAreas)
            {
                var areaOperations = area.GetFields();
                var areaName = area.Name;
                byte? areaId = null;
                foreach (var operation in areaOperations)
                {
                    short operationId = (AuthOperation) operation.GetValue(null);
                    var operationAreaId = (byte) (operationId >> BYTE_SIZE_SHIFT);

                    if (areaId.HasValue && areaId.Value != operationAreaId)
                    {
                        errors += string.Format("{0} → {1}; \n", areaName, operation.Name);
                    }
                    else
                    {
                        areaId = operationAreaId;
                    }
                }
            }

            // Assert
            errors.Should().BeNullOrEmpty(errors);
        }

        [Fact]
        public void CheckAccess_NoOperationsPermitted_AuthorizationExceptionThrown()
        {
            // Arrange
            var operationToCheck = Tuple.Create(AREA_1_ID, OPERATION_1_ID);
            var allowedOperations = new List<AuthOperation>();
            MockAllowedOperations(allowedOperations);

            var service = BuildSUT();

            // Act
            Action act = () => service.CheckAccess(operationToCheck);

            // Assert
            act.Should().Throw<AuthorizationException>();
        }

        [Fact]
        public void CheckAccess_OperationNotPermitted_AuthorizationExceptionThrown()
        {
            // Arrange
            var allowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID),
                Tuple.Create(AREA_1_ID, OPERATION_3_ID)
            };

            MockAllowedOperations(allowedOperations);

            var operationToCheck = Tuple.Create(AREA_1_ID, OPERATION_4_ID);
            var service = BuildSUT();

            // Act
            Action act = () => service.CheckAccess(operationToCheck);

            // Assert
            act.Should().Throw<AuthorizationException>();
        }

        [Fact]
        public void CheckAccess_OperationPermitted_AuthorizationExceptionNotThrown()
        {
            // Arrange
            var allowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID),
                Tuple.Create(AREA_1_ID, OPERATION_3_ID)
            };

            MockAllowedOperations(allowedOperations);

            var operationToCheck = Tuple.Create(AREA_1_ID, OPERATION_2_ID);
            var service = BuildSUT();

            // Act
            service.CheckAccess(operationToCheck);
        }

        [Fact]
        public void GetAllowedOperations_AllAllowedOperationsSpecified_AllAllowed()
        {
            // Arrange
            var allAllowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID),
                Tuple.Create(AREA_1_ID, OPERATION_3_ID)
            };
            MockAllowedOperations(allAllowedOperations);

            var requestedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID),
                Tuple.Create(AREA_1_ID, OPERATION_3_ID)
            };

            var service = BuildSUT();

            // Act
            var allowedOperations = service.GetAllowedOperations(requestedOperations);

            // Assert
            foreach (var item in requestedOperations)
            {
                allowedOperations.IsAllowed(item).Should().BeTrue();
            }
        }

        [Fact]
        public void GetAllowedOperations_EmptyListSpecified_NoOneAllowed()
        {
            // Arrange
            var allAllowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID)
            };
            MockAllowedOperations(allAllowedOperations);

            var requestedOperations = new List<AuthOperation>();
            var service = BuildSUT();

            // Act
            var allowedOperations = service.GetAllowedOperations(requestedOperations);

            // Assert
            foreach (var item in allAllowedOperations)
            {
                allowedOperations.IsAllowed(item).Should().BeFalse();
            }
        }

        [Fact]
        public void GetAllowedOperations_NotAllAllowedOperationsSpecified_OnlySpecifiedAllowed()
        {
            // Arrange
            var operationToCheck = Tuple.Create(AREA_1_ID, OPERATION_1_ID);
            var allAllowedOperations = new List<AuthOperation> {
                operationToCheck,
                Tuple.Create(AREA_1_ID, OPERATION_2_ID),
                Tuple.Create(AREA_1_ID, OPERATION_3_ID)
            };
            MockAllowedOperations(allAllowedOperations);

            var requestedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_2_ID),
                Tuple.Create(AREA_1_ID, OPERATION_3_ID)
            };

            var service = BuildSUT();

            // Act
            var allowedOperations = service.GetAllowedOperations(requestedOperations);

            // Assert
            allowedOperations.IsAllowed(operationToCheck).Should().BeFalse();
            foreach (var item in requestedOperations)
            {
                allowedOperations.IsAllowed(item).Should().BeTrue();
            }
        }

        [Fact]
        public void GetAllowedOperations_NotAllowedOperationSpecified_NoOneAllowed()
        {
            // Arrange
            var allAllowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID)
            };
            MockAllowedOperations(allAllowedOperations);
            var requestedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_3_ID),
                Tuple.Create(AREA_1_ID, OPERATION_4_ID)
            };

            var service = BuildSUT();

            // Act
            var allowedOperations = service.GetAllowedOperations(requestedOperations);

            // Assert
            foreach (var item in allAllowedOperations)
            {
                allowedOperations.IsAllowed(item).Should().BeFalse();
            }

            foreach (var item in requestedOperations)
            {
                allowedOperations.IsAllowed(item).Should().BeFalse();
            }
        }

        [Fact]
        public void GetAllowedOperations_NotAllowedOperationsSpecified_NoOperationsAllowed()
        {
            // Arrange
            var allAllowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID)
            };
            MockAllowedOperations(allAllowedOperations);

            var requestedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_3_ID),
                Tuple.Create(AREA_1_ID, OPERATION_4_ID)
            };

            var service = BuildSUT();

            // Act
            var allowedOperations = service.GetAllowedOperations(requestedOperations);

            // Assert
            foreach (var item in allAllowedOperations)
            {
                allowedOperations.IsAllowed(item).Should().BeFalse();
            }

            foreach (var item in requestedOperations)
            {
                allowedOperations.IsAllowed(item).Should().BeFalse();
            }
        }

        [Fact]
        public void GetAllowedOperations_NullListSpecified_ArgumentNullExceptionThrown()
        {
            // Arrange
            var allAllowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID)
            };
            MockAllowedOperations(allAllowedOperations);

            List<AuthOperation> requestedOperations = null;
            var service = BuildSUT();

            // Act
            Action act = () => service.GetAllowedOperations(requestedOperations);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetAllowedOperations_NullSpecified_ArgumentNullExceptionThrown()
        {
            // Arrange
            var allAllowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID)
            };
            MockAllowedOperations(allAllowedOperations);

            AuthOperation requestedOperations = null;
            var service = BuildSUT();

            // Act
            Action act = () => service.GetAllowedOperations(requestedOperations);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetAllowedOperations_OneAllowedOperationSpecified_OnlySpecifiedAllowed()
        {
            // Arrange
            var allAllowedOperations = new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID),
                Tuple.Create(AREA_1_ID, OPERATION_3_ID)
            };
            MockAllowedOperations(allAllowedOperations);

            var requestedOperation = Tuple.Create(AREA_1_ID, OPERATION_2_ID);
            var service = BuildSUT();

            // Act
            var allowedOperations = service.GetAllowedOperations(requestedOperation);

            // Assert
            allowedOperations.IsAllowed(Tuple.Create(AREA_1_ID, OPERATION_1_ID)).Should().BeFalse();
            allowedOperations.IsAllowed(Tuple.Create(AREA_1_ID, OPERATION_3_ID)).Should().BeFalse();
            allowedOperations.IsAllowed(requestedOperation).Should().BeTrue();
        }

        [Fact]
        public void IsAllowed_NoOperationsPermitted_OperationIsNotAllowed()
        {
            // Arrange
            var operationToCheck = Tuple.Create(AREA_1_ID, OPERATION_3_ID);
            var allowedOperations = new AllowedOperations(new List<AuthOperation>());

            // Act
            var result = allowedOperations.IsAllowed(operationToCheck);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsAllowed_OperationNotPermitted_OperationIsNotAllowed()
        {
            // Arrange
            var operationToCheck = Tuple.Create(AREA_1_ID, OPERATION_3_ID);
            var allowedOperations = new AllowedOperations(new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID)
            });

            // Act
            var result = allowedOperations.IsAllowed(operationToCheck);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsAllowed_OperationPermitted_OperationIsAllowed()
        {
            // Arrange
            var operationToCheck = Tuple.Create(AREA_1_ID, OPERATION_1_ID);
            var allowedOperations = new AllowedOperations(new List<AuthOperation> {
                Tuple.Create(AREA_1_ID, OPERATION_1_ID),
                Tuple.Create(AREA_1_ID, OPERATION_2_ID)
            });

            // Act
            var result = allowedOperations.IsAllowed(operationToCheck);

            // Assert
            result.Should().BeTrue();
        }
    }
}