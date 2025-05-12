using PhotosManager.Models;
using System.Web.Mvc;

public class NotificationsController : Controller
{
    public JsonResult Pop()
    {
        var result = DB.Notifications.Pop();
        return Json(result, JsonRequestBehavior.AllowGet);
    }
}
