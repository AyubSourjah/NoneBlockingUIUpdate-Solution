// =================================================================================
// Description          : Demonstration of the use of thread safe operations
// Author               : Ayub Sourjah
// Created Timestamp    : 27/08/2023
// =================================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoneBlockingControlUpdate
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private Action<int> _updateProgressAction;
        private Action _updateButtonsAction;
        private Task _doSomeWork;

        public Form1()
        {
            InitializeComponent();
        }
        
        //The start button allows the user to initiate heavy operation.
        //Assume this operation is similar to our daily attendance upload process,
        //where the user initiates the upload manually.
        private void buttonStart_Click(object sender, EventArgs e)
        {
            //For the purpose of demonstration, I am introducing the custom
            //implementation of the cancellation token.
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            //Hook the Action delegate with the underlining methods to be executed
            //on the respective threads.
            _updateProgressAction = new Action<int>(UpdateProgress);
            _updateButtonsAction = new Action(UpdateButtons);
            
            //Execute the process on a thread pool thread and cleanup when over.
            _doSomeWork = Task.Run(() => DoSomeWork(_cancellationToken));
            _doSomeWork.ContinueWith((task) =>
            {
                this.UpdateProgress(0);
                this.UpdateButtons();
            });
                
            buttonStart.Enabled = false;
            buttonAbort.Enabled = true;
        }

        //The UpdateProgress method is responsible in updating the progressbar control
        //that is visible to the user. Please note, the progress bar is created on the
        //UI thread which is equelent to the Forms thread.
        private void UpdateProgress(int value)
        {
            //This condition validates whether the an invokation is required, if the thread in which
            //the control was created and the thread from which the method was called differ.
            if (progressBar1.InvokeRequired)
            {
                //Marshel the method using the Action delegate to be executed on the underlining thread,
                //in this case its the UI thread.
                this.Invoke(_updateProgressAction, value);
                return;
            }

            //Since I have written this as a loop back method, if there is no invokation required,
            //simply update the progress bar value.
            progressBar1.Value = value;
        }

        //This method is called when the user clicks on the Start Button, which would simulate
        //the daily attendance process manually.
        private void DoSomeWork(CancellationToken cancellation)
        {
            for (int i = 1; i < 100000; i++)
            {
                //I check if the user has aborted the operatin before continuing,
                //to gracefully shutdown the process without crashing.
                if (cancellation.IsCancellationRequested)
                {
                    //Reset the progress bar
                    UpdateProgress(0);
                    break;
                }

                UpdateProgress(i);
            }
        }

        //This allows the user to abort the operation at any point,
        //giving more control to the user over the process.
        private void buttonAbort_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();

            buttonAbort.Enabled = false;
            buttonStart.Enabled = true;
        }

        //This method marshels the updating of the buttons state
        //through the respective threads.
        private void UpdateButtons()
        {
            if (buttonAbort.InvokeRequired)
            {
                buttonAbort.Invoke(_updateButtonsAction);
                return;
            }

            buttonAbort.Enabled = false;
            buttonStart.Enabled = true;
        }

        //The reason I hooked on the closing event of the Forms is to be able to
        //control and grassfully shutdown without simiply killing the process. Which would result
        //in an application crash and an unplesent user experience.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (buttonStart.Enabled == false)
            {
                MessageBox.Show("Work in progress, please click on [Abort] to exit.");
                e.Cancel = true;
            }
                
        }
    }
}
