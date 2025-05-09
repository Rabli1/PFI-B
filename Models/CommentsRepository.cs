using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace PhotosManager.Models
{
    public class CommentsRepository : Repository<Comment>
    {
        public override bool Delete(int Id)
        {
            Comment comment = DB.Comments.Get(Id);
            if (comment != null)
            {
                List<Comment> responses = ToList().Where(c => c.ParentId == Id).ToList();
                responses.ForEach(r => Delete(r.Id));
                comment.Likes.ForEach(l => DB.Likes.Delete(l.Id));
                return base.Delete(Id);
            }
            return false;
        }

        public void DeleteByPhotoId(int Id)
        {
            // select all comments related to photo of Id
            List<Comment> comments = ToList().Where(c => c.PhotoId == Id).ToList();
            // Do not need to call recursive Delete, all photo Id comments are selected
            comments.ForEach(c => base.Delete(c.Id));
        }
        public void DeleteByUserId(int Id)
        {
            // select all comments related to user of Id
            List<Comment> comments = ToList().Where(c => c.OwnerId == Id).ToList();
            // some comments might have responses, must call recursive delete
            comments.ForEach(c => Delete(c.Id));
        }
    }
}