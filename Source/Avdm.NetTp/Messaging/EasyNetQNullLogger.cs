using System;
using EasyNetQ;

namespace Avdm.NetTp.Messaging
{
    public class EasyNetQNullLogger : IEasyNetQLogger
    {
        public void DebugWrite( string format, params object[] args )
        {
        }

        public void InfoWrite( string format, params object[] args )
        {
        }

        public void ErrorWrite( string format, params object[] args )
        {
        }

        public void ErrorWrite( Exception exception )
        {
        }
    }
}
