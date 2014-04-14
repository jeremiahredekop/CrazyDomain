namespace CrazyDomain.HttpApi.WebHost

open System
open System.Collections.Concurrent
open System.Web.Http
open CrazyDomain.Api
open CrazyDomain.Api.Reservations
open CrazyDomain.Api.Infrastructure
open System.Reactive
open FSharp.Reactive

type Agent<'T> = Microsoft.FSharp.Control.MailboxProcessor<'T>
type HttpRouteDefaults = { Controller : string; Id: obj }

type Global() =
    inherit System.Web.HttpApplication()
    member this.Application_Start (sender : obj) (e: EventArgs) = 
        let seatingCapacity = 10
        let reservations = ConcurrentBag<Envelope<Reservation>>()

        let reservationSubject = new Subjects.Subject<Envelope<Reservation>>()
        reservationSubject.Subscribe reservations.Add |> ignore

        let agent = new Agent<Envelope<MakeReservation>>(fun inbox -> 
            let rec loop() =
                async{
                    let! cmd = inbox.Receive()
                    let rs = reservations |> ToReservations
                    let handle = Handle seatingCapacity rs
                    let newReservations = handle cmd
                    match newReservations with
                    | Some(r) -> reservationSubject.OnNext r
                    | _ -> ()
                    return! loop() }
            loop())
        do agent.Start()
        
        Configure 
            (reservations |> ToReservations)
            (Observer.Create agent.Post)
            GlobalConfiguration.Configuration            