using System.Threading.Tasks;
using NUnit.Framework;
using CK.Core;
using CK.SqlServer;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.DB.User.UserGithub;

using static CK.Testing.DBSetupTestHelper;
using FluentAssertions;
using System;

namespace CK.Sample.Data.Tests
{
    [TestFixture]
    public class GithubSetup
    {
        [Test]
        public async Task Create_fake_Github_user()
        {
            var uTable = TestHelper.StObjMap.StObjs.Obtain<UserTable>();
            var gHTable = TestHelper.StObjMap.StObjs.Obtain<UserGithubTable>();
            var gHInfoFactory = TestHelper.StObjMap.StObjs.Obtain<IPocoFactory<IUserGithubInfo>>();

            using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
            {
                var userId = await uTable.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                userId.Should().BeGreaterThan( 1 );

                var uGInfo = gHInfoFactory.Create();
                uGInfo.GithubAccountId = Guid.NewGuid().ToString();

                var githubResponse = await gHTable.CreateOrUpdateGithubUserAsync( ctx, 1, userId, uGInfo, UCLMode.CreateOnly );
                githubResponse.OperationResult.Should().Be( UCResult.Created );
            }
        }
    }
}
