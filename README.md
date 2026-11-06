# SqlBuilder

An F# library and console demo for generating SQL Server `SELECT` statements from query records. It supports dynamic field lists, aliases, inner and outer joins, and grouped `AND`/`OR` filters.

## Run and test

Use the .NET 10 SDK. In Visual Studio 2026, open the solution and run `SqlBuilder.Cli`. From the solution folder:

```powershell
dotnet run --project SqlBuilder.Cli
dotnet test SqlBuilder.Tests/SqlBuilder.Tests.fsproj
```

## Example

```sql
SELECT e.Id AS EventId, a.Name AS AttendeeName FROM [Events] AS [e]
INNER JOIN [EventAttendee] AS [ea] ON e.Id = ea.EventId
INNER JOIN [Attendee] AS [a] ON ea.AttendeeId = a.Id
WHERE (a.Name = 'bob' OR e.Important = 1);
```

The library returns SQL text. Table names are single identifiers, such as `Events` or `Order Details`.
