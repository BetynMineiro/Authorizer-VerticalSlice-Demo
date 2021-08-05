using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Authorizer.Features.Accounts;
using Authorizer.Features.Transactions;
using Authorizer.Strategies;
using Xunit;

namespace AuthorizerTests
{
    public class ProcessFileStrategyTest
    {
        private Mock<ICreateTransactionService> createTransactionServiceMock;
        private Mock<ICreateAccountService> createAccountServiceMock;
        private IProcessFileStrategy ProcessFileStrategy;
        private AccountOutPut AccountResponseMock = new AccountOutPut(new Account(true, 100));
        private TransactionOutPut TransactionResponseMock = new TransactionOutPut(new Account(true, 100));

        public ProcessFileStrategyTest()
        {
            createTransactionServiceMock = new Mock<ICreateTransactionService>();
            createAccountServiceMock = new Mock<ICreateAccountService>();

            createAccountServiceMock.Setup(c => c.CreateAccountAsync(It.IsAny<AccountInput>())).Returns(new ValueTask<AccountOutPut>(AccountResponseMock));
            createTransactionServiceMock.Setup(c => c.CreateTransactionAsync(It.IsAny<TransactionInput>())).Returns(new ValueTask<TransactionOutPut>(TransactionResponseMock));

            ProcessFileStrategy = new ProcessFileStrategy(createAccountServiceMock.Object, createTransactionServiceMock.Object);
        }

        [Fact]
        public async Task Create_CreateAccount_Strategy_TestAsync()
        {
            var input = @"{""account"": {""active-card"": true, ""available-limit"": 1000}}";

            var process = ProcessFileStrategy.GetProcessLineStrategy(input);
            var result = await process.ProcessAsync().ConfigureAwait(false);
            process.Should().BeOfType<CreateAccountStrategy>("Invalid type returned");
            createAccountServiceMock.Verify(c => c.CreateAccountAsync(It.IsAny<AccountInput>()), Times.Once);
            result.Should().Be(JsonSerializer.Serialize(AccountResponseMock));
        }

        [Fact]
        public async Task Create_CreateTransaction_Strategy_TestAsync()
        {
            var input = @"{""transaction"": {""merchant"": ""Uber"", ""amount"": 80, ""time"": ""2019-02-13T11:01:31.000Z""}}";

            var process = ProcessFileStrategy.GetProcessLineStrategy(input);
            var result = await process.ProcessAsync().ConfigureAwait(false);
            process.Should().BeOfType<CreateTransactionStrategy>("Invalid type returned");
            createTransactionServiceMock.Verify(c => c.CreateTransactionAsync(It.IsAny<TransactionInput>()), Times.Once);
            result.Should().Be(JsonSerializer.Serialize(TransactionResponseMock));
        }
    }
}
