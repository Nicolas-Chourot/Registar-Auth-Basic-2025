using JSON_DAL;
using Newtonsoft.Json;
using System;

namespace JsonDemo.Models
{
    public class Registration : Record
    {
        public Registration()
        {
            Year = NextSession.Year;
        }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int Year { get; set; }
        [JsonIgnore] public Course Course { get { return DB.Courses.Get(CourseId); } }
        [JsonIgnore] public Student Student { get { return DB.Students.Get(StudentId); } }
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