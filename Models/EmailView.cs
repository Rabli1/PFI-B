using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Models
{
    public class EmailView
    {
        [Display(Name = "Courriel"), EmailAddress(ErrorMessage = "Invalide"), Required(ErrorMessage = "Obligatoire")]
        /*[Remote("EmailExist", "Accounts", HttpMethod = "POST", ErrorMessage = "Courriel introuvable.")]*/
        public string Email { get; set; }
    }
}