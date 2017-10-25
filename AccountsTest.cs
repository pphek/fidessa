using Fidessa.DesktopApi.Base;
using Fidessa.DesktopApi.OpenAccess;
using Fidessa.DesktopApi.Business.Accounts;
using Fidessa.DesktopApi.Business.Clients;
using System;
using System.ComponentModel;
using System.Text;

namespace Hawkeye
{
    class AccountsTest
    {
        public String Output { get; set; }
        public event PropertyChangedEventHandler OutputReceived;

        private AccountQuery query;

        public AccountsTest()
        {
        }

        public void Sample(string clientCode)
        {
            // Create query and add event handler
            query = new AccountQuery(null);
            query.QueryResponse += query_QueryResponse;

            // Create filter and add criterion to filter by client code
            AccountFilter filter = new AccountFilter();
            filter.AddFilterCriteria(AccountFilter.FilterBy.Client, clientCode);

            // Start query to start receiving data
            query.Start(filter);
        }

        private void query_QueryResponse(object sender, QueryResponseEventArgs e)
        {
            if (e.ResponseEventType.Equals(QueryResponseEventType.Error))
            {
                Error(e.Errors[0]);
            }
            else
            {
                AccountQuery query = sender as AccountQuery;
                // Request more data
                if (query != null)
                {
                    if (query.State == QueryState.MoreImagesAvailable)
                    {
                        query.RequestMoreData();
                    }
                }

                // Consume data
                BindingList<Account> list = this.query.GetList();
                foreach (Account account in list)
                {
                    this.Output += account.AccountCode;
                }

                OnPropertyChanged(new PropertyChangedEventArgs("Output"));
            }
        }

        private void Error(String error)
        {
            Output = error;
            OnPropertyChanged(new PropertyChangedEventArgs("Output"));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.OutputReceived != null)
            {
                this.OutputReceived(this, e);
            }
        }
    }
}
