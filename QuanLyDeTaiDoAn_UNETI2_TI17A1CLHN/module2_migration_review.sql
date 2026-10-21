IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
ALTER TABLE [DeTais] DROP CONSTRAINT [FK_DeTais_GiangViens_GiangVienMaGV];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GiangViens]') AND [c].[name] = N'LinhVuc');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [GiangViens] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [GiangViens] DROP COLUMN [LinhVuc];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GiangViens]') AND [c].[name] = N'NgayVaoTruong');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [GiangViens] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [GiangViens] DROP COLUMN [NgayVaoTruong];

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeTais]') AND [c].[name] = N'LinhVuc');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [DeTais] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [DeTais] DROP COLUMN [LinhVuc];

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeTais]') AND [c].[name] = N'MaGV');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [DeTais] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [DeTais] DROP COLUMN [MaGV];

EXEC sp_rename N'[GiangViens].[SoSinhVienToiDa]', N'SoSinhVienHuongDanToiDa', 'COLUMN';

EXEC sp_rename N'[GiangViens].[MaGV]', N'MaGiangVien', 'COLUMN';

EXEC sp_rename N'[DeTais].[SoSinhVienDaDangKy]', N'MaLinhVuc', 'COLUMN';

EXEC sp_rename N'[DeTais].[GiangVienMaGV]', N'LinhVucMaLinhVuc', 'COLUMN';

EXEC sp_rename N'[DeTais].[IX_DeTais_GiangVienMaGV]', N'IX_DeTais_LinhVucMaLinhVuc', 'INDEX';

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GiangViens]') AND [c].[name] = N'SoDienThoai');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [GiangViens] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [GiangViens] ALTER COLUMN [SoDienThoai] nvarchar(20) NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GiangViens]') AND [c].[name] = N'HocVi');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [GiangViens] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [GiangViens] ALTER COLUMN [HocVi] nvarchar(50) NOT NULL;

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GiangViens]') AND [c].[name] = N'HoTen');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [GiangViens] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [GiangViens] ALTER COLUMN [HoTen] nvarchar(100) NOT NULL;

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GiangViens]') AND [c].[name] = N'Email');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [GiangViens] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [GiangViens] ALTER COLUMN [Email] nvarchar(100) NOT NULL;

ALTER TABLE [GiangViens] ADD [LinhVucMaLinhVuc] int NULL;

ALTER TABLE [GiangViens] ADD [MaLinhVuc] int NOT NULL DEFAULT 0;

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeTais]') AND [c].[name] = N'TenDeTai');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [DeTais] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [DeTais] ALTER COLUMN [TenDeTai] nvarchar(200) NOT NULL;

DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeTais]') AND [c].[name] = N'NamHoc');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [DeTais] DROP CONSTRAINT ' + @var9 + ';');
ALTER TABLE [DeTais] ALTER COLUMN [NamHoc] nvarchar(20) NOT NULL;

CREATE TABLE [LinhVucs] (
    [MaLinhVuc] int NOT NULL IDENTITY,
    [TenLinhVuc] nvarchar(100) NOT NULL,
    [MoTa] nvarchar(max) NULL,
    [TrangThai] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_LinhVucs] PRIMARY KEY ([MaLinhVuc])
);

CREATE INDEX [IX_GiangViens_LinhVucMaLinhVuc] ON [GiangViens] ([LinhVucMaLinhVuc]);

ALTER TABLE [DeTais] ADD CONSTRAINT [FK_DeTais_LinhVucs_LinhVucMaLinhVuc] FOREIGN KEY ([LinhVucMaLinhVuc]) REFERENCES [LinhVucs] ([MaLinhVuc]);

ALTER TABLE [GiangViens] ADD CONSTRAINT [FK_GiangViens_LinhVucs_LinhVucMaLinhVuc] FOREIGN KEY ([LinhVucMaLinhVuc]) REFERENCES [LinhVucs] ([MaLinhVuc]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261006124330_UpdateModule2Models', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [DeTais] DROP CONSTRAINT [FK_DeTais_LinhVucs_LinhVucMaLinhVuc];

ALTER TABLE [GiangViens] DROP CONSTRAINT [FK_GiangViens_LinhVucs_LinhVucMaLinhVuc];

DROP INDEX [IX_GiangViens_LinhVucMaLinhVuc] ON [GiangViens];

DROP INDEX [IX_DeTais_LinhVucMaLinhVuc] ON [DeTais];

DECLARE @var10 nvarchar(max);
SELECT @var10 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GiangViens]') AND [c].[name] = N'LinhVucMaLinhVuc');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [GiangViens] DROP CONSTRAINT ' + @var10 + ';');
ALTER TABLE [GiangViens] DROP COLUMN [LinhVucMaLinhVuc];

DECLARE @var11 nvarchar(max);
SELECT @var11 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DeTais]') AND [c].[name] = N'LinhVucMaLinhVuc');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [DeTais] DROP CONSTRAINT ' + @var11 + ';');
ALTER TABLE [DeTais] DROP COLUMN [LinhVucMaLinhVuc];

CREATE TABLE [DangKyDeTaiModule2s] (
    [MaDangKy] int NOT NULL IDENTITY,
    [MaDeTai] int NOT NULL,
    [MaSinhVien] int NOT NULL,
    [TrangThai] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_DangKyDeTaiModule2s] PRIMARY KEY ([MaDangKy]),
    CONSTRAINT [FK_DangKyDeTaiModule2s_DeTais_MaDeTai] FOREIGN KEY ([MaDeTai]) REFERENCES [DeTais] ([MaDeTai]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_GiangViens_Email] ON [GiangViens] ([Email]);

CREATE INDEX [IX_GiangViens_MaLinhVuc] ON [GiangViens] ([MaLinhVuc]);

CREATE INDEX [IX_DeTais_MaLinhVuc] ON [DeTais] ([MaLinhVuc]);

CREATE INDEX [IX_DangKyDeTaiModule2s_MaDeTai] ON [DangKyDeTaiModule2s] ([MaDeTai]);

ALTER TABLE [DeTais] ADD CONSTRAINT [FK_DeTais_LinhVucs_MaLinhVuc] FOREIGN KEY ([MaLinhVuc]) REFERENCES [LinhVucs] ([MaLinhVuc]) ON DELETE NO ACTION;

ALTER TABLE [GiangViens] ADD CONSTRAINT [FK_GiangViens_LinhVucs_MaLinhVuc] FOREIGN KEY ([MaLinhVuc]) REFERENCES [LinhVucs] ([MaLinhVuc]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261021070617_AddDangKyDeTaiModule2', N'10.0.12');

COMMIT;
GO

