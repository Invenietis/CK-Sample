-- SetupConfig: { "Requires": "CK.sUserCreate" }
--
--
create procedure SMPL.sStudentUserCreate
(
    @ActorId int,
    @UserName nvarchar(255),
    @StudentKey uniqueidentifier,
    @UserIdResult int output
)
as
begin
    --[beginsp]

    --<PreCreate />

    exec CK.sUserCreate @ActorId, @UserName, @UserIdResult output;

    --<PreCreateStudent />

    insert into SMPL.tStudentUser( UserId, StudentKey )
      values( @UserIdResult, @StudentKey );

    --<PostCreateStudent revert />

    --[endsp]
end
