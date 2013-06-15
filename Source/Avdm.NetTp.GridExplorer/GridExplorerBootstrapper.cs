using System.Collections.Generic;
using System.IO;
using Avdm.Config;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.TinyIoc;
using Newtonsoft.Json;

namespace Avdm.NetTp.GridExplorer
{
    public class GridExplorerBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup( TinyIoCContainer container, IPipelines pipelines )
        {
            base.ApplicationStartup( container, pipelines );

            //pipelines.EnableBasicAuthentication( new BasicAuthenticationConfiguration( container.Resolve<IUserValidator>(), "GridRealm" ) );
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = ConfigManager.AppSettings["GridExplorer.Password"] ?? "NetTp" }; }
        }

        //protected override NancyInternalConfiguration InternalConfiguration
        //{
        //    get
        //    {
        //        // Insert at position 0 so it takes precedence over the built in one.
        //        return NancyInternalConfiguration.WithOverrides( c => c.Serializers.Insert( 0, typeof( JsonNetSerializer ) ) );
        //    }
        //}

        public class JsonNetSerializer : ISerializer
        {
            private readonly JsonSerializer m_serializer;

            public JsonNetSerializer()
            {
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.None;
                m_serializer = JsonSerializer.Create( settings );
                Extensions = new string[]{};
            }

            public bool CanSerialize( string contentType )
            {
                return contentType == "application/json";
            }

            public void Serialize<TModel>( string contentType, TModel model, Stream outputStream )
            {
                using( var writer = new JsonTextWriter( new StreamWriter( outputStream ) ) )
                {
                    m_serializer.Serialize( writer, model );
                    writer.Flush();
                }
            }

            public IEnumerable<string> Extensions { get; private set; }
        }
    }
}