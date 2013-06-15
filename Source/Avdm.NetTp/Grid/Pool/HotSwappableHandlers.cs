using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Avdm.NetTp.Messaging;

namespace Avdm.NetTp.Grid.Pool
{
    public class HotSwappableHandlers : MarshalByRefObject, IHotSwappableHandlerContainer
    {
        private readonly ConcurrentDictionary<Type, Type> m_commandHandlers = new ConcurrentDictionary<Type, Type>();

        public IEnumerable<Type> RegisterHandlers( string loaderTypeName, string loaderArgs )
        {
            var loaderType = Type.GetType( loaderTypeName, true );
            var loader = (IHotSwappableHandlerLoader)Activator.CreateInstance( loaderType );
            loader.Init( loaderArgs );
            loader.Register( this );

            return m_commandHandlers.Keys;
        }
 
        public void DispatchCommand( INetTpCommandMessage command )
        {
            if( command == null )
            {
                return;
            }

            var handlerType = m_commandHandlers[command.GetType()];
            dynamic handler = Activator.CreateInstance( handlerType );
            dynamic msg = command;
            handler.HandleCommand( msg );
        }

        public List<string> GetLoadedAssemblyNames()
        {
            var loaded = (from m in AppDomain.CurrentDomain.GetAssemblies()
                          select m.FullName).ToList();

            return loaded;
        }

        public void RegisterCommandHanlder<TCommand, TCommandHandler>()
            where TCommand : INetTpCommandMessage, new()
            where TCommandHandler : IHandleCommand<TCommand>
        {
            m_commandHandlers[typeof(TCommand)] = typeof(TCommandHandler);
        }

        public void RegisterCommandHanlder( Type commandType, Type handlerType )
        {
            m_commandHandlers[commandType] = handlerType;
        }
    }
}