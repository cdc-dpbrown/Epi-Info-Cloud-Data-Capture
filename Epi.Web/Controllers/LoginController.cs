﻿using System;
using System.Web.Mvc;
using Epi.Web.MVC.Facade;
using Epi.Web.MVC.Models;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Epi.Core.EnterInterpreter;
using System.Web.Security;
using System.Reflection;
using System.Diagnostics;
using Epi.Web.Common.Constants;
using System.Linq;

namespace Epi.Web.MVC.Controllers
{
    public class LoginController : Controller
    {
        //declare SurveyTransactionObject object
        private Epi.Web.MVC.Facade.ISurveyFacade _isurveyFacade;
        /// <summary>
        /// Injectinting SurveyTransactionObject through Constructor
        /// </summary>
        /// <param name="iSurveyInfoRepository"></param>

        public LoginController(Epi.Web.MVC.Facade.ISurveyFacade isurveyFacade)
        {
            _isurveyFacade = isurveyFacade;
        }

        // GET: /Login/
        [HttpGet]
        public ActionResult Index(string responseId, string ReturnUrl)
        {
            //string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //ViewBag.Version = version;

            //   //get the responseId
            //    responseId = GetResponseId(ReturnUrl);
            //    //get the surveyId
            //     string SurveyId = _isurveyFacade.GetSurveyAnswerResponse(responseId).SurveyResponseList[0].SurveyId;
            //     //put surveyId in viewbag so can be retrieved in Login/Index.cshtml
            //     ViewBag.SurveyId = SurveyId;

            return View("Index");
        }
        [HttpPost]

        public ActionResult Index(UserLoginModel Model, string Action, string ReturnUrl)
        {

            return ValidateUser(Model.UserName, Model.Password, ReturnUrl);

            //if (ReturnUrl == null || !ReturnUrl.Contains("/"))
            //{
            //    ReturnUrl = "/Home/Index";
            //}


            //Epi.Web.Common.Message.UserAuthenticationResponse result = _isurveyFacade.ValidateUser(Model.UserName, Model.Password);

            //if (result.UserIsValid)
            //{
            //    if (result.User.ResetPassword)
            //    {
            //        return ResetPassword(Model.UserName);
            //    }
            //    else
            //    {

            //        FormsAuthentication.SetAuthCookie(Model.UserName, false);
            //        string UserId = Epi.Web.Common.Security.Cryptography.Encrypt(result.User.UserId.ToString());
            //        Session["UserId"] = UserId;
            //        return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Home", new { surveyid = "" });
            //    }
            //}
            //else
            //{
            //    ModelState.AddModelError("", "The email or password you entered is incorrect.");
            //    return View();
            //}
        }
        /// <summary>
        /// parse and return the responseId from response Url 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private string GetResponseId(string returnUrl)
        {
            string responseId = string.Empty;
            string[] expressions = returnUrl.Split('/');

            foreach (var expression in expressions)
            {
                if (Epi.Web.MVC.Utility.SurveyHelper.IsGuid(expression))
                {

                    responseId = expression;
                    break;
                }

            }
            return responseId;
        }


        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }

        [HttpGet]
        public ActionResult ResetPassword(string UserName)
        {
            UserResetPasswordModel model = new UserResetPasswordModel();
            model.UserName = UserName;
            return View("ResetPassword", model);
        }

        [HttpPost]
        public ActionResult ForgotPassword(UserForgotPasswordModel Model, string Action, string ReturnUrl)
        {
            switch (Action.ToUpper())
            {
                case "CANCEL":
                    return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Login");
                default:
                    break;
            }

            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                List<string> errorMessages = new List<string>();

                string msg = ModelState.First().Value.Errors.First().ErrorMessage.ToString();

                ModelState.AddModelError("", msg);


                return View("ForgotPassword", Model);
            }

            bool success = _isurveyFacade.UpdateUser(new Common.DTO.UserDTO() { UserName = Model.UserName, Operation = Constant.OperationMode.UpdatePassword });
            if (success)
            {
                return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Login");
            }
            else
            {
                ModelState.AddModelError("", "Error sending email.");
                return View("ForgotPassword", Model);
            }

        }

        [HttpPost]
        public ActionResult ResetPassword(UserResetPasswordModel Model, string Action, string ReturnUrl)
        {

            switch (Action.ToUpper())
            {
                case "CANCEL":
                    return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Login");
                default:
                    break;
            }

            if (!ModelState.IsValid)
            {
                UserResetPasswordModel model = new UserResetPasswordModel();
                model.UserName = Model.UserName;
                ModelState.AddModelError("", "Passwords do not match. Please try again.");
                return View("ResetPassword", Model);
            }

            _isurveyFacade.UpdateUser(new Common.DTO.UserDTO() { UserName = Model.UserName, PasswordHash = Model.Password, Operation = Constant.OperationMode.UpdatePassword, ResetPassword = true });

            return ValidateUser(Model.UserName, Model.Password, ReturnUrl);

        }

        private ActionResult ValidateUser(string UserName, string Password, string ReturnUrl)
        {
            if (ReturnUrl == null || !ReturnUrl.Contains("/"))
            {
                ReturnUrl = "/Home/Index";
            }


            Epi.Web.Common.Message.UserAuthenticationResponse result = _isurveyFacade.ValidateUser(UserName, Password);

            if (result.UserIsValid)
            {
                if (result.User.ResetPassword)
                {
                    return ResetPassword(UserName);
                }
                else
                {

                    FormsAuthentication.SetAuthCookie(UserName, false);
                    string UserId = Epi.Web.Common.Security.Cryptography.Encrypt(result.User.UserId.ToString());
                    Session["UserId"] = UserId;
                    return RedirectToAction(Epi.Web.MVC.Constants.Constant.INDEX, "Home", new { surveyid = "" });
                }
            }
            else
            {
                ModelState.AddModelError("", "The email or password you entered is incorrect.");
                return View();
            }
        }

    }
}
