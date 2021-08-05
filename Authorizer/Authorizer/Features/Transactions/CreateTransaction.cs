using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Authorizer.Common;
using Authorizer.Features.Accounts;
using Authorizer.Strategies;

namespace Authorizer.Features.Transactions
{
    public class CreateTransactionStrategy : ProcessLineStrategy
    {
        private ICreateTransactionService _createTransactionService;

        public string TransactionJson { get; private set; }

        public CreateTransactionStrategy(ICreateTransactionService createTransactionService, string transactionJson)
        {
            _createTransactionService = createTransactionService;
            TransactionJson = transactionJson;
        }


        public override async ValueTask<string> ProcessAsync()
        {
            var transaction = JsonSerializer.Deserialize<TransactionInput>(TransactionJson);
            var response = await _createTransactionService.CreateTransactionAsync(transaction).ConfigureAwait(false);

            return JsonSerializer.Serialize(response);

        }
    }

    public interface ICreateTransactionService
    {
        ValueTask<TransactionOutPut> CreateTransactionAsync(TransactionInput transactionInput);
    }

    public class CreateTransactionService : ICreateTransactionService
    {
        private readonly IDao<Account> _accountDao;
        private readonly IDao<Transaction> _transactionDao;

        public CreateTransactionService(IDaoFactory factory)
        {
            _accountDao = factory.GetInstance<Account>();
            _transactionDao = factory.GetInstance<Transaction>();
        }

        public async ValueTask<TransactionOutPut> CreateTransactionAsync(TransactionInput transactionInput)
        {
            var account = (await _accountDao.GetAsync().ConfigureAwait(false)).FirstOrDefault();
            var transations = (await _transactionDao.GetAsync().ConfigureAwait(false)).ToList();
            var valid = IsValidTransaction(account, transations, transactionInput.Transaction);
            if (valid.sucess)
            {
                var addedTransaction = await _transactionDao.InsertAsync(transactionInput.Transaction).ConfigureAwait(false);
                var newAccount = await _accountDao.UpdateAsync(account, UpdateAccountAmountByTransaction(account, addedTransaction)).ConfigureAwait(false);
                return new TransactionOutPut(newAccount);
            }

            return new TransactionOutPut(account, valid.errors);
        }


        private Account UpdateAccountAmountByTransaction(Account account, Transaction transaction)
        {
            return new Account(account.Activecard, account.AvaliableLimit - transaction.Amount);
        }


        private (bool sucess, IList<string> errors) IsValidTransaction(Account account, IList<Transaction> allTransactions, Transaction currentTransaction)
        {
            var sucess = true;
            var listError = new List<string>();


            if (ValidateCondition(account is null, TransactionViolation.AccountNotInitialized, ref sucess, ref listError) ||
                ValidateCondition(!account.Activecard, AccountViolation.CardNotActive, ref sucess, ref listError))
            {
                return (sucess: sucess, errors: listError);
            }

            ValidateCondition(currentTransaction.Amount > account.AvaliableLimit, AccountViolation.InsufficientLimit, ref sucess, ref listError);
            ValidateCondition(allTransactions.Where(c => c.Time >= currentTransaction.Time.AddMinutes(-2)).Count() >= 3, TransactionViolation.HighFrequencySmallInterval, ref sucess, ref listError);
            ValidateCondition(allTransactions.Where(c => c.Amount.Equals(currentTransaction.Amount) && c.Merchant.Equals(currentTransaction.Merchant) && c.Time >= currentTransaction.Time.AddMinutes(-2)).Count() >= 2, TransactionViolation.DoubledTransaction, ref sucess, ref listError);

            return (sucess: sucess, errors: listError);
        }

        private bool ValidateCondition(bool condition, string error, ref bool sucess, ref List<string> listError)
        {

            if (condition)
            {
                listError.Add(error);
                sucess = false;
                return true;
            }

            return false;
        }
    }
}
