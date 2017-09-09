namespace HttpApi

open System

// The HttpApi is a Message Endpoint. Need to convert Http traffic to messages. Put each message in an envelope.
[<CLIMutable>]
type MakeReservation =
  { Date : DateTime
    Name : string
    Email : string
    Quantity : int }

[<AutoOpen>]
module Envelope =
    [<CLIMutable>]
    type Envelope<'T> =
      { Id : Guid
        Created : DateTimeOffset
        Item : 'T }

    let envelop id created item =
      { Id = id; Created = created; Item = item }

    let envelopWithDefaults item =
        envelop (Guid.NewGuid ()) DateTimeOffset.Now item

