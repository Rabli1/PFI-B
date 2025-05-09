using JSON_DAL;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Web;

namespace PhotosManager.Models
{
    public class Photo
    {
        const string PhotosFolder = @"/App_Assets/Photos/";
        const string DefaultPhoto = @"No_Image.png";

        public int Id { get; set; }
        public int OwnerId { get; set; }            // Id du propriétaire de la photo
        [Display(Name = "Titre"), Required(ErrorMessage = "Obligatoire")]
        public string Title { get; set; }           // Titre de la photo
        [Display(Name = "Description"), Required(ErrorMessage = "Obligatoire")]
        public string Description { get; set; }     // Description de la photo
        public DateTime CreationDate { get; set; }  // Date de création
        public bool Shared { get; set; }            // Indicateur de partage ("true" ou "false")

        [JsonIgnore]
        private int _likesCount = -1;
        [JsonIgnore]
        private List<Like> _likesList = null;
        [JsonIgnore]
        private int _commentsCount = -1;
        [JsonIgnore]
        private List<Comment> _commentsList = null;
        public void ResetCountsCalc()
        {
            _likesCount = -1;
            _likesList = null;
            _commentsCount = -1;
            _commentsList = null;
        }
        [JsonIgnore]
        public int LikesCount
        {
            get
            {
                if (_likesCount == -1)
                    _likesCount = Likes.Count();
                return _likesCount;
            }
        }
        [JsonIgnore]
        public List<Like> Likes
        {
            get
            {
                if (_likesList == null)
                    _likesList = DB.Likes.ToList().Where(l => l.PhotoId == Id && l.CommentId == 0).ToList();
                return _likesList;
            }
        }

        [JsonIgnore]
        public string UsersLikesList
        {
            get
            {
                string UsersLikesList = "";
                foreach (var like in Likes)
                {
                    UsersLikesList += DB.Users.Get(like.UserId).Name + "\n";
                }
                return UsersLikesList;
            }
        }
        [JsonIgnore]
        public string UsersCommentList
        {
            get
            {
                string UsersCommentList = "";
                foreach (var comment in Comments.DistinctBy(c=>c.OwnerId))
                {
                    User owner = comment.Owner;
                    if (owner != null)
                        UsersCommentList += owner.Name + "\n";
                }
                return UsersCommentList;
            }
        }
        [ImageAsset(PhotosFolder, DefaultPhoto)]
        public string Image { get; set; }           // Url relatif de l'image

        public Photo()
        {
            Id = 0;
            CreationDate = DateTime.Now;
            Shared = false;
            Image = PhotosFolder + DefaultPhoto;
        }
        [JsonIgnore]
        public User Owner => DB.Users.Get(OwnerId);
        [JsonIgnore]
        public int CommentsCount
        {
            get
            {
                if (_commentsCount == -1)
                    _commentsCount = Comments.Count();
                return _commentsCount;
            }
        }
        [JsonIgnore]
        public List<Comment> Comments
        {
            get
            {
                if (_commentsList == null)
                    _commentsList = DB.Comments.ToList().Where(c => c.PhotoId == Id && c.ParentId == 0).ToList();
                return _commentsList;
            }
        }
    }
}