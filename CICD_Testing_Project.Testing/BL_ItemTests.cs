using CICD_Testing_Project.Api.Domain.Features.Item;
using CICD_Testing_Project.Api.Models.Features.Item;
using Moq;

namespace CICD_Testing_Project.Testing
{
    public class BL_ItemTests
    {
        [Fact]
        public async Task GetByIdTest()
        {
            // Arrange
            var mockDa = new Mock<IDA_Item>();
            mockDa.Setup(x => x.GetById(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ItemResponseModel { Id = 1, Name = "Test Item" });

            var bl = new BL_Item(mockDa.Object);

            // Act
            var result = await bl.GetById(1, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Item", result!.Name);
        }
    }
}
