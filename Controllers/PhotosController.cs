﻿using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using static PhotosManager.Controllers.AccessControl;

namespace PhotosManager.Controllers
{
    [UserAccess]
    public class PhotosController : Controller
    {
        const string IllegalAccessUrl = "/Accounts/Login?message=Tentative d'accès illégal!&success=false";
        private const string SortAscKey = "PhotosSortAsc";
        private const string SortTypeKey = "PhotosSortType";
        public ActionResult ToggleSortDirection()
        {
            bool current = Session[SortAscKey] != null && (bool)Session[SortAscKey];
            Session[SortAscKey] = !current;

            string sortType = Session[SortTypeKey]?.ToString() ?? "date";
            return RedirectToAction("List", new { sortType = sortType });
        }
        public ActionResult ToggleSearch()
        {
            Session["ShowSearch"] = !(bool)Session["ShowSearch"];
            return RedirectToAction("List");
        }

        public ActionResult SetSearchKeywords(string keywords)
        {
            Session["SetSearchKeywords"] = keywords;
            return RedirectToAction("List");
        }

        public ActionResult SetPhotoOwnerSearchId(int id)
        {
            Session["SetPhotoOwnerSearchId"] = id;
            return RedirectToAction("List");
        }


        public ActionResult GetPhotos(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Photos.HasChanged || DB.Likes.HasChanged || DB.Users.HasChanged || DB.Comments.HasChanged)
            {
                if (DB.Likes.HasChanged || DB.Comments.HasChanged)
                    DB.Photos.ResetLikesCount();

                List<Photo> photos = DB.Photos.ToList();
                User connectedUser = (User)Session["ConnectedUser"];
                string sortType = Session["PhotosSortType"]?.ToString() ?? "date";
                bool asc = Session["PhotosSortAsc"] != null && (bool)Session["PhotosSortAsc"];
                int selectedUser = Session["SetPhotoOwnerSearchId"] != null ? (int)Session["SetPhotoOwnerSearchId"] : 0;
                string keyword = Session["SetSearchKeywords"] != null ? Session["SetSearchKeywords"].ToString() : string.Empty;



                switch (sortType)
                {
                    case "likes":
                        photos = asc
                            ? photos.OrderBy(p => p.Likes.Count).ToList()
                            : photos.OrderByDescending(p => p.Likes.Count).ToList();
                        break;

                    case "comments":
                        photos = asc
                            ? photos.OrderBy(p => p.Comments.Count).ToList()
                            : photos.OrderByDescending(p => p.Comments.Count).ToList();
                        break;

                    case "owner":
                        if (connectedUser != null)
                        {
                            var ownerPhotos = photos.Where(p => p.OwnerId == connectedUser.Id);
                            photos = asc
                                ? ownerPhotos.OrderBy(p => p.CreationDate).ToList()
                                : ownerPhotos.OrderByDescending(p => p.CreationDate).ToList();
                        }
                        break;

                    case "ownerLikes":
                        if (connectedUser != null)
                        {
                            var liked = photos.Where(p => p.Likes.Any(l => l.UserId == connectedUser.Id));
                            photos = asc
                                ? liked.OrderBy(p => p.CreationDate).ToList()
                                : liked.OrderByDescending(p => p.CreationDate).ToList();
                        }
                        break;

                    case "ownerDontLike":
                        if (connectedUser != null)
                        {
                            var unliked = photos.Where(p => !p.Likes.Any(l => l.UserId == connectedUser.Id));
                            photos = asc
                                ? unliked.OrderBy(p => p.CreationDate).ToList()
                                : unliked.OrderByDescending(p => p.CreationDate).ToList();
                        }
                        break;

                    case "ownerComments":
                        if (connectedUser != null)
                        {
                            var commented = photos.Where(p => p.Comments.Any(c => c.OwnerId == connectedUser.Id));
                            photos = asc
                                ? commented.OrderBy(p => p.CreationDate).ToList()
                                : commented.OrderByDescending(p => p.CreationDate).ToList();
                        }
                        break;

                    case "ownerNoComment":
                        if (connectedUser != null)
                        {
                            var uncommented = photos.Where(p => !p.Comments.Any(c => c.OwnerId == connectedUser.Id));
                            photos = asc
                                ? uncommented.OrderBy(p => p.CreationDate).ToList()
                                : uncommented.OrderByDescending(p => p.CreationDate).ToList();
                        }
                        break;

                    case "date":
                    default:
                        photos = asc
                            ? photos.OrderBy(p => p.CreationDate).ToList()
                            : photos.OrderByDescending(p => p.CreationDate).ToList();
                        break;
                }

                if (Session["ShowSearch"] != null && (bool)Session["ShowSearch"])
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        keyword = keyword.ToLower();
                        photos = photos.Where(p => p.Title.ToLower().Contains(keyword) || p.Description.ToLower().Contains(keyword)).ToList();
                    }

                    if (selectedUser > 0)
                    {
                        photos = photos.Where(p => p.OwnerId == selectedUser).ToList();
                    }
                }


                return PartialView(photos);
            }

            return null;
        }


        public ActionResult List(string sortType = "date")
        {
            Session["id"] = null;
            Session["IsOwner"] = null;

            if (!string.IsNullOrEmpty(sortType))
            {
                Session["PhotosSortType"] = sortType;
            }

            DB.Photos.ResetLikesCount();

            var users = DB.Users
                .ToList()
                .Select(u => new
                {
                    u.Id,
                    Name = u.Name,
                    PhotoCount = DB.Photos.ToList().Count(p => p.OwnerId == u.Id)
                })
                .Where(u => u.PhotoCount > 0)
                .OrderBy(u => u.Name)
                .ToList();

            ViewBag.UsersWithPhotos = users;

            return View();
        }
        public ActionResult Create()
        {
            return View(new Photo());
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Create(Photo photo)
        {
            photo.OwnerId = ((User)Session["ConnectedUser"]).Id;
            photo.CreationDate = DateTime.Now;
            DB.Photos.Add(photo);
            return RedirectToAction("List");
        }
        public ActionResult Edit()
        {
            if (Session["id"] != null && Session["IsOwner"] != null && (bool)Session["IsOwner"])
            {
                int id = (int)Session["id"];
                Photo photo = DB.Photos.Get(id);
                User connectedUser = (User)Session["ConnectedUser"];
                if (photo != null)
                {
                    if (connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id)
                    {
                        return View(photo);
                    }
                    return Redirect(IllegalAccessUrl);
                }
            }
            return Redirect(IllegalAccessUrl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(Photo photo)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            if (Session["IsOwner"] != null? (bool)Session["IsOwner"] : false)
            {
                Photo storedPhoto = DB.Photos.Get((int)Session["id"]);
                photo.Id = storedPhoto.Id;
                photo.OwnerId = storedPhoto.OwnerId;
                photo.CreationDate = storedPhoto.CreationDate;
                DB.Photos.Update(photo);
                return RedirectToAction("List");
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult GetDetails(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Photos.HasChanged || DB.Users.HasChanged || DB.Comments.HasChanged || DB.Likes.HasChanged)
            {
                int photoId = Session["currentPhotoId"] != null ? (int)Session["currentPhotoId"] : 0;
                DB.Photos.ResetLikesCount();
                Photo photo = DB.Photos.Get(photoId);
                if (photo != null)
                    return PartialView(photo);
            }
            return null;
        }
        public ActionResult Details(int id)
        {
            Photo photo = DB.Photos.Get(id);
            if (photo != null)
            {
                Session["currentPhotoId"] = id;
                User connectedUser = ((User)Session["ConnectedUser"]);
                Session["IsOwner"] = connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id;
                if ((bool)Session["IsOwner"] || photo.Shared)
                    return View();
                else
                    return Redirect(IllegalAccessUrl);
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult Delete()
        {
            if (Session["IsOwner"] != null ? (bool)Session["IsOwner"] : false)
            {
                int id = (int)Session["id"];
                Photo photo = DB.Photos.Get(id);
                if (photo != null)
                {
                    User connectedUser = (User)Session["ConnectedUser"];
                    if (connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id)
                    {
                        DB.Photos.Delete(id);
                        return RedirectToAction("List");
                    }
                    else
                        return Redirect(IllegalAccessUrl);
                }
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult TogglePhotoLike(int id)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            Photo photo = DB.Photos.Get(id);

            bool alreadyLiked = DB.Likes.ToList().Any(l => l.PhotoId == id && l.UserId == connectedUser.Id);

            DB.Likes.ToggleLike(id, connectedUser.Id);

            string action = alreadyLiked ? "n'aime plus" : "aime";
            string message = $"{connectedUser.Name} {action} votre photo [{photo.Title}]";

                DB.Notifications.Add(new Notification
                {
                    TargetUserId = photo.OwnerId,
                    Message = message,
                    Created = DateTime.Now,
                });

            return null;
        }



        public ActionResult Comments(int photoId, int parentId = 0)
        {
            List<Comment> comments = DB.Comments.ToList().Where(c => c.PhotoId == photoId && c.ParentId == parentId).ToList();
            return PartialView("RenderComments", comments);
        }
        public ActionResult GetComments(bool forceRefresh = false)
        {
            if (Session["currentPhotoId"] != null)
            {
                int photoId = (int)Session["currentPhotoId"];
                if (forceRefresh || true)
                {
                    List<Comment> comments = DB.Comments.ToList().Where(c => c.PhotoId == photoId && c.ParentId == 0).ToList();
                    return PartialView("RenderComments", comments);
                }
            }
            return null;
        }
        [HttpPost]
        public ActionResult CreateComment(int parentId, string commentText)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            int photoId = (int)Session["currentPhotoId"];
            Photo photo = DB.Photos.Get(photoId);

            Comment comment = new Comment
            {
                ParentId = parentId,
                PhotoId = photoId,
                OwnerId = connectedUser.Id,
                Text = commentText,
                CreationDate = DateTime.Now
            };

            DB.Comments.Add(comment);

                string message = $"{connectedUser.Name} a commenté votre photo \"{photo.Title}\"";

                DB.Notifications.Add(new Notification
                {
                    TargetUserId = photo.OwnerId,
                    Message = message,
                    Created = DateTime.Now
                });

            return null;
        }



        [HttpPost]
        public ActionResult UpdateComment(int commentId, string commentText)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = DB.Comments.Get(commentId);
            if (comment != null && comment.Owner.Id == connectedUser.Id)
            {
                comment.Text = commentText;
                DB.Comments.Update(comment);
            }
            return null;
        }
        public ActionResult ToggleCommentLike(int id)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            Comment comment = DB.Comments.Get(id);

            bool alreadyLiked = DB.Likes.ToList().Any(l => l.CommentId == id && l.UserId == connectedUser.Id);

            DB.Likes.ToggleCommentLike(id, connectedUser.Id);

                string action = alreadyLiked ? "n'aime plus" : "aime";

                string message = $"{connectedUser.Name} {action} un de vos commentaire";

                DB.Notifications.Add(new Notification
                {
                    TargetUserId = comment.OwnerId,
                    Message = message,
                    Created = DateTime.Now
                });

            return null;
        }


        public ActionResult DeleteComment(int id)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = DB.Comments.Get(id);
            if (comment != null && comment.Owner.Id == connectedUser.Id)
            {
                DB.Comments.BeginTransaction();
                DB.Comments.Delete(comment.Id);
                DB.Comments.EndTransaction();
            }
            return null;
        }
    }
}