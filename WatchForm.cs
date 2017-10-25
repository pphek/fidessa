using Fidessa.DesktopApi.OrderManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Hawkeye
{
    public partial class WatchForm : Form
    {
        private Task task = null;

        public WatchForm()
        {
            InitializeComponent();
        }

        
        private void buttonWatch_Click(object sender, EventArgs e)
        {
            if (task == null)
            {
                // try and create a task from user-specified parameters
                try
                {
                    task = new Task();
                    task.Instrument = textInstrument.Text;
                    task.Quantity = Int32.Parse(textQuantity.Text);
                    task.Trigger = Double.Parse(this.textBoxTriggerPrice.Text);
                    task.Client = textCounterparty.Text;
                    task.Account = textAccount.Text;
                    task.Side = (Direction)Enum.Parse(typeof(Direction), comboBoxSide.Text);
                    task.OType = (OrderType)Enum.Parse(typeof(OrderType), comboBoxType.Text);
                    task.Condition = (Task.ConditionType)EnumDescriptionTypeConverter.GetEnumValue(typeof(Task.ConditionType), comboBoxCondition.Text);
                    task.Price = (Task.PriceWatchType)Enum.Parse(typeof(Task.PriceWatchType), comboBoxWatch.Text);

                    this.buttonWatch.Text = "Cancel"; 
                    task.TaskStateChange += task_onStateChange;
                    task.Start();
                }
                catch (Exception exception)
                {
                    System.Windows.Forms.MessageBox.Show(exception.Message);
                    if (task != null) task.Stop();
                    task = null;
                }
            }
            else
            {
                task.Stop();
            }
        }

        void task_onStateChange(object sender, Task.TaskStateChangeEventArgs e)
        {
            // update display
            labelState.Text = String.Format("State: {0}" , e.State.ToString());
            labelCurrentPrice.Text = String.Format("Current Price: {0}", e.CurrentPrice != null ? e.CurrentPrice.ToString() : "");
            labelErrorString.Text = (sender as Task).ErrorString;

            switch (e.State)
            {
                case Task.TaskStateType.Done:
                case Task.TaskStateType.Error:
                case Task.TaskStateType.Dormant:
                    task = null;
                    this.buttonWatch.Text = "Watch";
                    break;
                case Task.TaskStateType.Live:
                case Task.TaskStateType.Placing:
                    this.buttonWatch.Text = "Cancel";
                    break;
            }
        }
    }
}
