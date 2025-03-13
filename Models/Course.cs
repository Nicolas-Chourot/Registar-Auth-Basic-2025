using JSON_DAL;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace JsonDemo.Models
{
    public class Course : Record
    {
        public Course()
        {
            Session = 1;
        }

        [Display(Name = "Sigle"), Required(ErrorMessage = "Obligatoire")]
        public string Code { get; set; }

        [Display(Name = "Titre"), Required(ErrorMessage = "Obligatoire")]
        public string Title { get; set; }

        [Display(Name = "Session"), Required(ErrorMessage = "Obligatoire")]
        [Range(1, 6)]
        public int Session { get; set; }

        [JsonIgnore]
        public bool IsNextSession { get { return NextSession.ValidSessions.Contains(Session); } }

        [JsonIgnore]
        public string Caption
        {
            get { return "[" + Session + "] " + Code + " " + Title; }
        }

        [JsonIgnore]
        public List<Registration> Registrations
        {
            get
            {
                return DB.Registrations.ToList().Where(r => r.CourseId == Id).ToList();
            }
        }

        [JsonIgnore]
        public List<Allocation> Allocations
        {
            get
            {
                return DB.Allocations.ToList().Where(r => r.CourseId == Id).ToList();
            }
        }

        [JsonIgnore]
        public List<Student> Students
        {
            get
            {
                var students = new List<Student>();
                foreach (var registration in Registrations.OrderBy(r => r.Student.LastName).ThenBy(r => r.Student.FirstName))
                {
                    students.Add(registration.Student);
                }
                return students;
            }
        }

        public bool IsAllocated(int year)
        {
            return DB.Allocations.ToList().Where(a => a.Year == year && a.CourseId == Id).Any();
        }

        public void DeleteAllRegistrations()
        {
            foreach (Registration registration in Registrations)
                DB.Registrations.Delete(registration.Id);
        }
        public void DeleteAllAllocations()
        {
            foreach (Allocation allocation in Allocations)
                DB.Allocations.Delete(allocation.Id);
        }
        /*
         * This feature was removed
         * 
         public void UpdateRegistrations(List<int> selectedStudentsId)
         {
             DeleteAllRegistrations();
             if (selectedStudentsId != null)
                 foreach (int studentId in selectedStudentsId)
                     DB.Registrations.Add(new Registration { StudentId = studentId, CourseId = Id });
         }

         */
    }
}