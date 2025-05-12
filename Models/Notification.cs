using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class Notification : Record
    {
        public int TargetUserId { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}