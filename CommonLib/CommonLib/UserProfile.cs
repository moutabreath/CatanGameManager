﻿namespace CatanGameManager.CommonObjects
{
    public class UserProfile : Entity
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int TotalPoints { get; set; }
    }
}
