namespace lerve.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open lerve.Services.LispService
open lerve.Services.LispService.Lexer
open lerve.Services.LispService.Parser
open lerve.Services.CommonService.Result
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
            match lexer req.source <*> parser with
            | Ok(tokens) -> ActionResult<BaseModel<Token>>(base.Ok(Success tokens))
            | Error(err) -> ActionResult<BaseModel<Token>>(base.BadRequest(Failure {ErrorRecord.Message = err; ErrorRecord.Source = base.Url.PageLink()}))
        else
            ActionResult<BaseModel<Token>>(base.BadRequest())
