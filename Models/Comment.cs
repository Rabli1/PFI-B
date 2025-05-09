using JSON_DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class Comment : Record
    {
        public int PhotoId { get; set; }
        public int ParentId { get; set; }
        public int OwnerId { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }

        [JsonIgnore] public User Owner => DB.Users.Get(OwnerId);
        [JsonIgnore] public Photo Photo => DB.Photos.Get(PhotoId);
        [JsonIgnore] public List<Comment> Comments => DB.Comments.ToList().Where(c => c.PhotoId == PhotoId && c.ParentId == Id).ToList();
        [JsonIgnore] public List<Like> Likes => DB.Likes.ToList().Where(l => l.CommentId == Id).ToList();

        [JsonIgnore]
        public string UsersLikesList
        {
            get
            {
                string UsersLikesList = "";
                foreach (var like in Likes) UsersLikesList += DB.Users.Get(like.UserId).Name + "\n";
                return UsersLikesList;
            }
        }
    }
}
