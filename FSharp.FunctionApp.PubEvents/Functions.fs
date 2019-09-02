namespace FSharp.FunctionApp.PubEvents.Functions

open Microsoft.Azure.WebJobs
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging


module FunctionModule =
    open FSharp.FunctionApp.PubEvents.Services.PublishEventService

    [<FunctionName("pub-event")>]
    let runPublishOrderPlacedEvent([<HttpTrigger(AuthorizationLevel.Function, "post")>] req: HttpRequest, log: ILogger) =
        let response = req |> publishEvent
        match response with
        | Ok(_)  -> new OkObjectResult("The event was published successfully!") :> ObjectResult
        | Error(s) -> new BadRequestObjectResult(s) :> ObjectResult
        
