using Fidessa.DesktopApi.MarketData;
using Fidessa.DesktopApi.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using Fidessa.DesktopApi.OrderManagement;

namespace Hawkeye
{
    public class Task 
    {
        private Level1 level1;
        private OrderEntry orderEntry;

        public event EventHandler<TaskStateChangeEventArgs> TaskStateChange;

        public class TaskStateChangeEventArgs : EventArgs
        {
            public TaskStateChangeEventArgs(Task.TaskStateType State, Double? CurrentPrice)
            {
                this.State = State;
                this.CurrentPrice = CurrentPrice;
            }

            public Double? CurrentPrice { get; set; }
            public TaskStateType State { get; set; }
        }
        
        public enum PriceWatchType
        {
            Bid,
            Ask
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum ConditionType
        {
            [Description(">=")]
            GreaterThanOrEqual,
            [Description("<=")]
            LessThanOrEqual,
            [Description(">")]
            GreaterThan,
            [Description("<")]
            LessThan
        }

        public enum TaskStateType
        {
            Dormant,
            Live,
            Placing,
            Done,
            Error
        }

        /// <summary>
        /// class representing a single task of watching the stock price until a trigger condition is met, 
        /// then placing an order onto the market
        /// </summary>
        public Task()
        {
            Instrument = "";
            Quantity = 0;
            Trigger = 0.0F;
            Condition = ConditionType.GreaterThan;
            OType = OrderType.Limit;
            Price = PriceWatchType.Bid;
            Side = Direction.Buy;
            State = TaskStateType.Dormant;
            ErrorString = "";
        }

        /// <summary>
        /// The stock to watch
        /// </summary>
        public string Instrument { get; set; }

        /// <summary>
        /// Quantity of stock to order
        /// </summary>
        public Int32 Quantity { get; set; }

        /// <summary>
        /// Buy or Sell
        /// </summary>
        public Direction Side { get; set; }

        /// <summary>
        /// Linit or Market. A limit order will be entered at the trigger price
        /// </summary>
        public OrderType? OType { get; set; }

        /// <summary>
        /// Bid or Ask - price to watch
        /// </summary>
        public PriceWatchType Price { get; set; }

        /// <summary>
        /// Trigger Condition
        /// </summary>
        public ConditionType Condition { get; set; }

        /// <summary>
        /// Trigger price value
        /// </summary>
        public double Trigger { get; set; }

        /// <summary>
        /// Client for order entry
        /// </summary>
        public String Client { get; set; }

        /// <summary>
        /// Account for order entry
        /// </summary>
        public String Account { get; set; }


        /// <summary>
        /// state of task
        /// </summary>
        public TaskStateType State { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String ErrorString { get; set; }

        /// <summary>
        /// Start the task.
        /// </summary>
        public void Start()
        {
            if (State == TaskStateType.Dormant)
            {
                // Create a Level1 object
                level1 = new Level1(Instrument);

                // Wire up the event handler for server response
                level1.PublicationResponse += level1_PublicationResponse;

                // Request the appropriate price
                string error;
                if (Price == PriceWatchType.Bid)
                {
                    level1.RequestProperty("BidPrice", out error);
                }
                else if (Price == PriceWatchType.Ask)
                {
                    level1.RequestProperty("AskPrice", out error);
                }

                // Subscribe
                State = TaskStateType.Live;
                level1.Subscribe();
            }
        }

        /// <summary>
        /// Stop the task. Cannot stop if an order has been sent whilst awaiting response
        /// </summary>
        public void Stop()
        {
            if (State == TaskStateType.Live)
            {
                ClearPublication();
                SetState(TaskStateType.Dormant);
            }
        }

        /// <summary>
        /// clear up publication
        /// </summary>
        private void ClearPublication()
        {
            if (level1 != null)
            {
                // disconnect handler - do this first so as not to receive Close event
                level1.PublicationResponse -= level1_PublicationResponse;

                // remove level1 object - automatically unsubscribes
                level1.Dispose();
                level1 = null;
            }
        }

        private void level1_PublicationResponse(object sender, PublicationResponseEventArgs e)
        {
            switch (e.ResponseEventType)
            {
                // Price update received - check condition
                case PublicationResponseEventType.Image:
                case PublicationResponseEventType.Update:
                    TestCondition();
                    break;

                // Report error (e.g. unknown instrument)
                case PublicationResponseEventType.Error:
                    ErrorString = e.Errors[0];
                    ClearPublication();
                    SetState(TaskStateType.Error);
                    break;

                // Should only receive this as  a result of server closing publication, which is reported here as an error
                case PublicationResponseEventType.Close:
                    ErrorString = "Server closed publication";
                    ClearPublication();
                    SetState(TaskStateType.Error);
                    break;

                case PublicationResponseEventType.ServiceDown:
                    ErrorString = "Service down";
                    ClearPublication();
                    SetState(TaskStateType.Error);
                    break;
            }
        }

        /// <summary>
        /// Tests if trigger condition met, and places an order if so
        /// </summary>
        private void TestCondition()
        {
            Boolean conditionHit = false;
            double? triggerPrice = 0.0;

            if (Price == PriceWatchType.Bid)
            {
                triggerPrice = level1.BidPrice;
            }
            else if (Price == PriceWatchType.Ask)
            {
                triggerPrice = level1.AskPrice;
            }

            SetState(TaskStateType.Live, triggerPrice);

            switch (Condition)
            {
                case ConditionType.GreaterThan:
                    conditionHit = triggerPrice > Trigger;
                    break;
                case ConditionType.GreaterThanOrEqual:
                    conditionHit = triggerPrice >= Trigger;
                    break;
                case ConditionType.LessThan:
                    conditionHit = triggerPrice < Trigger;
                    break;
                case ConditionType.LessThanOrEqual :
                    conditionHit = triggerPrice <= Trigger;
                    break;
            }

            if (conditionHit)
            {
                ClearPublication();
                PlaceOrder();
            }
        }

        /// <summary>
        /// Place the order
        /// </summary>
        private void PlaceOrder()
        {
            SetState(TaskStateType.Placing);

            // Create OrderEntry object
            orderEntry = new OrderEntry();

            // Wire up event handler for server response
            orderEntry.TransactionResponse += orderEntry_TransactionResponse;

            // Set order details
            orderEntry.Client = Client;
            orderEntry.Account = Account;
            orderEntry.Quantity = Quantity;
            orderEntry.Direction = Side;
            orderEntry.OrderType = OType;
            orderEntry.Instrument = Instrument;
            if (OType == OrderType.Limit)
            {
                orderEntry.LimitPrice = Trigger;
            }

            // Send, ignoring any warnings
            orderEntry.SendTransaction(true);
        }

        private void orderEntry_TransactionResponse(object sender, TransactionResponseEventArgs e)
        {
            // Report error or newly-created OrderID
            if (e.Error)
            {
                ErrorString = e.Errors[0];
                SetState(TaskStateType.Error);
            }
            else
            {
                ErrorString = orderEntry.OrderID;
                SetState(TaskStateType.Done);
            }

            // clear up
            orderEntry.TransactionResponse -= orderEntry_TransactionResponse;
            orderEntry.Dispose();
            orderEntry = null;
        }

        private void SetState(TaskStateType state, Double? currentPrice)
        {
            this.State = state;
           
            if (TaskStateChange != null)
            {
                TaskStateChangeEventArgs args = new TaskStateChangeEventArgs(state, currentPrice);
                TaskStateChange.Invoke(this, args);
            }
        }

        private void SetState(TaskStateType state)
        {
            SetState(state, null);
        }
    }
}
