using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ServiceDashBoard1.Services
{

    // Custom action filter to check if user session exists.
    // If no session is found (i.e., user not logged in), it redirects to the Login page.
    // Otherwise, it allows the action to execute normally.

    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if session exists, e.g. Username stored in session
            var username = context.HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                // User is not logged in, redirect to Login page
                context.Result = new RedirectToActionResult("Login", "Users", null);
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }



    }
}
