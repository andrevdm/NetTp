using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Avdm.Config;
using Avdm.NetTp.Messaging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using StructureMap;

namespace Avdm.NetTp.Grid.Pool
{
    public class SbinAutoUpdateFileWatcherWorker
    {
        private FileSystemWatcher m_watcher;
        private readonly ConcurrentDictionary<string,bool> m_updated = new ConcurrentDictionary<string, bool>();
        private DateTime m_lastChanged;
        private Timer m_timer;
        private long m_version = 1; //TODO
        private string m_host = "localhost"; //TODO
        private int m_port = 27017; //TODO
        private string m_basePath;
        private MongoServer m_svr;
        private MongoDatabase m_db;
        private MongoGridFS m_grid;
        private INetTpMessageBus m_bus;

        public void Run()
        {
            m_bus = ObjectFactory.GetInstance<INetTpMessageBus>();
            m_basePath = m_version.ToString() + "\\";

            Console.WriteLine( "Uploading to " + m_basePath );

            string mongoConnection = "mongodb://" + m_host + ":" + m_port;

            var client = new MongoClient( mongoConnection );
            m_svr = client.GetServer();
            m_db = m_svr.GetDatabase( "sbin" );
            m_grid = m_db.GridFS;

            if( m_watcher == null )
            {
                m_watcher = new FileSystemWatcher( ConfigManager.AppSettings["sbinWatcher.MonitorDirectory"] );
                m_watcher.Changed += ( o, e ) => Updated( e );
                m_watcher.Created += ( o, e ) => Updated( e );
                m_watcher.Renamed += ( o, e ) => Updated( e );
                m_watcher.EnableRaisingEvents = true;
            }

            m_timer = new Timer( _ => Notifier(), null, 500, 500 );
        }

        private void Updated( FileSystemEventArgs fse )
        {
            if( !Regex.IsMatch( fse.Name, @".*(exe|dll|pdb)$", RegexOptions.IgnoreCase ) )
            {
                return;
            }
                   
            m_lastChanged = DateTime.Now;

            lock( m_updated )
            {
                m_updated[fse.FullPath.Trim()] = true;
            }
        }

        private void Notifier()
        {                                                                     
            if( m_updated.Count == 0 )
            {
                return;
            }

            if( (DateTime.Now - m_lastChanged).TotalSeconds < 2 )
            {
                return;
            }

            lock( m_updated )
            {
                var update = new List<string>( m_updated.Keys );
                
                Console.WriteLine();
                Console.WriteLine( DateTime.Now );
                m_updated.Clear();
                update.ForEach( fullPath =>
                    {
                        string fileName = Path.GetFileName( fullPath );

                        using( var strm = File.OpenRead( fullPath ) )
                        {
                            string remoteFileName = Path.Combine( m_basePath, fileName );

                            Console.WriteLine( "{0} -> {1}", fileName, remoteFileName );

                            m_grid.Delete( remoteFileName );
                            m_grid.Upload( strm, remoteFileName );
                        }
                    } );

                Console.WriteLine();

                var msg = new SbinFilesUpdatedEventMessage();
                msg.FileNames.AddRange( update );
                msg.ExpireAt = DateTime.Now.AddMinutes( 1 );
                m_bus.PublishEvent( msg );
            }
        }
    }
}
