module SqlBuilder.Core

type JoinKind =
    | Inner
    | LeftOuter
    | RightOuter
    | FullOuter

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

let private quoteIdentifier (name: string) =
    "[" + name.Replace("]", "]]") + "]"

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

let rec renderCondition condition =
    match condition with
    | Equal (column, value) -> $"{column} = {renderValue value}"
    | And (left, right) -> $"({renderCondition left} AND {renderCondition right})"
    | Or (left, right) -> $"({renderCondition left} OR {renderCondition right})"



let toSql (query: SelectQuery) =
    let columns =
        match query.Fields with
        | [] -> "*"
        | fields -> fields |> List.map renderField |> String.concat ", "

    let tableName = quoteIdentifier query.Table

    let fromClause = $"FROM {renderTable query.Table query.TableAlias}"

    let selectLine = $"SELECT {columns} {fromClause}"
    let joinLines = query.Joins |> List.map renderJoin

    let whereLines =
        match query.Where with
        | Some condition -> [ $"WHERE {renderCondition condition}" ]
        | None -> []

    let lines = (selectLine :: joinLines) @ whereLines
    String.concat "\n" lines + ";"


