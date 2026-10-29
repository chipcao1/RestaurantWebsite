/* DATABASE.sql — SQL Server 2012 trở lên
   Tạo database RestaurantWebsite nếu chưa có, tạo schema đúng InitialCreate của main,
   ghi lịch sử EF6 và thêm dữ liệu mẫu. Không DROP database/bảng, không thêm migration.
   Chạy toàn bộ file trong SSMS. Đổi tên ở CREATE DATABASE / USE nếu cần tên khác.
   Có thể dùng thay cho Update-Database trên database mới.
   Update-Database sau đó vẫn nhận diện migration và không tạo lại bảng.
   Tài khoản mẫu: admin.yamy@example.test / staff.yamy@example.test /
     khach1.yamy@example.test / khach2.yamy@example.test
   Mật khẩu chung: YamyDemo@2026! (chỉ dùng cho bài nộp / thử nghiệm).
   Chạy lại giữ mật khẩu, giá và nội dung bản ghi mẫu đã có.
*/
USE [master];
GO
IF DB_ID(N'RestaurantWebsite') IS NULL
    EXEC(N'CREATE DATABASE [RestaurantWebsite]');
GO
USE [RestaurantWebsite];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
DECLARE @CurrentMigration [nvarchar](max)

IF object_id('[dbo].[__MigrationHistory]') IS NOT NULL
    SELECT @CurrentMigration =
        (SELECT TOP (1) 
        [Project1].[MigrationId] AS [MigrationId]
        FROM ( SELECT 
        [Extent1].[MigrationId] AS [MigrationId]
        FROM [dbo].[__MigrationHistory] AS [Extent1]
        WHERE [Extent1].[ContextKey] = N'RestaurantWebsite.Migrations.Configuration'
        )  AS [Project1]
        ORDER BY [Project1].[MigrationId] DESC)

IF @CurrentMigration IS NULL
    SET @CurrentMigration = '0'

IF @CurrentMigration < '202609281435011_InitialCreate'
BEGIN
    CREATE TABLE [dbo].[Categories] (
        [Id] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500),
        [ImageUrl] [nvarchar](250),
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.Categories] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[Dishes] (
        [Id] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](150) NOT NULL,
        [Description] [nvarchar](1000),
        [Price] [decimal](18, 2) NOT NULL,
        [PreparationTime] [int] NOT NULL,
        [Status] [nvarchar](30) NOT NULL,
        [CategoryId] [int] NOT NULL,
        [CreatedAt] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.Dishes] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_CategoryId] ON [dbo].[Dishes]([CategoryId])
    CREATE TABLE [dbo].[ComboItems] (
        [Id] [int] NOT NULL IDENTITY,
        [ComboId] [int] NOT NULL,
        [DishId] [int] NOT NULL,
        [Quantity] [int] NOT NULL,
        CONSTRAINT [PK_dbo.ComboItems] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_ComboId] ON [dbo].[ComboItems]([ComboId])
    CREATE INDEX [IX_DishId] ON [dbo].[ComboItems]([DishId])
    CREATE TABLE [dbo].[Comboes] (
        [Id] [int] NOT NULL IDENTITY,
        [Name] [nvarchar](150) NOT NULL,
        [Description] [nvarchar](500),
        [Price] [decimal](18, 2) NOT NULL,
        [ImageUrl] [nvarchar](500),
        [IsActive] [bit] NOT NULL,
        [StartDate] [datetime],
        [EndDate] [datetime],
        CONSTRAINT [PK_dbo.Comboes] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[OrderItems] (
        [Id] [int] NOT NULL IDENTITY,
        [OrderId] [int] NOT NULL,
        [DishId] [int],
        [ComboId] [int],
        [Quantity] [int] NOT NULL,
        [UnitPrice] [decimal](18, 2) NOT NULL,
        [Note] [nvarchar](500),
        [SubTotal] [decimal](18, 2) NOT NULL,
        CONSTRAINT [PK_dbo.OrderItems] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_OrderId] ON [dbo].[OrderItems]([OrderId])
    CREATE INDEX [IX_DishId] ON [dbo].[OrderItems]([DishId])
    CREATE INDEX [IX_ComboId] ON [dbo].[OrderItems]([ComboId])
    CREATE TABLE [dbo].[Orders] (
        [Id] [int] NOT NULL IDENTITY,
        [UserId] [int] NOT NULL,
        [ReservationId] [int],
        [TableId] [int],
        [OrderType] [nvarchar](30) NOT NULL,
        [Status] [nvarchar](30) NOT NULL,
        [DeliveryAddress] [nvarchar](500),
        [Phone] [nvarchar](20),
        [SubTotal] [decimal](18, 2) NOT NULL,
        [DiscountAmount] [decimal](18, 2) NOT NULL,
        [DeliveryFee] [decimal](18, 2) NOT NULL,
        [TotalAmount] [decimal](18, 2) NOT NULL,
        [Note] [nvarchar](500),
        [CreatedAt] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.Orders] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_UserId] ON [dbo].[Orders]([UserId])
    CREATE INDEX [IX_ReservationId] ON [dbo].[Orders]([ReservationId])
    CREATE INDEX [IX_TableId] ON [dbo].[Orders]([TableId])
    CREATE TABLE [dbo].[OrderPromotions] (
        [Id] [int] NOT NULL IDENTITY,
        [OrderId] [int] NOT NULL,
        [PromotionId] [int] NOT NULL,
        [DiscountAmount] [decimal](18, 2) NOT NULL,
        CONSTRAINT [PK_dbo.OrderPromotions] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_OrderId] ON [dbo].[OrderPromotions]([OrderId])
    CREATE INDEX [IX_PromotionId] ON [dbo].[OrderPromotions]([PromotionId])
    CREATE TABLE [dbo].[Promotions] (
        [Id] [int] NOT NULL IDENTITY,
        [Code] [nvarchar](50) NOT NULL,
        [Name] [nvarchar](150) NOT NULL,
        [Description] [nvarchar](500),
        [DiscountType] [nvarchar](30) NOT NULL,
        [DiscountValue] [decimal](18, 2) NOT NULL,
        [MinimumOrderAmount] [decimal](18, 2) NOT NULL,
        [MaximumDiscount] [decimal](18, 2),
        [StartDate] [datetime] NOT NULL,
        [EndDate] [datetime] NOT NULL,
        [UsageLimit] [int],
        [UsedCount] [int] NOT NULL,
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.Promotions] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[Payments] (
        [Id] [int] NOT NULL IDENTITY,
        [OrderId] [int] NOT NULL,
        [PaymentMethod] [nvarchar](30) NOT NULL,
        [Amount] [decimal](18, 2) NOT NULL,
        [Status] [nvarchar](30) NOT NULL,
        [PaymentDate] [datetime] NOT NULL,
        [TransactionCode] [nvarchar](100),
        CONSTRAINT [PK_dbo.Payments] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_OrderId] ON [dbo].[Payments]([OrderId])
    CREATE TABLE [dbo].[Reservations] (
        [Id] [int] NOT NULL IDENTITY,
        [UserId] [int] NOT NULL,
        [TableId] [int] NOT NULL,
        [ReservationDate] [datetime] NOT NULL,
        [PartySize] [int] NOT NULL,
        [SpecialRequest] [nvarchar](500),
        [Status] [nvarchar](30) NOT NULL,
        [CreatedAt] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.Reservations] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_UserId] ON [dbo].[Reservations]([UserId])
    CREATE INDEX [IX_TableId] ON [dbo].[Reservations]([TableId])
    CREATE TABLE [dbo].[RestaurantTables] (
        [Id] [int] NOT NULL IDENTITY,
        [TableNumber] [nvarchar](20) NOT NULL,
        [Capacity] [int] NOT NULL,
        [Zone] [nvarchar](50) NOT NULL,
        [Status] [nvarchar](30) NOT NULL,
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.RestaurantTables] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[Users] (
        [Id] [int] NOT NULL IDENTITY,
        [FullName] [nvarchar](100) NOT NULL,
        [Email] [nvarchar](150) NOT NULL,
        [PasswordHash] [nvarchar](max) NOT NULL,
        [Phone] [nvarchar](20),
        [Address] [nvarchar](250),
        [Role] [nvarchar](30) NOT NULL,
        [IsActive] [bit] NOT NULL,
        [CreatedAt] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.Users] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[Reviews] (
        [Id] [int] NOT NULL IDENTITY,
        [UserId] [int] NOT NULL,
        [DishId] [int],
        [OrderId] [int] NOT NULL,
        [Rating] [int] NOT NULL,
        [Comment] [nvarchar](1000),
        [CreatedAt] [datetime] NOT NULL,
        [IsApproved] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.Reviews] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_UserId] ON [dbo].[Reviews]([UserId])
    CREATE INDEX [IX_DishId] ON [dbo].[Reviews]([DishId])
    CREATE INDEX [IX_OrderId] ON [dbo].[Reviews]([OrderId])
    CREATE TABLE [dbo].[DishImages] (
        [Id] [int] NOT NULL IDENTITY,
        [DishId] [int] NOT NULL,
        [ImageUrl] [nvarchar](500) NOT NULL,
        [IsPrimary] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.DishImages] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_DishId] ON [dbo].[DishImages]([DishId])
    ALTER TABLE [dbo].[Dishes] ADD CONSTRAINT [FK_dbo.Dishes_dbo.Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id])
    ALTER TABLE [dbo].[ComboItems] ADD CONSTRAINT [FK_dbo.ComboItems_dbo.Comboes_ComboId] FOREIGN KEY ([ComboId]) REFERENCES [dbo].[Comboes] ([Id])
    ALTER TABLE [dbo].[ComboItems] ADD CONSTRAINT [FK_dbo.ComboItems_dbo.Dishes_DishId] FOREIGN KEY ([DishId]) REFERENCES [dbo].[Dishes] ([Id])
    ALTER TABLE [dbo].[OrderItems] ADD CONSTRAINT [FK_dbo.OrderItems_dbo.Comboes_ComboId] FOREIGN KEY ([ComboId]) REFERENCES [dbo].[Comboes] ([Id])
    ALTER TABLE [dbo].[OrderItems] ADD CONSTRAINT [FK_dbo.OrderItems_dbo.Dishes_DishId] FOREIGN KEY ([DishId]) REFERENCES [dbo].[Dishes] ([Id])
    ALTER TABLE [dbo].[OrderItems] ADD CONSTRAINT [FK_dbo.OrderItems_dbo.Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id])
    ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [FK_dbo.Orders_dbo.Reservations_ReservationId] FOREIGN KEY ([ReservationId]) REFERENCES [dbo].[Reservations] ([Id])
    ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [FK_dbo.Orders_dbo.RestaurantTables_TableId] FOREIGN KEY ([TableId]) REFERENCES [dbo].[RestaurantTables] ([Id])
    ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [FK_dbo.Orders_dbo.Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
    ALTER TABLE [dbo].[OrderPromotions] ADD CONSTRAINT [FK_dbo.OrderPromotions_dbo.Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id])
    ALTER TABLE [dbo].[OrderPromotions] ADD CONSTRAINT [FK_dbo.OrderPromotions_dbo.Promotions_PromotionId] FOREIGN KEY ([PromotionId]) REFERENCES [dbo].[Promotions] ([Id])
    ALTER TABLE [dbo].[Payments] ADD CONSTRAINT [FK_dbo.Payments_dbo.Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id])
    ALTER TABLE [dbo].[Reservations] ADD CONSTRAINT [FK_dbo.Reservations_dbo.RestaurantTables_TableId] FOREIGN KEY ([TableId]) REFERENCES [dbo].[RestaurantTables] ([Id])
    ALTER TABLE [dbo].[Reservations] ADD CONSTRAINT [FK_dbo.Reservations_dbo.Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
    ALTER TABLE [dbo].[Reviews] ADD CONSTRAINT [FK_dbo.Reviews_dbo.Dishes_DishId] FOREIGN KEY ([DishId]) REFERENCES [dbo].[Dishes] ([Id])
    ALTER TABLE [dbo].[Reviews] ADD CONSTRAINT [FK_dbo.Reviews_dbo.Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id])
    ALTER TABLE [dbo].[Reviews] ADD CONSTRAINT [FK_dbo.Reviews_dbo.Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
    ALTER TABLE [dbo].[DishImages] ADD CONSTRAINT [FK_dbo.DishImages_dbo.Dishes_DishId] FOREIGN KEY ([DishId]) REFERENCES [dbo].[Dishes] ([Id])
    CREATE TABLE [dbo].[__MigrationHistory] (
        [MigrationId] [nvarchar](150) NOT NULL,
        [ContextKey] [nvarchar](300) NOT NULL,
        [Model] [varbinary](max) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])
    )
    INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES (N'202609281435011_InitialCreate', N'RestaurantWebsite.Migrations.Configuration',  0x1F8B0800000000000003EC1D5D6FDCB8F1FD7EC5629FDA22E7B5131C700DEC3BA44ED206BD24AE37772DEE259077695BA83EB69236B55BF497F5A13FA97FA194A80F7ECC50A4444ABB572340E025C5992139331C0E879CFFFEFB3FE7DF3FC4D1E20BC9F2304D2E966727A7CB054936E9364CEE2E96FBE2F6EB6F97DF7FF7D5F99B6DFCB0F8A9F9EC45F9196D98E417CBFBA2D8BD5CADF2CD3D8983FC240E37599AA7B7C5C9268D57C1365D3D3F3DFDEDEAEC6C452888E5775F2D16E7D7FBA4086352FE4D7F5DA6C986EC8A7D10BD4FB724CA5931AD585720171F8298E4BB60432E96D7242F827D1624C59FC94D1E16E48435592E5E456140895993E876B90892242D828292FAF2C79CAC8B2C4DEED63B5A10449F1E77847E771B4439A9BBF0B2FBDCB437A7CFCBDEACBA860DA8CD3E2FD2D812E0D98B7A785672F34183BC6C06900EE11B3AD0C563D9E96A182F96974141EED2EC71B99071BDBC8CB2F2437C944F9AC6CF16CA27CF5AE6A02C54FE7BB6B8DC47C53E231709D91759103D5B5CED6FA270F347F2F829FD2B492E927D1471B4526A6915FF9B965C65E98E64C5E335B9AD3BF06EBB5CAC84562BA959DB886BC13AF62E295E3C5F2E3E50C4C14D445A36E006615DA419F93D494846FBBABD0A8A8264490983542329E3965095FF37C828DB51195A2EDE070F3F90E4AEB8A7D2754AC5E66DF840B64D494DC08F4948458E362AB23D0108D4227D4DF24D16EE18BFA0B8BF31C4AD45F52E0EEEC88F59A4C1F3FC1B1778F2579B22FCD28EE5EFD2342241D23B321F822FE15D3591F21885F93DA16AE29A4455757E1FEE98B63829AB3E7762F1364BE3EB34AADBB4159F3F05D91D2928412954BB4EF7D94624E77CD5099F4E224B4083A4B16CF824890325D1904B7D4822D5020E44E42A0B376D0F5F934D1807542CAF32FA57BD947FBB5CAC3741493B34D23DC0C92EC8AA79F814760389CD9A16D69ACEE73ED78CC70B1F53D148663FCBE9C164A4E4BE57453BD2F4271B92A1AAA85335C39551A36E6065D4A82A5382D2F8267D579018D68F6DF567A6A93A9AC41A45434AD5808AECD3D8D572836BEDAA5AA14AAC01F536576D49D5C76C4B327CACDA6A852AB146A14AAAB6A4EA9A7C09C9DF6192589D420F57AC10C3D78D58D8DAF91F666B36AD9F96384C375523344EBF55D2300EC49FF6414DAD0D10BD36EA5144F5379026AAFE52F4A35C6FA92199F08CD18D384182040E95B2E112F6245D08AA0336209D6CE5BCDA8F06FB4437FBD181FB44D544CD8AD2B253AD3C6DBB37C9D6A4D578CBAB57E1E1B697A8105D9B390A615295C6D0410933D57C2DA841DAAF6DFDA40111546C843CD81703AC1A0F16880484AA9DC2AB4EFC9016BA05C5893E5CEF6F3ED1C98F9CF561809966A11C64AB08531E63CD34E36D194E1066A6F56AD11E82EA6F208AAABF3424B1FA11A6638D7BA8F27C529C982AC947EB4D3AB224FB521163A70B3F9538EC9A5493597E3EB58F6E1ECFE06B12518B317B7CB5DD6624D7A1776367DFA7896E649F1FA2DA5715EB26DD27C5ABB8FCDF1B967A62DE126F6B70354A7E7B31C532EFC12B6D6CF1F72E58B8C52F2E683684D1B238AD48C2A96BBF414894EA613AE58F2C89BD0A1E63BA44C054D6952A7942854297586BED196ED7117CE43E0B5F49C3C657C2C688F085A58964E0B956868B2FC77CD78306AB5A3B35C354D7CB0354152343C3EA2C07A5B41F3464B06A998AB21421A2AA1A6B27B69231DC606C413C598E082A175BEE7694C76FDD7D2CFA43774BB6EA1D14056C0D3055EF9D0CF493C97D8C92DAFDD5436EF7D708391E27C24FD2DB7F20B7D59B7E1E7633BFF4638A460BCDB13F6D70FF14447B6F5B92F76112C6FBB89277BF3B133A5C25A6A657E668869E9DD891871EA658FA71F3E08EFC10C661B1B4F0815013697BC98FCAA025D379D8E280CD8FDDB2D3B30952D7A721CB0EDBC00C5B7458DBA7250741E5C4606463FC9E14F7E9766A1DEB57E5CDE361AC07D48536FB44393E0F3625A7F45817A6B1E5A3AD714337866CD0C24E8E01EA4470560C50295CFB27B582A0727082011F440C3E0571214C57D452795C87FF20A3885AEFA86A0AA26BF2B73D651EEF27BAF3444F7B72306B2C1873272468B3806ECAD17E3F11ADE4FD532A159DA77EE1CA13C84396FD81729D96ACB1BEC14E81D6E3334C23F3309EB4B24EA57ED8C737E57C8F3D50B4BD4EB10B3663436C7ED69F847A7155CCA33FFD6C0475DA537B36016B4C5167D91F2861A736562A533DBFC194EA00E5C434E3008D54367C524308AAB714729FDBD1CB45D7377110EA428AFD383BAF823CFF7B9A6DFF1094A1632872FAA70FE413448FF407C238B9D35BCAF7B1E8E103308735A7BCB03A174C3D1FDABCD7CED4E9F261D4F50607A834B5C55868004689B9C55B821968E8964D9F1616049503AF837DBCB90B0FEA35ED7DA9CEC65C344E63E624F77C6D7CB82A5355EB6E97A55FC8D699918BC66F1BDC6155F7B8EAFDD6D1AE4FA3882484924101079AFD7FAFFA43E818BBE16FEF4F0F7EB3A26AFDA4046D34D86CB7EF6C75C25516C641F6E85F2518DFFF879E4A009E07E8978457799E6EC28A0E8EB8EEB106A1436F92ED42FB70039B98A627746E28A7873BCADB14F1C5F237F20061F05A1BA783D7BD1E21C23C936152812059C993417449C7960A5898148AF084C926DC05910EBBD8C64CE4CA616E814B15AFC98E24A5B4E886D00029FF068882BC45222A829E41395F716CA0650EF91A14329DE895A86E42B97B8EC65C825EC3E4F884DDCE12419E9E9C38E11304BF6F4E4146D384579A6B8733324AA5097A2754D46AAED8447C0844AF9DDC33098F7D3A1EE147D2006BB332CFC32172FCAB6E46D15858895BB858213B9641EF524808BCAC413A0A26611E64780D70B79BCDB979A89B79B369060295DDF3121096D62141E1FBE029859289F94A196E03FC42DCFFE4FC25460521F38D44087193DC840A1AB31072776A0A3D04A2F6CD28E0101EB4E6510F0491A9D4C45374D3294489187389E6DC51002D863278E0199410DF7C830EAE01E236946B06DEE91C7FE8D4024E407E5299B7DE82558087F1A6B18701D4FED94219BD43B6840507AC7E02D135662847CCB0C2409827E288635A5F3A57B87E0645B7F8788E104F193B78CC2BEF8F1F78BC13B1033F7406289B93BC598D0D3D476031922E4D8D191804463EA575712CACA2461DEB76A9F033083D8B40DFD6170E6B36E240A73E38888E4976BCD0A81A20965E479A8975B49B19F06988B1EC3274EBE2965926DDB700E378D03B162E784B3B95D8A2338C31265F6854B4D330C2512D2EDC7B49BAD9831EFB7375AC33977B5E453E097FA84379D05B17F9815F643AD1D7CDA5C35C4B2E41DF10EE392376C2250872DF5C820CA501DAF90E88A547E67B671373858DE3111B8798630E99D221060FE321FBC4A448286422B1A8287122EBC03EAB602428D7CA04EC01A3F6CD1EF0301E0E7BB0C835DAA2A0DF93ACB9E1B1DBBDBE290BC903F40C07B592EAC8CDBC8EFB93A6BB84B92685185615969979BA483925FA4CE619114893904D01C01846DF987F375FA5A05373065050087DAD5B7B0382C0196D0650500846ADF9E7616030DCD1B91E9E16943994F6114C154673BEAB87205E4B51A0086E915E48FCC6188126EE9DF510CB2D0504866DB5FAA8A92FB40044301774BFD03439B140C1A9F5B70885D31422282ED118F70D98894CD25A7DE1AD2DE19DA0CB8AAF2FA29503C12B1C296E5BEC5B7FB7E5A84CA0E3BAC04D6497259AE51CE5BC92C007000BD6E487A0D655CEBACFD4ACAEF78A3581D02CD81323FA2E58107D0C34B0EB72101D3600BA603B957C2CDC4E1E0C5EBDF68C0816602783743E2E9C96EF1D1B38884CD719358CCCC918A981631C5814A2FD58494F1FA90384474109E4C371503CCDED128A8F011CF9E49E41800BFD6AC7F5413E02DD78980F47BB6801E0838007F688C0242360F49070F7D1A0C180A35624CAD5B81581E6DA54D0F55D8D5471AF37C53B6F68677B381E0CCA18D45DFFECCEDDAC43BBAB38F82152051FFFA0CE0A7E7D0E426D843A946CBCBF9A03755414F19EDB8BB5DF31504F81B1750F3D2A06CE63A0C3E23E56ED0103EB356C34870E04AAE0B1234F806A54A99B767B3A45CE9DD6A17DD6733D709A37A0C713F0389FBC4367F69B18BCC089D508C3DFB74657D228AADDD71DC4181DC5C85BB5DEEE63872FEEF77C728E5A5DEFF5260B72C430A2EF9E6D17F99230ECE5409CE726EE7389E4C637A3F775000EF3E15D6F2E2BB72EDFA6EA7CB5DEDC933860BFCF57F4830DD915FB206257F1EBF2F7C16E17267779DBAC2E58AC77C1A69CABAFD7CBC5431C25F9C5F2BE28762F57ABBC829B9FC4E1264BF3F4B638D9A4F12AD8A6ABE7A7A7BF5D9D9DAD620663B5C9B911957DD32DA222CDE88048B5EC35FDB7619697EFE80637417969FC721B2B9F09BE6DD87BD66052DCD7CA34352EB5A645F9B7EC4894B328C32EB26E24DFD2DE951BB5AAA344756BA92D69DBF2C1E320035E21B84CA37D9CA027197863F66217DF9C951803101EFDE7E10815C6E0BA470A842EB5A5E680DA379E04406DA902E87C25CD8C7C24A3F081288A325F99305D2DD52E180E505406CC06B6FABF60B43AA92B0FA82EB20041764156295FF6548E084CAA3406DBBC4DC9436BCA8C81F08F1CF080348F1F688075AF0309B0BAE2839026CECC70A2C2DB13437B1D8E36F5225B6D108A303B48640A0EA6398C16840B3EA0C68174599779305DE9E1708A432E19C8214F9A77A0E69DCF4AD028ED26DF8DA4B79B6263506DC61B1E505B7810F2C3EDE55DC8501717612D4778532FB2D40685F210B048511C8C134DEB48EB5B2B6C0CD08F495800B2CD159BABAC54E67F56622E8E6DEA63411ADBD2C39122871234507AA6919C26E65E600D380E1F0722DDF0E161E92FFFE020DB3B213C30ECA2080EA61AC972A200D5C08AA7B5FF9574E6E2BA2D559AAFDDECE16961ED6645FE4453A344854CA4923215EAACC7ADCA360E8D595561CE5D7C667181C3F88AE9F4E291ECE4E4980F672A128B4D31D59578FB43363784378044BB1B7F1C0807374AF0E66027C79C34828926E61F961C4EB40FB75686D881ED1DC5BCAF10E3592EB652325708625D650C124ADDCAC385EACD81CBD95A05C872E5116C5F717BB14BD82ADA8C5DB98DF1D9E47195ECCFA6F89775AED0060A3A51777574BEBDB2431A1EF45229663D15164BB1CA1824A403ACE5DEC9A640C8400A74CD5244952CA482812B571E846008B1512E84038FFE3210105DE3C3DD843BDA312B593C91DDBC255372D93C450E6F8BCD654E4AE929C89E5437F1E9DE716CE394C03C4712275CCC1A22755A005E244F48D2A8084E536171BCDB245E140F779B5263403F2BFE9C9F2DDD392EF8F9288C2A1675E9828581D85103BE055B7961D62E951F0FA22B35DF0BB0D47CC24E801559E8733ECF9EA8D2F99A295D98A053D5DE99CA52DF09CB5EAADC34D54EB2ABD3CBA3594FD80D0937CB4875B576C0EA01B63B5C73CDC959A3A3DD5593114DE0F9BACCE6DC93DD1697CE3D812BE45A30F63CAF91C336E99924896DF9410810178DEC4286BABBE5D6628437F522494E84C061D4499B104B6497B678426E5142C5A52F5ACCACA0FDD9848AD781DA7CF878D5EF321ABCEA6F5E878CCB81DBEC93E5820ECE97705B066DAF1FF382C427E50727EBBF459751588974F3C1FB20096F292FB2246FCBE7A767CF978B575118E42C6CBF8E487F29BF646214A27EF6A20C5127DB782537B70F742FA1E4F996CF41A766CC833384F94E54179603DA9B8ACE2EBF1B330A19FCE44B906DEE834CCDFB6C99B1933F1ED080FE46046D90EA524E830782AD521BDB8195720ADF940E6A5D9F6D322BFE3FF0C837DE7884A548B59ACD3A608901DD920D55CB51A988E85F552ACAE5D9B77488A86EA7D5D6891895D0706ED8ED20351B6D4DE75F580F2B1F22CE51A6BC0FF52ED992878BE53FAB662F17EFFEF2B96BF96C5119872F17A78B7F8D4EABBDA53F8BDE5CB4A6F204C7661FA550B5C17E96D3C49A8D9923311DA92962D66A0CDE2E2CD15066ACB8E2F839624E356BBF147BD5B246EBFC00F3C1769D07B57673AA8E2B380352DA43750B28A6F200C7551FA54CB46E0A3B6555373B342D6992CCDDCBB26080D85A3BAB20B868713F6A81054C3A55095DECAA0B92AD24F4F8A5B3F167DAF12A6B354636A578713BF442E31102D39E9ADBA1AF9B8D40CCC5A53BDD3B78D99028E1EA6E8D1076F8A37306CCAB10C0754508B2F584858F78F7834288823F1A7DEF7983AA09343F4A0D3F9FFD2584B7DB61E79A8EB4009D4BAA291FFD82588845106A85F897BD5316A3DBDDAEAE6298BB1F2D0CC5BC7BC224C7C0DBA33159E14DB6F026E41AECE1CDACE82E2A1EDC7B19F4898B861FBC7DF3760A02C5921FA52A9B713514E3D79D2A11BF32ED655721C4BC8F933E25D6DDFC2C74B0830ECD50798C42319713C0CFFEDBD2FDE08203B9F0F6E187895248BB5B1F9997934ABF1B315DA0F8518A99106B6EECF0303B336E22CF07B3DFCF7D6E187B7BDC0BD3793371D4ECA0C7C8635D94B8DB80A03A6EDCED864D0C211761FF2A0E1E7E6D0DD0BD33D1C4EB691FAEC422CDE7158C1914BA12BB7D94323697B934D379E67C5BA626367DF0A2D686A5BB0D4F1B2827A0CCB611EA1EE216D508EFA394B7B9A29D8684B098CD7A1B683E6ED2E1B4A3707E38FEA5F736C11C432FA588EC490B2A3E9BDE9B5654C9533A28EB2CFA9CB701F72E2C33891AE51DC6DFF19D328D797F82EAFEC4D1A727279A39E672667280B8521FB30DBFE9EA78AA35AF5E02F33D5B9E694DEA3F5B513CBC993615B7E9267AB684D1BD190EE5A952A7C94275CB196865505C958769479ECBF431EFF0292C820EBB5139F9EC2367C75C9A4170EAD0593B584EB09A9E89B9E14AF33A9F7F8EC093563A55046DEE679E8D9AB2A3157DF4CDAC4392797D764E31EF9E980B9B9B2BA5CE78E685EC7A22C4AEDC0307F43E20E39817B44F44A9B8B09794A6E0073841A95B33AFC9692ECC382B3A56030F7B4BE280AC3B341FAB53653EEDDC4EA3CACD27774E4D0EA79FE5A68665B3E42686151CE8D4C22F24CD36B3C8A32BD32ED17E67779EB578A279B65A80679B6C7D3EE171B365E07AE9D7FCAECC2F9BD918BFE732C1A2CF1E31D5D4FB32C20F6AF227B5BD8D19603EAB1B4927ED50B94F39C1932874E3599D57952389B3DDFB5127769A4FE83D35F59ACF677AEB7284DB9F856966BA7BB7448172E407619A449B877410A6C9876EEB2139B0699EC4496235CBB3F9493499DF1D4E72171B2303A94B8F7592352F42CE36C9D2F386DC9542318FBC3457F57B8952BCC572D105C228812BEC45C38BE5B652F52C9686CB2BAFB0838882F190029E1543A0EBECE13D6039D5A192DED581B47709954D90600834C0FBC9E74C19053A570761E0B2949A20C11068809B01E64E30610CDC0728AAAB2E87550F4E1DBA1E4C16489A233A15455303226812D3F480173C190A0AA1164223A4F9E84725EE9D2174E217084A31D7410F5AB6875370B16208017B7EBEB733CC250BF48155C0A4B367B50DF453BD3C814AAAAEC33455FDEEB0848453D922A62E8870C17DC3A183A30C455B4A52DA2CF3765B28AF3BF2FACE356205524CAA487B7FB7E4B039A063BAC83A683FC1F78B95E09D5234A99440D95DF7D868E97A07182396C33F7DD7E4D828AC83BA182A68F72F53DCDB5145C143F9291D779903DCDB6D2CC8050E1FE2BA80513FD31088113040BFF11099B1F32C2DAF423AB5B1DD52C33E80AEE96343F48E698E66A50EEF32B0E42B49B2C677BD8B70003B0D07408C534EE29ACC279170D41D943FD143FFB1ECE9BD4BCC0C427B047899157F39471B2B98A73BD2F96D8FA839EF987F99524F2DB115023DDA1C49F3006E1EDA4D545F6207783E34A5DF2E626C8A9C538D6350AF5DE1CE5FB4C6A82733CDA33D2A1F3A00DDD39D4B8CDD49282EA5AE99E3EE612BB5C6173F6EB1F6DD35C9BF8CEC6D110FF4B8AE29FB7729118F69D79484314DD5F98AEDFDD96FFA4BCE0C73BEBADE27E535D5EAC76B9287776DF3730A2E211BDE77DB7EF12EB94D1BF7B1484AF385FC081829826D5004AFB222BC0D3605ADDE903CAFEEF256EF9C95EF09DC90EDBBE4E3BED8ED0BDA5512DF44BC4FA0F4426BB09FAF647ACF3F560FBFE5E3A9A71486E585DE8FC9EFF661B46D497EAB5ECB442094CEEDFAFA6B397B45790DF6EEB105F4414E7787C1A9C7ADF5C77F22F12EA2B0F28FC93A286FFA5B534617841FC85DB079BCAA93FAA0307A274018EFF3D76170970571CE00748DBFFB8AF2EB367EF8EE7F000000FFFF030074FC10CBD61E0100 , N'6.5.2')
END


    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
/*
  DỮ LIỆU MẪU YAMY — SQL Server 2012 trở lên, schema main / InitialCreate.
  Chạy Update-Database trước để tạo bảng, rồi chọn đúng database trong SSMS.
  File này chỉ INSERT dữ liệu, không tạo/đổi/xóa bảng, không sửa lịch sử migration.
  Chạy lại không thêm trùng các bản ghi mẫu còn nguyên khóa nhận diện.
  Không ghi đè mật khẩu, giá, trạng thái hoặc nội dung bản ghi đang có.

  Tài khoản minh họa (chỉ dùng ở môi trường thử nghiệm):
    admin.yamy@example.test   — Quản trị viên
    staff.yamy@example.test   — Nhân viên
    khach1.yamy@example.test  — Khách hàng
    khach2.yamy@example.test  — Khách hàng
  Mật khẩu chung: YamyDemo@2026!
  Hash PBKDF2 tương thích PasswordHasher của ứng dụng, salt riêng mỗi tài khoản.
  Tài khoản đã tồn tại sẽ giữ nguyên mật khẩu cũ.

  Nội dung tiếng Việt; mã role/trạng thái giữ nguyên để ứng dụng xử lý.
  Thời gian tính theo Việt Nam (UTC+7), tương thích dữ liệu main.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51000, N'Hãy chọn database RestaurantWebsite trong SSMS trước khi chạy.', 1;

DECLARE @RequiredTables TABLE (Name sysname NOT NULL);
INSERT INTO @RequiredTables VALUES
(N'Users'), (N'Categories'), (N'Dishes'), (N'DishImages'), (N'RestaurantTables'),
(N'Comboes'), (N'ComboItems'), (N'Promotions'), (N'Reservations'), (N'Orders'),
(N'OrderItems'), (N'Payments'), (N'OrderPromotions'), (N'Reviews');
IF EXISTS (SELECT 1 FROM @RequiredTables WHERE OBJECT_ID(N'dbo.' + Name, N'U') IS NULL)
    THROW 51001, N'Thiếu bảng của schema main. Chạy Update-Database trước; bảng suất ăn phải là dbo.Comboes.', 1;

DECLARE @Now datetime = DATEADD(HOUR, 7, GETUTCDATE());
DECLARE @Today datetime = CAST(CAST(@Now AS date) AS datetime);

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @LockResult int;
    EXEC @LockResult = sys.sp_getapplock @Resource=N'YamyDemoSeed',
        @LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=15000;
    IF @LockResult < 0 THROW 51002, N'Không lấy được khóa khởi tạo dữ liệu mẫu. Vui lòng thử lại.', 1;

    -- 1. Tài khoản: không thay mật khẩu của tài khoản đã tồn tại.
    DECLARE @UserSeed TABLE (Email nvarchar(150), FullName nvarchar(100),
        Role nvarchar(30), Phone nvarchar(20), PasswordHash nvarchar(max));
    INSERT INTO @UserSeed VALUES
    (N'admin.yamy@example.test', N'Quản trị viên mẫu', N'Admin', N'0901000001', N'$PBKDF2$V1$50000$WGyOseWNC9SdFAYs+q/mEg==$jWOrqJHOPqruTe8EK44Ro2+VS8rPNQsY4EJT4hRTN0k='),
    (N'staff.yamy@example.test', N'Nhân viên mẫu', N'Staff', N'0901000002', N'$PBKDF2$V1$50000$n1XA3zvLfD+uFSe+/SHsXQ==$X9hLrU7djehNbhyPN41mBfvWSJX2zy9Ig+CSgRSGKB4='),
    (N'khach1.yamy@example.test', N'Nguyễn An', N'Customer', N'0901000003', N'$PBKDF2$V1$50000$Io+8XOKOrgrJAoG2Hixvaw==$YgGMvjhApP/VJ0ceRe4v7PvoZPMVudHt/nlb6hJVAn8='),
    (N'khach2.yamy@example.test', N'Trần Bình', N'Customer', N'0901000004', N'$PBKDF2$V1$50000$lVkKblsQprdEGk+lCy8CiQ==$TDUCbPpx20DwEudxc8QTA3bX+a2HOfJSnheqRN92c1o=');

    IF EXISTS (SELECT 1 FROM dbo.Users u JOIN @UserSeed s ON s.Email=u.Email WHERE u.Role<>s.Role)
        THROW 51003, N'Email mẫu đã có vai trò khác. Không thay đổi tài khoản hiện có.', 1;
    INSERT INTO dbo.Users (Email, FullName, Role, Phone, PasswordHash, Address, IsActive, CreatedAt)
    SELECT s.Email, s.FullName, s.Role, s.Phone, s.PasswordHash,
        N'123 Nguyễn Huệ, Quận 1, TP. Hồ Chí Minh', 1, @Now
    FROM @UserSeed s WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.Email=s.Email);

    -- 2. Danh mục và 30 món: giữ nguyên các bản ghi cùng tên đã có.
    DECLARE @CategorySeed TABLE (Name nvarchar(100), Description nvarchar(500), ImageUrl nvarchar(250));
    INSERT INTO @CategorySeed VALUES
    (N'Khai vị', N'Các món khai vị tươi ngon.', N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
    (N'Món chính', N'Món chính từ thịt bò, heo và gà.', N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
    (N'Cơm và phở', N'Các món cơm và phở truyền thống.', N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
    (N'Hải sản', N'Hải sản tươi ngon từ biển.', N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
    (N'Đồ uống', N'Nước giải khát và nước ép trái cây.', N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
    (N'Tráng miệng', N'Bánh ngọt và các món tráng miệng mát lạnh.', N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png');
    INSERT INTO dbo.Categories (Name, Description, ImageUrl, IsActive)
    SELECT s.Name, s.Description, s.ImageUrl, 1 FROM @CategorySeed s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories c WHERE c.Name=s.Name);

    DECLARE @DishSeed TABLE (Name nvarchar(150), Description nvarchar(1000),
        Price decimal(18,2), PreparationTime int, CategoryName nvarchar(100), ImageUrl nvarchar(500));
    INSERT INTO @DishSeed VALUES
    (N'Gỏi cuốn tôm', N'Gỏi cuốn tôm tươi kiểu Việt Nam.', 45000, 10, N'Khai vị', N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
    (N'Cánh gà chiên', N'Cánh gà giòn dùng cùng sốt tương ớt ngọt.', 65000, 15, N'Khai vị', N'https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg'),
    (N'Khoai tây chiên', N'Khoai tây chiên vàng giòn.', 35000, 10, N'Khai vị', N'https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg'),
    (N'Há cảo chiên', N'Há cảo nhân thịt heo đậm vị.', 50000, 12, N'Khai vị', N'https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp'),
    (N'Bánh mì bơ tỏi', N'Bánh mì nướng phủ bơ tỏi thơm lừng.', 40000, 8, N'Khai vị', N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg'),
    (N'Bò nướng sốt nấm', N'Thịt bò nướng mềm dùng cùng sốt nấm.', 189000, 25, N'Món chính', N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
    (N'Gà nướng thảo mộc', N'Gà nướng thơm hương thảo mộc.', 99000, 20, N'Món chính', N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
    (N'Cơm sườn nướng', N'Sườn heo nướng mọng nước dùng cùng cơm.', 119000, 22, N'Món chính', N'https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg'),
    (N'Bánh mì kẹp bò', N'Bánh mì kẹp thịt bò và phô mai.', 89000, 15, N'Món chính', N'https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg'),
    (N'Bánh mì kẹp gà', N'Bánh mì kẹp gà chiên giòn.', 79000, 15, N'Món chính', N'https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg'),
    (N'Cơm chiên trứng', N'Cơm chiên cùng trứng và rau củ.', 65000, 12, N'Cơm và phở', N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
    (N'Cơm chiên bò', N'Cơm chiên đặc biệt cùng thịt bò mềm.', 85000, 15, N'Cơm và phở', N'https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png'),
    (N'Cơm chiên gà', N'Cơm chiên thịt gà đậm đà.', 75000, 15, N'Cơm và phở', N'https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp'),
    (N'Phở bò', N'Phở bò truyền thống Việt Nam.', 79000, 15, N'Cơm và phở', N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg'),
    (N'Phở gà', N'Phở dùng cùng thịt gà luộc.', 69000, 15, N'Cơm và phở', N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg'),
    (N'Tôm nướng tỏi', N'Tôm tươi nướng cùng tỏi.', 149000, 20, N'Hải sản', N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
    (N'Mực chiên giòn', N'Khoanh mực chiên vàng giòn.', 129000, 18, N'Hải sản', N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg'),
    (N'Cá hồi nướng', N'Cá hồi nướng dùng cùng sốt chanh.', 199000, 25, N'Hải sản', N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg'),
    (N'Lẩu hải sản', N'Lẩu hải sản dùng cùng rau tươi.', 249000, 30, N'Hải sản', N'https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg'),
    (N'Tôm sốt bơ tỏi', N'Tôm áp chảo với sốt bơ tỏi.', 169000, 20, N'Hải sản', N'https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg'),
    (N'Nước ngọt Coca-Cola', N'Nước ngọt dùng lạnh.', 20000, 2, N'Đồ uống', N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
    (N'Nước cam ép', N'Nước cam tươi ép nguyên chất.', 35000, 5, N'Đồ uống', N'https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg'),
    (N'Trà chanh', N'Trà chanh đá thanh mát.', 30000, 5, N'Đồ uống', N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg'),
    (N'Cà phê sữa đá', N'Cà phê sữa đá kiểu Việt Nam.', 35000, 5, N'Đồ uống', N'https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png'),
    (N'Nước khoáng', N'Nước khoáng đóng chai.', 15000, 1, N'Đồ uống', N'https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg'),
    (N'Bánh phô mai', N'Bánh phô mai phủ quả mọng.', 55000, 5, N'Tráng miệng', N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
    (N'Bánh sô-cô-la', N'Bánh sô-cô-la mềm mịn.', 50000, 5, N'Tráng miệng', N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg'),
    (N'Kem vani', N'Kem vani thơm ngọt.', 40000, 3, N'Tráng miệng', N'https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg'),
    (N'Trái cây theo mùa', N'Trái cây tươi tuyển chọn theo mùa.', 45000, 5, N'Tráng miệng', N'https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png'),
    (N'Bánh tiramisu', N'Bánh cà phê tiramisu kiểu Ý.', 65000, 5, N'Tráng miệng', N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg');
    DECLARE @NewDishes TABLE (Id int, Name nvarchar(150));
    INSERT INTO dbo.Dishes (Name, Description, Price, PreparationTime, Status, CategoryId, CreatedAt)
    OUTPUT inserted.Id, inserted.Name INTO @NewDishes
    SELECT s.Name, s.Description, s.Price, s.PreparationTime, N'Available', c.Id, @Now
    FROM @DishSeed s
    CROSS APPLY (SELECT TOP (1) Id FROM dbo.Categories WHERE Name=s.CategoryName ORDER BY Id) c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Dishes d WHERE d.Name=s.Name);
    INSERT INTO dbo.DishImages (DishId, ImageUrl, IsPrimary)
    SELECT d.Id, s.ImageUrl, 1 FROM @NewDishes d JOIN @DishSeed s ON s.Name=d.Name;

    -- 3. Bàn mẫu riêng; không dùng hoặc sửa trạng thái bàn đang có của nhà hàng.
    DECLARE @TableSeed TABLE (TableNumber nvarchar(20), Capacity int, Zone nvarchar(50));
    INSERT INTO @TableSeed VALUES
    (N'YM01',2,N'Indoor'), (N'YM02',2,N'Indoor'), (N'YM03',4,N'Indoor'),
    (N'YM04',4,N'Indoor'), (N'YM05',4,N'Window'), (N'YM06',6,N'Window'),
    (N'YM07',6,N'Outdoor'), (N'YM08',8,N'Outdoor'), (N'YM09',8,N'VIP'), (N'YM10',10,N'VIP');
    INSERT INTO dbo.RestaurantTables (TableNumber, Capacity, Zone, Status, IsActive)
    SELECT s.TableNumber, s.Capacity, s.Zone, N'Available', 1 FROM @TableSeed s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.RestaurantTables t WHERE t.TableNumber=s.TableNumber);

    -- 4. Suất ăn: tên bảng chính xác là Comboes theo migration main.
    DECLARE @ComboSeed TABLE (Name nvarchar(150), Description nvarchar(500), Price decimal(18,2), ImageUrl nvarchar(500));
    INSERT INTO @ComboSeed VALUES
    (N'Suất gia đình (mẫu Yamy)',N'Gỏi cuốn, gà nướng, cơm chiên và nước ngọt.',299000,N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
    (N'Suất gà (mẫu Yamy)',N'Gà nướng, khoai tây chiên và nước ngọt.',139000,N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
    (N'Suất tráng miệng (mẫu Yamy)',N'Bánh phô mai, bánh sô-cô-la và nước cam.',99000,N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png');
    DECLARE @NewCombos TABLE (Id int, Name nvarchar(150));
    INSERT INTO dbo.Comboes (Name, Description, Price, ImageUrl, IsActive, StartDate, EndDate)
    OUTPUT inserted.Id, inserted.Name INTO @NewCombos
    SELECT s.Name, s.Description, s.Price, s.ImageUrl, 1, DATEADD(DAY,-1,@Now), DATEADD(MONTH,6,@Now)
    FROM @ComboSeed s WHERE NOT EXISTS (SELECT 1 FROM dbo.Comboes c WHERE c.Name=s.Name);
    DECLARE @ComboLineSeed TABLE (ComboName nvarchar(150), DishName nvarchar(150), Quantity int);
    INSERT INTO @ComboLineSeed VALUES
    (N'Suất gia đình (mẫu Yamy)',N'Gỏi cuốn tôm',1),
    (N'Suất gia đình (mẫu Yamy)',N'Gà nướng thảo mộc',2),
    (N'Suất gia đình (mẫu Yamy)',N'Cơm chiên trứng',2),
    (N'Suất gia đình (mẫu Yamy)',N'Nước ngọt Coca-Cola',2),
    (N'Suất gà (mẫu Yamy)',N'Gà nướng thảo mộc',1),
    (N'Suất gà (mẫu Yamy)',N'Khoai tây chiên',1),
    (N'Suất gà (mẫu Yamy)',N'Nước ngọt Coca-Cola',1),
    (N'Suất tráng miệng (mẫu Yamy)',N'Bánh phô mai',1),
    (N'Suất tráng miệng (mẫu Yamy)',N'Bánh sô-cô-la',1),
    (N'Suất tráng miệng (mẫu Yamy)',N'Nước cam ép',1);
    INSERT INTO dbo.ComboItems (ComboId, DishId, Quantity)
    SELECT c.Id, d.Id, s.Quantity FROM @ComboLineSeed s JOIN @NewCombos c ON c.Name=s.ComboName
    CROSS APPLY (SELECT TOP (1) Id FROM dbo.Dishes WHERE Name=s.DishName ORDER BY Id) d;

    -- 5. Mã khuyến mãi dùng loại Fixed Amount mà ứng dụng hỗ trợ.
    DECLARE @PromoSeed TABLE (Code nvarchar(50), Name nvarchar(150), DiscountType nvarchar(30),
        DiscountValue decimal(18,2), MinimumOrderAmount decimal(18,2), MaximumDiscount decimal(18,2));
    INSERT INTO @PromoSeed VALUES
    (N'YM10',N'Chào khách mới - giảm 10%',N'Percentage',10,100000,50000),
    (N'YM50K',N'Tiết kiệm 50.000 đồng',N'Fixed Amount',50000,300000,50000),
    (N'YM15',N'Ưu đãi cuối tuần - giảm 15%',N'Percentage',15,200000,70000),
    (N'YM20',N'Khách thân thiết - giảm 20%',N'Percentage',20,500000,150000),
    (N'YM30K',N'Ưu đãi 30.000 đồng',N'Fixed Amount',30000,150000,30000);
    DECLARE @NewPromotions TABLE (Id int, Code nvarchar(50));
    DECLARE @UseSamplePromo bit = CASE
        WHEN NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Note=N'[YAMY-DEMO-O01] Ít cay.')
         AND (SELECT TOP (1) Price*2 FROM dbo.Dishes WHERE Name=N'Cơm chiên bò' ORDER BY Id)>=100000
        THEN 1 ELSE 0 END;
    INSERT INTO dbo.Promotions (Code, Name, Description, DiscountType, DiscountValue,
        MinimumOrderAmount, MaximumDiscount, StartDate, EndDate, UsageLimit, UsedCount, IsActive)
    OUTPUT inserted.Id, inserted.Code INTO @NewPromotions
    SELECT s.Code, s.Name, N'Mã khuyến mãi minh họa Yamy.', s.DiscountType, s.DiscountValue,
        s.MinimumOrderAmount, s.MaximumDiscount, DATEADD(DAY,-1,@Now), DATEADD(MONTH,6,@Now), 100,
        CASE WHEN s.Code=N'YM10' AND @UseSamplePromo=1 THEN 1 ELSE 0 END, 1
    FROM @PromoSeed s WHERE NOT EXISTS (SELECT 1 FROM dbo.Promotions p WHERE p.Code=s.Code);

    -- 6. Lịch đặt bàn: mã nhận diện nằm trong yêu cầu đặc biệt.
    DECLARE @Customer1 int = (SELECT TOP (1) Id FROM dbo.Users WHERE Email=N'khach1.yamy@example.test' ORDER BY Id);
    DECLARE @Customer2 int = (SELECT TOP (1) Id FROM dbo.Users WHERE Email=N'khach2.yamy@example.test' ORDER BY Id);
    DECLARE @ReservationSeed TABLE (Marker nvarchar(100), UserId int, TableNumber nvarchar(20),
        ReservationDate datetime, PartySize int, Status nvarchar(30));
    INSERT INTO @ReservationSeed VALUES
    (N'[YAMY-DEMO-R01] Ưu tiên chỗ yên tĩnh.',@Customer1,N'YM01',DATEADD(HOUR,18,DATEADD(DAY,1,@Today)),2,N'Pending'),
    (N'[YAMY-DEMO-R02] Gia đình dùng bữa tối.',@Customer2,N'YM03',DATEADD(HOUR,18,DATEADD(DAY,1,@Today)),4,N'Confirmed'),
    (N'[YAMY-DEMO-R03] Buổi hẹn đã phục vụ.',@Customer1,N'YM05',DATEADD(HOUR,12,DATEADD(DAY,-1,@Today)),2,N'Completed'),
    (N'[YAMY-DEMO-R04] Khách đã hủy lịch.',@Customer2,N'YM07',DATEADD(HOUR,18,DATEADD(DAY,2,@Today)),4,N'Cancelled');
    INSERT INTO dbo.Reservations (UserId, TableId, ReservationDate, PartySize, SpecialRequest, Status, CreatedAt)
    SELECT s.UserId, t.Id, s.ReservationDate, s.PartySize, s.Marker, s.Status, DATEADD(DAY,-2,@Now)
    FROM @ReservationSeed s
    CROSS APPLY (SELECT TOP (1) Id FROM dbo.RestaurantTables WHERE TableNumber=s.TableNumber ORDER BY Id) t
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Reservations r WHERE r.SpecialRequest=s.Marker);

    -- 7. Đơn mẫu ở các trạng thái, giá lấy từ món/suất ăn thực tế.
    -- Tất cả bản ghi con chỉ được thêm lúc tạo đơn, không sửa đơn đã tồn tại.
    DECLARE @OrderSeed TABLE (Marker nvarchar(100), UserId int, OrderType nvarchar(30),
        Status nvarchar(30), CreatedAt datetime, DishName nvarchar(150), Quantity int, ComboName nvarchar(150),
        ReservationMarker nvarchar(100), PaymentStatus nvarchar(30));
    INSERT INTO @OrderSeed VALUES
    (N'[YAMY-DEMO-O01] Ít cay.',@Customer1,N'Takeaway',N'Pending',DATEADD(MINUTE,-30,@Now),N'Cơm chiên bò',2,NULL,NULL,N'Pending'),
    (N'[YAMY-DEMO-O02] Gọi trước khi giao.',@Customer2,N'Delivery',N'Confirmed',DATEADD(MINUTE,-25,@Now),N'Cơm sườn nướng',2,NULL,NULL,N'Pending'),
    (N'[YAMY-DEMO-O03] Không thêm hành.',@Customer1,N'Takeaway',N'Preparing',DATEADD(MINUTE,-20,@Now),N'Phở bò',2,NULL,NULL,N'Pending'),
    (N'[YAMY-DEMO-O04] Khách sắp đến lấy.',@Customer2,N'Takeaway',N'Ready',DATEADD(MINUTE,-15,@Now),N'Gà nướng thảo mộc',1,NULL,NULL,N'Completed'),
    (N'[YAMY-DEMO-O05] Đơn tại bàn đã hoàn tất.',@Customer1,N'Dine-in',N'Delivered',DATEADD(HOUR,12,DATEADD(DAY,-1,@Today)),N'Bò nướng sốt nấm',2,NULL,N'[YAMY-DEMO-R03] Buổi hẹn đã phục vụ.',N'Completed'),
    (N'[YAMY-DEMO-O06] Khách hủy trước khi chế biến.',@Customer2,N'Delivery',N'Cancelled',DATEADD(HOUR,12,DATEADD(DAY,-2,@Today)),N'Phở gà',1,NULL,NULL,N'Failed'),
    (N'[YAMY-DEMO-O07] Đơn suất ăn đã hoàn tất.',@Customer2,N'Takeaway',N'Delivered',DATEADD(HOUR,12,DATEADD(DAY,-3,@Today)),N'Bánh phô mai',1,N'Suất gà (mẫu Yamy)',NULL,N'Completed'),
    (N'[YAMY-DEMO-O08] Đã hoàn tiền và hủy trước chế biến.',@Customer1,N'Takeaway',N'Cancelled',DATEADD(HOUR,12,DATEADD(DAY,-4,@Today)),N'Gỏi cuốn tôm',2,NULL,NULL,N'Refunded');

    DECLARE @Marker nvarchar(100), @UserId int, @OrderType nvarchar(30), @Status nvarchar(30),
        @CreatedAt datetime, @DishName nvarchar(150), @Quantity int, @ComboName nvarchar(150),
        @ReservationMarker nvarchar(100), @PaymentStatus nvarchar(30);
    DECLARE order_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT Marker, UserId, OrderType, Status, CreatedAt, DishName, Quantity, ComboName, ReservationMarker, PaymentStatus
        FROM @OrderSeed ORDER BY Marker;
    OPEN order_cursor;
    FETCH NEXT FROM order_cursor INTO @Marker,@UserId,@OrderType,@Status,@CreatedAt,@DishName,@Quantity,@ComboName,@ReservationMarker,@PaymentStatus;
    WHILE @@FETCH_STATUS=0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM dbo.Orders WHERE Note=@Marker)
        BEGIN
            DECLARE @DishId int, @DishPrice decimal(18,2), @ComboId int, @ComboPrice decimal(18,2),
                @ReservationId int, @TableId int, @SubTotal decimal(18,2), @OrderId int,
                @PromoId int, @Discount decimal(18,2);
            SELECT @DishId=NULL,@DishPrice=NULL,@ComboId=NULL,@ComboPrice=0,@ReservationId=NULL,@TableId=NULL,@PromoId=NULL,@Discount=0;
            SELECT TOP (1) @DishId=Id,@DishPrice=Price FROM dbo.Dishes WHERE Name=@DishName ORDER BY Id;
            IF @ComboName IS NOT NULL
                SELECT TOP (1) @ComboId=Id,@ComboPrice=Price FROM dbo.Comboes WHERE Name=@ComboName ORDER BY Id;
            IF @ReservationMarker IS NOT NULL
                SELECT TOP (1) @ReservationId=Id,@TableId=TableId FROM dbo.Reservations WHERE SpecialRequest=@ReservationMarker ORDER BY Id;
            IF @DishId IS NULL OR (@ComboName IS NOT NULL AND @ComboId IS NULL)
                THROW 51004, N'Không tìm thấy món hoặc suất ăn mẫu.', 1;
            SET @SubTotal=@DishPrice*@Quantity+@ComboPrice;
            -- Chỉ dùng mã YM10 vừa tạo; không đổi UsedCount của mã đã tồn tại.
            IF @Marker=N'[YAMY-DEMO-O01] Ít cay.' AND @SubTotal>=100000
            BEGIN
                SELECT @PromoId=Id FROM @NewPromotions WHERE Code=N'YM10';
                IF @PromoId IS NOT NULL
                    SET @Discount=CASE WHEN ROUND(@SubTotal*0.10,0)>50000 THEN 50000 ELSE ROUND(@SubTotal*0.10,0) END;
            END;
            INSERT INTO dbo.Orders (UserId, ReservationId, TableId, OrderType, Status, DeliveryAddress,
                Phone, SubTotal, DiscountAmount, DeliveryFee, TotalAmount, Note, CreatedAt)
            SELECT @UserId,@ReservationId,@TableId,@OrderType,@Status,
                CASE WHEN @OrderType=N'Delivery' THEN Address ELSE NULL END,
                Phone,@SubTotal,@Discount,0,@SubTotal-@Discount,@Marker,@CreatedAt FROM dbo.Users WHERE Id=@UserId;
            SET @OrderId=CAST(SCOPE_IDENTITY() AS int);
            INSERT INTO dbo.OrderItems (OrderId,DishId,ComboId,Quantity,UnitPrice,Note,SubTotal)
            VALUES (@OrderId,@DishId,NULL,@Quantity,@DishPrice,N'Món minh họa.',@DishPrice*@Quantity);
            IF @ComboId IS NOT NULL
                INSERT INTO dbo.OrderItems (OrderId,DishId,ComboId,Quantity,UnitPrice,Note,SubTotal)
                VALUES (@OrderId,NULL,@ComboId,1,@ComboPrice,N'Suất ăn minh họa.',@ComboPrice);
            IF @PromoId IS NOT NULL
                INSERT INTO dbo.OrderPromotions (OrderId,PromotionId,DiscountAmount)
                VALUES (@OrderId,@PromoId,@Discount);
            INSERT INTO dbo.Payments (OrderId,PaymentMethod,Amount,Status,PaymentDate,TransactionCode)
            VALUES (@OrderId,N'Cash',@SubTotal-@Discount,@PaymentStatus,@CreatedAt,
                N'YAMY-DEMO-'+CAST(@OrderId AS nvarchar(20)));
            -- Không tạo đánh giá cho đơn chưa hoàn tất.
            IF @Status=N'Delivered'
                INSERT INTO dbo.Reviews (UserId,DishId,OrderId,Rating,Comment,CreatedAt,IsApproved)
                VALUES (@UserId,@DishId,@OrderId,5,N'Món ngon, phục vụ chu đáo. Đây là đánh giá minh họa.',DATEADD(HOUR,2,@CreatedAt),
                    CASE WHEN @UserId=@Customer1 THEN 1 ELSE 0 END);
        END;
        FETCH NEXT FROM order_cursor INTO @Marker,@UserId,@OrderType,@Status,@CreatedAt,@DishName,@Quantity,@ComboName,@ReservationMarker,@PaymentStatus;
    END;
    CLOSE order_cursor;
    DEALLOCATE order_cursor;

    COMMIT TRANSACTION;
    PRINT N'Đã thêm dữ liệu mẫu Yamy. Các bản ghi đã tồn tại được giữ nguyên.';
    SELECT Email,FullName,Role,IsActive FROM dbo.Users WHERE Email IN (SELECT Email FROM @UserSeed) ORDER BY Role,Email;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
/* CẬP NHẬT URL ẢNH MÓN / SUẤT ĂN — giữ nguyên schema main.
   Chọn đúng database trong SSMS rồi Execute. Không chạy Add-Migration.
   Chỉ sửa URL ảnh; không sửa tên, giá, tài khoản, đơn hoặc dữ liệu thanh toán.
   URL mới được chọn theo đúng tên món; nguồn ghi trong food-image-catalog.json.
   Bạn có thể sửa cột ImageUrl trong @DishImages / @ComboImages thành URL HTTPS
   ảnh thật bạn muốn. Để thay cả ảnh HTTPS đang có, đổi cờ dưới đây thành 1.
   Mặc định 0: giữ ảnh HTTPS tùy chỉnh, thay ảnh trống, ảnh mẫu cục bộ và URL ảnh main cũ.
   Cần kết nối internet từ trình duyệt để tải ảnh; khi lỗi web dùng SVG nhẹ.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() IN (N'master',N'model',N'msdb',N'tempdb')
    THROW 51010,N'Chọn đúng database của RestaurantWebsite trước khi chạy.',1;
IF OBJECT_ID(N'dbo.Dishes',N'U') IS NULL OR OBJECT_ID(N'dbo.DishImages',N'U') IS NULL
    OR OBJECT_ID(N'dbo.Comboes',N'U') IS NULL OR OBJECT_ID(N'dbo.Categories',N'U') IS NULL
    THROW 51011,N'Thiếu bảng schema main. Bảng suất ăn phải là dbo.Comboes.',1;
DECLARE @ReplaceExistingExternal bit = 0;
DECLARE @Root nvarchar(250) = N'https://raw.githubusercontent.com/chipcao1/RestaurantWebsite/2447b85b446d447c5bd7c051271d5dcccce7b7f9/RestaurantWebsite/Resources/User/images/';
DECLARE @Legacy TABLE (LocalUrl nvarchar(250) PRIMARY KEY,RemoteUrl nvarchar(500));
INSERT INTO @Legacy VALUES
(N'/Resources/User/images/combo01.jpg',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'/Resources/User/images/combo03.jpg',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'/Resources/User/images/dish01.jpg',N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
(N'/Resources/User/images/dish02.jpg',N'https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg'),
(N'/Resources/User/images/dish03.jpg',N'https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg'),
(N'/Resources/User/images/dish04.jpg',N'https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp'),
(N'/Resources/User/images/dish05.jpg',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg'),
(N'/Resources/User/images/dish06.jpg',N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
(N'/Resources/User/images/dish07.jpg',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'/Resources/User/images/dish08.jpg',N'https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg'),
(N'/Resources/User/images/dish09.jpg',N'https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg'),
(N'/Resources/User/images/dish10.jpg',N'https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg'),
(N'/Resources/User/images/dish11.jpg',N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
(N'/Resources/User/images/dish12.jpg',N'https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png'),
(N'/Resources/User/images/dish13.jpg',N'https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp'),
(N'/Resources/User/images/dish14.jpg',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg'),
(N'/Resources/User/images/dish15.jpg',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg'),
(N'/Resources/User/images/dish16.jpg',N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
(N'/Resources/User/images/dish17.jpg',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg'),
(N'/Resources/User/images/dish18.jpg',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg'),
(N'/Resources/User/images/dish19.jpg',N'https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg'),
(N'/Resources/User/images/dish20.jpg',N'https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg'),
(N'/Resources/User/images/dish21.jpg',N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
(N'/Resources/User/images/dish22.jpg',N'https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg'),
(N'/Resources/User/images/dish23.jpg',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg'),
(N'/Resources/User/images/dish24.jpg',N'https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png'),
(N'/Resources/User/images/dish25.jpg',N'https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg'),
(N'/Resources/User/images/dish26.jpg',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'/Resources/User/images/dish27.jpg',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg'),
(N'/Resources/User/images/dish28.jpg',N'https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg'),
(N'/Resources/User/images/dish29.jpg',N'https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png'),
(N'/Resources/User/images/dish30.jpg',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg'),
(N'/Resources/User/images/f1.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f2.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f3.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f4.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f5.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f6.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f7.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f8.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/f9.png',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/o1.jpg',N'/Content/images/food-placeholder.svg'),
(N'/Resources/User/images/o2.jpg',N'/Content/images/food-placeholder.svg'),
(N'/Content/images/categories/appetizers.jpg',N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
(N'/Content/images/categories/main-dishes.jpg',N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
(N'/Content/images/categories/rice-noodles.jpg',N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
(N'/Content/images/categories/seafood.jpg',N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
(N'/Content/images/categories/drinks.jpg',N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
(N'/Content/images/categories/desserts.jpg',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png');
DECLARE @DishImages TABLE (Name nvarchar(150) PRIMARY KEY,ImageUrl nvarchar(500));
INSERT INTO @DishImages VALUES
(N'Gỏi cuốn tôm',N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
(N'Spring Rolls',N'https://raw.githubusercontent.com/jeamarie/odin-recipes/eef6c2e98df6b54b2fe71ccecc0ef0559e556876/recipes/images/spring-roll.png'),
(N'Cánh gà chiên',N'https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg'),
(N'Fried Chicken Wings',N'https://raw.githubusercontent.com/achohan1/odin-recipes/4e17a42a03203e35a5c0f767b338cdf9c5b51eec/images/chickenwings.jpg'),
(N'Khoai tây chiên',N'https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg'),
(N'French Fries',N'https://raw.githubusercontent.com/DarieldonMedeiros/odin-recipes/74a5597fc45c3ab1491457800251d1f82447af02/images/fries.jpg'),
(N'Há cảo chiên',N'https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp'),
(N'Fried Dumplings',N'https://raw.githubusercontent.com/YunxiangDeng/odin-recipes/a341bd2adc2b648eedcf0ea47d8b39ce2caa7aad/recipes/imgs/pork-gyoza.webp'),
(N'Bánh mì bơ tỏi',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg'),
(N'Garlic Bread',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/garlic_bread-1275456.jpg'),
(N'Bò nướng sốt nấm',N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
(N'Grilled Beef Steak',N'https://raw.githubusercontent.com/bragiarya/mtm_pos/61700be8ece5fd89c8ba80be995a13ed7dc31d2d/assets/img/MENTAHAN/steaks-with-tarragon-mushrooms-sq-024.jpg'),
(N'Gà nướng thảo mộc',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Grilled Chicken',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Cơm sườn nướng',N'https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg'),
(N'Pork Chop',N'https://raw.githubusercontent.com/Harly-1506/4VNfoods-Deep-learning/0c58b48ff644d1854703eae16b50f7fba7a567c6/datasets/dataset/Test/Com%20tam/108.jpg'),
(N'Bánh mì kẹp bò',N'https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg'),
(N'Beef Burger',N'https://raw.githubusercontent.com/its-krishnendu-dev/food-recipe-website/11c200e5885779c0a660e6915f5a8046e4b5bbbf/ClassicCheeseBurger_9.jpg'),
(N'Bánh mì kẹp gà',N'https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg'),
(N'Chicken Burger',N'https://raw.githubusercontent.com/stefanthoss/chickenonabun.com/a343420da1f3b55b544fa317815f2a4ab5bae942/assets/images/original/gotts-roadside.jpg'),
(N'Cơm chiên trứng',N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
(N'Fried Rice',N'https://raw.githubusercontent.com/jszecz/odin-recipes/439fe2da83cfc56b75d89e79a135627e6b00f5bb/images/fried-rice.jpg'),
(N'Cơm chiên bò',N'https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png'),
(N'Beef Fried Rice',N'https://raw.githubusercontent.com/oh3vci/TIA/4d556919c3130ac4ffabd3916822b51b0f54c326/images/0722_dinner.png'),
(N'Cơm chiên gà',N'https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp'),
(N'Chicken Fried Rice',N'https://raw.githubusercontent.com/Kevin272-dot/odin-recipes/a64019be1aa53f609297fb1584db448b59120a71/images/chicken_fried_rice.webp'),
(N'Phở bò',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg'),
(N'Beef Noodles',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-bo.jpg'),
(N'Phở gà',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg'),
(N'Chicken Noodles',N'https://raw.githubusercontent.com/kinoss123/Food-Ordering/5f543aaf7002f147a79a5531c4ad79a553a1bc53/client/assets/food/pho-ga.jpg'),
(N'Tôm nướng tỏi',N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
(N'Grilled Shrimp',N'https://raw.githubusercontent.com/lemon7h3chick/odin-recipes/16b4ee306afc28f1af4be9dfdb9de4a674c8d782/images/spicy-grilled-shrimp.jpg'),
(N'Mực chiên giòn',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg'),
(N'Fried Calamari',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/fried_calamari-168241.jpg'),
(N'Cá hồi nướng',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg'),
(N'Grilled Salmon',N'https://raw.githubusercontent.com/vionwinnie/food101samples/470eb1bad85aa1d57e4ba5c2a14eb53b38eb4534/food101-sample/grilled_salmon-3673579.jpg'),
(N'Lẩu hải sản',N'https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg'),
(N'Seafood Hotpot',N'https://raw.githubusercontent.com/quang-anh26/food-ordering-website/78d9584ec63cc4885a8ca8325702095c10eb430b/images/foods/f41-lau-thai-hai-san.jpg'),
(N'Tôm sốt bơ tỏi',N'https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg'),
(N'Butter Garlic Prawns',N'https://raw.githubusercontent.com/getprpd-sudo/getprpd-website-repo/e9ab215cc3dbb8f3821ddce32d4d611719e9376f/assets/images/garlic-butter-shrimp.jpg'),
(N'Nước ngọt Coca-Cola',N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
(N'Coca Cola',N'https://raw.githubusercontent.com/Vinayak-E/Responsive-coca-cola-website/691f86ab839c04fc3ce80921c2026e4dc6383ed6/width1960%20%281%29.webp'),
(N'Nước cam ép',N'https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg'),
(N'Orange Juice',N'https://raw.githubusercontent.com/SowjanyaAnanthasetty/Juice-Menu-Frontened/78398d48290c83347c8994204348b6c5478f502c/product%20app/Images/orange.jpg'),
(N'Trà chanh',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg'),
(N'Lemon Tea',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/lemon-Iced-Tea.jpg'),
(N'Cà phê sữa đá',N'https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png'),
(N'Milk Coffee',N'https://raw.githubusercontent.com/babobi161504/Highlands-Coffee/c58c2f5872a78eb1c5d4dbcf1e6a310389177c7d/image/menu-PSD_3.png'),
(N'Nước khoáng',N'https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg'),
(N'Mineral Water',N'https://raw.githubusercontent.com/JonasMATHIEU/Landing-page-Evian/f6ca778eac2ea0e1de129c10a79efcf1a066ca1e/image/eviantrr.jpg'),
(N'Bánh phô mai',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'Cheesecake',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'Bánh sô-cô-la',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg'),
(N'Chocolate Cake',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/choco_cake.jpg'),
(N'Kem vani',N'https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg'),
(N'Ice Cream',N'https://raw.githubusercontent.com/Daniel-Sha14/Assignment-1-Ice-Cream_Shop/40c08c516138b869f497c71aa203c25d0b782f89/vanilla.jpeg'),
(N'Trái cây theo mùa',N'https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png'),
(N'Fruit Salad',N'https://raw.githubusercontent.com/matinsoleymni/24Salad-Landing-Page/b16e00656f1a929d413d8cee34023afa1139b193/images/main/eat1.png'),
(N'Bánh tiramisu',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg'),
(N'Tiramisu',N'https://raw.githubusercontent.com/Shristi21/Dessert-Recipe-Website/df472c2568278210153e6c6f73d323d3181d8ebc/images/Tiramisu.jpeg');
DECLARE @ComboImages TABLE (Name nvarchar(150) PRIMARY KEY,ImageUrl nvarchar(500));
INSERT INTO @ComboImages VALUES
(N'Suất ăn gia đình',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Family Combo',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Suất gia đình (mẫu Yamy)',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Suất gà và nước uống',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Chicken Meal Combo',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Suất gà (mẫu Yamy)',N'https://raw.githubusercontent.com/daddydeus/odin-recipes-redo/308598099f158d8deecf07c05b821f232a66fc14/images/nandos-butterfly-chicken-hero1.jpg'),
(N'Suất tráng miệng',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'Dessert Combo',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png'),
(N'Suất tráng miệng (mẫu Yamy)',N'https://raw.githubusercontent.com/EufranioDiogo/cheesecake-recipe/43a27ed0b1c8a0d48f40307cde9296f3dfaa57f9/IMG/photo1.png');
IF EXISTS (SELECT 1 FROM @DishImages WHERE ImageUrl NOT LIKE N'https://%' OR LEN(ImageUrl)>500)
    OR EXISTS (SELECT 1 FROM @ComboImages WHERE ImageUrl NOT LIKE N'https://%' OR LEN(ImageUrl)>500)
    THROW 51012,N'Ảnh mới cần URL HTTPS trực tiếp, tối đa 500 ký tự.',1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @LockResult int;
    EXEC @LockResult=sys.sp_getapplock @Resource=N'YamyFoodImageUrls',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=15000;
    IF @LockResult<0 THROW 51013,N'Không lấy được khóa cập nhật URL ảnh, vui lòng thử lại.',1;

    -- Ảnh đại diện theo tên món (tên Việt và tên gốc main đều được hỗ trợ).
    UPDATE i SET ImageUrl=m.ImageUrl
    FROM dbo.DishImages i JOIN dbo.Dishes d ON d.Id=i.DishId JOIN @DishImages m ON m.Name=d.Name
    WHERE ( @ReplaceExistingExternal=1 OR NULLIF(LTRIM(RTRIM(i.ImageUrl)),N'') IS NULL
        OR i.ImageUrl LIKE @Root+N'%' OR EXISTS (SELECT 1 FROM @Legacy l WHERE l.LocalUrl=REPLACE(i.ImageUrl,N'~/',N'/')) );

    -- Món chưa có ảnh: thêm một ảnh đại diện, không tạo trùng khi chạy lại.
    INSERT INTO dbo.DishImages (DishId,ImageUrl,IsPrimary)
    SELECT d.Id,m.ImageUrl,1 FROM dbo.Dishes d JOIN @DishImages m ON m.Name=d.Name
    WHERE NOT EXISTS (SELECT 1 FROM dbo.DishImages i WHERE i.DishId=d.Id);

    -- Đường dẫn mẫu còn lại được đổi sang HTTPS. Các món nhận diện được đã sửa theo tên ở trên.
    UPDATE i SET ImageUrl=l.RemoteUrl FROM dbo.DishImages i JOIN @Legacy l ON l.LocalUrl=REPLACE(i.ImageUrl,N'~/',N'/') OR @Root+REPLACE(l.LocalUrl,N'/Resources/User/images/',N'')=i.ImageUrl;

    UPDATE c SET ImageUrl=m.ImageUrl FROM dbo.Comboes c JOIN @ComboImages m ON m.Name=c.Name
    WHERE @ReplaceExistingExternal=1 OR NULLIF(LTRIM(RTRIM(c.ImageUrl)),N'') IS NULL
        OR c.ImageUrl LIKE @Root+N'%' OR EXISTS (SELECT 1 FROM @Legacy l WHERE l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/'));
    UPDATE c SET ImageUrl=l.RemoteUrl FROM dbo.Comboes c JOIN @Legacy l ON l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/') OR @Root+REPLACE(l.LocalUrl,N'/Resources/User/images/',N'')=c.ImageUrl;

    -- Một số dữ liệu mẫu dùng chung ảnh món cho danh mục: chỉ đổi URL mẫu này.
    UPDATE c SET ImageUrl=l.RemoteUrl FROM dbo.Categories c JOIN @Legacy l ON l.LocalUrl=REPLACE(c.ImageUrl,N'~/',N'/') OR @Root+REPLACE(l.LocalUrl,N'/Resources/User/images/',N'')=c.ImageUrl;
    COMMIT TRANSACTION;
    PRINT N'Đã cập nhật URL ảnh. Giá, tài khoản, đơn và schema được giữ nguyên.';
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
