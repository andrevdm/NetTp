using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Avdm.NetTp.Core
{
    /// <summary>
    /// From http://stackoverflow.com/questions/229565/what-is-a-good-pattern-for-using-a-global-mutex-in-c
    /// </summary>
    public class SingleMachineGlobalInstance : IDisposable
    {
        private readonly Guid m_machineUniqueGuid;
        private readonly bool m_hasHandle;
        private Mutex m_mutex;

        public SingleMachineGlobalInstance( Guid machineUniqueGuid, TimeSpan timeOut )
        {
            m_machineUniqueGuid = machineUniqueGuid;
            InitMutex();

            try
            {
                m_hasHandle = m_mutex.WaitOne( timeOut, false );

                if( !m_hasHandle )
                {
                    throw new TimeoutException( "Timeout waiting for exclusive access on SingleInstance" );
                }
            }
            catch( AbandonedMutexException )
            {
                m_hasHandle = true;
            }
        }

        private void InitMutex()
        {
            //string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( GuidAttribute ), false ).GetValue( 0 )).Value.ToString();
            string mutexId = string.Format( "Global\\{{{0}}}", m_machineUniqueGuid );
            m_mutex = new Mutex( false, mutexId );

            var allowEveryoneRule = new MutexAccessRule( new SecurityIdentifier( WellKnownSidType.WorldSid, null ), MutexRights.FullControl, AccessControlType.Allow );
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule( allowEveryoneRule );
            m_mutex.SetAccessControl( securitySettings );
        }

        public void Dispose()
        {
            if( m_mutex != null )
            {
                if( m_hasHandle )
                {
                    m_mutex.ReleaseMutex();
                }

                m_mutex.Dispose();
                m_mutex = null;
            }
        }
    }
}