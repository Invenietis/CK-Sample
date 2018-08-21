using System;
using System.Collections.Generic;
using System.Text;

namespace CKS.Data
{
    public interface IInvitationInfo : CK.DB.User.SimpleInvitation.IUserSimpleInvitationInfo
    {
        /// <summary>
        /// Gets or sets the invited user first name.
        /// </summary>
        string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the invited user last name.
        /// </summary>
        string LastName { get; set; }
    }
}
