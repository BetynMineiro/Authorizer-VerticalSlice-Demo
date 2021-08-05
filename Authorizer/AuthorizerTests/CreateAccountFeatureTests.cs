using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Authorizer.Common;
using Authorizer.Features.Accounts;
using Xunit;

namespace AuthorizerTests
{
    public class CreateAccountFeatureTests
    {
        private ICreateAccountService createAccountService;
        private IDaoFactory _factoryDao;

        public CreateAccountFeatureTests()
        {

            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
            _factoryDao = new DaoFactory(cache);
            createAccountService = new CreateAccountService(_factoryDao);
        }

        [Fact]
        public async Task Create_First_Account_TestAsync()
        {
            var input = new AccountInput() { Account = new Account(true, 100) };
            var createdAccount = await createAccountService.CreateAccountAsync(input).ConfigureAwait(false);
            createdAccount.Should().BeOfType<AccountOutPut>("Invalid type returned");
            createdAccount.Account.Activecard.Should().BeTrue();
            createdAccount.Account.AvaliableLimit.Should().Be(100);
        }


        [Fact]
        public async Task Create_Registred_Account_TestAsync()
        {
            var input = new AccountInput() { Account = new Account(true, 100) };
            await createAccountService.CreateAccountAsync(input).ConfigureAwait(false);
            var createdAccount = await createAccountService.CreateAccountAsync(input).ConfigureAwait(false);
            createdAccount.Should().BeOfType<AccountOutPut>("Invalid type returned");
            createdAccount.Account.Activecard.Should().BeTrue();
            createdAccount.Account.AvaliableLimit.Should().Be(100);
            createdAccount.Violations.Should().HaveCount(1).And.OnlyContain(c => c.Equals(AccountViolation.AccountAlreadyInitialized));
        }
    }
}
