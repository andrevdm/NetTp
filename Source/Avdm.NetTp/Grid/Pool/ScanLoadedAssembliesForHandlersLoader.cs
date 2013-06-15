using System;
using System.Linq;
using System.Reflection;
using Avdm.Core;
using Avdm.NetTp.Core;

namespace Avdm.NetTp.Grid.Pool
{
    public class ScanLoadedAssembliesForHandlersLoader : IHotSwappableHandlerLoader
    {
        public void Init( string arguments )
        {
            Assembly.Load( arguments );
        }

        public void Register( IHotSwappableHandlerContainer container )
        {
            Preconditions.CheckNotNull( container, "container" );

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var handlerTypes = (from asm in assemblies
                                from type in asm.GetTypes()
                                where type.IsClass
                                where (type.IsPublic || type.IsNestedPublic)
                                where !type.IsAbstract
                                where !type.IsGenericTypeDefinition
                                where typeof( IHandleCommand ).IsAssignableFrom( type )
                                select type);

            var interfaces = (from type in handlerTypes
                              from intf in type.GetInterfaces()
                              where intf.IsGenericType
                              where intf.GetGenericTypeDefinition() == typeof(IHandleCommand<>)
                              select new Tuple<Type, Type>( type, intf ));

            foreach( var inteface in interfaces )
            {
                var commandType = inteface.Item2.GetGenericArguments()[0];
                var handlerType = inteface.Item1;

                Console.WriteLine( "Registering command handler: command={0}, handler={1}", commandType.Name, handlerType.Name );

                container.RegisterCommandHanlder( commandType, handlerType );
            }
        }
    }
}
