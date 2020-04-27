using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngicateWpf
{
    public class AuthResult
    {
        public string Error { get; set; }
        public string HTTPStatus { get; set; }
        public string Message { get; set; }
        public string Access_Token { get; set; }
        public string Callback_URL { get; set; }
        public string AuthCodeName { get; set; }
        public string Login_URL { get; set; }
        public string Client_ID_Body { get; set; }
        public string Client_Secret_Body { get; set; }
        public string Grant_Type_Body { get; set; }
        public string Content_Type_Header { get; set; }
        public string Scope_Body { get; set; }
        public string AuthCode_Body { get; set; }
        public string Refresh_Token { get; set; }
        public string ID_Token { get; set; }
        public string TenantID { get; set; }
    }
    public class MSGraphAuthTokens
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string scope { get; set; }
        public string refresh_token { get; set; }
        public string id_token { get; set; }
    }
    public class MSGraphUserModel
    {
        public string[] businessPhones { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string jobTitle { get; set; }
        public string mail { get; set; }
        public string mobilePhone { get; set; }
        public string officeLocation { get; set; }
        public string preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
        public string id { get; set; }
    }
    public class CEDManagementConsoleUser
    {
        public string RollTitle { get; set; }
        public bool IsAssembler { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAssistant { get; set; }
        public bool IsReviewer { get; set; }
        public string AuthCode { get; set; }
        public MSGraphUserModel AzureADProperties { get; set; }
        public MSGraphAuthTokens Tokens { get; set; }
        public TokenState TokensState { get; set; }
        public AuthErrorModel AuthError { get; set; }
    }
    public class AuthErrorModel
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public List<int> error_codes { get; set; }
        public string timestamp { get; set; }
        public string trace_id { get; set; }
        public string correlation_id { get; set; }
    }
    public class PasswordVerified
    {
        public DateTime CheckDate { get; set; }
        public String CheckedBy { get; set; }
        public AccountStore StoreType { get; set; }
        bool CheckResult { get; set; }
        public LogModel CheckLog { get; set; }
        public string CryptedPassword { get; set; }
    }
    public enum AccountStore
    {
        Undefined,
        Domain,
        CRMS
    }
    [Flags]
    public enum TokenState
    {
        Undefined = 1,
        Initial = 2,
        ActiveAuthCode = 4,
        ActiveAccessToken = 8,
        ActiveRefreshToken = 16
    }
}
