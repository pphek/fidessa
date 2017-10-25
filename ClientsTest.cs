using Fidessa.DesktopApi.Base;
using Fidessa.DesktopApi.OpenAccess;
using Fidessa.DesktopApi.Business.Accounts;
using Fidessa.DesktopApi.Business.Clients;
using System;
using System.ComponentModel;
using System.Text;

namespace Hawkeye
{
    class ClientsTest
    {
        public String Output { get; set; }
        public event PropertyChangedEventHandler OutputReceived;

        private ClientQuery query;

        public ClientsTest()
        {
        }

        public void Sample()
        {
            // Create query and add event handler
            query = new ClientQuery(null);
            query.QueryResponse += query_QueryResponse;

            query.MaxCount = 10;

            // Create filter and add criterion to filter by client code
            ClientFilter filter = new ClientFilter();
            //filter.AddFilterCriteria(ClientFilter.FilterBy.OriginCountry, originCountry);

            // Start query to start receiving data
            query.Start(filter);
        }

        private void query_QueryResponse(object sender, QueryResponseEventArgs e)
        {
            this.Output += "e.ResponseEventType = " + e.ResponseEventType + " | ";

            if (e.ResponseEventType.Equals(QueryResponseEventType.Error))
            {
                Error(e.Errors[0]);
            }
            else
            {
                //AccountQuery query = sender as AccountQuery;
                ClientQuery query = sender as ClientQuery;

                // Request more data
                if (query != null)
                {
                    this.Output += "Query.State = " + query.State + " | ";

                    if (query.State == QueryState.MoreImagesAvailable)
                    {
                        query.RequestMoreData();
                    }
                }

                // Consume data
                BindingList<Client> list = this.query.GetList();
                foreach (Client client in list)
                {
                    this.Output += client.ClientCode;
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
