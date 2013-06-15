using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Avdm.Config;
using Avdm.NetTp.Grid.Executors;
using Avdm.NetTp.Grid.NodeResponsibilityHandlers;
using Avdm.NetTp.Grid.Nodes;
using Avdm.NetTp.Messaging;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using StructureMap;

namespace Avdm.NetTp.GridExplorer
{
    public class GridModule : GridNancyModuleBase
    {
        private static readonly string g_vesion;
        private static readonly Timer g_timer;
        private static readonly ConcurrentDictionary<Guid, NodeMonitorUpdateEventMessage> g_updatesByNodeId = new ConcurrentDictionary<Guid, NodeMonitorUpdateEventMessage>();

        static GridModule()
        {
            g_vesion = typeof( GridProgram ).Assembly.GetName().Version.ToString();
            g_timer = new Timer( CleanupTick, null, TimeSpan.FromMilliseconds( 5 ), TimeSpan.FromSeconds( int.Parse( ConfigManager.AppSettings["GridExplorer.CleanupPeriodSeconds"] ?? "10" ) ) );

            var bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            bus.SubscribeToEvent<NodeMonitorUpdateEventMessage>( "GridExplorer_" + Environment.MachineName, OnNodeUpdateEvent );
            bus.SubscribeToEvent<NodeLoggingEventMessage>( "GridExplorer_" + Environment.MachineName, OnNodeLoggingEvent );
        }

        public GridModule()
        {
            //this.RequiresAuthentication();

            Get["/"] = x => new StreamResponse( () => GetFile( "index.html" ), MimeTypes.GetMimeType( "index.html" ) );
            Get["/resources/{name*}"] = x => new StreamResponse( () => GetFile( (string)(x.name) ), MimeTypes.GetMimeType( (string)(x.name) ) );
            Get["/topology.dot"] = GetTopologyDot;
            Get["/topology.png"] = GetTopologyDotPng;
            Get["/topology"] = GetTopology;
        }

        private Response GetTopology( dynamic arg )
        {
            if( Request.Form.id.HasValue && Request.Form.id != 0 )
            {
                return null;
            }

            return Response.AsJson( GetTree() );
        }

        private Response GetTopologyDotPng( dynamic arg )
        {
            string dot = GetDot();
            File.WriteAllText( "tree.dot", dot );

            var process = Process.Start( @"C:\Program Files (x86)\Graphviz2.30\bin\dot", @"dot -Tcmapx -otree.map -Tpng -otree.png tree.dot" );
            process.WaitForExit();

            return Response.AsImage( "tree.png" );
        }

        private Response GetTopologyDot( dynamic arg )
        {
            return Response.AsText( GetDot() );
        }

        private string GetDot()
        {
            var update = new List<NodeMonitorUpdateEventMessage>( g_updatesByNodeId.Values );

            var byServer = (from u in update
                            group u by u.MachineName
                            into mg
                            select new
                                {
                                    Machine = mg.Key,
                                    Processes = (from p in mg
                                                 group p by p.ProcessId
                                                 into pg
                                                 select new
                                                     {
                                                         Process = pg.Key,
                                                         Items = pg.ToList()
                                                     }).ToDictionary( k => k.Process, v => v.Items )
                                }).ToDictionary( k => k.Machine, v => v.Processes );

            var dot = new StringBuilder();
            var edges = new StringBuilder();
            var processes = new StringBuilder();
            dot.AppendLine( "digraph G {" );
            dot.AppendLine( "   compound=true;" );

            foreach( string serverName in byServer.Keys )
            {
                dot.AppendFormat( "   subgraph cluster_{0}{{", serverName ).AppendLine();
                dot.AppendFormat( "      label = \"{0}\";", serverName ).AppendLine();
                dot.AppendFormat( "      " ).AppendLine();

                var server = byServer[serverName];

                foreach( int pid in server.Keys )
                {
                    List<NodeMonitorUpdateEventMessage> process = server[pid];

                    dot.AppendFormat( "      subgraph cluster_pid_{0}{{", pid ).AppendLine();
                    dot.AppendFormat( "         node [style=filled];" ).AppendLine();
                    dot.AppendFormat( "         color=blue" ).AppendLine();
                    dot.AppendFormat( "         label = \"{0} - {1}\";", pid, process[0].ProcessName  ).AppendLine();
                    

                    foreach( var node in process )
                    {
                        dot.AppendFormat( "         {0} [label=\"{1}\"]", GetIdString( node.NodeId ), node.NodeDescription ).AppendLine();
                    }

                    foreach( var node in process )
                    {
                        foreach( IExecutorInfo child in node.Executors )
                        {
                            if( child.Type == "Node" )
                            {
                                edges.AppendFormat( "         {0} -> {1}", GetIdString( node.NodeId ), GetIdString( Guid.Parse( child.ChildId.ToString() ) ) ).AppendLine();
                            }

                            if( child.Type == "Process" )
                            {
                                var childPid = int.Parse( child.ChildId.ToString() );
                                if( server.ContainsKey( childPid ) )
                                {
                                    edges.AppendFormat( "         {0} -> {1} [lhead=cluster_pid_{2}]", GetIdString( node.NodeId ), GetIdString( server[childPid][0].NodeId ), child.ChildId ).AppendLine();
                                }
                                else
                                {
                                    processes.AppendFormat( "{0} [label=\"{0}: {1}\"]", child.ChildId, child.ChildName ).AppendLine();
                                    edges.AppendFormat( "         {0} -> {1}", GetIdString( node.NodeId ), child.ChildId ).AppendLine();
                                }
                            }
                        }
                    }

                    dot.AppendFormat( "      }}" ).AppendLine();
                }

                dot.AppendLine( processes.ToString() );
                dot.AppendFormat( "   }}" ).AppendLine();
            }

            dot.AppendLine( edges.ToString() );
            dot.AppendLine( "}" );

            return dot.ToString();
        }

        private string GetIdString( Guid nodeId )
        {
            return "n" + Regex.Replace( nodeId.ToString(), @"[\{\}\-]", "" );
        }

        private NodeData[] GetTree()
        {
            Console.WriteLine( "#-------------------------------------------" );
            DumpObject( g_updatesByNodeId.Values );
            Console.WriteLine( "#-------------------------------------------" );

            var update = new List<NodeMonitorUpdateEventMessage>( g_updatesByNodeId.Values );

            var byMachine = (from u in update
                             group u by u.MachineName
                                 into g
                                 select new
                                     {
                                         MachineName = g.Key,
                                         Items = (from x in g select new NodeData( x )),
                                     }).ToDictionary( k => k.MachineName, v => v.Items.ToList() );


            foreach( KeyValuePair<string, List<NodeData>> kv in byMachine )
            {
                var machineNodes = kv.Value;

                foreach( var node in machineNodes )
                {
                    node.data.title = node.UpdateMessage.NodeDescription + ": " + node.UpdateMessage.NodeId;
                    node.data.icon = "/grid/resources/node.png";

                    foreach( var executor in node.UpdateMessage.Executors )
                    {
                        if( executor.Type == "Node" )
                        {
                            var nodeChild = (from n in machineNodes
                                             where n.UpdateMessage.NodeId == Guid.Parse( executor.ChildId.ToString() )
                                             select n).FirstOrDefault();

                            if( nodeChild != null )
                            {
                                if( node.Parent != nodeChild )
                                {
                                    node.children.Add( nodeChild );
                                    nodeChild.Parent = node;
                                }
                            }
                        }

                        if( executor.Type == "Process" )
                        {
                            var child = new NodeData( null )
                            {
                                data = new TextData( executor.ChildId + ": " + executor.ChildName ) { icon = "/grid/resources/process.gif" },
                                state = "opened",
                            };

                            node.children.Add( child );
                            child.Parent = node;

                            var children = (from n in machineNodes
                                            where n.UpdateMessage.ProcessId == int.Parse( executor.ChildId.ToString() )
                                            select n);

                            foreach( var nodeChild in children )
                            {
                                if( child.Parent == null )
                                {
                                    child.children.Add( nodeChild );
                                    nodeChild.Parent = child;
                                }
                            }
                        }
                    }

                    if( node.children != null )
                    {
                        node.children.Sort( ( x, y ) => x.data.title.CompareTo( y.data.title ) );
                    }
                }
            }

            var rootItemsByMachine = new Dictionary<string, List<NodeData>>();

            foreach( var machineName in byMachine.Keys )
            {
                var rootItemsByPid = (from m in byMachine[machineName]
                                      where m.Parent == null
                                      group m by m.UpdateMessage.ProcessId
                                      into g
                                      orderby g.Key
                                      select new NodeData( null )
                                          {
                                              data = new TextData( g.Key + ": " + g.First().UpdateMessage.ProcessName ) { icon = "/grid/resources/process.gif" },
                                              children = g.ToList()
                                          }).ToList();

                rootItemsByMachine[machineName] = rootItemsByPid;
            }

            Console.WriteLine( "?-------------------------------------------" );
            DumpObject( byMachine );
            Console.WriteLine( "?-------------------------------------------" );


            var tree = new NodeData[]
                {
                    new NodeData( null )
                    {
                        data = new TextData( "Servers" ),
                        children = (from kv in rootItemsByMachine
                                    orderby kv.Key
                                    select new NodeData( null )
                                    {
                                        data = new TextData( kv.Key ) { icon = "/grid/resources/server.png" },
                                        state = "opened",
                                        children = kv.Value,
                                    }).ToList()
                    },
                };

            Console.WriteLine( "!-------------------------------------------" );
            DumpObject( tree );
            Console.WriteLine( "!-------------------------------------------" );

            return tree;
        }

        private static void OnNodeUpdateEvent( NodeMonitorUpdateEventMessage msg )
        {
            g_updatesByNodeId[msg.NodeId] = msg;
        }

        private static void CleanupTick( object state )
        {
            var all = new List<NodeMonitorUpdateEventMessage>( g_updatesByNodeId.Values );

            foreach( var remove in all.Where( a => (DateTime.Now - a.CreatedDate).TotalSeconds > 15 ) )
            {
                NodeMonitorUpdateEventMessage x;
                g_updatesByNodeId.TryRemove( remove.NodeId, out x );
            }
        }

        private static void DumpObject( object o, TextWriter output )
        {
            var writer = new JsonTextWriter( output );
            var json = new JsonSerializer();
            json.Formatting = Newtonsoft.Json.Formatting.Indented;
            json.Serialize( writer, o );
        }

        private static void DumpObject( object o )
        {
            DumpObject( o, Console.Out );
        }
        
        private static void OnNodeLoggingEvent( NodeLoggingEventMessage message )
        {
            DumpObject( message );
        }
    }


    [Serializable]
    public class NodeData
    {
        public NodeData( NodeMonitorUpdateEventMessage eventMessage )
        {
            children = new List<NodeData>();
            UpdateMessage = eventMessage;
            state = "open";
            data = new TextData( "?" );
        }

        public TextData data { get; set; }
        public string state { get; set; }
        public string Type { get; set; }
        public List<NodeData> children { get; set; }

        [JsonIgnore]
        public NodeMonitorUpdateEventMessage UpdateMessage { get; private set; }

        [JsonIgnore]
        public NodeData Parent { get; set; }
    }

    public class TextData
    {
        public TextData( string title )
        {
            this.title = title;
        }

        public string title { get; set; }
        public string icon { get; set; }
    }
}