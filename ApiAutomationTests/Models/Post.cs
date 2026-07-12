using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAutomationTests.Models;

public class Post
{
    public int? Id { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int UserId { get; set; }
}