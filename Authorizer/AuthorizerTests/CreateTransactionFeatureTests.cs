using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Authorizer.Common;
using Authorizer.Features.Accounts;
using Authorizer.Features.Transactions;
using Xunit;

namespace AuthorizerTests
{
    public class CreateTransactionFeatureTests
    {
        private ICreateTransactionService createTransactionService;
        private ICreateAccountService createAccountService;
        private IDaoFactory _factoryDao;

        public CreateTransactionFeatureTests()
        {
            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
            _factoryDao = new DaoFactory(cache);
            createTransactionService = new CreateTransactionService(_factoryDao);
            createAccountService = new CreateAccountService(_factoryDao);
        }

        [Fact]
        public async Task Create_Transaction_with_Account_Initialized_TestAsync()
        {
            var account = new AccountInput() { Account = new Account(true, 100) };
            await createAccountService.CreateAccountAsync(account).ConfigureAwait(false);

            var input = new TransactionInput() { Transaction = new Transaction("Bobs", 80, DateTimeOffset.UtcNow) };
            var createdTransaction = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);
            createdTransaction.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction.Account.AvaliableLimit.Should().Be(20, "Invalid current limit");

        }

        [Fact]
        public async Task Create_Transaction_with_Account_not_Initialized_TestAsync()
        {
            var input = new TransactionInput() { Transaction = new Transaction("Bobs", 100, DateTimeOffset.UtcNow) };
            var createdTransaction = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);
            createdTransaction.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction.Account.Should().BeNull();
            createdTransaction.Violations.Should().HaveCount(1).And.OnlyContain(c => c.Equals(TransactionViolation.AccountNotInitialized));
        }

        [Fact]
        public async Task Create_Transaction_with_Inative_Card_TestAsync()
        {
            var account = new AccountInput() { Account = new Account(false, 100) };
            await createAccountService.CreateAccountAsync(account).ConfigureAwait(false);

            var input = new TransactionInput() { Transaction = new Transaction("Bobs", 80, DateTimeOffset.UtcNow) };
            var createdTransaction = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);
            createdTransaction.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction.Account.AvaliableLimit.Should().Be(100, "Invalid current limit");
            createdTransaction.Violations.Should().HaveCount(1).And.OnlyContain(c => c.Equals(AccountViolation.CardNotActive));
        }

        [Fact]
        public async Task Create_Transaction_with_Insufficient_Limit_Account_TestAsync()
        {
            var account = new AccountInput() { Account = new Account(true, 100) };
            await createAccountService.CreateAccountAsync(account).ConfigureAwait(false);

            var input = new TransactionInput() { Transaction = new Transaction("Bobs", 180, DateTimeOffset.UtcNow) };
            var createdTransaction = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);
            createdTransaction.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction.Account.AvaliableLimit.Should().Be(100, "Invalid current limit");
            createdTransaction.Violations.Should().HaveCount(1).And.OnlyContain(c => c.Equals(AccountViolation.InsufficientLimit));
        }

        [Fact]
        public async Task Create_Transaction_with_High_Frequency_In_Small_Interval_TestAsync()
        {
            var account = new AccountInput() { Account = new Account(true, 1000) };
            await createAccountService.CreateAccountAsync(account).ConfigureAwait(false);

            var input1 = new TransactionInput() { Transaction = new Transaction("Bobs", 100, DateTimeOffset.UtcNow) };
            var input2 = new TransactionInput() { Transaction = new Transaction("Bk", 200, DateTimeOffset.UtcNow) };
            var input3 = new TransactionInput() { Transaction = new Transaction("Mac", 300, DateTimeOffset.UtcNow) };
            var input4 = new TransactionInput() { Transaction = new Transaction("Habbibs", 400, DateTimeOffset.UtcNow) };

            var createdTransaction1 = await createTransactionService.CreateTransactionAsync(input1).ConfigureAwait(false);
            var createdTransaction2 = await createTransactionService.CreateTransactionAsync(input2).ConfigureAwait(false);
            var createdTransaction3 = await createTransactionService.CreateTransactionAsync(input3).ConfigureAwait(false);
            var createdTransaction4 = await createTransactionService.CreateTransactionAsync(input4).ConfigureAwait(false);


            createdTransaction1.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction1.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction1.Account.AvaliableLimit.Should().Be(900, "Invalid current limit");
            createdTransaction1.Violations.Should().HaveCount(0);

            createdTransaction2.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction2.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction2.Account.AvaliableLimit.Should().Be(700, "Invalid current limit");
            createdTransaction2.Violations.Should().HaveCount(0);

            createdTransaction3.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction3.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction3.Account.AvaliableLimit.Should().Be(400, "Invalid current limit");
            createdTransaction3.Violations.Should().HaveCount(0);


            createdTransaction4.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction4.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction4.Account.AvaliableLimit.Should().Be(400, "Invalid current limit");
            createdTransaction4.Violations.Should().HaveCount(1).And.OnlyContain(c => c.Equals(TransactionViolation.HighFrequencySmallInterval));
        }

        [Fact]
        public async Task Create_Transaction_with_Double_Transaction_TestAsync()
        {
            var account = new AccountInput() { Account = new Account(true, 1000) };
            await createAccountService.CreateAccountAsync(account).ConfigureAwait(false);

            var input = new TransactionInput() { Transaction = new Transaction("Bobs", 100, DateTimeOffset.UtcNow) };

            var createdTransaction1 = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);
            var createdTransaction2 = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);
            var createdTransaction3 = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);



            createdTransaction1.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction1.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction1.Account.AvaliableLimit.Should().Be(900, "Invalid current limit");
            createdTransaction1.Violations.Should().HaveCount(0);

            createdTransaction2.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction2.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction2.Account.AvaliableLimit.Should().Be(800, "Invalid current limit");
            createdTransaction2.Violations.Should().HaveCount(0);

            createdTransaction3.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction3.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction3.Account.AvaliableLimit.Should().Be(800, "Invalid current limit");
            createdTransaction3.Violations.Should().HaveCount(1).And.OnlyContain(c => c.Equals(TransactionViolation.DoubledTransaction));
        }

        [Fact]
        public async Task Create_Transaction_with_High_Frequency_And_Insufficient_Limit_And_Double_Transaction_TestAsync()
        {
            var account = new AccountInput() { Account = new Account(true, 1000) };
            await createAccountService.CreateAccountAsync(account).ConfigureAwait(false);

            var input = new TransactionInput() { Transaction = new Transaction("Bobs", 250, DateTimeOffset.UtcNow) };
            var input2 = new TransactionInput() { Transaction = new Transaction("Bk", 400, DateTimeOffset.UtcNow) };

            var createdTransaction1 = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);
            var createdTransaction2 = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);
            var createdTransaction3 = await createTransactionService.CreateTransactionAsync(input2).ConfigureAwait(false);
            var createdTransaction4 = await createTransactionService.CreateTransactionAsync(input).ConfigureAwait(false);



            createdTransaction1.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction1.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction1.Account.AvaliableLimit.Should().Be(750, "Invalid current limit");
            createdTransaction1.Violations.Should().HaveCount(0);

            createdTransaction2.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction2.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction2.Account.AvaliableLimit.Should().Be(500, "Invalid current limit");
            createdTransaction2.Violations.Should().HaveCount(0);

            createdTransaction3.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction3.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction3.Account.AvaliableLimit.Should().Be(100, "Invalid current limit");
            createdTransaction3.Violations.Should().HaveCount(0);

            createdTransaction4.Should().BeOfType<TransactionOutPut>("Invalid type returned");
            createdTransaction4.Account.Should().BeOfType<Account>("Invalid type returned").And.NotBeNull();
            createdTransaction4.Account.AvaliableLimit.Should().Be(100, "Invalid current limit");
            createdTransaction4.Violations.Should().HaveCount(3)
                .And.Contain(c => c.Equals(TransactionViolation.DoubledTransaction))
                .And.Contain(c => c.Equals(TransactionViolation.HighFrequencySmallInterval))
                .And.Contain(c => c.Equals(TransactionViolation.DoubledTransaction));
        }
    }
}
