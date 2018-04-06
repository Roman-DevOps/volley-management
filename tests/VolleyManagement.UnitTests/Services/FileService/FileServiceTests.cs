using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Xunit;

namespace VolleyManagement.UnitTests.Services.FileService
{
    [ExcludeFromCodeCoverage]
    public class FileServiceTests
    {
        private VolleyManagement.Services.FileService BuildSUT()
        {
            return new VolleyManagement.Services.FileService();
        }

        [Fact]
        public void Delete_InvalidNullFile_FileNotFoundException()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            Action act = () => sut.Delete(null);

            //Assert
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void FileExists_NoFile_FileNotFound()
        {
            var expected = false;

            // Arrange
            var sut = BuildSUT();

            // Act
            var actual = sut.FileExists(null);

            // Assert
            actual.Should().Be(expected, "There is no file on server");
        }

        [Fact]
        public void Upload_InvalidNullFile_ArgumentException()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            Action act = () => sut.Upload(null, null);

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}