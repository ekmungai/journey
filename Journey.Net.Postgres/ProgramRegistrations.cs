using Journey.Postgres;

namespace Journey.Net;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneyPostgresRegistration.Register();
    }
}
