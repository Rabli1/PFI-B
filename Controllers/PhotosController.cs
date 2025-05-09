using PhotosManager.Models;
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

        public ActionResult SetPhotoOwnerSearchId(int id)
        {
            Session["photoOwnerSearchId"] = id;
            return RedirectToAction("List");
        }
        public ActionResult SetSearchKeywords(string keywords)
        {
            Session["searchKeywords"] = keywords;
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
                string sortType = Session["PhotosSortType"] != null ? (string)Session["PhotosSortType"] : "date";

                switch (sortType)
                {
                    case "likes":
                        photos = photos.OrderByDescending(p => p.Likes.Count).ToList();
                        break;

                    case "keywords":
                        string keywords = Session["searchKeywords"]?.ToString().ToLower() ?? "";
                        if (!string.IsNullOrWhiteSpace(keywords))
                        {
                            photos = photos
                                .Where(p =>
                                    (!string.IsNullOrEmpty(p.Title) && p.Title.ToLower().Contains(keywords)) ||
                                    (!string.IsNullOrEmpty(p.Description) && p.Description.ToLower().Contains(keywords)))
                                .OrderByDescending(p => p.CreationDate)
                                .ToList();
                        }
                        break;

                    case "user":
                        photos = photos
                            .OrderBy(p => p.Owner.Name)
                            .ThenByDescending(p => p.CreationDate)
                            .ToList();
                        break;

                    case "owner":
                        if (connectedUser != null)
                        {
                            photos = photos
                                .Where(p => p.OwnerId == connectedUser.Id)
                                .OrderByDescending(p => p.CreationDate)
                                .ToList();
                        }
                        break;

                    default:
                        photos = photos.OrderByDescending(p => p.CreationDate).ToList();
                        break;
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
            DB.Likes.ToggleLike(id, connectedUser.Id);
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
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = new Comment();
            comment.ParentId = parentId;
            comment.PhotoId = (int)Session["currentPhotoId"];
            comment.OwnerId = connectedUser.Id;
            comment.Text = commentText;
            comment.CreationDate = DateTime.Now;
            DB.Comments.Add(comment);
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
            DB.Likes.ToggleCommentLike(id, connectedUser.Id);
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