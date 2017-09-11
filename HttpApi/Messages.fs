namespace HttpApi

open System

// The HttpApi is a Message Endpoint. Need to convert Http traffic to messages. Put each message in an envelope.

// Command to make reservation
type MakeReservation =
  { Date : DateTime
    Name : string
    Email : string
    Quantity : int }

// Message that a reservation was made
type Reservation =
  { Date : DateTime
    Name : string
    Email : string
    Quantity : int }

[<AutoOpen>]
module Envelope =
    type Envelope<'T> =
      { Id : Guid
        Created : DateTimeOffset
        Item : 'T }

    let envelop id created item =
      { Id = id; Created = created; Item = item }

    let envelopWithDefaults item =
        envelop (Guid.NewGuid ()) DateTimeOffset.Now item

