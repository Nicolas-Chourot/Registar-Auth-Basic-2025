using JSON_DAL;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System;
using MDB.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace JsonDemo.Models
{

    public class Teacher : Record
    {
        public const string Avatars_Folder = @"/App_Assets/Teachers/";
        public const string Default_Avatar = @"no_avatar.png";

        [JsonIgnore]
        public static string DefaultImage { get { return Avatars_Folder + Default_Avatar; } }

        public Teacher()
        {
            Code = GenerateCode();
            Phone = "(450) 430-3120 poste 00000";
            StartDate = NextSession.CurrentDate;
        }
        private static string GenerateCode()
        {
            string code = "CLG-420-";
            Random rnd = new Random();
            for (int i = 0; i < 5; i++) code += rnd.Next(0, 9).ToString();
            return code;
        }
        public string Code { get; set; }

        [Display(Name = "Date d'embauche")]
        [Required(ErrorMessage = "La date d'embauche est requise")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Prénom"), Required(ErrorMessage = "Obligatoire")]
        public string FirstName { get; set; }

        [Remote("NameAvailable", "Teachers", AdditionalFields = "FirstName", HttpMethod = "POST", ErrorMessage = "Ce nom est déjà utilisé.")]
        [Display(Name = "Nom"), Required(ErrorMessage = "Obligatoire")]
        public string LastName { get; set; }

        [Display(Name = "Téléphone"), Required(ErrorMessage = "Obligatoire")]
        public string Phone { get; set; }

        [ImageAsset(Avatars_Folder, Default_Avatar)]
        public string Avatar { get; set; } = DefaultImage;
        [JsonIgnore]
        public double YearsOfService
        {
            get
            {
                double now = NextSession.CurrentDate.Year + Math.Round(NextSession.CurrentDate.Month / 12.0 * 10.0) / 10.0;
                double sdt = StartDate.Year + Math.Round(StartDate.Month / 12.0 * 10.0) / 10.0;
                return now - sdt;
            }
        }
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
        public string Email
        {
            get { return FirstName + "." + LastName + "@clg.qc.ca"; }
        }
        [JsonIgnore]
        public List<Allocation> Allocations
        {
            get
            {
                return DB.Allocations.ToList().Where(r => r.TeacherId == Id).ToList();
            }
        }
        [JsonIgnore]
        public List<Allocation> NextSessionAllocations
        {
            get
            {
                return DB.Allocations.ToList().Where(a => a.TeacherId == Id && a.IsNextSession).ToList();
            }
        }
        [JsonIgnore]
        public List<Course> Courses
        {
            get
            {
                var courses = new List<Course>();
                foreach (var allocation in Allocations.OrderBy(r => r.Course.Code))
                {
                    courses.Add(allocation.Course);
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
                foreach (var allocation in NextSessionAllocations.OrderBy(r => r.Course.Code))
                {
                        courses.Add(allocation.Course);
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
        public void DeleteAllAllocations()
        {
            foreach (Allocation allocation in Allocations)
                DB.Allocations.Delete(allocation.Id);
        }

        public void DeleteNextSessionAllocations()
        {
            foreach (Allocation allocation in NextSessionAllocations)
                DB.Allocations.Delete(allocation.Id);
        }
        public void UpdateAllocations(List<int> selectedCoursesId)
        {
            DeleteNextSessionAllocations();
            if (selectedCoursesId != null)
                foreach (int courseId in selectedCoursesId)
                {
                    DB.Allocations.Add(new Allocation { TeacherId = Id, CourseId = courseId });
                }
        }
    }
}