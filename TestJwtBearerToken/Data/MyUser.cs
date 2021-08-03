using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestJwtBearerToken.Data
{
    public class MyUser : IdentityUser
    {
        public MyUser()
        {
            SystemRefreshTokens = new HashSet<SystemRefreshToken>();
        }
        public DateTime? BirthDate { get; set; }
        public virtual HashSet<SystemRefreshToken> SystemRefreshTokens { get; set; }
    }
}
