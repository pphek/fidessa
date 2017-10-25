using Fidessa.DesktopApi.Base;
using Fidessa.DesktopApi.OpenAccess;
using Fidessa.DesktopApi.Risk;
using Fidessa.DesktopApi.Risk.MarginUtilisation;
using Fidessa.DesktopApi.Risk.RiskEntity;
using System;
using System.ComponentModel;
using System.Text;

namespace Hawkeye
{
    class RiskTest
    {

        public String Output { get; set; }
        public event PropertyChangedEventHandler OutputReceived;

        private MarginUtilisationQuery muq;
        private ByProductQuery muByProductq;
        private ByInstrumentQuery muByInstrumentq;
        private RiskEntityQuery req;
        private MarginUtilisationItem firstMarginUtilisation;
        private MarginUtilisationItem firstMarginUtilisationByProduct;
        private MarginUtilisationItem firstMarginUtilisationByInstrument;
        private RiskItemDetails riskItemDetails1;
        private RiskItemDetails riskItemDetails2;
        private RiskItemDetails riskItemDetails3;

        public RiskTest()
        {
        }

        public void RiskMarginUtilisationExample(string code)
        {
            //// Open a connection
            //Connection con = new Connection();
            //con.ConnectionFile = @"C:\default.pcf";

            //if (con.LogOn())
            //{
                // Retrieved the client entity that matches the provided code
                GetRiskEntity(code);
            //}
            //else
            //{
            //    Error("Connect to Fidessa");
            //}
        }


        private void GetRiskEntity(String code)
        {
            // Create a new RiskEntityQuery for margin utilisation
            // Please note that there is a different risk entity query and filter for each fo the
            // supported risk queries
            req = new Fidessa.DesktopApi.Risk.MarginUtilisation.RiskEntityQuery(null);

            // Create a new filter to retrieve entities for margin Utilisation
            RiskEntityFilter filter = new Fidessa.DesktopApi.Risk.MarginUtilisation.RiskEntityFilter();

            // Specify the EntityLevel and Code (please note the filter could also use the CodeContains filter option) 
            filter.AddFilterCriteria(RiskEntityFilter.FilterBy.EntityLevel, RiskEntityFilter.RiskEntityLevel.Client);

            filter.AddFilterCriteria(RiskEntityFilter.FilterBy.Code, code);

            // Provide a responde handler and start the query
            req.QueryResponse += req_QueryResponse;
            req.Start(filter);

        }

        void req_QueryResponse(object sender, QueryResponseEventArgs e)
        {
            // In this example we are not handleing errors or service state
            // we are just interested on the first RiskItem returned by the query.
            if (!e.ResponseEventType.Equals(QueryResponseEventType.Error))
            {
                if (req.GetList().Count > 0)
                {
                    // Request Margin Utilisation data for the first entity.
                    GetMarginUtilisation(req.GetList()[0]);

                    // Dispose RiskQuery as we are not using it anymore
                    req.Dispose();
                }
            }
            else
            {
                Error(e.Errors[0]);
            }
        }

        private void GetMarginUtilisation(RiskEntityItem riskEntity)
        {
            // Create a new MarginUtilisationQuery
            muq = new MarginUtilisationQuery(null);

            // Create a new filter for the query
            // Please note that all the risk queries use the same filter
            RiskFilter filter = new RiskFilter();

            // MarginUtilisation query filter requires an entity that should have beed 
            // retrieved via the MarginUtilisation RiskEntityQuery.
            filter.AddFilterCriteria(RiskFilter.FilterBy.RiskEntity,
                                     riskEntity);

            // Provide a query handler and start the query.
            muq.QueryResponse += muq_QueryResponse;
            muq.Start(filter);
        }

        void muq_QueryResponse(object sender, QueryResponseEventArgs e)
        {
            // In this example we are not handleing errors or service state
            // we are just interested on the first MarginUtilisationItem returned by the query.
            if (!e.ResponseEventType.Equals(QueryResponseEventType.Error))
            {
                if (muq.GetList().Count > 0)
                {
                    // Get the first margin utilisation for the entity.
                    // If we compare this to the grid provided in ITP, 
                    // this would be equivalent to the first row on the 
                    // MarginUtilisation grid.
                    firstMarginUtilisation = muq.GetList()[0];

                    // Get the details for the risk item received (code and description)
                    // using the RiskItemDetails publication. Please note that
                    // the constructor accepts a MarginUtilisationItem.
                    riskItemDetails1 = new RiskItemDetails(firstMarginUtilisation);
                    GetRiskItemDetails(riskItemDetails1);

                    // Request Margin Utilisation by product
                    // Note it requires the Reference from the MarginUtilisationItem
                    // we are interested on, in this, case the first item received.
                    GetMarginUtilisationByProduct(firstMarginUtilisation.Reference);

                    // Dispose MarginUtilisationQuery as we are not using it anymore
                    muq.Dispose();
                }
            }
            else
            {
                Error(e.Errors[0]);
            }
        }

        private void GetMarginUtilisationByProduct(String reference)
        {
            // Create a new MarginUtilisation ByProductQuery
            muByProductq = new ByProductQuery(null);

            // Create a new filter for the query
            // Note the only filter supported by this query is a ReferenceFilter.
            // The Reference provided must be the Reference retrieved from a
            // MarginUtilisationItem retrieved from the MarginUtilisationQuery
            ReferenceFilter filter = new ReferenceFilter();
            filter.AddFilterCriteria(ReferenceFilter.FilterBy.Reference, reference);

            // Provide a query handler and start the query.
            muByProductq.QueryResponse += muByProductq_QueryResponse;
            muByProductq.Start(filter);
        }

        void muByProductq_QueryResponse(object sender, QueryResponseEventArgs e)
        {
            // In this example we are not handleing errors or service state
            // we are just interested on the first MarginUtilisationItem returned by the
            // ByProductQuery.
            if (!e.ResponseEventType.Equals(QueryResponseEventType.Error))
            {
                if (muByProductq.GetList().Count > 0)
                {
                    // Get the first MarginUtilisationItem by product.
                    // If we compare this to the grid provided in ITP, 
                    // this would be equivalent to the first row on the first drill down
                    // on the Margin Utilisation grid.
                    firstMarginUtilisationByProduct = muByProductq.GetList()[0];

                    // Get the details for the risk item received (code and description)
                    // using the RiskItemDetails publication. Please note that
                    // the constructor accepts a MarginUtilisationItem.
                    riskItemDetails2 = new RiskItemDetails(firstMarginUtilisationByProduct);
                    GetRiskItemDetails(riskItemDetails2);

                    // Note it requires the Reference from the MarginUtilisationItem
                    // retrieved from the ByProduct query
                    GetMarginUtilisationByInstrument(firstMarginUtilisationByProduct.Reference);

                    // Dispose ByProductQuery as we are not using it anymore
                    muByProductq.Dispose();
                }
            }
            else
            {
                Error(e.Errors[0]);
            }
        }

        private void GetMarginUtilisationByInstrument(String reference)
        {
            // Create a new MarginUtilisation ByInstrumentQuery
            muByInstrumentq = new ByInstrumentQuery(null);

            // Create a new filter for the query
            // Note the only filter supported by this query is a ReferenceFilter.

            // The Reference provided must be the Reference retrieved from a
            // MarginUtilisationItem retrieved from the MarginUtilisation ByProductQuery
            ReferenceFilter filter = new ReferenceFilter();
            filter.AddFilterCriteria(ReferenceFilter.FilterBy.Reference, reference);

            // Provide a query handler and start the query.
            muByInstrumentq.QueryResponse += muByInstrumentq_QueryResponse;
            muByInstrumentq.Start(filter);
        }

        void muByInstrumentq_QueryResponse(object sender, QueryResponseEventArgs e)
        {
            // In this example we are not handleing errors or service state
            // we are just interested on the first MarginUtilisationItem returned by the
            // ByInstrumentQuery.
            if (!e.ResponseEventType.Equals(QueryResponseEventType.Error))
            {
                if (muByInstrumentq.GetList().Count > 0)
                {
                    // Get the first MarginUtilisationItem by instrument.
                    // If we compare this to the grid provided in ITP, 
                    // this would be equivalent to the first row on the second drill down
                    // on the Margin Utilisation grid.
                    firstMarginUtilisationByInstrument = muByInstrumentq.GetList()[0];

                    // Get the code and description for the risk item
                    riskItemDetails3 = new RiskItemDetails(firstMarginUtilisationByInstrument);
                    riskItemDetails3.PublicationResponse += riskItemDetails3_PublicationResponse;
                    GetRiskItemDetails(riskItemDetails3);
                }
            }
            else
            {
                Error(e.Errors[0]);
            }
        }



        private void GetRiskItemDetails(RiskItemDetails rid)
        {
            String error;
            rid.RequestProperty("Code", out error);
            rid.RequestProperty("Description", out error);
            rid.Subscribe();

        }

        void riskItemDetails3_PublicationResponse(object sender, PublicationResponseEventArgs e)
        {
            if (e.ResponseEventType.Equals(PublicationResponseEventType.Image))
            {
                if (riskItemDetails3 != null &&
                    riskItemDetails3.Code != null && riskItemDetails3.Description != null)
                {
                    StringBuilder outputs = new StringBuilder();

                    string formatColumns = "{0},{1},{2},{3}";
                    outputs.AppendLine(string.Format(formatColumns, "Query", "Code", "Desc", "NetDeltaPosition"));
                    outputs.AppendLine(string.Format(formatColumns, "MarginUtilisation", riskItemDetails1.Code, riskItemDetails1.Description, firstMarginUtilisation.NetDeltaPosition.ToString()));
                    outputs.AppendLine(string.Format(formatColumns, "ByProduct", riskItemDetails2.Code, riskItemDetails2.Description, firstMarginUtilisationByProduct.NetDeltaPosition.ToString()));
                    outputs.AppendLine(string.Format(formatColumns, "ByInstrument", riskItemDetails3.Code, riskItemDetails3.Description, firstMarginUtilisationByInstrument.NetDeltaPosition.ToString()));

                    this.Output = outputs.ToString();

                    riskItemDetails1.Unsubscribe();
                    riskItemDetails2.Unsubscribe();
                    riskItemDetails3.Unsubscribe();

                    // Stop listening to the publication
                    riskItemDetails3.PublicationResponse -= riskItemDetails3_PublicationResponse;

                    OnPropertyChanged(new PropertyChangedEventArgs("Output"));
                }
            }

            if (e.ResponseEventType.Equals(PublicationResponseEventType.Error))
            {
                Error(e.Errors[0]);
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
