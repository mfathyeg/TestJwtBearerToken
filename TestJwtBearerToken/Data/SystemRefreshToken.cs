using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestJwtBearerToken.Data
{
    public class SystemRefreshToken
    {
        public long Id { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenTime { get; set; }

        [ForeignKey("MyUser")]
        public string UserId { get; set; }
        public MyUser MyUser { get; set; }  
    }
}
