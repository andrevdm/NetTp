using System;
using Avdm.Core.Console;
using Avdm.NetTp.Core;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Grid.RestartStrategies;
using Avdm.NetTp.Grid.SupervisionStrategies;
using Avdm.NetTp.Messaging;
using StructureMap;

namespace Avdm.NetTp.Tester
{
    public class TestOutOfProcRpcB : INodeFactory
    {
        public Node Create( string applicationName, string nodeName )
        {
            var nodeB = new Node(
                applicationName,
                nodeName,
                NodeWorkerStrategy.Supervise,
                NodeRestartStrategy.OneForOne,
                NodeSupervisionStrategy.DefaultImortal );

            nodeB.StartWorker( ( _, cancel ) =>
                {
                    var bus = ObjectFactory.GetInstance<INetTpMessageBus>();

                    bus.SubscribeToRpc<DummyRpcRequestMessage, DummyRpcResponseMessage>(
                        "TestOutOfProcRpcB",
                        request =>
                            {
                                ConsoleAsync.WriteLine( ConsoleColor.DarkYellow, "NodeB - got request {0}", request.RequestMessage );
                                return new DummyRpcResponseMessage( "response:" + request.RequestMessage );
                            } );

                    cancel.WaitHandle.WaitOne();
                } );

            return nodeB;
        }
    }
}