using JsonDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static JsonDemo.Controllers.AccessControl;

namespace JsonDemo.Controllers
{
    public class AccountsController : Controller
    {
        public JsonResult EmailExist(string Email)
        {
            bool exist = DB.Users.EmailExist(Email);
            return Json(exist, JsonRequestBehavior.AllowGet);
        }
        [UserAccess]
        public JsonResult EmailConflict(string Email)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            User foundUser = DB.Users.ToList().Where(u => u.Email == Email).FirstOrDefault();
            bool conflict = false;
            if (foundUser != null) conflict = foundUser.Id != connectedUser.Id;
            return Json(conflict, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ExpiredSession()
        {
            return Redirect("/Accounts/Login?message=Session expirée, veuillez vous reconnecter.");
        }
        public ActionResult Logout()
        {
            return RedirectToAction("Login", "Accounts");
        }
        public ActionResult Login(string message = "", bool success = false)
        {
            Session["LoginSuccess"] = success;
            Session["LoginMessage"] = message;
            if (Session["CurrentLoginEmail"] == null) Session["currentLoginEmail"] = "";
            Session["ConnectedUser"] = null;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Login(LoginCredential credential)
        {
            credential.Email = credential.Email.Trim();
            credential.Password = credential.Password.Trim();
            Session["CurrentLoginEmail"] = credential.Email;
            Session["ConnectedUser"] = DB.Users.GetUser(credential);
            if (Session["ConnectedUser"] == null)
            {
                Session["LoginMessage"] = "Courriel ou mot de passe incorrect";
                return View();
            }
            else
            {
                User user = (User)Session["ConnectedUser"];
                if (user.Blocked)
                {
                    return Redirect("/Accounts/Login?message=Votre compte a été bloqué!");
                }
            }
            return RedirectToAction("Index", "Students");
        }
        public ActionResult Subscribe()
        {
            Session["ConnectedUser"] = null;
            Session["CurrentLoginEmail"] = "";
            return View(new User());
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Subscribe(User user)
        {
            DB.Users.Add(user);
            return Redirect("/Accounts/Login?message=Création de compte effectué avec succès!&success=true");
        }
        [UserAccess]
        public ActionResult EditProfil()
        {
            User connectedUser = (User)Session["ConnectedUser"];
            if (connectedUser != null)
            {
                return View(connectedUser);
            }
            return RedirectToAction("Login", "Accounts");
        }
        [UserAccess]
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult EditProfil(User user)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            user.Id = connectedUser.Id;
            user.Blocked = false;
            user.Admin = connectedUser.Admin;
            if (DB.Users.Update(user))
            {
                Session["ConnectedUser"] = DB.Users.Get(user.Id);
            }
            return RedirectToAction("Index", "Students");
        }
        [UserAccess]
        public ActionResult DeleteProfil()
        {
            User connectedUser = (User)Session["ConnectedUser"];
            DB.Users.Delete(connectedUser.Id);
            return RedirectToAction("Login?message=Votre compte a été effacé avec succès!");
        }
        [AdminAccess]
        public ActionResult ManageUsers()
        {
            return View(DB.Users.ToList().OrderBy(u => u.Name).ToList());
        }
        [AdminAccess]
        public ActionResult TogglePromoteUser(int id)
        {
            User user = DB.Users.Get(id);
            if (user != null)
            {
                user.Admin = !user.Admin;
                DB.Users.Update(user);
            }
            return RedirectToAction("ManageUsers");
        }
        [AdminAccess]
        public ActionResult ToggleBlockUser(int id)
        {
            User user = DB.Users.Get(id);
            if (user != null)
            {
                user.Blocked = !user.Blocked;
                DB.Users.Update(user);
            }
            return RedirectToAction("ManageUsers");
        }
        [AdminAccess]
        public ActionResult DeleteUser(int id)
        {
            DB.Users.Delete(id);
            return RedirectToAction("ManageUsers");
        }
    }
}