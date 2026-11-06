
open SqlBuilder.Core
// example usage of the SqlBuilder library to construct and render SQL queries.
let includeName = true
let id = { Name = "a.Id"; Alias = None }
let name = { Name = "a.Name"; Alias = Some "AttendeeName" }
let fields = if includeName then [ id; name ] else [ id ]
let Joins =
    [ { Kind = Inner
        Table = "EventAttendee"
        Alias = Some "ea"
        LeftColumn = "a.Id"
        RightColumn = "ea.AttendeeId" }

      {  Kind = FullOuter
         Table = "Events"
         Alias = Some "e"
         LeftColumn = "ea.EventId"
         RightColumn = "e.Id" }
         ]


let query =
    { Table = "Attendee"
      Fields = []
      TableAlias = Some "a"
      Joins = Joins
      Where = 
        Some (
            And (
                Equal("a.Id", Number 42),
                Or (
                   Equal ("a.Name", Text "bob"),
                   Equal ("e.Important", Number 1))))}


let eventsQuery =
    { Table = "Events"
      TableAlias = Some "e"
      Fields =
          [ { Name = "e.Id"; Alias = Some "EventId" }
            { Name = "a.Name"; Alias = Some "AttendeeName" } ]
      Joins =
          [ { Kind = Inner
              Table = "EventAttendee"
              Alias = Some "ea"
              LeftColumn = "e.Id"
              RightColumn = "ea.EventId" }
            { Kind = Inner
              Table = "Attendee"
              Alias = Some "a"
              LeftColumn = "ea.AttendeeId"
              RightColumn = "a.Id" } ]
      Where =
          Some (
              Or (
                  Equal ("a.Name", Text "bob"),
                  Equal ("e.Important", Number 1)
              )
          ) }


    
printfn "%s" (toSql query)
printfn "%s" (toSql eventsQuery)

