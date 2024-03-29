USE [JobInterview]
GO
/****** Object:  Table [dbo].[Dependent]    Script Date: 8/31/2019 4:52:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Dependent](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[DependentTypeId] [tinyint] NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_Dependent] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DependentType]    Script Date: 8/31/2019 4:52:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DependentType](
	[Id] [tinyint] NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DependentType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Employee]    Script Date: 8/31/2019 4:52:59 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employee](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](20) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[MiddleInitial] [nvarchar](1) NULL,
	[AnnualSalary] [money] NOT NULL,
	[PaycheckGross] [money] NOT NULL,
	[DeductionBenefits] [money] NOT NULL,
	[Discount] [money] NOT NULL,
	[PaycheckNet] [money] NOT NULL,
 CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Dependent] ON 
GO
INSERT [dbo].[Dependent] ([Id], [EmployeeId], [DependentTypeId], [Name]) VALUES (1, 1, 1, N'Shelly')
GO
INSERT [dbo].[Dependent] ([Id], [EmployeeId], [DependentTypeId], [Name]) VALUES (2, 1, 2, N'Percy')
GO
INSERT [dbo].[Dependent] ([Id], [EmployeeId], [DependentTypeId], [Name]) VALUES (3, 1, 2, N'Charlie')
GO
INSERT [dbo].[Dependent] ([Id], [EmployeeId], [DependentTypeId], [Name]) VALUES (4, 2, 1, N'Marcy')
GO
INSERT [dbo].[Dependent] ([Id], [EmployeeId], [DependentTypeId], [Name]) VALUES (5, 2, 2, N'Sheri')
GO
INSERT [dbo].[Dependent] ([Id], [EmployeeId], [DependentTypeId], [Name]) VALUES (6, 3, 2, N'Quintus')
GO
SET IDENTITY_INSERT [dbo].[Dependent] OFF
GO
INSERT [dbo].[DependentType] ([Id], [Type]) VALUES (1, N'Spouse')
GO
INSERT [dbo].[DependentType] ([Id], [Type]) VALUES (2, N'Child')
GO
SET IDENTITY_INSERT [dbo].[Employee] ON 
GO
INSERT [dbo].[Employee] ([Id], [FirstName], [LastName], [MiddleInitial], [AnnualSalary], [PaycheckGross], [DeductionBenefits], [Discount], [PaycheckNet]) VALUES (1, N'Brian', N'Allen', N'Q', 120000.0000, 5000.0000, 333.3333, 50.0000, 4716.6667)
GO
INSERT [dbo].[Employee] ([Id], [FirstName], [LastName], [MiddleInitial], [AnnualSalary], [PaycheckGross], [DeductionBenefits], [Discount], [PaycheckNet]) VALUES (2, N'Barney', N'Fife', NULL, 140000.0000, 5833.3333, 291.6667, 0.0000, 5541.6667)
GO
INSERT [dbo].[Employee] ([Id], [FirstName], [LastName], [MiddleInitial], [AnnualSalary], [PaycheckGross], [DeductionBenefits], [Discount], [PaycheckNet]) VALUES (3, N'Mary', N'Albright', N'P', 160000.0000, 6666.6667, 250.0000, 37.5000, 6454.1667)
GO
SET IDENTITY_INSERT [dbo].[Employee] OFF
GO
ALTER TABLE [dbo].[Dependent]  WITH CHECK ADD  CONSTRAINT [FK_Dependent_DependentType] FOREIGN KEY([DependentTypeId])
REFERENCES [dbo].[DependentType] ([Id])
GO
ALTER TABLE [dbo].[Dependent] CHECK CONSTRAINT [FK_Dependent_DependentType]
GO
ALTER TABLE [dbo].[Dependent]  WITH CHECK ADD  CONSTRAINT [FK_Dependent_Employee] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([Id])
GO
ALTER TABLE [dbo].[Dependent] CHECK CONSTRAINT [FK_Dependent_Employee]
GO
