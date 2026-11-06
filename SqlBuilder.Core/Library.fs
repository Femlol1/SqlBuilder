module SqlBuilder.Core

type JoinKind =
    | Inner
    | LeftOuter
    | RightOuter
    | FullOuter
/// Joins a table by matching two column references, such as e.Id and ea.EventId.
type Join =
    { Kind: JoinKind
      Table: string
      Alias: string option
      LeftColumn: string
      RightColumn: string }


type SelectField =
    { Name: string
      Alias: string option }

type SqlValue =
    | Text of string
    | Number of int

type Condition =
    | Equal of column: string * value: SqlValue
    | And of Condition * Condition
    | Or of Condition * Condition

type SelectQuery =
    { Table: string
      Fields: SelectField list
      TableAlias: string option
      Joins: Join list
      Where: Condition option }

let renderField (field: SelectField) =
    match field.Alias with
    | Some alias -> $"{field.Name} AS {alias}"
    | None -> field.Name

// Escape closing brackets inside SQL Server identifiers.
let private quoteIdentifier (name: string) =
    "[" + name.Replace("]", "]]") + "]"


// Double apostrophes so names like O'Neil render correctly.
let private renderTable name alias =
    let tableName = quoteIdentifier name

    match alias with
    | Some value -> $"{tableName} AS {quoteIdentifier value}"
    | None -> tableName

let renderJoin (join: Join) =
    let joinType =
        match join.Kind with
        | Inner -> "INNER JOIN"
        | LeftOuter -> "LEFT OUTER JOIN"
        | RightOuter -> "RIGHT OUTER JOIN"
        | FullOuter -> "FULL OUTER JOIN"
    let table = renderTable join.Table join.Alias
    $"{joinType} {table} ON {join.LeftColumn} = {join.RightColumn}"


let renderValue value =
    match value with
    | Text text -> "'" + text.Replace("'", "''") + "'"
    | Number number -> sprintf "%d" number

// Parenthesise AND and OR groups to preserve their meaning.
let rec renderCondition condition =
    match condition with
    | Equal (column, value) -> $"{column} = {renderValue value}"
    | And (left, right) -> $"({renderCondition left} AND {renderCondition right})"
    | Or (left, right) -> $"({renderCondition left} OR {renderCondition right})"


/// Converts a query record into a SQL Server SELECT statement.
let toSql (query: SelectQuery) =
    // An empty field list means SELECT *.
    let columns =
        match query.Fields with
        | [] -> "*"
        | fields -> fields |> List.map renderField |> String.concat ", "

    let tableName = quoteIdentifier query.Table

    let fromClause = $"FROM {renderTable query.Table query.TableAlias}"

    let selectLine = $"SELECT {columns} {fromClause}"
    // Preserves join order because a later join can use an earlier alias.
    let joinLines = query.Joins |> List.map renderJoin

    let whereLines =
        match query.Where with
        | Some condition -> [ $"WHERE {renderCondition condition}" ]
        | None -> []

    let lines = (selectLine :: joinLines) @ whereLines
    String.concat "\n" lines + ";"


