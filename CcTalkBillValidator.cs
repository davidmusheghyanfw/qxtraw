using System;
using System.Text;
using CCnetWPF.Adapter;
using CCnetWPF.Events;
using MPOST;


namespace CCnetWPF.Connections.CCtalk
{
    public class CcTalkBillValidator : IBillValidator
    {
        private MPOST.Acceptor BillAcceptor = new MPOST.Acceptor();

        private CalibrateFinishEventHandler CalibrateFinishDelegate;
        private CalibrateProgressEventHandler CalibrateProgressDelegate;
        private CalibrateStartEventHandler CalibrateStartDelegate;
        private CashBoxCleanlinessEventHandler CashBoxCleanlinessDelegate;
        private CashBoxAttachedEventHandler CashBoxAttachedDelegate;
        private CashBoxRemovedEventHandler CashBoxRemovedDelegate;
        private CheatedEventHandler CheatedDelegate;
        private ClearAuditEventHandler ClearAuditDelegate;
        private ConnectedEventHandler ConnectedDelegate;
        private DisconnectedEventHandler DisconnectedDelegate;
        private DownloadFinishEventHandler DownloadFinishDelegate;
        private DownloadProgressEventHandler DownloadProgressDelegate;
        private DownloadRestartEventHandler DownloadRestartDelegate;
        private DownloadStartEventHandler DownloadStartDelegate;
        private ErrorOnSendMessageEventHandler ErrorOnSendMessageDelegate;
        private EscrowEventHandler EscrowedDelegate;
        private FailureClearedEventHandler FailureClearedDelegate;
        private FailureDetectedEventHandler FailureDetectedDelegate;
        private InvalidCommandEventHandler InvalidCommandDelegate;
        private JamClearedEventHandler JamClearedDelegate;
        private JamDetectedEventHandler JamDetectedDelegate;
        private NoteRetrievedEventHandler NoteRetrievedDelegate;
        private PauseClearedEventHandler PauseClearedDelegate;
        private PauseDetectedEventHandler PauseDetectedDelegate;
        private PowerUpCompleteEventHandler PowerUpCompleteDelegate;
        private PowerUpEventHandler PowerUpDelegate;
        private PUPEscrowEventHandler PUPEscrowDelegate;
        private RejectedEventHandler RejectedDelegate;
        private ReturnedEventHandler ReturnedDelegate;

        private StackedEventHandler StackedDelegate;

        // A new stacked event with document information has been added. Recommanded to be used.
        private StackedWithDocInfoEventHandler StackedWithDocInfoDelegate;
        private StackerFullClearedEventHandler StackerFullClearedDelegate;
        private StackerFullEventHandler StackerFullDelegate;
        private StallClearedEventHandler StallClearedDelegate;
        private StallDetectedEventHandler StallDetectedDelegate;


        private bool _isConnected;

        public bool IsConnected
        {
            get => _isConnected;
            set => _isConnected = true;
        }

        public event Action<object, BillReceivedEventArgs> BillReceived;
        public event Action<object, BillStackedEventArgs> BillStacked;
        public event Action<object, BillConnectedEventArgs> OnConnectedEvent;
        public event Action<object, BillPowerUpEventArgs> OnPowerUpEvent;
        public event Action<object, CalibrateFinishEventArgs> OnCalibrateFinishEvent;
        public event Action<object, CalibrateProgressEventArgs> OnCalibrateProgressEvent;
        public event Action<object, CashBoxCleanlinessEventArgs> OnCashBoxCleanlinessEvent;
        public event Action<object, EventArgs> OnCashBoxAttachedEvent;
        public event Action<object, EventArgs> OnCashBoxRemovedEvent;
        public event Action<object, EventArgs> OnCheatedEvent;
        public event Action<object, ClearAuditEventArgs> OnClearAuditEvent;

        public event Action<object, EventArgs> OnDisconnectedEvent;
        //public event Action<object, ClearAuditEventArgs> OnClearAuditEvent;
        //public event Action<object, ClearAuditEventArgs> OnClearAuditEvent;
        //public event Action<object, ClearAuditEventArgs> OnClearAuditEvent;

        public CcTalkBillValidator()
        {
            ErrorOnSendMessageDelegate = new ErrorOnSendMessageEventHandler(HandleSendMessageErrorEvent);
            EscrowedDelegate = new EscrowEventHandler(HandleEscrowedEvent);
            PowerUpCompleteDelegate = new PowerUpCompleteEventHandler(HandlePowerUpCompleteEvent);
            PowerUpDelegate = new PowerUpEventHandler(HandlePowerUpEvent);
            RejectedDelegate = new RejectedEventHandler(HandleRejectedEvent);
            ReturnedDelegate = new ReturnedEventHandler(HandleReturnedEvent);
            StackedDelegate = new StackedEventHandler(HandleStackedEvent);
            // A new stacked event with document information has been added. Recommanded to be used.


            // Connect to the events.
            BillAcceptor.OnCalibrateFinish += CalibrateFinishDelegate;
            BillAcceptor.OnCalibrateProgress += CalibrateProgressDelegate;
            BillAcceptor.OnCalibrateStart += CalibrateStartDelegate;
            BillAcceptor.OnCashBoxCleanlinessDetected += CashBoxCleanlinessDelegate;
            BillAcceptor.OnCashBoxAttached += CashBoxAttachedDelegate;
            BillAcceptor.OnCashBoxRemoved += CashBoxRemovedDelegate;
            BillAcceptor.OnClearAuditComplete += ClearAuditDelegate;
            BillAcceptor.OnDisconnected += DisconnectedDelegate;
            BillAcceptor.OnDownloadFinish += DownloadFinishDelegate;
            BillAcceptor.OnDownloadProgress += DownloadProgressDelegate;
            BillAcceptor.OnDownloadRestart += DownloadRestartDelegate;
            BillAcceptor.OnDownloadStart += DownloadStartDelegate;
            BillAcceptor.OnSendMessageFailure += ErrorOnSendMessageDelegate;
            BillAcceptor.OnEscrow += EscrowedDelegate;
            BillAcceptor.OnFailureCleared += FailureClearedDelegate;
            BillAcceptor.OnFailureDetected += FailureDetectedDelegate;
            BillAcceptor.OnInvalidCommand += InvalidCommandDelegate;
            BillAcceptor.OnJamCleared += JamClearedDelegate;
            BillAcceptor.OnJamDetected += JamDetectedDelegate;
            BillAcceptor.OnNoteRetrieved += NoteRetrievedDelegate;
            BillAcceptor.OnPauseCleared += PauseClearedDelegate;
            BillAcceptor.OnPauseDetected += PauseDetectedDelegate;
            BillAcceptor.OnPowerUpComplete += PowerUpCompleteDelegate;
            BillAcceptor.OnPUPEscrow += PUPEscrowDelegate;

            BillAcceptor.OnPowerUp += BillAcceptor_OnPowerUp;
            BillAcceptor.OnRejected += BillAcceptor_OnRejected;
            BillAcceptor.OnReturned += BillAcceptor_OnReturned;
            BillAcceptor.OnEscrow += BillAcceptor_OnEscrewed;
            BillAcceptor.OnStacked += BillAcceptor_OnStacked;
            BillAcceptor.OnConnected += BillAcceptor_OnConnected;
        }


        // public PortConnectedArgs Init(string portName, UnityMainThreadDispatcher unityMainThreadDispatcher)
        // {
        //     try
        //     {
        //         _unityMainThreadDispatcher = unityMainThreadDispatcher;
        //         Logger.Instance.Initialize(unityMainThreadDispatcher);
        //         _openTask = new();
        //         // Open the bill acceptor on the specified port
        //         BillAcceptor.Open(portName, PowerUp.A); // Adjust as necessary

        //         // Check if connected successfully
        //         IsConnected = true; // Set connection state
        //         OnConnectedEvent?.Invoke(this,
        //             new BillConnectedEventArgs(BillConnectedEventArgs.BillConnectedStatus.Connected, 1, "OnConnected"));

        //         return new PortConnectedArgs(PortConnectionEnum.SUCCESS, "Success", portName);
        //     }
        //     catch (Exception ex)
        //     {
        //         IsConnected = false; // Set connection state to false on error
        //         _openTask.TrySetResult();
        //         Console.WriteLineError(ex.ToString());
        //         return new PortConnectedArgs(PortConnectionEnum.ERROR, ex.Message, portName);
        //     }
        // }

        public PortConnectedArgs Init(string portName)
        {
            BillAcceptor.Open(portName, PowerUp.A);
            IsConnected = true;
            OnConnectedEvent?.Invoke(this,
                 new BillConnectedEventArgs(BillConnectedEventArgs.BillConnectedStatus.Connected, 1, "OnConnected"));
            return new PortConnectedArgs(PortConnectionEnum.SUCCESS, "Success", portName);
        }

        private string CheckDeviceStatus()
        {
            // Hypothetical implementation to check device status
            // You may need to send a specific command to get the actual status
            return BillAcceptor.DeviceState.ToString();
        }

        public string EnableBill()
        {
            BillAcceptor.EnableAcceptance = true;
            //BillAcceptor.EnableBarCodes = true;
            BillAcceptor.AutoStack = false;
            return "enabled";
        }

        public string DisableBill()
        {
            BillAcceptor.EnableAcceptance = false;
            //BillAcceptor.EnableBarCodes = false;
            return "disabled";
        }

        public void EscrowStack()
        {
            BillAcceptor.EscrowStack();
        }

        public void ReturnBill()
        {
            try
            {
                BillAcceptor.EscrowReturn();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReturnBill {ex}");
            }
        }

        public Tuple<bool, string> ToggleBarcode(bool state)
        {
            BillAcceptor.EnableBarCodes = state;

            return Tuple.Create(BillAcceptor.EnableBarCodes, BillAcceptor.DocType.ToString());
        }

        public void CloseValidator()
        {
            BillAcceptor.Close();
        }


        private void BillAcceptor_OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Bill acceptor connected.");
            IsConnected = true; // Ensure this is set correctly
            OnConnectedEvent?.Invoke(sender,
                new BillConnectedEventArgs(BillConnectedEventArgs.BillConnectedStatus.Connected, 1, "OnConnected"));
        }

        private void BillAcceptor_OnDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Bill acceptor disconnected.");
            IsConnected = false; // Ensure this reflects actual state
            OnDisconnectedEvent?.Invoke(sender, EventArgs.Empty);
        }

        private void BillAcceptor_OnEscrewed(object sender, EventArgs e)
        {
            switch (BillAcceptor.DocType)
            {
                case DocumentType.Barcode:
                    BillReceived?.Invoke(sender,
                        new BillReceivedEventArgs(BillReceivedEventArgs.BillRecievedStatus.Accepted,
                            decimal.Parse(BillAcceptor.BarCode), e.ToString()));
                    break;
                case DocumentType.Bill:
                    BillReceived?.Invoke(sender,
                        new BillReceivedEventArgs(BillReceivedEventArgs.BillRecievedStatus.Accepted,
                            (decimal)(BillAcceptor.Bill.Value), e.ToString()));
                    break;
            }
        }

        private void BillAcceptor_OnStacked(object sender, EventArgs e)
        {
            BillStacked?.Invoke(sender, new BillStackedEventArgs((int)(BillAcceptor.Bill.Value)));
        }

        private void BillAcceptor_OnReturned(object sender, EventArgs e)
        {
            Console.WriteLine("ON RETURNED");
            BillReceived?.Invoke(sender,
                       new BillReceivedEventArgs(BillReceivedEventArgs.BillRecievedStatus.Rejected,
                           (int)(BillAcceptor.Bill.Value), e.ToString()));
        }

        private void BillAcceptor_OnRejected(object sender, EventArgs e)
        {
            BillReceived?.Invoke(sender,
                         new BillReceivedEventArgs(BillReceivedEventArgs.BillRecievedStatus.Rejected,
                             (int)(BillAcceptor.Bill.Value), e.ToString()));
        }

        private void BillAcceptor_OnPowerUp(object sender, EventArgs e)
        {
            OnPowerUpEvent?.Invoke(sender,
                        new BillPowerUpEventArgs(BillPowerUpEventArgs.BillPowerUpStatus.PoweredUp,
                            "Bill Acceptor powered up successfully."));
        }


        private void HandleSendMessageErrorEvent(object sender, AcceptorMessageEventArgs e)
        {
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Event: Error in send message. ");
                sb.Append(e.Msg.Description);
                sb.Append("  ");

                foreach (byte b in e.Msg.Payload)
                {
                    sb.Append(b.ToString("X2") + " ");
                }

                Console.WriteLine(sb.ToString());

                if (BillAcceptor.DeviceState == State.Escrow)
                {
                    Console.WriteLine("Escrewed");
                    //StackBtn.Enabled = true;
                    //ReturnBtn.Enabled = true;
                }
            }
        }

        private void HandleEscrowedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("HandleEscrowedEvent");

            EscrowedDelegate?.Invoke(sender, e);
        }



        private void HandleInvalidCommandEvent(object sender, EventArgs e)
        {
            Console.WriteLine($"HandleInvalidCommandEvent: {e.ToString()}");

            OnDisconnectedEvent?.Invoke(sender, e);
        }


        private void HandlePowerUpCompleteEvent(object sender, EventArgs e)
        {
            Console.WriteLine($"HandlePowerUpCompleteEvent: {e.ToString()}");
            PowerUpCompleteDelegate?.Invoke(sender, e);
        }

        private void HandlePowerUpEvent(object sender, EventArgs e)
        {
            Console.WriteLine($"HandlePowerUpCompleteEvent: {e.ToString()}");
            PowerUpDelegate?.Invoke(sender, e);
        }


        private void HandleRejectedEvent(object sender, EventArgs e)
        {
            Console.WriteLine($"HandleRejectedEvent: {e.ToString()}");
        }

        private void HandleReturnedEvent(object sender, EventArgs e)
        {
            Console.WriteLine($"HandleReturnedEvent: {e.ToString()}");
        }


        private void HandleStackedEvent(object sender, EventArgs e)
        {
            Console.WriteLine($"HandleStackedEvent: {e.ToString()}");
        }


    }
}