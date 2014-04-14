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
        let notifications = ConcurrentBag<Envelope<CrazyDomain.Api.Notification>>()

        let reservationSubject = new Subjects.Subject<Envelope<Reservation>>()
        reservationSubject.Subscribe reservations.Add |> ignore

        let notificationSubject = new Subjects.Subject<CrazyDomain.Api.Notification>()
        notificationSubject
        |> Observable.map EnvelopeWithDefaults
        |> Observable.subscribe notifications.Add ignore ignore
        |> ignore

        let agent = new Agent<Envelope<MakeReservation>>(fun inbox -> 
            let rec loop() =
                async{
                    let! cmd = inbox.Receive()
                    let rs = reservations |> ToReservations
                    let handle = Handle seatingCapacity rs
                    let newReservations = handle cmd
                    match newReservations with
                    | Some(r) -> 
                        reservationSubject.OnNext r
                        notificationSubject.OnNext 
                            {
                                About = cmd.Id
                                Type = "Success"
                                Message = 
                                    sprintf
                                        "Your reservation for %s was completed.  We look forward to see you"
                                        (cmd.Item.Date.ToString "yyyy.MM.dd")
                            }
                    | _ -> 
                        notificationSubject.OnNext
                            {
                                About = cmd.Id
                                Type = "Failure"
                                Message = 
                                    sprintf
                                        "We regret to inform you that your reservation for %s could not be completed."
                                        (cmd.Item.Date.ToString "yyyy.MM.dd")
                            }
                    return! loop() }
            loop())
        do agent.Start()
        
        Configure 
            (reservations |> ToReservations)
            (Observer.Create agent.Post)
            GlobalConfiguration.Configuration            