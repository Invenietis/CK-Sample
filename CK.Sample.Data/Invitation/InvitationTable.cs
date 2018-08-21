using CK.DB.User.SimpleInvitation;
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
    /// This extends the <see cref="InvitationTable"/> by adding FirstName and LastName to the table
    /// and to the <see cref="IInvitationInfo"/> POCO.
    /// </summary>
    [SqlTable( "tUserSimpleInvitation", Package = typeof( Package ), ResourcePath = "~CKS.Data.Invitation.Res" )]
    [Versions( "1.0.0" )]
    [SqlObjectItem( "transform:sUserSimpleInvitationCreate" )]
    public abstract class InvitationTable : UserSimpleInvitationTable
    {
        /// <summary>
        /// Starts the response and returns an extended <see cref="IInvitationInfo"/> whith the first and last name
        /// of the invited user.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="invitationToken">The invitation token.</param>
        /// <returns>The invitation info.</returns>
        [SqlProcedure( "transform:sUserSimpleInvitationStartResponse" )]
        public abstract Task<IInvitationInfo> StartInvitationAsync( ISqlCallContext ctx, int actorId, string invitationToken );
    }
}
