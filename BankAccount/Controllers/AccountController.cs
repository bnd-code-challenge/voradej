using BankAccountModel;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using BankService;
using BankAccountDAL;
using System.Reflection.Metadata.Ecma335;
using System.Net;
using System.Collections.Generic;

namespace Controllers
{
    public class AccountController : Controller
    {
        public string Message { get; set; }
        public static List<BankAccountEntity> BankAccounts = new List<BankAccountEntity>();
        BankAccountService _bankAccountService;

        public AccountController(BankAccountService bankAccountService)
        {
            Message = "";
            _bankAccountService = bankAccountService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("CreateBankAccount")]
        public async Task<DataResponse<List<BankAccountEntity>>?> CreateBankAccount(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.FirstName) || string.IsNullOrEmpty(customer.LastName))
            {
                Message = "Please fill first name and last name";
                _bankAccountService.WriteLog(new LogEntity(){LogDate = DateTime.Now, FirstName = customer.FirstName, LastName = customer.LastName, LogMessage = $"CreateBankAccount : {Message}"});
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, Message, null);                
            }

            string url = "https://randommer.io/iban-generator";
            using (HttpClient client = new HttpClient())
            {
                var html = await client.GetStringAsync(url);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var ibanNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='div-result']");
                if (ibanNode != null)
                {
                    var bankAccount = new BankAccountEntity()
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Account = ibanNode.InnerText,
                        Amount = 0
                    };

                    if (!_bankAccountService.ValidateUniqueBankAccount(bankAccount.Account))
                    {
                        Message = "Bank account must be unique";
                        _bankAccountService.WriteLog(new LogEntity(){LogDate = DateTime.Now,FirstName = customer.FirstName,LastName = customer.LastName, LogMessage = $"CreateBankAccount : {Message}" });
                        return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, Message, null);
                    }
                    else
                        return new DataResponse<List<BankAccountEntity>>(_bankAccountService.CreateAccount(bankAccount));                                           
                }
                else
                {
                    Message = "Can not connect to iban-generator";
                    _bankAccountService.WriteLog(new LogEntity() { LogDate = DateTime.Now, FirstName = customer.FirstName, LastName = customer.LastName, LogMessage = $"CreateBankAccount : {Message}" });
                    return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.Conflict, Message, null);
                }
            }
        }

        [HttpPost("DepositAccount")]
        public DataResponse<List<BankAccountEntity>>? DepositAccount(DepositAccount depositAccount)
        {
            if (string.IsNullOrEmpty(depositAccount.Account) || depositAccount.Amount == null)
            {
                Message = "Please fill account and amount";
                _bankAccountService.WriteLog(new LogEntity() { LogDate = DateTime.Now, AccountSource = depositAccount.Account, LogMessage = $"DepositAccount : {Message}" });
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, Message, null);
            }

            if (depositAccount.Amount <= 0)
            {
                Message = "The amount must be positive";
                _bankAccountService.WriteLog(new LogEntity() { LogDate = DateTime.Now, AccountSource = depositAccount.Account, LogMessage = $"DepositAccount : {Message}" });
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, Message, null);
            }

            if (!_bankAccountService.ValidateExistBankAccount(depositAccount.Account))
            {
                Message = "This bank account does not exist";
                _bankAccountService.WriteLog(new LogEntity() { LogDate = DateTime.Now, AccountSource = depositAccount.Account, LogMessage = $"DepositAccount : {Message}" });
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, Message, null);
            }

            return new DataResponse<List<BankAccountEntity>>(_bankAccountService.DepositAccount(depositAccount));
        }

        [HttpPost("TransferAccount")]
        public DataResponse<List<BankAccountEntity>>? TransferAccount(TransferAccount transferAccount)
        {
            if (string.IsNullOrEmpty(transferAccount.AccountSource))
            {
                Message = "Please fill source account";
                _bankAccountService.WriteLog(new LogEntity() { LogDate = DateTime.Now, AccountSource = transferAccount.AccountSource, 
                AccountDestination = transferAccount.AccountDestination, LogMessage = $"TransferAccount : {Message}" });
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, Message, null);
            }


            if (string.IsNullOrEmpty(transferAccount.AccountDestination))
            {
                Message = "Please fill destination account";
                _bankAccountService.WriteLog(new LogEntity(){LogDate = DateTime.Now, AccountSource = transferAccount.AccountSource, 
                AccountDestination = transferAccount.AccountDestination, LogMessage = $"TransferAccount : {Message}"});
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, Message, null);
            }

            if (transferAccount.Amount == null)
            {
                Message = "Please fill amount";
                _bankAccountService.WriteLog(new LogEntity(){LogDate = DateTime.Now, AccountSource = transferAccount.AccountSource,
                AccountDestination = transferAccount.AccountDestination, LogMessage = $"TransferAccount : {Message}"});
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, Message, null);
            }

            if (transferAccount.Amount <= 0)
            {
                Message = "The amount must be positive";
                _bankAccountService.WriteLog(new LogEntity(){LogDate = DateTime.Now, AccountSource = transferAccount.AccountSource,
                AccountDestination = transferAccount.AccountDestination,LogMessage = $"TransferAccount : {Message}"});
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, "The amount must be positive", null);
            }

            if (!_bankAccountService.ValidateExistBankAccount(transferAccount.AccountSource))
            {
                Message = "This account source does not exist";
                _bankAccountService.WriteLog(new LogEntity(){LogDate = DateTime.Now, AccountSource = transferAccount.AccountSource,
                AccountDestination = transferAccount.AccountDestination,LogMessage = $"TransferAccount : {Message}"});
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, "This account source does not exist", null);
            }

            if (!_bankAccountService.ValidateExistBankAccount(transferAccount.AccountDestination))
            {
                Message = "This account destination does not exist";
                _bankAccountService.WriteLog(new LogEntity(){LogDate = DateTime.Now, AccountSource = transferAccount.AccountSource,
                AccountDestination = transferAccount.AccountDestination,LogMessage = $"TransferAccount : {Message}"});
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, "This account destination does not exist", null);
            }

            if (!_bankAccountService.ValidateTransferRemainAmount(transferAccount))
            {
                Message = "Insufficient balance";
                _bankAccountService.WriteLog(new LogEntity(){LogDate = DateTime.Now, AccountSource = transferAccount.AccountSource,
                AccountDestination = transferAccount.AccountDestination, LogMessage = $"TransferAccount : {Message}"});
                return new DataResponse<List<BankAccountEntity>>((int)HttpStatusCode.BadRequest, "Insufficient balance", null);
            }

            return new DataResponse<List<BankAccountEntity>>(_bankAccountService.TransferAccount(transferAccount));
        }
    }
}
