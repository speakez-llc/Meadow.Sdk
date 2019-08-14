﻿using System;
using System.IO.Ports;
using System.Text;

namespace MeadowCLI.Hcom
{
    public class ReceiveTargetData
    {
        SerialPort _serialPort;
        HostCommBuffer _hostCommBuffer;
        const int maxRecvSize = 2048;
        string F7ReadFileListPrefix { get { return "FileList: "; } }
        string F7MonoMessagePrefix { get { return "MonoMsg: "; } }

        //Timer _readTimer;
        readonly byte[] _prevRecvUnusedBytes = new byte[maxRecvSize * 2];

        // It seems that the .Net SerialPort class is not all it could be.
        // To acheive reliable operation some SerialPort class methods must
        // not be used. When receiving, the BaseStream must be used.
        // The following blog post was very helpful.
        // http://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport

        //-------------------------------------------------------------
        // Constructor
        public ReceiveTargetData(SerialPort serialPort, HostCommBuffer hostCommBuffer)
        {
            _serialPort = serialPort;
            //_readTimer = new Timer(CheckForMessage, null, 1000, 1000);

            _hostCommBuffer = hostCommBuffer;

            // Setup circular buffer
            if (_hostCommBuffer.Init(256) != HcomBufferReturn.HCOM_CIR_BUF_INIT_OK)
            {
                Console.WriteLine("Error setting up Circular Buffer");
                return;
            }

            _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialReceiveData);
        }

        //-------------------------------------------------------------
        public void Shutdown()
        {
        }

        //-------------------------------------------------------------
        // All received data handled here
        void SerialReceiveData(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[maxRecvSize];
            Action initiateRead = null;

            try
            {
                initiateRead = delegate
                {
                    try
                    {
                        _serialPort.BaseStream.BeginRead(buffer, 0, buffer.Length, delegate (IAsyncResult ar)
                        {
                            try
                            {
                                int actualLength = _serialPort.BaseStream.EndRead(ar);
                                // Copy not needed but helpful in testing
                                byte[] received = new byte[actualLength];
                                Buffer.BlockCopy(buffer, 0, received, 0, actualLength);
                                AddMessageToBuffer(received, actualLength);
                            }
                            catch (System.IO.IOException ioe)
                            {
                                // Todo Handle exception
                                Console.WriteLine("IOException: {0}", ioe);
                            }
                            catch (Exception except)
                            {
                                // Todo Handle exception
                                Console.WriteLine("Exception: {0}", except);
                                //Console.ReadKey();
                            }
                            initiateRead();
                        }, null);

                    }
                    catch (Exception except)
                    {
                        Console.WriteLine("Exception: {0} usually means the Target dropped the connection", except);
                    }
                };
            }
            catch (Exception except)
            {
                Console.WriteLine("Exception: {0} usually means the Target dropped the connection", except);
            }
            initiateRead();
        }

        //-------------------------------------------------------------
        // Includes trailing zero
        void AddMessageToBuffer(byte[] buffer, int actualLength)
        {
            while (true)
            {
                var result = _hostCommBuffer.AddBytes(buffer, 0, actualLength);
                if (result == HcomBufferReturn.HCOM_CIR_BUF_ADD_WONT_FIT)
                {
                    CheckForMessage(null);
                    continue;
                }
                CheckForMessage(null);
                break;
            }
        }

        //-------------------------------------------------------------
        void CheckForMessage(object state)
        {
            while (true)
            {
                byte[] packetBuffer = new byte[maxRecvSize * 2];
                int packetLength;
                var result = _hostCommBuffer.GetNextPacket(packetBuffer, maxRecvSize * 2, out packetLength);
                switch (result)
                {
                    case HcomBufferReturn.HCOM_CIR_BUF_GET_FOUND_MSG:
                        ParseReceivedPacket(packetBuffer, packetLength);
                        continue;
                    case HcomBufferReturn.HCOM_CIR_BUF_GET_NONE_FOUND:
                        break;
                    case HcomBufferReturn.HCOM_CIR_BUF_GET_BUF_NO_ROOM:// The packetBuffer is too small, we're in trouble
                        throw new InsufficientMemoryException("Received a message too big for our buffer");
                    default:
                        throw new NotSupportedException("Circular buffer retuned unknown result.");
                }
            }
        }

        //-------------------------------------------------------------
        // This is a quick hack until true messages can be received
        void ParseReceivedPacket(byte[] newData, int dataLength)
        {
            // - 1 strips of the terminating null
            string rcvdString = Encoding.ASCII.GetString(newData, 0, dataLength - 1);
            if (rcvdString.StartsWith(F7ReadFileListPrefix))
            {
                // This is a comma separated list
                string baseMessage = rcvdString.Substring(F7ReadFileListPrefix.Length);
                DisplayFileList(baseMessage);
            }
            else if (rcvdString.StartsWith(F7MonoMessagePrefix))
            {
                string baseMessage = rcvdString.Substring(F7MonoMessagePrefix.Length);
                Console.WriteLine($"mono runtime message: {baseMessage}");
            }
            else
            {
                Console.WriteLine($"Received: '{rcvdString}'");
            }
        }

        //-------------------------------------------------------------
        void DisplayFileList(string receivedTextMsg)
        {
            Console.WriteLine($"Received File List:");

            string[] fileList = receivedTextMsg.Split(',');
            for (int i = 0; i < fileList.Length; i++)
            {
                Console.WriteLine($"{i + 1}) {fileList[i]}");
            }
        }
    }
}