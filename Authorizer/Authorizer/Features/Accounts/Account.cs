using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Authorizer.Common;

namespace Authorizer.Features.Accounts
{

    public class Account
    {
        public Account(bool activecard, int avaliableLimit)
        {
            Activecard = activecard;
            AvaliableLimit = avaliableLimit;
        }
        [JsonPropertyName("active-card")]
  
        public bool Activecard { get; private set; } 

        [JsonPropertyName("available-limit")]
      
        public int AvaliableLimit { get; private set; }
    }

    public class AccountInput
    {
        [JsonPropertyName("account")]
        public Account Account { get; set; }
    }

    public class AccountOutPut : CommonOutPut
    {
        [JsonPropertyName("account")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Account Account { get; set; }

        public AccountOutPut(Account account)
        {
            Account = account;
        }

        public AccountOutPut(Account account, IList<string> violations)
        {
            Account = account;
            Violations = violations;
        }

    }

    public static class AccountViolation
    {
        public const string AccountAlreadyInitialized = "account-already-initialized";
        public const string CardNotActive = "card-not-active";
        public const string InsufficientLimit = "insufficient-limit";

    }
}
