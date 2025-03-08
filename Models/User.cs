using JSON_DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JsonDemo.Models
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
        }
        public User Clone()
        {
            return JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(this));
        }
        [JsonIgnore]
        public static string DefaultImage { get { return Avatars_Folder + Default_Avatar; } }

        #region Data Members
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Admin { get; set; }
        public bool Blocked { get; set; }

        [ImageAsset(Avatars_Folder, Default_Avatar)]
        public string Avatar { get; set; } = DefaultImage;
        #endregion

        #region View members
        [JsonIgnore]
        public bool IsAdmin { get { return Admin; } }
        [JsonIgnore]
        public bool IsBlocked { get { return Blocked; } }
        #endregion    
    }
}