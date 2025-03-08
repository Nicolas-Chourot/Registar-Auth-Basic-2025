using JSON_DAL;
using MDB.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace JsonDemo.Models
{
    public class Student : Record
    {
        public Student()
        {
            Code = GenerateCode();
        }
        private static string GenerateCode()
        {
            string code = NextSession.CurrentDate.Year.ToString();
            Random rnd = new Random();
            for (int i = 0; i < 5; i++) code += rnd.Next(0, 9).ToString();
            return code;
        }
        public string Code { get; set; }

        [Display(Name = "Prénom"), Required(ErrorMessage = "Obligatoire")]
        public string FirstName { get; set; }

        [Display(Name = "Nom"), Required(ErrorMessage = "Obligatoire")]
        public string LastName { get; set; }

        [Display(Name = "Date de naissance")]
        [Required(ErrorMessage = "La date de naissance est requise")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Courriel"), Required(ErrorMessage = "Obligatoire")]
        [EmailAddress(ErrorMessage = "Invalide")]
        public string Email { get; set; }

        [Display(Name = "Téléphone"), Required(ErrorMessage = "Obligatoire")]
        public string Phone { get; set; }

        [JsonIgnore]
        public string FullName
        {
            get { return LastName + " " + FirstName; }
        }
        [JsonIgnore]
        public string Caption
        {
            get { return Code + " " + LastName + " " + FirstName; }
        }
        [JsonIgnore]
        public int Year
        {
            get
            {
                return int.Parse(Code.Substring(0, 4));
            }
        }
        [JsonIgnore]
        public List<Registration> Registrations
        {
            get
            {
                return DB.Registrations.ToList().Where(r => r.StudentId == Id).ToList();
            }
        }
        [JsonIgnore]
        public List<Registration> NextSessionRegistrations
        {
            get
            {
                return DB.Registrations.ToList().Where(r => r.StudentId == Id && r.IsNextSession).ToList();
            }
        }
        [JsonIgnore]
        public List<Course> Courses
        {
            get
            {
                var courses = new List<Course>();
                foreach (var registration in Registrations.OrderBy(r => r.Course.Code))
                {
                    courses.Add(registration.Course);
                }
                return courses;
            }
        }
        [JsonIgnore]
        public List<Course> NextSessionCourses
        {
            get
            {
                var courses = new List<Course>();
                foreach (var registration in NextSessionRegistrations.OrderBy(r => r.Course.Code))
                {
                    courses.Add(registration.Course);
                }
                return courses;
            }
        }
        [JsonIgnore]
        public SelectList CoursesSelectList { get { return SelectListUtilities<Course>.Convert(Courses, "Caption"); } }

        [JsonIgnore]
        public SelectList NextSessionCoursesToSelectList
        {
            get
            {
                return SelectListUtilities<Course>.Convert(NextSessionCourses, "Caption");
            }
        }
        public void DeleteAllRegistrations()
        {
            foreach (Registration registration in Registrations)
                DB.Registrations.Delete(registration.Id);
        }

        public void DeleteNextSessionRegistrations()
        {
            foreach (Registration registration in NextSessionRegistrations)
                DB.Registrations.Delete(registration.Id);
        }
        public void UpdateRegistrations(List<int> selectedCoursesId)
        {
            DeleteNextSessionRegistrations();
            if (selectedCoursesId != null)
                foreach (int courseId in selectedCoursesId)
                {
                    DB.Registrations.Add(new Registration { StudentId = Id, CourseId = courseId });
                }
        }
    }
}