using Journey.Sqlite;

namespace Journey.Net;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneySqliteRegistration.Register();
    }
}
