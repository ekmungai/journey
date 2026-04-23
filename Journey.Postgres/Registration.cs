using System.Runtime.CompilerServices;
using PgDb = Journey.Databases.Postgres;
using TsDb = Journey.Databases.TimescaleDb;
using CrDb = Journey.Databases.CockroachDb;

namespace Journey.Postgres;

public static class JourneyPostgresRegistration {
    [ModuleInitializer]
    internal static void AutoRegister() => Register();

    public static void Register() {
        JourneyFacade.RegisterDatabase(
            PgDb.Name,
            async (cs, schema) => await new PgDb().Connect(cs, schema ?? "public"));
        JourneyFacade.RegisterDatabase(
            TsDb.Name,
            async (cs, schema) => await new TsDb().Connect(cs, schema ?? "public"));
        JourneyFacade.RegisterDatabase(
            CrDb.Name,
            async (cs, schema) => await new CrDb().Connect(cs, schema ?? "public"));
    }
}
