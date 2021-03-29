namespace lerve.Models

type ErrorRecord = { Source: string; Message: string; }

type BaseModel<'a> =
    | Success of 'a
    | Failure of ErrorRecord