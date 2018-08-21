
alter table CK.tUserSimpleInvitation add
    FirstName nvarchar(127) not null constraint DF_TEMP0 default(N''),
    LastName nvarchar(127) not null constraint DF_TEMP1 default(N'');

alter table CK.tUserSimpleInvitation drop constraint DF_TEMP0;
alter table CK.tUserSimpleInvitation drop constraint DF_TEMP1;


