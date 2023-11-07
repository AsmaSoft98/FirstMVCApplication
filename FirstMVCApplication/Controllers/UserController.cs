using FirstMVCApplication.Data;
using FirstMVCApplication.Models;
using FirstMVCApplication.Utils;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace FirstMVCApplication.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        //Registration POST action 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Exclude = "IsEmailVerified,ActivationCode")] User request)
        {
            bool Status = false;
            string message = "";
            //
            // Model Validation 
            if (ModelState.IsValid)
            {

                #region //Email Exist 
                var isExist = IsEmailExist(request.Email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(request);
                }
                #endregion

                #region Generate Activation Code 
                request.ActivationCode = Guid.NewGuid();
                #endregion

                #region  Password Hashing 
                request.Password = Crypto.Hash(request.Password);
                request.ConfirmPassword = Crypto.Hash(request.ConfirmPassword); //
                #endregion

                //temp values
                request.IsEmailVerified = false;
                request.PhoneNumber = "06698745124";
                request.UserName = request.Email;
             
                #region Save to Database
                using (ApplicationDbContext dc = new ApplicationDbContext())
                {
                    dc.Users.Add(request);
                    dc.SaveChanges();

                    //Send Email to User
                    SendVerificationLinkEmail(request.Email, request.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link " +
                              " has been sent to your email id:" + request.Email;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(request);
        }

        //Verify Account  
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (ApplicationDbContext dc = new ApplicationDbContext())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // This line I have added here to avoid 
                                                                // Confirm password does not match issue on save changes
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Login 
        [HttpGet]
        //[AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl = "")
        {
            string message = "";
            using (ApplicationDbContext dc = new ApplicationDbContext())
            {
                var v = dc.Users.Where(a => a.Email == login.Email).FirstOrDefault();
                if (v != null)
                {
                    if (!v.IsEmailVerified)
                    {
                        ViewBag.Message = "Please verify your email first";
                        return View();
                    }

                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.Email, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (ApplicationDbContext dc = new ApplicationDbContext())
            {
                var v = dc.Users.Where(a => a.Email == email).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string email, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            GMailer mailer = new GMailer();
            mailer.ToEmail = email;
            mailer.Subject = "verify your email - flight concept ltd";
            mailer.Body = "<br/><br/>We are excited to tell you that your account is" +
                            " successfully created. Please click on the below link to verify!" +
                            " <br/><br/><a href='" + link + "'>" + link + "</a> ";
            mailer.IsHtml = true;

            mailer.Send();
        }
    }
}