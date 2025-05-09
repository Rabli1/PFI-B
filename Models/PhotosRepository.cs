using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class PhotosRepository : Repository<Photo>
    {
        public void DeleteByOwnerId(int ownerId)
        {
            List<Photo> list = ToList().Where(p => p.OwnerId == ownerId).ToList();
            list.ForEach(p => {
                DB.Likes.DeleteByPhotoId(p.Id);
                DB.Comments.DeleteByPhotoId(p.Id);
                base.Delete(p.Id);
            });
        }
        public override bool Delete(int photoId)
        {
            try
            {
                Photo photoToDelete = DB.Photos.Get(photoId);
                if (photoToDelete != null)
                {
                    BeginTransaction();
                    DB.Likes.DeleteByPhotoId(photoId);
                    DB.Comments.DeleteByPhotoId(photoId);
                    base.Delete(photoId);
                    EndTransaction();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete Photo failed : Message - {ex.Message}");
                EndTransaction();
                return false;
            }
        }

        public void ResetLikesCount()
        {
            foreach (Photo photo in ToList())
            {
                photo.ResetCountsCalc();
            }
        }
    }
}