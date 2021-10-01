using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountRepo;

        public PaymentService(IAccountDataStore accountRepo)
        {
            _accountRepo = accountRepo;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            if (request is null)
            {
                return new MakePaymentResult { Success = false };
            }

            var account = _accountRepo.GetAccount(request.DebtorAccountNumber);

            if (!ValidateRequest(request, account))
            {
                return new MakePaymentResult { Success = false };
            }
            
            account.Balance -= request.Amount;

            _accountRepo.UpdateAccount(account);

            return new MakePaymentResult { Success = true };
        }

        private bool ValidateRequest(MakePaymentRequest request, Account account)
        {
            if (account is null)
            {
                return false;
            }

            var vc = new ValidationContext(request);

            if (!Validator.TryValidateObject(request, vc, new List<ValidationResult>()))
            {
                return false;
            }

            if (request.PaymentScheme != account.AllowedPaymentSchemes)
            {
                return false;
            }

            if (request.PaymentScheme == PaymentScheme.ExpeditedPayments && account.Balance < request.Amount)
            {
                return false;
            }

            if (request.PaymentScheme == PaymentScheme.AutomatedPaymentSystem & account.Status != AccountStatus.Live)
            {
                return false;
            }

            return true;
        }
    }
}
