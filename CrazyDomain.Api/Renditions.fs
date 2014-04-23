namespace CrazyDomain.Api

open System

[<CLIMutable>]
type MakeReservationRendition = {
    Date : string
    Name : string
    Email : string
    Quantity : int }


[<CLIMutable>]
type NotificationRendition = {
    About : string
    Type : string
    Message : string
}

[<CLIMutable>]
type NotificationRenditions = {
    Notifications : NotificationRendition array }

[<CLIMutable>]
type AtomLinkRendition = {
    Rel: string
    Href : string
}

[<CLIMutable>]
type LinkListRendition = {
    Links : AtomLinkRendition array
}