using AVP.Services;
using AVP.ViewModels;
using Moq;
using Xunit;
using System;

namespace AVP.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly Mock<IMediaPlayerService> _mockMediaPlayerService;
    private readonly Mock<IOscService> _mockOscService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;

    public MainViewModelTests()
    {
        _mockMediaPlayerService = new Mock<IMediaPlayerService>();
        _mockOscService = new Mock<IOscService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
    }

    [Fact]
    public void Constructor_WhenOscServiceStartsSuccessfully_SetsIsOscRunningToTrue()
    {
        // Arrange
        _mockOscService.Setup(s => s.Start(It.IsAny<int>())).Returns(true);

        // Act
        var viewModel = new MainViewModel(
            _mockMediaPlayerService.Object,
            _mockOscService.Object,
            _mockServiceProvider.Object);

        // Assert
        Assert.True(viewModel.IsOscRunning);
        _mockOscService.Verify(s => s.Start(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void Constructor_WhenOscServiceFailsToStart_SetsIsOscRunningToFalse()
    {
        // Arrange
        _mockOscService.Setup(s => s.Start(It.IsAny<int>())).Returns(false);

        // Act
        var viewModel = new MainViewModel(
            _mockMediaPlayerService.Object,
            _mockOscService.Object,
            _mockServiceProvider.Object);

        // Assert
        Assert.False(viewModel.IsOscRunning);
        _mockOscService.Verify(s => s.Start(It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void StartOscCommand_WhenServiceStartsSuccessfully_SetsIsOscRunningToTrue()
    {
        // Arrange
        _mockOscService.Setup(s => s.Start(It.IsAny<int>())).Returns(false);

        var viewModel = new MainViewModel(
            _mockMediaPlayerService.Object,
            _mockOscService.Object,
            _mockServiceProvider.Object);

        Assert.False(viewModel.IsOscRunning);

        _mockOscService.Setup(s => s.Start(It.IsAny<int>())).Returns(true);

        // Act
        viewModel.StartOscCommand.Execute(null);

        // Assert
        Assert.True(viewModel.IsOscRunning);
        _mockOscService.Verify(s => s.Start(It.IsAny<int>()), Times.Exactly(2));
    }

    [Fact]
    public void StartOscCommand_WhenServiceFailsToStart_DoesNotSetIsOscRunningToTrue()
    {
        // Arrange
        _mockOscService.Setup(s => s.Start(It.IsAny<int>())).Returns(false);

        var viewModel = new MainViewModel(
            _mockMediaPlayerService.Object,
            _mockOscService.Object,
            _mockServiceProvider.Object);

        Assert.False(viewModel.IsOscRunning);

        _mockOscService.Setup(s => s.Start(It.IsAny<int>())).Returns(false);

        // Act
        viewModel.StartOscCommand.Execute(null);

        // Assert
        Assert.False(viewModel.IsOscRunning);
        _mockOscService.Verify(s => s.Start(It.IsAny<int>()), Times.Exactly(2));
    }
}
