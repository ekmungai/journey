using Journey.Sqlite;

namespace Journey.Command;

internal partial class Program {
    static partial void RegisterDatabases() {
        JourneySqliteRegistration.Register();
    }
}
