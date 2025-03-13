using JsonDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static JsonDemo.Controllers.AccessControl;

namespace JsonDemo.Controllers
{
    [UserAccess]
    public class TeachersController : Controller
    {
        public ActionResult ToggleSearch()
        {
            Session["ShowTeachersSearch"] = !(bool)Session["ShowTeachersSearch"];
            return RedirectToAction("Index");
        }
        public ActionResult SearchName(string name)
        {
            Session["SearchTeacherName"] = name;
            return RedirectToAction("Index");
        }
        
        public void InitSessionVariables()
        {
            Session["id"] = 0;
            if (Session["ShowTeachersSearch"] == null)
                Session["ShowTeachersSearch"] = false;

            if (Session["SearchTeacherName"] == null)
                Session["SearchTeacherName"] = "";

            if (Session["StudentsYearsList"] == null)
                Session["StudentsYearsList"] = DB.Students.YearsList;
        }

        [HttpPost]
        public JsonResult NameAvailable(string LastName, string FirstName)
        {
            int Id = Session["Id"] != null ? (int)Session["Id"] : 0;
            bool available = true;
            Teacher teacher = 
                DB.Teachers.ToList().Where(
                    t => 
                    t.Id != Id &&
                    t.FirstName.ToLower() == FirstName.ToLower() &&
                    t.LastName.ToLower() == LastName.ToLower()).FirstOrDefault();
            if (teacher != null) available = false;
            return Json(available);
        }

        public ActionResult GetTeachers(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Teachers.HasChanged)
            {
                string searchName = ((string)Session["SearchTeacherName"]).ToLower();
                var teachers = DB.Teachers.ToList().OrderBy(m => m.LastName).ThenBy(m => m.FirstName).ToList();

                if ((bool)Session["ShowTeachersSearch"])
                {
                    if (searchName != "")
                        teachers = teachers.Where(s => s.LastName.ToLower().StartsWith(searchName)).ToList();

                }
                return PartialView(teachers);
            }
            return null;
        }
        public ActionResult Index()
        {
            InitSessionVariables();
            return View();
        }
        public ActionResult Details(int id)
        {
            Teacher teacher = DB.Teachers.Get(id);
            if (teacher != null)
            {
                string d = teacher.StartDate.ToFrenchDateString();
                Session["id"] = id;
                Session["code"] = teacher.Code;
                return View(teacher);
            }
            return RedirectToAction("Index");
        }
        [AdminAccess]
        public ActionResult Create()
        {
            return View(new Teacher());
        }
        [HttpPost]
        [AdminAccess]
        public ActionResult Create(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                DB.Teachers.Add(teacher);
                return RedirectToAction("Index");
            }
            return View(teacher);
        }
        [AdminAccess]
        public ActionResult Edit()
        {
            int id = (int)Session["id"];
            Teacher teacher = DB.Teachers.Get(id);
            if (teacher != null)
            {
                ViewBag.Allocations = teacher.NextSessionCoursesToSelectList;
                ViewBag.Courses = DB.Courses.NextSessionUnAllocatedToSelectList;
                return View(teacher);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [AdminAccess]
        public ActionResult Edit(Teacher teacher, List<int> selectedCoursesId)
        {
            if (ModelState.IsValid)
            {
                teacher.Id = (int)Session["id"];
                teacher.Code = (string)Session["code"];
                DB.Teachers.Update(teacher, selectedCoursesId);
                return RedirectToAction("Details", new { id = teacher.Id });

            }
            ViewBag.Allocations = teacher.NextSessionCoursesToSelectList;
            ViewBag.Courses = DB.Courses.NextSessionUnAllocatedToSelectList;
            return View(teacher);
        }
        [AdminAccess]
        public ActionResult Delete()
        {
            int id = (int)Session["id"];
            DB.Teachers.Delete(id);
            return RedirectToAction("Index");
        }
    }
}