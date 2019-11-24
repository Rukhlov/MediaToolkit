using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TestClient
{
    public class WcfDiscoveryAddressCustomEndpointBehavior : IEndpointBehavior, IDispatchMessageInspector
    {
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // Attach ourselves to the MessageInspectors of reply messages
            clientRuntime.CallbackDispatchRuntime.MessageInspectors.Add(this);
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            object messageProperty;
            if (!OperationContext.Current.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out messageProperty)) return null;
            var remoteEndpointProperty = messageProperty as RemoteEndpointMessageProperty;
            if (remoteEndpointProperty == null) return null;

            // Extract message body
            string messageBody;
            using (var oldMessageStream = new MemoryStream())
            {
                using (var xw = XmlWriter.Create(oldMessageStream))
                {
                    request.WriteMessage(xw);
                    xw.Flush();
                    messageBody = Encoding.UTF8.GetString(oldMessageStream.ToArray());
                }
            }

            // Replace instances of 0.0.0.0 with actual remote endpoint address
            var remoteAddr = remoteEndpointProperty.Address;
            var remotePort = remoteEndpointProperty.Port;
            var remoteEndpoint = remoteAddr + ":" + remotePort;
           // messageBody = messageBody.Replace("0.0.0.0:0", remoteEndpoint);

            messageBody = messageBody.Replace("0.0.0.0", remoteEndpointProperty.Address);

            // NOTE: Do not close or dispose of this MemoryStream. It will be used by WCF down the line.
            var newMessageStream = new MemoryStream(Encoding.UTF8.GetBytes(messageBody));
            XmlDictionaryReader xdr = XmlDictionaryReader.CreateTextReader(newMessageStream, new XmlDictionaryReaderQuotas());

            // Create a new message with our modified endpoint address and
            // copy over existing properties and headers
            Message newMessage = Message.CreateMessage(xdr, int.MaxValue, request.Version);
            newMessage.Properties.CopyProperties(request.Properties);
            newMessage.Headers.CopyHeadersFrom(request.Headers);
            request = newMessage;


            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }
    }



    public class InternalCommand
    {
        public string command = "";
        public object[] args = null;
    }


    public class CommandQueue
    {
        private readonly LinkedList<InternalCommand> list = new LinkedList<InternalCommand>();

        private readonly Dictionary<string, LinkedListNode<InternalCommand>> dict = new Dictionary<string, LinkedListNode<InternalCommand>>();

        private readonly object locker = new object();

        public InternalCommand Dequeue()
        {
            lock (locker)
            {
                InternalCommand command = null;
                if (list.Count > 0)
                {
                    command = list.First();
                    list.RemoveFirst();

                    var key = command.command;
                    if (dict.ContainsKey(key))
                    {
                        dict.Remove(key);
                    }
                }
                return command;
            }
        }

        public void Enqueue(InternalCommand command)
        {
            lock (locker)
            {
                //if(list.Count> maxCount)
                //{
                //    //...
                //}
                var key = command.command;
                if (dict.ContainsKey(key))
                {
                    var node = dict[key];
                    node.Value = command;
                }
                else
                {
                    LinkedListNode<InternalCommand> node = list.AddLast(command);
                    dict.Add(key, node);
                }

            }
        }

        public void Clear()
        {
            lock (locker)
            {
                list.Clear();
                dict.Clear();
            }
        }
    }
}
