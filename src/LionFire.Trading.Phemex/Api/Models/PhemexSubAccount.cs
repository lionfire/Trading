using Newtonsoft.Json;

namespace LionFire.Trading.Phemex.Api.Models;

public class PhemexSubAccount
{
    [JsonProperty("userId")]
    public long UserId { get; set; }
    
    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonProperty("nickName")]
    public string NickName { get; set; } = string.Empty;
    
    [JsonProperty("passwordState")]
    public int PasswordState { get; set; }
    
    [JsonProperty("clientCnt")]
    public int ClientCount { get; set; }
    
    [JsonProperty("totp")]
    public int TotpState { get; set; }
    
    [JsonProperty("logon")]
    public int LogonState { get; set; }
    
    [JsonProperty("parentId")]
    public long ParentId { get; set; }
    
    [JsonProperty("parentEmail")]
    public string ParentEmail { get; set; } = string.Empty;
    
    [JsonProperty("status")]
    public int Status { get; set; }
    
    [JsonProperty("isParent")]
    public bool IsParent { get; set; }
    
    [JsonProperty("createTime")]
    public long CreateTime { get; set; }
}

public class PhemexSubAccountsResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }
    
    [JsonProperty("msg")]
    public string Message { get; set; } = string.Empty;
    
    [JsonProperty("data")]
    public PhemexSubAccountData? Data { get; set; }
}

public class PhemexSubAccountData
{
    [JsonProperty("rows")]
    public List<PhemexSubAccount> Rows { get; set; } = new();
}