-- SetupConfig: {}
--
--
create procedure SMPL.sPowerUserUnplug
(
    @ActorId int,
    @UserId int
)
as
begin
    --[beginsp]

    --<PreUnplug revert />

    delete from SMPL.tPowerUser where UserId  = @UserId;

    --<PostUnplug />

    --[endsp]
end
