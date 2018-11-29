using System;
using System.Collections.Generic;
using System.IO.Ports;
using OwenioNet;
using OwenioNet.IO;
using OwenioNet.Types;
using OwenioNet.DataConverter.Converter;
using System.Threading;

namespace DetectorsPowerManagement
{
    class MU110_32R
    {
        public IOwenProtocolMaster OwenProtocol { get; private set; }

        private int ChannelCount { get; set; }

        private byte[] TurnOn { get; }

        private byte[] TurnOff { get; }

        public MU110_32R()
        {
            TurnOn = new ConverterFloat(3).Convert(1);
            TurnOff = new ConverterFloat(3).Convert(0);
            ChannelCount = 32;
        }

        public MU110_32R(int port)
        {
            TurnOn = new ConverterFloat(3).Convert(1);
            TurnOff = new ConverterFloat(3).Convert(0);
            ChannelCount = 32;
            OpenPort(port);
        }

        private int portNumber;
        private int PortNumber
        {
            get { return portNumber; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("PortNumber must be integer and greater than zero.");
                else portNumber = value;
            }
        }

        public void OpenPort(int portToOpen)
        {
            PortNumber = portToOpen;
            try
            {
                SerialPortAdapter port = new SerialPortAdapter(PortNumber, 9600, Parity.None, 8, StopBits.One);
                port.Open();
                OwenProtocol = OwenProtocolMaster.Create(port);
            }
            catch
            {
                throw new Exception(string.Format("Cannot open port {0}", PortNumber));
            }
        }

        // i = 1...32
        public void TurnOnSingleOutput(int i, bool on)
        {
            byte[] turnOn;
            if (on)
                turnOn = new ConverterFloat(3).Convert(1);
            else
                turnOn = new ConverterFloat(3).Convert(0);
            try
            {
                OwenProtocol.OwenWrite(15 + i, AddressLengthType.Bits8, "r.OE", turnOn);
            }
            catch
            {
               throw new Exception(string.Format("Program cannot access to the output № {0}.", (15 + i).ToString()));
            }
        }

        public byte[] GetCurrentStateOfSingleOutput(int outputNumber)
        {
            return OwenProtocol.OwenRead(15 + outputNumber, AddressLengthType.Bits8, "r.OE");
        }

        public List<byte[]> GetCurrentState()
        {
            var result = new List<byte[]>();
            for (int i = 0; i < ChannelCount; i++)
                result.Add(OwenProtocol.OwenRead(16 + i, AddressLengthType.Bits8, "r.OE"));
            return result;
        }

        public void TurnOffAll()
        {
            for (int i = 0; i < ChannelCount; i++)
            {
                try
                {
                    OwenProtocol.OwenWrite(16 + i, AddressLengthType.Bits8, "r.OE", TurnOff);
                }
                catch
                {
                    throw new Exception(string.Format("Program cannot turn off output № {0}.", (16 + i).ToString()));
                }
            }
        }

        public void TurnOnAll()
        {
            for (int i = 0; i < ChannelCount; i++)
            {
                try
                {
                    OwenProtocol.OwenWrite(16 + i, AddressLengthType.Bits8, "r.OE", TurnOn);
                }
                catch
                {
                    throw new Exception(string.Format("Program cannot turn on output № {0}.", (16 + i).ToString()));
                }
            }
        }

        public void TestOneByOne(int timeSleep)
        {
            for (int i = 0; i < ChannelCount; i++)
            {
                try
                {
                    OwenProtocol.OwenWrite(16 + i, AddressLengthType.Bits8, "r.OE", TurnOn);
                    Thread.Sleep(timeSleep);
                    OwenProtocol.OwenWrite(16 + i, AddressLengthType.Bits8, "r.OE", TurnOff);
                }
                catch
                {
                    throw new Exception(string.Format("Program cannot access to the output № {0}.", (16 + i).ToString()));
                }
            }
        }
    }
}
