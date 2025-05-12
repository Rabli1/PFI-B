using JSON_DAL;
using System;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class NotificationsRepository : Repository<Notification>
    {
        public object Pop()
        {
            var connectedUser = HttpContext.Current.Session["ConnectedUser"] as User;
            if (connectedUser == null || !connectedUser.hasNotifications)
                return null;

            var notifications = ToList();
            var notification = notifications
                .Where(n => n.TargetUserId == connectedUser.Id)
                .OrderBy(n => n.Created)
                .FirstOrDefault();

            if (notification != null)
            {
                Delete(notification.Id);

                return new
                {
                    Message = notification.Message,
                    Avatar = connectedUser.Avatar
                };
            }

            return null;
        }

    }
}
