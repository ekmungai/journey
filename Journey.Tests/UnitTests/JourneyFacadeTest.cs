using Journey.Interfaces;
using Moq;

namespace Journey.Tests.UnitTests;

public class JourneyFacadeTest {
    [Fact]
    public async Task Init_WithCommandTimeout_AppliesItToTheDatabase() {
        var database = new Mock<IDatabase>();
        // Stop Init as soon as the facade hands over to the migrator; only the setup matters here.
        database.Setup(d => d.CheckDatabase()).ThrowsAsync(new InvalidOperationException("stop"));
        JourneyFacade.RegisterDatabase("command-timeout-probe", (_, _) => Task.FromResult(database.Object));

        var journey = new JourneyFacade("command-timeout-probe", "", "", null, false) { CommandTimeout = 0 };
        await Assert.ThrowsAsync<InvalidOperationException>(() => journey.Init(true));

        database.Verify(d => d.SetCommandTimeout(0), Times.Once);
    }

    [Fact]
    public async Task Init_WithoutCommandTimeout_LeavesTheDriverDefault() {
        var database = new Mock<IDatabase>();
        database.Setup(d => d.CheckDatabase()).ThrowsAsync(new InvalidOperationException("stop"));
        JourneyFacade.RegisterDatabase("command-timeout-default-probe", (_, _) => Task.FromResult(database.Object));

        var journey = new JourneyFacade("command-timeout-default-probe", "", "", null, false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => journey.Init(true));

        database.Verify(d => d.SetCommandTimeout(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Init_WithNegativeCommandTimeout_Throws() {
        var journey = new JourneyFacade("unregistered", "", "", null, false) { CommandTimeout = -1 };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => journey.Init(true));
    }
}
