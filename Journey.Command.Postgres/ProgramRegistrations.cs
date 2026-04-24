using Journey.Postgres;

namespace Journey.Command;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyPostgresRegistration.Register();
    }
}
