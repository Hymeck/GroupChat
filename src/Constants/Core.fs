namespace GroupChat.Shared.Constants

open System.Net

module Ports =
    
    /// <summary>
    /// Port used for network participants to communicate.
    /// </summary>
    let NetworkPort = 9000
    
    /// <summary>
    /// Port used for group participants to communicate.
    /// </summary>
    let ChatPort = 9100
    
    
module IpAddresses =
    
    /// <summary>
    /// Multicast IPv4 address used for network participants for multicast data exchanging.
    /// </summary>
    let NetworkMulticastIpAddress = IPAddress.Parse "224.0.0.0"
    
    