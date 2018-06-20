
create table SMPL.tPowerUser
(
    UserId int not null,

    constraint PK_SMPL_PowerUser primary key( UserId ),
    constraint FK_SMPL_PowerUser_UserID foreign key( UserId ) references CK.tUser( UserId )
);

insert into SMPL.tPowerUser( UserId ) values ( 0 );

