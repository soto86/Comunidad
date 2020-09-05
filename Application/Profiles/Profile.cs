﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using Domain;

namespace Application.Profiles
{
    public class Profile
    {
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Image { get; set; }
        public string Bio { get; set; }

        [JsonPropertyName("following")]
        public bool IsFollowed { get; set; }
        public int FollowingCount { get; set; }
        public int FollowerCount { get; set; }
        public virtual ICollection<Photo> Photos { get; set; }
    }
}
