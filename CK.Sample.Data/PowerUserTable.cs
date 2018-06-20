using CK.DB.Actor;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CKS.Data
{
    [SqlTable( "tPowerUser", Package = typeof( Package ) )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:CK.sUserDestroy" )]
    public abstract class PowerUserTable : SqlTable
    {
        void StObjConstruct( UserTable userTable )
        {
        }

        [SqlProcedure( "sPowerUserPlug" )]
        public abstract void PlugPowerUser( ISqlCallContext ctx, int actorId, int userId );

        [SqlProcedure( "sPowerUserPlug" )]
        public abstract Task PlugPowerUserAsync( ISqlCallContext ctx, int actorId, int userId );

        [SqlProcedure( "sPowerUserUnplug" )]
        public abstract void Unplug( ISqlCallContext ctx, int actorId, int userId );

        [SqlProcedure( "sPowerUserUnplug" )]
        public abstract Task UnplugAsync( ISqlCallContext ctx, int actorId, int userId );

    }

}
