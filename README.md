# Welcome to Journey 
[![.NET](https://github.com/ekmungai/journey/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ekmungai/journey/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Journey is a script based database migration tool that treats a migration a journey, a collection of steps taken from the beginning (origin) state of the database to the final (destination) state.

## Why Journey?

### Code/Database Version decoupling
Most if not all migration tools available make the assumption that the version of the database will always be in sync with, and therefore should be updated in tandem with changes to the code using version control. This assumption results with all migrations up to the latest being applied whenever the source code is deployed. While this might be sufficient for a majority of cases, there are circumstances where the code goes out of sync with the database at which point the one can either make another deployment to rollback the database or update the code to a version compatible with it. In either case CI pipelines can make this process long and cumbersome. 

Using stepwise migration decouples the deployment of the code versions from the database allowing for surgically precise database versioning via configuration rather than source control.    

### Enhanced Developer Experience
By allowing them to up and downgrade their local database to any step in the migration journey, the tool allows fine control over the data in contact with the sections of code that each developer/team is working on, while at the same time still maintaining sync across the organization since all migration scripts are checked into version control. This can be particularly useful when revisiting legacy code for refactoring/optimization. 

### In Built Atomicity
Each step is prepared in a standard format that ensures that both migrations and rollbacks are atomic. To assist with this, the tool provides a scaffold mode that produces a template with all the required sections prefilled and only awaiting the queries for making the actual changes to the database. 

Each version file is validated before execution which when combined with a dry run mode which both applies the migration and rolls it back immediately, the tool provides a layer of safety however limited against irrecoverable damage to the database. 

## Supported Databases
 - Sql Databases
    - Postgres
        - CoackroachDb
        - TimescaleDb
    - Mysql
        - MariaDb
    - Mssql
    - Sqlite
 - NoSql Databases
    - Cassandra

## Usage

### Production
To make use of Journey in an .NET production application, install it as a Nuget package:
```bash
Install-Package Journey
```
Then during the startup of the application, add the following code:
```c#
# initialize the library
var journey = new JourneyFacade(databaseType, connectionString, versionsDirectory); 
# initialize migrations (in quiet mode) in case its the first time
await journey.init(true);
# sync the database
await journey.Update();
```
This will sync the database to the highest version available in the `versionsDirectory`. If you'd like to migrate up/down to a specific version, you can provide the target version number to the update function. 

#### Logging
By default, journey logs messages to the console using its internal logger. You can provide a custom logger by calling the `UseSerilogLogging` and  UseMicrosoftLogging`methods just before caling the update method.
```c#
# use a pre initialized serilog logger
journey.UseSerilogLogging(logger);
# use a pre initialized microsoft logger/logger factory
journey.UseSerilogLogging(logger);
# Or
journey.UseSerilogLogging(loggerFactory);
```

```c#
await journey.Update(versionNumber);
```

### Development
For local development, the Journey.Command tool is best suited. You can download it from the [releases](https://github.com/ekmungai/journey/releases) page.

#### Scaffolding
The first step is to prepare the file version of the database, which you do by running the `scaffold` command.

```bash
journey scaffold -p "path\to\versions-dir" -d sqlite -c "Data Source=journal.db"
```
This will create a template with the sections required by the journey tool pre filled, as well as instructions on how to put in custom queries for your migration.

NB: When running the tool against a database for the first time, you'll be prompted to confirm that you wish to initialize journey migrations on it. Confirming the prompt will create and apply a migration file that sets up the versions table.

#### Validation
After you've entered your queries in the migration and rollback section, the next section is to verify that your new migration file is actually runnable by the tool. You do this by running the validate command.
```bash
journey validate -p "path\to\versions-dir" -d sqlite -c "Data Source=journal.db"
```
Any errors found in the document will be reported together with the exact line number where the error occurs.

#### Migrating 
Now that we have valid migration file, the next step is to apply it. We accomlpish this with the migrate command.
```bash
journey migrate -p "path\to\versions-dir" -d sqlite -c "Data Source=journal.db"
```

#### Rollback 
In case the changes applied are not what we expected, we can reset the database to what it was before the migration using the rollback command. 
```bash
journey rollback -p "path\to\versions-dir" -d sqlite -c "Data Source=journal.db"
```

#### Update 
To sync the database to the highest version in the verisons directory, run the update command.
```bash
journey update -p "path\to\versions-dir" -d sqlite -c "Data Source=journal.db"
```

#### History 
If you want to view the metatdata of the migrations applied to the database up to the current version, run the history command.
```bash
journey history -p "path\to\versions-dir" -d sqlite -c "Data Source=journal.db"
```

## Documentation
You can read extensive documentation about Journey [here](https://https://ekmungai.github.io/journey-docs/).

## Changelog
Details of changes made are documented for each release.