﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using NetMQ;
using NetMQ.Sockets;
using Nyan.Core.Extensions;
using Nyan.Core.Modules.Log;

namespace Nyan.Modules.Log.ZeroMQ
{
    public class Channel : IDisposable
    {
        public delegate void MessageArrivedHandler(Message message);

        private readonly string _address;
        private readonly bool _canReceive;
        private readonly bool _canSend;
        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);


        private readonly NetMQContext _mqContext;
        private readonly PublisherSocket _publisherSocket;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly string _topic;
        private bool _disposed;

        public Channel(string topic = "", bool canSend = true, bool canReceive = false, string address = null)
        {
            _topic = topic;
            _canSend = canSend;
            _canReceive = canReceive;
            _address = address;

            var uri = new Uri(_address);

            Protocol = uri.Scheme;
            Uri = _address;

            if (!(_canSend || _canReceive))
                throw new InvalidOperationException("Channel was told not to Send or Receive.");

            if (_canSend)
            {
                _mqContext = NetMQContext.Create();
                _publisherSocket = _mqContext.CreatePublisherSocket();


                if (Protocol == "tcp")
                {
                    _publisherSocket.Connect(_address);
                }

                if (Protocol == "pgm") //Multicast
                {
                    _publisherSocket.Options.MulticastHops = 4;
                    _publisherSocket.Options.MulticastRate = 40*1024; // 40 megabit
                    _publisherSocket.Options.MulticastRecoveryInterval = TimeSpan.FromMinutes(10);
                    _publisherSocket.Options.SendBuffer = 1024*10; // 10 megabyte
                    _publisherSocket.Bind(_address);
                }

                _publisherSocket.SendReady += (s, a) => { };

                //do { Thread.Sleep(100); } while (!_SendReady);
            }


            if (!_canReceive) return;

            Task.Factory.StartNew(MonitorMessages);
            Task.Run(async () => await CleanupAsync());
        }

        public string Protocol { get; private set; }

        public string Uri { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Terminate()
        {
            _publisherSocket.Close();
            _publisherSocket.Dispose();
            _mqContext.Terminate();
        }

        public event MessageArrivedHandler MessageArrived;

        private void MonitorMessages()
        {
            if (!_canReceive) throw new Exception("Channel initialized with CanReceive set to false.");

            using (var context = NetMQContext.Create())
            using (var subscriber = context.CreateSubscriberSocket())
            {
                subscriber.Options.ReceiveBuffer = 1024*10;
                subscriber.Bind(_address);
                subscriber.Subscribe(_topic);

                if (Protocol == "tcp")
                {
                    //_publisherSocket.Connect(_address);
                }

                if (Protocol == "pgm") //Multicast
                    subscriber.Connect(_address);

                var prevMsgId = new Guid();

                while (!_tokenSource.Token.IsCancellationRequested) //Forever loop until Dispose() is called.
                {
                    var topic = subscriber.ReceiveFrameString();
                    var payload = subscriber.ReceiveFrameBytes();

                    Message message = null;

                    try { message = payload.FromSerializedBytes<Message>(); }
                    catch
                    {
                    }

                    if (message == null)
                    {
                        try { message = payload.GetString().FromJson<Message>(); }
                        catch
                        {
                        }
                    }

                    if (message == null) continue;

                    if (message.Id.Equals(prevMsgId)) continue; // Avoid ZeroMQ duplicates

                    if (MessageArrived == null) continue;


                    prevMsgId = message.Id;

                    MessageArrived(message);
                }
            }
        }

        public void Send(Message message)
        {
            if (!_canSend)
                throw new InvalidOperationException("Channel was told at construction time it's not allowed to Send.");

            var payload = message.ToSerializedBytes();

            if (message.Topic != "")
                _publisherSocket.SendMoreFrame(_topic).SendFrame(payload);
            else
                _publisherSocket.SendFrame(payload);
        }

        public async Task CleanupAsync()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), _tokenSource.Token);
            }
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {

            if (_disposed)
                return;

            if (disposing)
            {
                _handle.Dispose();
                _tokenSource.Cancel();
                _tokenSource.Dispose();

                if (_publisherSocket != null)
                {
                    _publisherSocket.Dispose();
                }
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
        }
    }
}