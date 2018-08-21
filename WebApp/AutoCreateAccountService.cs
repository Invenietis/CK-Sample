using CK.AspNet;
using CK.AspNet.Auth;
using CK.Auth;
using CK.Core;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.DB.User.SimpleInvitation;
using CK.SqlServer;
using CKS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    /// <summary>
    /// Auto creation of account from Github login based on a simple invitation.
    /// This implements the optional <see cref="IWebFrontAuthAutoCreateAccountService"/>
    /// that is registered in the DI container.
    /// </summary>
    public class AutoCreateAccountService : IWebFrontAuthAutoCreateAccountService
    {
        readonly UserTable _userTable;
        readonly InvitationTable _invitationTable;
        readonly IAuthenticationDatabaseService _dbAuth;
        readonly IAuthenticationTypeSystem _typeSystem;

        public AutoCreateAccountService(
            UserTable userTable,
            IAuthenticationDatabaseService dbAuth,
            InvitationTable invitationTable,
            IAuthenticationTypeSystem typeSystem )
        {
            _userTable = userTable;
            _invitationTable = invitationTable;
            _dbAuth = dbAuth;
            _typeSystem = typeSystem;
        }

        /// <summary>
        /// Called for each failed login <see cref="UserLoginResult.IsUnregisteredUser"/> 
        /// is true and when there is no current authentication.
        /// This checks that the authentication is from Github and that the
        /// request's <see cref="IWebFrontAuthAutoCreateAccountContext.UserData">UserData</see>
        /// contains an InvitationToken. This token is challenged and if it is valid the user is
        /// created (with a guid as its user name) and logged in.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="context">The user creation context.</param>
        /// <returns>NUll </returns>
        public async Task<UserLoginResult> CreateAccountAndLoginAsync( IActivityMonitor monitor, IWebFrontAuthAutoCreateAccountContext context )
        {
            ISqlCallContext ctx = context.HttpContext.GetSqlCallContext();
            ValidateResult r = await ValidateLoginContextAsync( ctx, monitor, context );
            if( r.ErrorId != null )
            {
                return new UserLoginResult( null, (int)KnownLoginFailureCode.Unspecified, "", false );
            }
            int idUser = await _userTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
            UCLResult dbResult = await r.AuthProvider.CreateOrUpdateUserAsync( ctx, 1, idUser, context.Payload, UCLMode.CreateOnly | UCLMode.WithActualLogin );
            if( dbResult.OperationResult != UCResult.Created ) return null;
            return await _dbAuth.CreateUserLoginResultFromDatabase( ctx, _typeSystem, dbResult.LoginResult );
        }

        struct ValidateResult
        {
            public readonly IGenericAuthenticationProvider AuthProvider;
            public readonly IInvitationInfo InvitationInfo;
            public readonly string ErrorId;
            public readonly string ErrorText;

            public ValidateResult( string errorId, string errorText )
            {
                AuthProvider = null;
                InvitationInfo = null;
                ErrorId = errorId;
                ErrorText = errorText;
            }

            public ValidateResult( IGenericAuthenticationProvider p, IInvitationInfo i )
            {
                AuthProvider = p;
                InvitationInfo = i;
                ErrorId = ErrorText = null;
            }
        }

        async Task<ValidateResult> ValidateLoginContextAsync( ISqlCallContext sqlCtx, IActivityMonitor monitor, IWebFrontAuthAutoCreateAccountContext context )
        {
            if( context.CallingScheme != "GitHub" )
            {
                throw new ArgumentException( "Only GitHub provider supports invitation." );
            }
            string invitationToken = context.UserData.FirstOrDefault( kv => kv.Key == "InvitationToken" ).Value;
            if( invitationToken == null )
            {
                throw new ArgumentException( "No InvitationToken found in UserData request." );
            }
            var invitationInfo = await _invitationTable.StartInvitationAsync( sqlCtx, 1, invitationToken );
            if( !invitationInfo.IsValid() )
            {
                monitor.Trace( $"Invitation token '{invitationToken}' not found or expired." );
                return new ValidateResult( "Invitation.InvalidOrExpiredToken.", "Invitation token not found or expired." );
            }
            return new ValidateResult( _dbAuth.FindProvider( context.CallingScheme ), invitationInfo );
        }
    }
}
