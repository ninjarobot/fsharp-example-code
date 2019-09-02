namespace FSharp.FunctionApp.PubEvents.Domain

open System
open Newtonsoft.Json

module RequestModels =

    [<CLSCompliant(true)>]
    type EventRequest = {
        [<JsonProperty("eventType", Required = Required.Always)>] EventType: string
        [<JsonProperty("eventSubject", Required = Required.Always)>] EventSubject: string
        [<JsonProperty("eventId", Required = Required.Default)>] EventId: string
        [<JsonProperty("body", Required = Required.Always)>]  Body: Object }
      

module EventGridTypes =

    type TopicKey = TopicKey of string
    type TopicHostName = TopicHostName of string
    type KeyHost = TopicKey * TopicHostName

    type EventType = EventType of string
    type EventSubject = EventSubject of string
    type EventId = EventId of string option
    type EventInfo = EventType * EventSubject * EventId

    


