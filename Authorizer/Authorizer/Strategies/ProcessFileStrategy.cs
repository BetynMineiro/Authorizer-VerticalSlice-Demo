using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Authorizer.Common;
using Authorizer.Features.Accounts;
using Authorizer.Features.Transactions;


namespace Authorizer.Strategies
{
    public abstract class ProcessLineStrategy
    {
        public abstract ValueTask<string> ProcessAsync();
    }

    public interface IProcessFileStrategy
    {

        ProcessLineStrategy GetProcessLineStrategy(string json);

    }

    public class ProcessFileStrategy : IProcessFileStrategy
    {
        private readonly ICreateAccountService _CreateAccountService;
        private ICreateTransactionService _createTransactionService;

        public ProcessFileStrategy(ICreateAccountService createAccountService, ICreateTransactionService createTransactionService)
        {
            _CreateAccountService = createAccountService;
            _createTransactionService = createTransactionService;
        }

        public ProcessLineStrategy GetProcessLineStrategy(string json)
        {
            if (json.Contains("account", StringComparison.OrdinalIgnoreCase))
            {
                return new CreateAccountStrategy(_CreateAccountService, json);
            }
            return new CreateTransactionStrategy(_createTransactionService, json);

        }
    }

}
