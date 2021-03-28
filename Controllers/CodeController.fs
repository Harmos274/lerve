namespace lerve.Controllers

open System
open FSharp.Json
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open lerve.Services.LispService

[<ApiController>]
[<Route("[controller]")>]
type CodeController (logger : ILogger<CodeController>) =
    inherit ControllerBase()
    
    [<HttpGet>]
    member _.Get() =
        match lexer """aazeazezaea (define toto a b (a + b))""" with
        | Ok(tokens) -> ActionResult<obj>(base.Ok(Json.serialize tokens))
        | Error(err)  ->  ActionResult<obj>(base.BadRequest err) 
    
    [<HttpPost>]
    member _.Post() = "titi"
    