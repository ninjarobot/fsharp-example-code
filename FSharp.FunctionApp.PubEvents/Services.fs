namespace FSharp.FunctionApp.PubEvents.Services

open System
open System.IO
open Microsoft.AspNetCore.Http
open System.Collections.Generic
open Microsoft.Azure.EventGrid
open Microsoft.Azure.EventGrid.Models
open Newtonsoft.Json

module EventGridService =
    open FSharp.FunctionApp.PubEvents.Domain.RequestModels
    open FSharp.FunctionApp.PubEvents.Domain.EventGridTypes

    let egDataVersion = Environment.GetEnvironmentVariable("EventGridDataVersion")

    let toOption input =
        match (String.IsNullOrWhiteSpace input) with 
        | false -> Some(input)
        | _  -> None

    let createEventGridEvent (info: EventInfo) (a: 'a) = 
         let (et, sub, EventId id) = info
         let ege = new EventGridEvent()
         let data = upcast a : obj 
         ege.Id <- match id with Some(s) -> s | None -> Guid.NewGuid().ToString()
         ege.EventTime <- DateTime.UtcNow
         ege.EventType <- match et with EventType s -> s
         ege.Subject <- match sub with EventSubject s -> s
         ege.Data <- data
         ege.DataVersion <- egDataVersion
         ege

    let publishEvent (topicCreds: KeyHost) (eventRequest: EventRequest) =
        async {
            let (TopicKey key, TopicHostName host) = topicCreds
            let et = EventType eventRequest.EventType
            let es  = EventSubject eventRequest.EventSubject
            let ei = EventId (eventRequest.EventId |> toOption)
            let info =  EventInfo (et, es,  ei)
            let eventGridEvent = eventRequest.Body |> createEventGridEvent info
            try
                let eventGridEvents = new List<EventGridEvent>()
                eventGridEvents.Add(eventGridEvent)
                use client = new EventGridClient(new TopicCredentials(key)) 
                let! response = client.PublishEventsWithHttpMessagesAsync(host, eventGridEvents) |> Async.AwaitTask
                return Ok(response)
            with 
                | _-> return Error("Something went wrong publishing event to EventGrid")
        }

module PublishEventService =
    open FSharp.FunctionApp.PubEvents.Domain.RequestModels
    open FSharp.FunctionApp.PubEvents.Domain.EventGridTypes
    open EventGridService

    let deserialize (req: HttpRequest) =
        use requestBody =  new StreamReader(req.Body)
        let body = requestBody.ReadToEndAsync() |> Async.AwaitTask |> Async.RunSynchronously
        JsonConvert.DeserializeObject<EventRequest>(body)
     
    let publishEvent (req: HttpRequest) =
        let topicKey = req.Headers.Item("Topic-Key").ToString()
        let topicHost = req.Headers.Item("Topic-Host").ToString()
        let topicCreds  = (TopicKey topicKey, TopicHostName topicHost)
        req  |> deserialize |> publishEvent topicCreds |> Async.RunSynchronously 
    

        



