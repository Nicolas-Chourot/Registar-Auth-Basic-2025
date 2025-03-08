using JSON_DAL;
using MDB.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace JsonDemo.Models
{
    public class CoursesRepository : Repository<Course>
    {
       /* 
        * This feature was removed
        public bool Update(Course course, List<int> selectedStudentsId)
        {
            BeginTransaction();
            var result = base.Update(course);
            if (result) course.UpdateRegistrations(selectedStudentsId);
            EndTransaction();
            return result;
        }
       */

        public override bool Delete(int Id)
        {
            Course course = DB.Courses.Get(Id);
            if (course != null)
            {
                BeginTransaction();
                course.DeleteAllRegistrations();
                course.DeleteAllAllocations();
                var result = base.Delete(Id);
                EndTransaction();
                return result;
            }
            return false;
        }
        [JsonIgnore]
        public SelectList NextSessionToSelectList
        {
            get
            {
                return SelectListUtilities<Course>.Convert(ToList().Where(c => c.IsNextSession), "Caption");
            }
        }
        [JsonIgnore]
        public SelectList NextSessionUnAllocatedToSelectList
        {
            get
            {
                return SelectListUtilities<Course>.Convert(ToList().Where(c => c.IsNextSession && !c.IsAllocated(NextSession.Year)), "Caption");
            }
        }
    }
}