﻿namespace DataProcessingService.Entities;

public class PaymentTransaction
{
    public string? City { get; set; }
    
    public List<Service>? Services { get; set; }

    public decimal? Total { get; set; }
}
