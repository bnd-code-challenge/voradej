using BankAccountDAL;
using BankAccountModel;
using System.Diagnostics.CodeAnalysis;

namespace BankService
{
    public class BankAccountService
    {
        public static List<BankAccountEntity> BankAccounts = new List<BankAccountEntity>();
        public static List<LogEntity> Logs = new List<LogEntity>();

        public BankAccountService() { 
        
        }

        public List<BankAccountEntity> CreateAccount(BankAccountEntity bankAccount)
        {
            if (string.IsNullOrEmpty(bankAccount.Account) == false)
            {
                if (ValidateUniqueBankAccount(bankAccount.Account))
                {
                    BankAccounts.Add(bankAccount);
                }
            }
            
            return BankAccounts;
        }

        public List<BankAccountEntity> DepositAccount(DepositAccount depositAccount)
        {
            var existAccount = BankAccounts.FirstOrDefault(x => x.Account == depositAccount.Account);
            if (existAccount != null)
            {
                var fee = (depositAccount.Amount * decimal.Parse("0.1")) / 100;
                var amtAfterFee = depositAccount.Amount - fee;
                existAccount.Amount += amtAfterFee;
            }

            return BankAccounts;
        }

        public List<BankAccountEntity> TransferAccount(TransferAccount transferAccount)
        {
            var accountSource = BankAccounts.FirstOrDefault(x => x.Account == transferAccount.AccountSource);
            var accountDestination = BankAccounts.FirstOrDefault(x => x.Account == transferAccount.AccountDestination);

            if (accountSource != null && accountDestination != null)
            {
                accountSource.Amount -= transferAccount.Amount;
                accountDestination.Amount += transferAccount.Amount;
            }

            return BankAccounts;
        }

        public bool ValidateUniqueBankAccount(string account)
        {
            var bankAccount = BankAccounts.FirstOrDefault(x => x.Account == account);
            if (bankAccount == null)
                return true;
            else
                return false;
        }

        public bool ValidateExistBankAccount(string account)
        {
            var bankAccount = BankAccounts.FirstOrDefault(x => x.Account == account);
            if (bankAccount != null)
                return true;
            else
                return false;
        }

        public bool ValidateTransferRemainAmount(TransferAccount transferAccount)
        {
            var accountSource = BankAccounts.FirstOrDefault(x => x.Account == transferAccount.AccountSource);
            if (accountSource != null)
            {
                if (accountSource.Amount >= transferAccount.Amount)
                    return true;
                else 
                    return false;
            }

            return false;
        }
        
        public bool WriteLog(LogEntity logEntity)
        {
            Logs.Add(logEntity);
            return true;
        }

    }
}
