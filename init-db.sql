-- Script para crear las tablas de Identity en SQL Server

-- Tabla AspNetRoles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(256) NULL,
        [NormalizedName] NVARCHAR(256) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        [Descripcion] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END

-- Tabla AspNetUsers
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id] NVARCHAR(450) NOT NULL,
        [UserName] NVARCHAR(256) NULL,
        [NormalizedUserName] NVARCHAR(256) NULL,
        [Email] NVARCHAR(256) NULL,
        [NormalizedEmail] NVARCHAR(256) NULL,
        [EmailConfirmed] BIT NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NULL,
        [SecurityStamp] NVARCHAR(MAX) NULL,
        [ConcurrencyStamp] NVARCHAR(MAX) NULL,
        [PhoneNumber] NVARCHAR(MAX) NULL,
        [PhoneNumberConfirmed] BIT NOT NULL,
        [TwoFactorEnabled] BIT NOT NULL,
        [LockoutEnd] DATETIMEOFFSET NULL,
        [LockoutEnabled] BIT NOT NULL,
        [AccessFailedCount] INT NOT NULL,
        [Nombre] NVARCHAR(100) NULL,
        [Apellido] NVARCHAR(100) NULL,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UltimoAcceso] DATETIME2 NULL,
        [Activo] BIT NOT NULL DEFAULT 1,
        [RefreshToken] NVARCHAR(MAX) NULL,
        [RefreshTokenExpiryTime] DATETIME2 NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
    CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
END

-- Tabla AspNetRoleClaims
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
END

-- Tabla AspNetUserClaims
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        [ClaimType] NVARCHAR(MAX) NULL,
        [ClaimValue] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
END

-- Tabla AspNetUserLogins
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [ProviderKey] NVARCHAR(450) NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX) NULL,
        [UserId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
END

-- Tabla AspNetUserRoles
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
END

-- Tabla AspNetUserTokens
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUserTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId] NVARCHAR(450) NOT NULL,
        [LoginProvider] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(450) NOT NULL,
        [Value] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END

-- Tabla Ubicaciones
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Ubicaciones]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Ubicaciones] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Nombre] NVARCHAR(200) NOT NULL,
        [Descripcion] NVARCHAR(500) NULL,
        [Direccion] NVARCHAR(500) NULL,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UsuarioCreacionId] NVARCHAR(450) NULL,
        [Activo] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Ubicaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ubicaciones_AspNetUsers_UsuarioCreacionId] FOREIGN KEY ([UsuarioCreacionId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE RESTRICT
    );
    
    CREATE INDEX [IX_Ubicaciones_UsuarioCreacionId] ON [dbo].[Ubicaciones] ([UsuarioCreacionId]);
END

-- Tabla Equipos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Equipos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Equipos] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Nombre] NVARCHAR(200) NOT NULL,
        [NumeroSerie] NVARCHAR(100) NULL,
        [Modelo] NVARCHAR(100) NULL,
        [Marca] NVARCHAR(100) NULL,
        [Estado] NVARCHAR(50) NOT NULL,
        [FechaAdquisicion] DATETIME2 NULL,
        [FechaUltimoMantenimiento] DATETIME2 NULL,
        [FechaProximoMantenimiento] DATETIME2 NULL,
        [Observaciones] NVARCHAR(MAX) NULL,
        [UbicacionId] INT NULL,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UsuarioCreacionId] NVARCHAR(450) NULL,
        [Activo] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Equipos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Equipos_Ubicaciones_UbicacionId] FOREIGN KEY ([UbicacionId]) REFERENCES [dbo].[Ubicaciones] ([Id]) ON DELETE RESTRICT,
        CONSTRAINT [FK_Equipos_AspNetUsers_UsuarioCreacionId] FOREIGN KEY ([UsuarioCreacionId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE RESTRICT
    );
    
    CREATE INDEX [IX_Equipos_UbicacionId] ON [dbo].[Equipos] ([UbicacionId]);
    CREATE INDEX [IX_Equipos_UsuarioCreacionId] ON [dbo].[Equipos] ([UsuarioCreacionId]);
END

-- Tabla FichasAveria
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FichasAveria]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FichasAveria] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Descripcion] NVARCHAR(MAX) NOT NULL,
        [FechaReporte] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [FechaSolucion] DATETIME2 NULL,
        [Estado] NVARCHAR(50) NOT NULL,
        [Solucion] NVARCHAR(MAX) NULL,
        [EquipoId] INT NOT NULL,
        [UsuarioReporteId] NVARCHAR(450) NULL,
        [UsuarioAsignadoId] NVARCHAR(450) NULL,
        [FechaCreacion] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [Activo] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_FichasAveria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FichasAveria_Equipos_EquipoId] FOREIGN KEY ([EquipoId]) REFERENCES [dbo].[Equipos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_FichasAveria_AspNetUsers_UsuarioReporteId] FOREIGN KEY ([UsuarioReporteId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE RESTRICT,
        CONSTRAINT [FK_FichasAveria_AspNetUsers_UsuarioAsignadoId] FOREIGN KEY ([UsuarioAsignadoId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE RESTRICT
    );
    
    CREATE INDEX [IX_FichasAveria_EquipoId] ON [dbo].[FichasAveria] ([EquipoId]);
    CREATE INDEX [IX_FichasAveria_UsuarioReporteId] ON [dbo].[FichasAveria] ([UsuarioReporteId]);
    CREATE INDEX [IX_FichasAveria_UsuarioAsignadoId] ON [dbo].[FichasAveria] ([UsuarioAsignadoId]);
END
