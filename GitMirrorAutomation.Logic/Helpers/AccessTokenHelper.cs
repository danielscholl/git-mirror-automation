using GitMirrorAutomation.Logic.Config;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Logic.Helpers
{
    public class AccessTokenHelper
    {
        private static readonly Regex _keyVaultRegex = new Regex(@"https:\/\/([^.])+\.vault\.azure\.net");

        public async Task<string> GetAsync(AccessToken accessToken, CancellationToken cancellationToken)
        {
            var match = _keyVaultRegex.Match(accessToken.Source);
            if (!match.Success)
                throw new ArgumentException("Currently only keyvault source is supported but found: " + accessToken.Source);
            var tokenProvider = new AzureServiceTokenProvider();
            var kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));

            var secret = await kvClient.GetSecretAsync(accessToken.Source, accessToken.SecretName, cancellationToken);
            return secret.Value;
        }
    }
}
