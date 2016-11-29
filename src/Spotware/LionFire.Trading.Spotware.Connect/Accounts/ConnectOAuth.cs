using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect.Accounts
{
    public class ConnectOAuth
    {


        public string WebAuthenticateUrl = "https://connect.spotware.com/oauth/v2/auth?access_type={AccessType}&approval_prompt={ApprovalPrompt}&client_id={ClientId}&redirect_uri={RedirectUri}&response_type={ResponseType}&scope={Scope}";



        public void Authenticate()
        {
            ISpotwareConnectAppInfo apiInfo = Defaults.Get<ISpotwareConnectAppInfo>();
            
            //var RedirectUri = "https://connect.spotware.com/apps/50";
            var Scope = "trading";
            //var message = "On the next screen, please allow access to cTrader ID and any accounts which you would like to use in the current session.";

            var AccessType = "online";
            var ApprovalPrompt = "auto";
            var ResponseType = "code";

            var url = WebAuthenticateUrl
                .Replace("{ClientId}", apiInfo.ClientPublicId)
                .Replace("{RedirectUri}", apiInfo.RedirectUri)
                .Replace("{Scope}", Scope)
                .Replace("{AccessType}", AccessType)
                .Replace("{ApprovalPrompt}", ApprovalPrompt)
                .Replace("{ResponseType}", ResponseType)
                ;
        }
    }
}
