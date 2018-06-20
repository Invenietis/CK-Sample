using CK.SqlServer;
using CK.Core;
using CKS.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.DBSetupTestHelper;
using FluentAssertions;

namespace CK.Sample.Data.Tests
{
    [TestFixture]
    public class StudentAndPowerUserTests
    {
        [Test]
        public void creating_and_destroying_StudentUser()
        {
            var s = TestHelper.StObjMap.Default.Obtain<StudentUserTable>();
            var u = TestHelper.StObjMap.Default.Obtain<CK.DB.Actor.UserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var id = s.CreateStudent( ctx, 1, Guid.NewGuid().ToString(), Guid.NewGuid() );
                id.Should().BeGreaterThan( 1 );

                u.Invoking( sut => sut.DestroyUser( ctx, 1, id ) )
                    .Should().NotThrow();
            }
        }

        [Test]
        public void creating_and_destroying_PowerUser()
        {
            var u = TestHelper.StObjMap.Default.Obtain<CK.DB.Actor.UserTable>();
            var p = TestHelper.StObjMap.Default.Obtain<PowerUserTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var anyUserId = u.CreateUser( ctx, 1, Guid.NewGuid().ToString() ); 
                p.PlugPowerUser( ctx, 1, anyUserId );

                int readBack = (int)p.Database.ExecuteScalar( "select UserId+67 from SMPL.tPowerUser where UserId = @0", anyUserId );
                readBack.Should().Be( anyUserId + 67 );

                p.Unplug( ctx, 1, anyUserId );
                object readBackNull = p.Database.ExecuteScalar( "select UserId+67 from SMPL.tPowerUser where UserId = @0", anyUserId );
                readBackNull.Should().BeNull();

                // Recreates it again.
                p.PlugPowerUser( ctx, 1, anyUserId );

                u.Invoking( sut => sut.DestroyUser( ctx, 1, anyUserId ) )
                    .Should().NotThrow( "Transformer on CK.sUserDestroy has done its job." );
            }
        }

    }
}
