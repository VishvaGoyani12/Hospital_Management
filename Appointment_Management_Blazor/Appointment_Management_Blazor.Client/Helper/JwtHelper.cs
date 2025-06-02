//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;

//namespace Appointment_Management_Blazor.Client.Helper
//{
//    public class JwtUser
//    {
//        public bool IsAuthenticated { get; set; }
//        public string? Email { get; set; }
//        public string? Role { get; set; }
//        public string? UserId { get; set; }
//    }

//    public static class JwtHelper
//    {
//        public static JwtUser ParseClaimsFromJwt(string? jwtToken)
//        {
//            if (string.IsNullOrEmpty(jwtToken))
//                return new JwtUser { IsAuthenticated = false };

//            try
//            {
//                var handler = new JwtSecurityTokenHandler();
//                var token = handler.ReadJwtToken(jwtToken);

//                var email = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email ||
//                                                            c.Type == ClaimTypes.Email ||
//                                                            c.Type == JwtRegisteredClaimNames.Sub)?.Value;
//                var role = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
//                var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

//                return new JwtUser
//                {
//                    IsAuthenticated = true,
//                    Email = email,
//                    Role = role,
//                    UserId = userId
//                };
//            }
//            catch
//            {
//                return new JwtUser { IsAuthenticated = false };
//            }
//        }
//    }
//}