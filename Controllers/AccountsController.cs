using JSON_DAL;
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
        [HttpPost]
        public JsonResult EmailExist(string Email)
        {
            return Json(DB.Users.ToList().Where(u => u.Email == Email).Any());
        }
        [HttpPost]
        public JsonResult EmailAvailable(string Email)
        {
            bool conflict = false;
            User connectedUser = (User)Session["ConnectedUser"];
            int currentId = connectedUser != null ? connectedUser.Id : 0;
            User foundUser = DB.Users.ToList().Where(u => u.Email == Email && u.Id != currentId).FirstOrDefault();
            conflict = foundUser != null;
            return Json(!conflict);
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
            LoginCredential credential = new LoginCredential();
            credential.Email = (string)Session["currentLoginEmail"];
            DB.Users.SetOnline(Session["ConnectedUser"], false);
            Session["ConnectedUser"] = null;
            return View(credential);
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Login(LoginCredential credential)
        {
            credential.Email = credential.Email.Trim();
            credential.Password = credential.Password.Trim();
            Session["CurrentLoginEmail"] = credential.Email;
            User connectedUser = DB.Users.GetUser(credential);
            Session["ConnectedUser"] = connectedUser;
            if (connectedUser == null)
            {
                Session["LoginMessage"] = "Courriel ou mot de passe incorrect";
                return View();
            }
            else
            {
                if (connectedUser.Blocked)
                {
                    return Redirect("/Accounts/Login?message=Votre compte a été bloqué!");
                }
                DB.Users.SetOnline(Session["ConnectedUser"], true);
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
                connectedUser.ConfirmEmail = connectedUser.Email;
                Session["CurrentEditingUserPassword"] = DateTime.Now.Ticks.ToString();
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
            user.Blocked = connectedUser.Blocked;
            user.Admin = connectedUser.Admin;
            user.Online = connectedUser.Online;
            // check password has been changed 
            if (user.Password == (string)Session["CurrentEditingUserPassword"])
                user.Password = connectedUser.Password; // no password change
            if (DB.Users.Update(user))
            {
                Session["ConnectedUser"] = DB.Users.Get(user.Id).Copy();
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
