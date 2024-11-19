using System.Security.Claims;

namespace WebCalenderAPI.Data
{
    public class CheckAccessToken
    {
        public ClaimsPrincipal claimsPrincipal { get; set; }
        public Boolean isExpired { get; set; }
    }
}
