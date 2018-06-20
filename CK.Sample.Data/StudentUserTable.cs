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
    /// <summary>
    /// A StudentUser is always a User. Creating a StudentUser requires to
    /// create the "base" User.
    /// Once created as a Student, it remains a Student: there is no API to "Unplug" the
    /// StudentUser (leaving the mere User alive).
    /// Destroying a StudentUser is done by destroying the User itself. The transfomer defined
    /// here on CK.sUserDestroy does the job.
    /// </summary>
    [SqlTable("tStudentUser", Package = typeof(Package))]
    [Versions( "1.0.0")]
    [SqlObjectItem( "transform:CK.sUserDestroy" )]
    public abstract class StudentUserTable : SqlTable
    {
        void StObjConstruct( UserTable userTable )
        {
        }

        [SqlProcedure( "sStudentUserCreate" )]
        public abstract int CreateStudent( ISqlCallContext ctx, int actorId, string userName, Guid studentKey );

        [SqlProcedure( "sStudentUserCreate" )]
        public abstract Task<int> CreateStudentAsync( ISqlCallContext ctx, int actorId, string userName, Guid studentKey );
    }
}
