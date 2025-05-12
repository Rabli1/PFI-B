using JSON_DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace PhotosManager.Models
{
    public class User : Record
    {
        const string Avatars_Folder = @"/App_Assets/Users/";
        const string Default_Avatar = @"no_avatar.png";

        public User()
        {
            Id = 0;
            Blocked = false;
            Admin = false;
            Online = false;
            Verified = false;
        }
        [JsonIgnore]
        public static string DefaultImage { get { return Avatars_Folder + Default_Avatar; } }

        #region Data Members

        [Display(Name = "Nom"), Required(ErrorMessage = "Obligatoire")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Remote("EmailAvailable", "Accounts", AdditionalFields = "", HttpMethod = "POST", ErrorMessage = "Ce courriel est déjà utilisé.")]
        [Display(Name = "Courriel"), Required(ErrorMessage = "Obligatoire")]
        public string Email { get; set; }

        [JsonIgnore]
        [Display(Name = "Confirmation")]
        [Compare("Email", ErrorMessage = "Ne correspond pas.")]
        public string ConfirmEmail { get; set; }

        [Display(Name = "Mot de passe")]
        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [StringLength(50, ErrorMessage = "Le mot de passe doit comporter au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [JsonIgnore]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmation")]
        [Compare("Password", ErrorMessage = "Ne correspond pas.")]
        public string ConfirmPassword { get; set; }

        public bool Online { get; set; }
        public bool Admin { get; set; }
        public bool Blocked { get; set; }
        public bool Verified { get; set; }
        public bool Notifications { get; set; }

        [ImageAsset(Avatars_Folder, Default_Avatar)]
        public string Avatar { get; set; } = DefaultImage;
        #endregion

        #region View members
        [JsonIgnore]
        public bool IsAdmin { get { return Admin; } }
        [JsonIgnore]
        public bool IsBlocked { get { return Blocked; } }
        [JsonIgnore]
        public bool IsOnline { get { return Online; } }
        [JsonIgnore]
        public bool hasNotifications { get { return Notifications; } }
        #endregion

        [JsonIgnore]
        public List<Login> Logins { get { return DB.Logins.ToList().Where(l => l.UserId == Id).ToList(); } }

        public void DeleteLogins()
        {
            Logins.ForEach(l => DB.Logins.Delete(l.Id));
        }

        public void DeletePhotos()
        {
            DB.Photos.DeleteByOwnerId(Id);
        }
    }
}