using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Controls;

namespace StrideFlyWPFClient
{
    class XBee
    {
        SerialPort xbeeComPort;
        private SerialDataReceivedEventHandler SerialDataReceivedEventHandler1;
        Object lockingObj = new Object();
        RichTextBox logBox;

        public XBee(RichTextBox xBox)
        {
            logBox = xBox;
        }

        public string[] initCOMPorts()
        {
            string[] comPortNames = SerialPort.GetPortNames();
            Array.Sort(comPortNames);

            return comPortNames;

        }

        public int OpenCloseComPort(string portName)
        {
            if (xbeeComPort != null && xbeeComPort.IsOpen) {
                xbeeComPort.Close();
                xbeeComPort = null;
                return 0; 
            }

            SerialDataReceivedEventHandler1 = new SerialDataReceivedEventHandler(DataReceived);

            xbeeComPort = new SerialPort();

            xbeeComPort.PortName = portName;
            xbeeComPort.BaudRate = 9600;
            xbeeComPort.Parity = Parity.None;
            xbeeComPort.DataBits = 8;
            xbeeComPort.StopBits = StopBits.One;

            try
            {
                xbeeComPort.Open();

                if (xbeeComPort.IsOpen)
                {
                    //  The port is open. Set additional parameters.
                    //  Timeouts are in milliseconds.

                    //xbeeComPort.ReadTimeout = 30000;
                    //xbeeComPort.WriteTimeout = 5000;

                    //  Specify the routines that run when a DataReceived or ErrorReceived event occurs.
                    xbeeComPort.DataReceived += SerialDataReceivedEventHandler1;
                    //  SelectedPort.ErrorReceived += SerialErrorReceivedEventHandler1;

                    return 1;
                }
                else
                    return -1;

            }
            catch (Exception ex)
            {
                return - 1;
            }
        }

        internal void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Int32 inByte;
            Int32 packetLength;
            char[] recvData = new char[128];
            byte[] sendAddress = new byte[8];
            byte[] discard = new byte[16];


            try
            {
                lock (lockingObj)
                {
                    //xbeeComPort.Read(
                    for (; ; )
                    {
                        inByte = xbeeComPort.ReadByte();

                        if (inByte == -1) // we're done here
                            return;

                        // continue on here
                        if (inByte == 0x7E)
                        {
                            // make sure we have 3 more bytes
                            while (xbeeComPort.BytesToRead < 3)
                                Thread.Sleep(10);

                            xbeeComPort.ReadByte(); // Offset 1 => Discard Length MSB
                            packetLength = xbeeComPort.ReadByte(); // Offset 2 => Length LSB

                            // check that this is a receive packet
                            if (xbeeComPort.ReadByte() != 0x90)
                                return;

                            // now we need to read the whole thing. make sure it's ready
                            while (xbeeComPort.BytesToRead < packetLength)
                                Thread.Sleep(10);

                            // Read the sender address
                            xbeeComPort.Read(sendAddress, 0, 8);

                            // discard the next 3 bytes
                            xbeeComPort.Read(discard, 0, 3);

                            // Read the data packet up to checksum into our buffer
                            xbeeComPort.Read(recvData, 0, packetLength - 12);

                            // Process incoming data packet
                            ProcessRFPacket(sendAddress, recvData);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message, "Error reading data from serial port", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void ProcessRFPacket(byte[] sender, char[] data)
        {
            string final;
            string dataString = "";
            string hexAddress = "0x";

            for (int i = 0; i < sender.Length; i++)
            {
                hexAddress = String.Concat(hexAddress, sender[i].ToString("X2"));
            }

            for (int j = 0; j < data.Length; j++)
            {
                if (data[j] == '\r' || data[j] == '\n')
                    continue;

                if (data[j] == '\0')
                    break;

                dataString = String.Concat(dataString, data[j]);
            }

            final = String.Format("[{0}] {1}{2}", hexAddress, dataString, System.Environment.NewLine);

            string log = String.Format("{0}{1}", dataString, System.Environment.NewLine);
        
            logBox.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            //gpsLog.Blocks.Add(new Paragraph(new Run(final)));
                            logBox.CaretPosition.DocumentStart.InsertTextInRun(log);
                            //logBox.AppendText(final);// + System.Environment.NewLine);

                        }
            ));
        }
    }

    
}
