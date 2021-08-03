namespace TestJwtBearerToken.Models
{
    public class LoginDto
    {
        public LoginDto()
        {
            Status = LoginStatus.NotAuthorized;
        }
        public LoginDto(LoginStatus status)
        {
            Status = status;
        }
        public LoginStatus Status { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; } 
    }
}
