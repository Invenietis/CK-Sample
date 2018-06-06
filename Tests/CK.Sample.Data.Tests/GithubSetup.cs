using System.Threading.Tasks;
using NUnit.Framework;
using CK.Core;
using CK.SqlServer;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.DB.User.UserGithub;

using static CK.Testing.DBSetupTestHelper;

namespace CK.Sample.Data.Tests
{
    [TestFixture]
    public class GithubSetup
    {
        [Explicit]
        [Test]
        public async Task Create_Github_user()
        {
            var uTable = TestHelper.StObjMap.Default.Obtain<UserTable>();
            var gHTable = TestHelper.StObjMap.Default.Obtain<UserGithubTable>();
            var gHInfoFactory = TestHelper.StObjMap.Default.Obtain<IPocoFactory<IUserGithubInfo>>();

            using( var ctx = new SqlStandardCallContext() )
            {
                var userId = await uTable.CreateUserAsync( ctx, 1, "cat" );
                Assert.Greater( userId, 0 );

                var uGInfo = gHInfoFactory.Create();
                uGInfo.GithubAccountId = "26920011";

                var githubResponse = await gHTable.CreateOrUpdateGithubUserAsync( ctx, 1, userId, uGInfo, UCLMode.CreateOnly );
                Assert.AreEqual( githubResponse.OperationResult, UCResult.Created );
            }
        }
    }
}
