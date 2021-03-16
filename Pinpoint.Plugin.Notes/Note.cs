using System;

namespace Pinpoint.Plugin.Notes
{
    public class Note
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}