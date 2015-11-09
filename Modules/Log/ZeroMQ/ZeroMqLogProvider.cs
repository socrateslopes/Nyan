﻿using System;
using System.Diagnostics;
using Nyan.Core.Modules.Log;

namespace Nyan.Modules.Log.ZeroMQ
{
    public class ZeroMqLogProvider : LogProvider
    {
        private Channel _in;
        private string _targetAddress = "";
        private Channel _out;

        public ZeroMqLogProvider()
        {
            //tcp://127.0.0.1:5002
            //pgm://239.255.42.99:5558
            Initialize("tcp://127.0.0.1:5002");
        }

        ~ZeroMqLogProvider()
        {
            if (_in != null) _in.Dispose();
            if (_out != null) _out.Dispose();
        }

        public ZeroMqLogProvider(string targetAddress)
        {
            Initialize(targetAddress);
        }

        public override string Protocol
        {
            get { return (_out != null ? _out.Protocol : (_in != null ? _in.Protocol : null)); }
        }

        public override string Uri
        {
            get { return (_out != null ? _out.Uri : (_in != null ? _in.Uri : null)); }
        }

        public override event Message.MessageArrivedHandler MessageArrived;

        private void Initialize(string targetAddress)
        {
            _targetAddress = targetAddress;
            _out = new Channel("Log_Service", true, false, _targetAddress);
        }

        public override void Dispatch(string content, Message.EContentType type = Message.EContentType.Generic)
        {
            if (Environment.UserInteractive)
            {
                Console.WriteLine(content);
                Debug.WriteLine(content);
            }

            var payload = new Message { Content = content, Subject = type.ToString(), Type = type };

            _out.Send(payload);
        }

        public override void StartListening()
        {
            if (_in != null) return;

            _in = new Channel("Log_Service", false, true, _targetAddress);
            _in.MessageArrived += _gateway_MessageArrived;
        }

        private void _gateway_MessageArrived(Message message)
        {
            if (MessageArrived == null) return;
            MessageArrived(message);
        }
    }
}