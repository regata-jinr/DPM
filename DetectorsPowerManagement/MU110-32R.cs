using System;
using System.Collections.Generic;
using System.IO.Ports;
using OwenioNet;
using OwenioNet.IO;
using OwenioNet.Types;
using OwenioNet.DataConverter.Converter;
using System.Threading;
using System.Linq;

namespace DetectorsPowerManagement
{
    /// <summary>
    /// 
    /// </summary>
    class MU110_32R
    {
        private IOwenProtocolMaster OwenProtocol { get; set; }
        private const int channelCount = 32;
        private readonly byte[] turnOn = new ConverterFloat(3).Convert(1);
        private readonly byte[] turnOff = new ConverterFloat(3).Convert(0);

        public MU110_32R(int port)
        {
            OpenPort(port);
        }

        public MU110_32R()
        {
        }

        private int portNumber;
        private int PortNumber
        {
            get { return portNumber; }
            set
            {
                try
                {
                    if (value < 0) throw new ArgumentException("PortNumber must be integer and greater than zero.");
                    portNumber = value;
                }
                catch { throw new ArgumentException($"Value = {value}. PortNumber must be integer and greater than zero."); }
            }
        }

        /// <summary>
        /// Opens the port.
        /// </summary>
        /// <param name="portToOpen">The port to open.</param>
        /// <exception cref="Exception"></exception>
        public void OpenPort(int portToOpen)
        {
            PortNumber = portToOpen;
            try
            {
                var port = new SerialPortAdapter(PortNumber, 9600, Parity.None, 8, StopBits.One);
                port.Open();
                OwenProtocol = OwenProtocolMaster.Create(port);
            }
            catch { throw new Exception($"Cannot open port {PortNumber}"); }
        }

        public void ClosePort()
        {
            try
            {
                OwenProtocol.CloseSerialPort();
            }
            catch { throw new Exception($"Cannot close port."); }
        }

        // i = 1...32
        /// <summary>
        /// Turns the on single output.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="on">if set to <c>true</c> [on].</param>
        /// <exception cref="Exception">Program cannot access to the output № {15 + i}</exception>
        public void TurnOnSingleOutput(int i, bool on)
        {
            var onOrOff = (on ? turnOn : turnOff);
            try { OwenProtocol.OwenWrite(16 + i, AddressLengthType.Bits8, "r.OE", onOrOff); }
            catch { throw new Exception($"Program cannot access to the output № {16 + i}"); }
        }

        /// <summary>
        /// Gets the current state of single output.
        /// </summary>
        /// <param name="outputNumber">The output number.</param>
        /// <returns></returns>
        public bool GetCurrentStateOfSingleOutput(int outputNumber)
        {
            return Enumerable.SequenceEqual(turnOn, OwenProtocol.OwenRead(16 + outputNumber, AddressLengthType.Bits8, "r.OE")) ? true : false;
        }

        /// <summary>
        /// Gets the state of the current.
        /// </summary>
        /// <returns></returns>
        public List<bool> GetCurrentState()
        {
            var result = new List<bool>();
            for (var i = 0; i < channelCount; ++i)
                result.Add(Enumerable.SequenceEqual(turnOn, OwenProtocol.OwenRead(16 + i, AddressLengthType.Bits8, "r.OE")) ? true : false);
            return result;
        }

        /// <summary>
        /// Turns all.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <exception cref="Exception">Program cannot turn off output № {16 + i}</exception>
        private void TurnAll(byte[] status)
        {
            var i = 0;
            try
            {
                for (; i < channelCount; ++i)
                    OwenProtocol.OwenWrite(16 + i, AddressLengthType.Bits8, "r.OE", status);
            }
            catch { throw new Exception($"Program cannot get acsess to output № {16 + i}"); }
        }

        public void TurnOffAll() { TurnAll(turnOff); }
        public void TurnOnAll() { TurnAll(turnOn); }

        /// <summary>
        /// Tests the one by one.
        /// </summary>
        /// <param name="timeSleep">The time sleep.</param>
        /// <exception cref="Exception">Program cannot access to the output № {16 + i}</exception>
        public void TestOneByOne(int timeSleep)
        {
            var i = 0;
            try
            {
                for (; i < channelCount; ++i)
                {
                    OwenProtocol.OwenWrite(16 + i, AddressLengthType.Bits8, "r.OE", turnOn);
                    Thread.Sleep(timeSleep);
                    OwenProtocol.OwenWrite(16 + i, AddressLengthType.Bits8, "r.OE", turnOff);
                }
            }
            catch { throw new Exception($"Program cannot get access to the output № {16 + i}"); }
        }
    }
}
