using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace PhotosManager.Models
{
    public class LikesRepository : Repository<Like>
    {

        public void ToggleLike(int photoId, int userId)
        {
            Like like = ToList().Where(l => (l.PhotoId == photoId && l.UserId == userId)).FirstOrDefault();

            if (like != null)
            {
                Delete(like.Id);
            }
            else
            {
                like = new Like { PhotoId = photoId, UserId = userId };
                Add(like);
            }
        }
        public void ToggleCommentLike(int commentId, int userId)
        {
            Like like = ToList().Where(l => (l.CommentId == commentId && l.UserId == userId)).FirstOrDefault()?.Copy();
            if (like != null)
            {
                Delete(like.Id);
            }
            else
            {
                like = new Like { CommentId = commentId, UserId = userId };
                Add(like);
            }
        }
        public void DeleteByPhotoId(int photoId)
        {
            List<Like> list = ToList().Where(l => l.PhotoId == photoId).ToList();
            list.ForEach(l => Delete(l.Id));
        }
        public void DeleteByUserId(int userId)
        {
            List<Like> list = ToList().Where(l => l.UserId == userId).ToList();
            list.ForEach(l => Delete(l.Id));
        }
    }
}