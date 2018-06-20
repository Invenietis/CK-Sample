
create table SMPL.tStudentUser
(
    UserId int not null,
    StudentKey uniqueidentifier not null, 

    constraint PK_SMPL_StudentUser primary key( UserId ),
    constraint FK_SMPL_StudentUser_UserID foreign key( UserId ) references CK.tUser( UserId )
);

insert into SMPL.tStudentUser( UserId, StudentKey ) values (0, '00000000-0000-0000-0000-000000000000' );


