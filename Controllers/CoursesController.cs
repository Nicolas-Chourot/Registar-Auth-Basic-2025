using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JsonDemo.Models;
using MDB.Models;
using static JsonDemo.Controllers.AccessControl;

namespace JsonDemo.Controllers
{
    [UserAccess]
    public class CoursesController : Controller
    {
        public ActionResult ToggleSearch()
        {
            Session["ShowCoursesSearch"] = !(bool)Session["ShowCoursesSearch"];
            return RedirectToAction("Index");
        }
        public ActionResult SearchTitle(string title)
        {
            Session["SearchCourseTitle"] = title;
            return RedirectToAction("Index");
        }
        public void InitSessionVariables()
        {
            Session["id"] = 0;
            if (Session["ShowCoursesSearch"] == null)
                Session["ShowCoursesSearch"] = false;

            if (Session["SearchCourseTitle"] == null)
                Session["SearchCourseTitle"] = "";

            if (Session["StudentsYearsList"] == null)
                Session["StudentsYearsList"] = DB.Students.YearsList;
        }
        public ActionResult Index()
        {
            InitSessionVariables();
            string searchTitle = ((string)Session["SearchCourseTitle"]).ToLower();
            var courses = DB.Courses.ToList().OrderBy(m => m.Session).ThenBy(m => m.Code).ToList();
            if ((bool)Session["ShowCoursesSearch"])
            {
                if (searchTitle != "")
                    courses = courses.Where(s => (s.Title.ToLower().IndexOf(searchTitle) > -1) ).ToList();
            }
            return View(courses);
        }
        public ActionResult Details(int id)
        {
            Course course = DB.Courses.Get(id);
            if (course != null)
            {
                Session["id"] = id;
                return View(DB.Courses.Get(id));
            }
            return RedirectToAction("Index");
        }
        [AdminAccess]
        public ActionResult Create()
        {
            return View(new Course());
        }
        [HttpPost]
        [AdminAccess]
        public ActionResult Create(Course course)
        {
            if (ModelState.IsValid)
            {
                DB.Courses.Add(course);
                return RedirectToAction("Index");
            }
            return View(course);
        }
        [AdminAccess]
        public ActionResult Edit()
        {
            int id = (int)Session["id"];
            Course course = DB.Courses.Get(id);
            if (course != null)
            {
                return View(DB.Courses.Get(id));
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [AdminAccess]
        public ActionResult Edit(Course course)
        {
            if (ModelState.IsValid)
            {
                course.Id = (int)Session["id"];

                DB.Courses.Update(course);
                return RedirectToAction("Details", new { id = course.Id });
            }
            return View(course);
        }
        [AdminAccess]
        public ActionResult Delete()
        {
            DB.Courses.Delete((int)Session["id"]);
            return RedirectToAction("Index");
        }
    }
}
