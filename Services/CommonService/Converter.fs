module lerve.Services.CommonService.Converter

open System

let tryParseInt (s: string): Option<int> =
    try s |> int |> Some with :? FormatException -> None