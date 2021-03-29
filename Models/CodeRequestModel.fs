namespace lerve.Models

[<CLIMutable>]
type CodeRequestModel = { source: string } with
    member this.isValid = this.source = null |> not
    