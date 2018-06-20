-- SetupConfig: {}
--
--
create procedure SMPL.sPowerUserPlug
(
    @ActorId int,
    @UserId int
)
as
begin
    --[beginsp]

    if not exists( select 1 from SMPL.tPowerUser where UserId = @UserId )
    begin
        --<PrePlug />

        insert into SMPL.tPowerUser( UserId )
            values( @UserId );

        --<PostPlug revert />
    end

    --[endsp]
end
