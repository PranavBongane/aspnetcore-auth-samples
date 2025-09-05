namespace JwtAuthDotNet9.Models
{
    public class TokenResponseDTO
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
