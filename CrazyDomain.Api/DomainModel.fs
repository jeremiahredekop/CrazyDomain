﻿namespace CrazyDomain.Api

open System

module Reservations =

    type IReservations = 
        inherit seq<Envelope<Reservation>>
        abstract Between : DateTime -> DateTime -> seq<Envelope<Reservation>>

    type ReservationsInMemory(reservations) =
        interface IReservations with
            member this.Between min max =
                reservations
                |> Seq.filter (fun r -> min <= r.Item.Date && r.Item.Date <= max)
            member this.GetEnumerator() =
                reservations.GetEnumerator()
            member this.GetEnumerator() =
                (this :> seq<Envelope<Reservation>>).GetEnumerator() :> System.Collections.IEnumerator

    let ToReservations reservations = ReservationsInMemory(reservations)

    let Between min max (reservations : IReservations) = 
        reservations.Between min max

    let On (date : DateTime) reservations =
        let min = date.Date
        let max = (min.AddDays 1.0) - TimeSpan.FromTicks 1L
        reservations |> Between min max

    let Handle capacity reservations (request : Envelope<MakeReservation>) =
        let reservedSeatsOnDate =
            reservations
            |> On request.Item.Date
            |> Seq.sumBy(fun r -> r.Item.Quantity)
        if capacity - reservedSeatsOnDate < request.Item.Quantity then
            None
        else
            {
                Date = request.Item.Date
                Name = request.Item.Name
                Email = request.Item.Email
                Quantity = request.Item.Quantity
            }
            |> EnvelopeWithDefaults
            |> Some

module Notifications =

    type INotifications =
        inherit seq<Envelope<Notification>>
        abstract About : Guid -> seq<Envelope<Notification>>

    type NotificationsInMemory(notifications : Envelope<Notification> seq) =
        interface INotifications with
            member this.About id = 
                notifications |> Seq.filter (fun n -> n.Item.About = id)
            member this.GetEnumerator() = notifications.GetEnumerator()
            member this.GetEnumerator() = 
                (this :> Envelope<Notification> seq).GetEnumerator() :> System.Collections.IEnumerator
    
    let ToNotifications notifications = NotificationsInMemory(notifications)

    let About id (notifications : INotifications) = notifications.About id