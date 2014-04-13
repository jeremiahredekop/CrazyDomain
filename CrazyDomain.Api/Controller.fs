namespace CrazyDomain.Api

open System.Net
open System.Net.Http
open System.Web.Http

type HomeController() = 
    inherit ApiController()
    member this.Get() = new HttpResponseMessage()

type ReservationController() = 
    inherit ApiController()
    member this.Post (rendition : MakeReservationRendition) = 
        new HttpResponseMessage(HttpStatusCode.Accepted)

