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
    /// <summary>
    /// 
    /// </summary>
    class MU110_32R
    {
        public IOwenProtocolMaster OwenProtocol { get; private set; }

        // channelCount я бы тоже заменил без геттеров и сеттров здесь на простую константу
        // если проверок не делаешь - смысла в геттерах и сеттерах нету
        //   private int ChannelCount { get; set; }

        // тут надо получше разобраться в разнице между static readonly и const 
        // https://stackoverflow.com/questions/7751680/public-const-string
        public const int channelCount = 32;
        private static readonly byte[] turnOn = { 0x20 };
        private static readonly byte[] turnOff = { 0x20 };

        private void Initialize()
        {
            // your initialisations
        }

        public MU110_32R() { Initialize(); }

        public MU110_32R(int port)
        {
            Initialize();
            OpenPort(port);
        }

        private int portNumber;

        // здесь надо проверить возможно в SerialPortAdapter уже есть проверки. попробуй передать туда 1, 0, string, null
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
                catch { throw new ArgumentException("PortNumber must be integer and greater than zero."); }
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

        // i = 1...32
        /// <summary>
        /// Turns the on single output.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="on">if set to <c>true</c> [on].</param>
        /// <exception cref="Exception">Program cannot access to the output № {15 + i}</exception>
        public void TurnOnSingleOutput(int i, bool on)
        {
            byte[] turnOn = new ConverterFloat(3).Convert(on ? 1 : 0);
            try
            {
                OwenProtocol.OwenWrite(15 + i, AddressLengthType.Bits8, "r.OE", turnOn);
            }
            catch { throw new Exception($"Program cannot access to the output № {15 + i}"); }
        }

        /// <summary>
        /// Gets the current state of single output.
        /// </summary>
        /// <param name="outputNumber">The output number.</param>
        /// <returns></returns>
        public byte[] GetCurrentStateOfSingleOutput(int outputNumber)
        {
            return OwenProtocol.OwenRead(15 + outputNumber, AddressLengthType.Bits8, "r.OE");
        }

        /// <summary>
        /// Gets the state of the current.
        /// </summary>
        /// <returns></returns>
        public List<byte[]> GetCurrentState()
        {
            var result = new List<byte[]>();
            for (var i = 0; i < channelCount; ++i)
                result.Add(OwenProtocol.OwenRead(16 + i, AddressLengthType.Bits8, "r.OE"));
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
            catch { throw new Exception($"Program cannot turn off output № {16 + i}"); }
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
            catch { throw new Exception($"Program cannot access to the output № {16 + i}"); }
        }
    }
}
