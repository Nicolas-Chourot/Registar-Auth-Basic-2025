using JSON_DAL;
using Newtonsoft.Json;
using System;

namespace JsonDemo.Models
{
    public class Allocation : Record
    {
        public Allocation()
        {
            Year = NextSession.Year;
        }
        public int TeacherId { get; set; }
        public int CourseId { get; set; }
        public int Year { get; set; }
        [JsonIgnore] public Course Course { get { return DB.Courses.Get(CourseId); } }
        [JsonIgnore] public Teacher Teacher { get { return DB.Teachers.Get(TeacherId); } }
        
        [JsonIgnore]
        public bool IsNextSession
        {
            get
            {
                return Year == NextSession.Year && NextSession.ValidSessions.Contains(Course.Session);
            }
        }
    }
}