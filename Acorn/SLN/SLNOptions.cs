﻿namespace Acorn.SLN;

public class SLNOptions
{
    public required bool Enabled { get; set; }
    public required string Url { get; set; } 
    public required int PingRate { get; set; }
    public required string UserAgent { get; set; }
    public required string Zone { get; set; }
}