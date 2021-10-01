using Smartwyre.DeveloperTest.Services;
using System;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;
using Xunit;
using Moq;

namespace Smartwyre.DeveloperTest.Tests
{
    public class PaymentServiceTests
    {
        private PaymentService paymentService;
        private Mock<IAccountDataStore> accountDataStoreMock;

        public void Init()
        {
            accountDataStoreMock = new Mock<IAccountDataStore>();
            paymentService = new PaymentService(accountDataStoreMock.Object);
        }

        [Fact]
        public void ValidateRequestReturnsFalseWhenAccountNull()
        {
            Init();

            var request = GetTestRequest();

            accountDataStoreMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns((Account)null);

            var result = paymentService.MakePayment(request);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidateRequestReturnsFalseWhenRequestNull()
        {
            Init();

            accountDataStoreMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(GetTestAccount());

            var result = paymentService.MakePayment(null);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidateRequestReturnsFalseWhenRequestMissingProperties()
        {
            Init();
            var request = GetTestRequest();
            request.DebtorAccountNumber = null;

            accountDataStoreMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(GetTestAccount());

            var result = paymentService.MakePayment(request);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidateRequestReturnsFalseWhenPaymentSchemeMismatch()
        {
            Init();
            var request = GetTestRequest();
            request.PaymentScheme = PaymentScheme.BankToBankTransfer;

            var account = GetTestAccount();
            account.AllowedPaymentSchemes = PaymentScheme.AutomatedPaymentSystem;

            accountDataStoreMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);

            var result = paymentService.MakePayment(request);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidateRequestReturnsFalseWhenExpeditedPaymentAmountInvalid()
        {
            Init();
            var request = GetTestRequest();
            request.PaymentScheme = PaymentScheme.ExpeditedPayments;
            request.Amount = 1000.00M;

            var account = GetTestAccount();
            account.AllowedPaymentSchemes = PaymentScheme.ExpeditedPayments;

            accountDataStoreMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);

            var result = paymentService.MakePayment(request);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidateRequestReturnsFalseWhenAutomatedPaymentInvalid()
        {
            Init();
            var request = GetTestRequest();
            request.PaymentScheme = PaymentScheme.AutomatedPaymentSystem;

            var account = GetTestAccount();
            account.AllowedPaymentSchemes = PaymentScheme.AutomatedPaymentSystem;
            account.Status = AccountStatus.Disabled;

            accountDataStoreMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);

            var result = paymentService.MakePayment(request);

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public void ValidateRequestUpdatesAccount()
        {
            Init();
            var request = GetTestRequest();

            var account = GetTestAccount();

            accountDataStoreMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account).Verifiable();

            var result = paymentService.MakePayment(request);

            var expectedAccount = GetTestAccount();
            expectedAccount.Balance -= request.Amount;

            Assert.NotNull(result);
            Assert.True(result.Success);
            accountDataStoreMock.Verify(x => x.UpdateAccount(It.Is<Account>(x => x.Balance == expectedAccount.Balance)), Times.Once);
        }

        private Account GetTestAccount()
        {
            return new Account
            {
                Balance = 100.00M,
                AccountNumber = "12345",
                AllowedPaymentSchemes = PaymentScheme.BankToBankTransfer,
                Status = AccountStatus.InboundPaymentsOnly
            };
        }
        
        private MakePaymentRequest GetTestRequest()
        {
            return new MakePaymentRequest
            {
                PaymentScheme = PaymentScheme.BankToBankTransfer,
                CreditorAccountNumber = "5555",
                DebtorAccountNumber = "4444",
                PaymentDate = DateTime.UtcNow,
                Amount = 500.00M
            };
        }
    }
}
