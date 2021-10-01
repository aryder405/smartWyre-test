using System;
using System.ComponentModel.DataAnnotations;


namespace Smartwyre.DeveloperTest.Types
{
    public class MakePaymentRequest
    {
        [Required]
        public string CreditorAccountNumber { get; set; }

        [Required]
        public string DebtorAccountNumber { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentScheme PaymentScheme { get; set; }
    }
}
