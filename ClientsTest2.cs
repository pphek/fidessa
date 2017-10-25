using Fidessa.DesktopApi.Base;
using Fidessa.DesktopApi.OpenAccess;
using Fidessa.DesktopApi.Business.Accounts;
using Fidessa.DesktopApi.Business.Clients;
using System;
using System.ComponentModel;
using System.Text;
namespace Hawkeye
{
    class ClientsTest2
    {
        public String Output { get; set; }
        public event PropertyChangedEventHandler OutputReceived;

        private QueryFactory factory;
        private COMQuery query;
        private IFilter filter;

        public ClientsTest2()
        {
        }

        public void Sample()
        {
            string error;

            factory = new QueryFactory();

            query = factory.CreateQuery(QueryClass.PositionUtilisationRiskEntityQuery);

            filter = query.CreateFilter();

            filter.AddFilterCriteria("EntityLevel", "Client", out error);

            query.MaxCount = 25;

            query.QueryResponse += query_QueryResponse;

            query.Start(filter);
        }

        private void query_QueryResponse(QueryResponseEventType responseEventType, ref string[] rows, ref string[] errors)
        {
            if (responseEventType.Equals(QueryResponseEventType.Error))
            {
                Error(errors[0]);
            }
            else
            {
                //AccountQuery query = sender as AccountQuery;
                //ClientQuery query = sender as ClientQuery;

                //// Request more data
                //if (query != null)
                //{
                //    this.Output += "Query.State = " + query.State + " | ";

                //    if (query.State == QueryState.MoreImagesAvailable)
                //    {
                //        query.RequestMoreData();
                //    }
                //}

                //// Consume data
                //BindingList<Client> list = this.query.GetList();
                //foreach (Client client in list)
                //{
                //    this.Output += client.ClientCode;
                //}

                //OnPropertyChanged(new PropertyChangedEventArgs("Output"));
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
