using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Authorizer.Common;
using Authorizer.Strategies;

namespace Authorizer.Features.Accounts
{
    public class CreateAccountStrategy : ProcessLineStrategy
    {
        private ICreateAccountService _createAccountService;

        public string AccountJson { get; private set; }

        public CreateAccountStrategy(ICreateAccountService createAccountService, string accountJson)
        {
            _createAccountService = createAccountService;
            AccountJson = accountJson;
        }

        public override async ValueTask<string> ProcessAsync()
        {

            var account = JsonSerializer.Deserialize<AccountInput>(AccountJson);
            var response = await _createAccountService.CreateAccountAsync(account).ConfigureAwait(false);

            return JsonSerializer.Serialize(response);

        }
    }

    public interface ICreateAccountService {
        ValueTask<AccountOutPut> CreateAccountAsync(AccountInput account);
    }

    public class CreateAccountService : ICreateAccountService
    {
        private readonly IDao<Account> _accountDto;

        public CreateAccountService(IDaoFactory factory)
        {
            _accountDto = factory.GetInstance<Account>(); 
        }
                
        public async ValueTask<AccountOutPut> CreateAccountAsync(AccountInput accountInput)
        {
            var oldAccount = (await _accountDto.GetAsync().ConfigureAwait(false)).FirstOrDefault();


            if(oldAccount is null)
            {
                var addedAccount = await _accountDto.InsertAsync(accountInput.Account).ConfigureAwait(false);
                return new AccountOutPut(addedAccount);
            }

            return new AccountOutPut(oldAccount, new[] { AccountViolation.AccountAlreadyInitialized});


        }
    }
}
