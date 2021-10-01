using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data
{
    public interface IAccountDataStore
    {
        public Account GetAccount(string accountNumber);

        public void UpdateAccount(Account account);
    }
}
