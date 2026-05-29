using AdysTech.CredentialManager;
using Newtonsoft.Json;
using System.Net;

namespace XIVLauncher.Accounts
{
    public class XivAccount
    {
        private const string CREDS_PREFIX = "XIVLAUNCHER";

        [JsonIgnore]
        public string Id => $"{UserName}-{UseOtp}-{UseSteamServiceAccount}";

        public override string ToString() => Id;

        public string UserName { get; private set; }

        [JsonIgnore]
        public string Password
        {
            get
            {
                var credentials = CredentialManager.GetCredentials($"{CREDS_PREFIX}-{UserName.ToLower()}");
                return credentials?.Password ?? string.Empty;
            }
            set
            {
                var target = $"{CREDS_PREFIX}-{UserName.ToLower()}";

                if (!SavePassword)
                {
                    if (CredentialManager.GetCredentials(target) != null)
                        CredentialManager.RemoveCredentials(target);
                }
                else if (!string.IsNullOrWhiteSpace(value))
                {
                    CredentialManager.SaveCredentials(target, new NetworkCredential
                    {
                        UserName = UserName,
                        Password = value
                    });
                }
            }
        }

        public bool SavePassword { get; set; }
        public bool UseSteamServiceAccount { get; set; }
        public bool UseOtp { get; set; }

        public string ChosenCharacterName;
        public string ChosenCharacterWorld;

        public string ThumbnailUrl;

        public string LastSuccessfulOtp;

        public XivAccount(string userName)
        {
            UserName = userName.ToLower();
        }

        public string FindCharacterThumb()
        {
            if (string.IsNullOrEmpty(ChosenCharacterName) || string.IsNullOrEmpty(ChosenCharacterWorld))
                return null;

            // STUB
            return null;
        }
    }
}
