using System;
using System.Collections.Generic;

namespace SmartMeetingRoom1.Models
{
    public class Room
    {
        public int Id { get; set; }             
        public string Name { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string Features { get; set; }      

        public DateTime CreatedAt { get; set; }

        public ICollection<Meeting> Meetings { get; set; }
    }
}
