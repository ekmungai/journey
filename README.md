# Welcome to Journey 
[![.NET](https://github.com/ekmungai/journey/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ekmungai/journey/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Journey is a script based database migration tool designed on the idea that a migration is a specialized kind of journey, and since every journey is a collection of steps it emphasizes on the steps taken from the beginning (origin) state of the database to the final (destination) state.

## Why Journey?

### Code/Database Version decoupling
Most if not all migration tools available make the assumption that the version of the database will always be in sync with, and can therefore should be updated in tandem with changes to the code using version control. This assumption results with all migrations up to the latest being applied whenever the source code is deployed. While this might be sufficient for a majority of cases, there are circumstances where the code goes out of sync with the database at which point the only recourse is to either make another deployment to rollback the database or update the code to a version compatible with it. In wither case CI pipelines can make this process long and cumbersome. 

Using stepwise migration decouples the deployment of the code from the versioning of the database allowing for surgically precise database versioning via configuration rather than source control.    

### Enhanced Developer Experience
By allowing them to up and downgrade their local database to any step in the migration journey, the tool allows fine control over the data in contact with the sections of code that each developer/team is working on, while at the same time still maintaining sync across the organization since all migration scripts are checked into version control. This can be particlarly useful when revisiting legacy code to be refactored/optimsed. 

### Enforced Atomicity
Each step is prepared in a standard format that ensures that both migrations and rollbacks are atomic. To assist with this, the tool provides a scaffold mode that produces a template with all the required sections prefilled and only awaiting the queries for making the actual changes to the database. 

Each version file is validated before execution which wehn combined with a dry run mode which both applies the migration and rolls it back immediately, the tool provides a layer of safety however limited against irrecoverable damage to the database. 

## Usage
You can make use of Journey my referencing it as a Nuget package, or downloading the Command tool.

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

## Documentation
You can read extensive documentation about Journey on readthedocs.io.

## Changelog
Details of changes made are documented for each release.