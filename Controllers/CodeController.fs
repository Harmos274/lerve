namespace lerve.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open lerve.Services.LispService.EvaluatorDomain
open lerve.Services.LispService.LexerDomain.Lexer
open lerve.Services.LispService.ParserDomain.Parser
open lerve.Services.CommonService.Result
open lerve.Services.LispService.EvaluatorDomain.Evaluator
open lerve.Models

[<ApiController>]
[<Route("[controller]")>]
type CodeController (logger : ILogger<CodeController>) =
    inherit ControllerBase()
    
    [<HttpGet>]
    member _.Get() = base.NotFound(Failure {ErrorRecord.Message = "Not Found"; ErrorRecord.Source = base.Url.PageLink()})
    
    [<HttpPost>]
    member _.Post(req: CodeRequestModel) =
        if req.isValid then
            match lexer req.source <*> parser <*> evaluator with
            | Ok(values) -> ActionResult<BaseModel<Value>>(base.Ok(Success values))
            | Error(err) -> ActionResult<BaseModel<Value>>(base.BadRequest(Failure {ErrorRecord.Message = err; ErrorRecord.Source = base.Url.PageLink()}))
        else
            ActionResult<BaseModel<Value>>(base.BadRequest())
