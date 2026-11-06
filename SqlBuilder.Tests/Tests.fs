module SqlBuilder.Tests.QueryTests

open Xunit
open SqlBuilder.Core

[<Fact>]
let ``An empty field list selects all columns`` () =
    let query =
        { Table = "Attendee"
          Fields = []
          TableAlias = None
          Joins = []
          Where = None }

    let actual = toSql query

    Assert.Equal<string>("SELECT * FROM [Attendee];", actual)

[<Fact>]
let ``Nested conditions preserve their grouping`` () =
    let filter =
        And (
            Equal ("a.Id", Number 42),
            Or (
                Equal ("a.Name", Text "bob"),
                Equal ("a.Name", Text "Alice")
            )
        )

    let query =
        { Table = "Attendee"
          Fields = []
          TableAlias = Some "a"
          Joins = []
          Where = Some filter }

    let expected =
        "SELECT * FROM [Attendee] AS [a]\nWHERE (a.Id = 42 AND (a.Name = 'bob' OR a.Name = 'Alice'));"

    let actual = toSql query

    Assert.Equal<string>(expected, actual)
[<Fact>]
let ``Joins render in the order supplied`` () =
    let query =
        { Table = "Attendee"
          Fields = []
          TableAlias = Some "a"
          Joins =
              [ { Kind = Inner
                  Table = "EventAttendee"
                  Alias = Some "ea"
                  LeftColumn = "a.Id"
                  RightColumn = "ea.AttendeeId" }
                { Kind = FullOuter
                  Table = "Events"
                  Alias = Some "e"
                  LeftColumn = "ea.EventId"
                  RightColumn = "e.Id" } ]
          Where = None }

    let expected =
        [ "SELECT * FROM [Attendee] AS [a]"
          "INNER JOIN [EventAttendee] AS [ea] ON a.Id = ea.AttendeeId"
          "FULL OUTER JOIN [Events] AS [e] ON ea.EventId = e.Id;" ]
        |> String.concat "\n"

    Assert.Equal<string>(expected, toSql query)

[<Fact>]
let ``Table names with spaces are quoted`` () =
    let query =
        { Table = "Order Details"
          Fields = []
          TableAlias = None
          Joins = []
          Where = None }

    Assert.Equal<string>("SELECT * FROM [Order Details];", toSql query)