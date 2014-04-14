namespace CrazyDomain.Api

open System

[<CLIMutable>]
type MakeReservation = {
    Date : DateTime
    Name : string
    Email : string
    Quantity : int }

[<AutoOpenAttribute>]
module Envelope =
    [<CLIMutable>]
    type Envelope<'T> ={
        Id : Guid
        Created : DateTimeOffset
        Item : 'T
    }

    let Envelope id created item = {
        Id = id
        Created = created
        Item = item
    }

    let EnvelopeWithDefaults item = 
        Envelope (Guid.NewGuid()) (DateTimeOffset.Now) item
   
[<CLIMutable>]
type Reservation = {
    Date : DateTime
    Name : string
    Email : string
    Quantity : int }

[<CLIMutable>]
type Notification = {
    About : Guid
    Type : string
    Message : string }