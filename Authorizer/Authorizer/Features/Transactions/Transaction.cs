using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Authorizer.Common;
using Authorizer.Features.Accounts;

namespace Authorizer.Features.Transactions
{
    public class Transaction
    {
        [JsonPropertyName("merchant")]
        public string Merchant { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("time")]
        [JsonConverter(typeof(DateTimeOffsetJsonConverter))]
        public DateTimeOffset Time { get; set; }

        public Transaction()
        {

        }

        public Transaction(string merchant, int amount, DateTimeOffset time)
        {
            Merchant = merchant;
            Amount = amount;
            Time = time;
        }
    }

    public class TransactionInput
    {
        [JsonPropertyName("transaction")]
        public Transaction Transaction { get; set; }
    }

    public class TransactionOutPut : CommonOutPut
    {
        [JsonPropertyName("account")]
        public Account Account { get; set; }

        public TransactionOutPut(Account account)
        {
            Account = account;
        }

        public TransactionOutPut(Account account, IList<string> violations)
        {
            Account = account;
            Violations = violations;
        }

    }

    public static class TransactionViolation
    {        
        public const string AccountNotInitialized = "account-not-initialized";
        public const string HighFrequencySmallInterval = "high-frequency-small-interval";
        public const string DoubledTransaction = "doubled-transaction";

    }
}
