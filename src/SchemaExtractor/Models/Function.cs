﻿namespace SchemaExtractor.Models;

public class Function
{
    public string Name { get; set; } = default!;
    public string Schema { get; set; } = default!;
    public string? Definition { get; set; }
    public string Type { get; set; } = default!;
}
