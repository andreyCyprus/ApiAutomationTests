using System;
using System.Collections.Generic;
using System.Text;

namespace ApiAutomationTests.Models
{
   public class Comment
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? body { get; set; }
        public int PostId { get; set; }

    }
}
