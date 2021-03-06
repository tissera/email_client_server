USE [master]
GO
/****** Object:  Database [MailServerDB]    Script Date: 4/14/2019 4:55:06 PM ******/
CREATE DATABASE [MailServerDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'MailServerDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\MailServerDB.mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'MailServerDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\MailServerDB_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [MailServerDB] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [MailServerDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [MailServerDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [MailServerDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [MailServerDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [MailServerDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [MailServerDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [MailServerDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [MailServerDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [MailServerDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [MailServerDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [MailServerDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [MailServerDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [MailServerDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [MailServerDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [MailServerDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [MailServerDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [MailServerDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [MailServerDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [MailServerDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [MailServerDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [MailServerDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [MailServerDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [MailServerDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [MailServerDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [MailServerDB] SET  MULTI_USER 
GO
ALTER DATABASE [MailServerDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [MailServerDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [MailServerDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [MailServerDB] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [MailServerDB] SET DELAYED_DURABILITY = DISABLED 
GO
USE [MailServerDB]
GO
/****** Object:  User [ELogin]    Script Date: 4/14/2019 4:55:06 PM ******/
CREATE USER [ELogin] FOR LOGIN [ELogin] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ELogin]
GO
/****** Object:  Table [dbo].[tblEmailTemplateDetails]    Script Date: 4/14/2019 4:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblEmailTemplateDetails](
	[TemplateID] [int] IDENTITY(1,1) NOT NULL,
	[SenderEmailAddress] [nvarchar](254) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Subject] [nvarchar](80) NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[SendDate] [datetime] NULL,
	[To] [nvarchar](254) NULL,
	[Cc] [nvarchar](254) NULL,
	[Bcc] [nvarchar](254) NULL,
 CONSTRAINT [PK_tblEmailTemplate] PRIMARY KEY CLUSTERED 
(
	[TemplateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblUserDetails]    Script Date: 4/14/2019 4:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUserDetails](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](20) NOT NULL,
	[Password] [nvarchar](10) NOT NULL,
	[EmailAddress] [nvarchar](254) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_tblUserDetails] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[tblUserDetails] ON 

GO
INSERT [dbo].[tblUserDetails] ([UserID], [UserName], [Password], [EmailAddress], [IsActive]) VALUES (1, N'Sara', N'Sara', N'sara.s@abc.com', 1)
GO
INSERT [dbo].[tblUserDetails] ([UserID], [UserName], [Password], [EmailAddress], [IsActive]) VALUES (2, N'Test', N'Test', N'testCC@t.com', 1)
GO
INSERT [dbo].[tblUserDetails] ([UserID], [UserName], [Password], [EmailAddress], [IsActive]) VALUES (3, N'Step', N'Step', N'Step@abc.com', 1)
GO
SET IDENTITY_INSERT [dbo].[tblUserDetails] OFF
GO
/****** Object:  StoredProcedure [dbo].[Sp_ActiveUser]    Script Date: 4/14/2019 4:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_ActiveUser] 
	-- Add the parameters for the stored procedure here
	@UserName AS nvarchar(20),
	@Password AS nvarchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT COUNT(*) FROM [dbo].[tblUserDetails]
	WHERE UserName=@UserName AND [Password]=@Password AND IsActive=1


END


GO
/****** Object:  StoredProcedure [dbo].[Sp_InsertEmail]    Script Date: 4/14/2019 4:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_InsertEmail] 
	-- Add the parameters for the stored procedure here
	@SenderEmailAddress AS NVARCHAR(254),
	@CreatedDate AS DATETIME,
	@Subject AS NVARCHAR(80),
	@Body AS NVARCHAR(MAX),
	--@xmlReceiver AS XML,
	@ToEmailAddress AS NVARCHAR(254),
	@CcEmailAddress AS NVARCHAR(254),
	@BccEmailAddress AS NVARCHAR(254),
	@Message AS NVARCHAR(100) OUT
		
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
BEGIN TRY
	BEGIN TRANSACTION 
    -- Insert statements for procedure here
	DECLARE @TemplateId AS int
	
	INSERT INTO [dbo].[tblEmailTemplateDetails]([SenderEmailAddress],[CreatedDate],[Subject],[Body],[To],[Cc],[Bcc])
	VALUES (@SenderEmailAddress,@CreatedDate, @Subject,@Body,@ToEmailAddress,@CcEmailAddress,@BccEmailAddress)
				
	SELECT @TemplateId = TemplateID FROM [dbo].[tblEmailTemplateDetails] WHERE [SenderEmailAddress]=@SenderEmailAddress AND [CreatedDate] =@CreatedDate AND [Subject]=@Subject
	
	IF @TemplateId <>0
	BEGIN
		-- multiple receiver records from xml
		
	--INSERT INTO [dbo].[tblReceiverDetails]
 --   SELECT
	--@TemplateId as TemplateID,
 --   receiver.value('(type/text())[1]','NVARCHAR(5)') AS [Type] , 
 --   receiver.value('(emailAddress/text())[1]','NVARCHAR(254)') AS EmailAddress
	
 --   FROM
 --   @xmlReceiver.nodes('/root/receiver')AS TEMPTABLE(receiver)
			
	SET @Message= 'Success'
			
		COMMIT TRANSACTION 
	END
	ELSE
	BEGIN
		ROLLBACK TRANSACTION 
	END

END TRY
	BEGIN CATCH
				BEGIN	
				SELECT ERROR_MESSAGE();
				ROLLBACK TRANSACTION 
					
				END
	END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[Sp_SelectEmail]    Script Date: 4/14/2019 4:55:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_SelectEmail] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	CREATE TABLE #TEMP(
	[TemplateID] [int] ,
	[SenderEmailAddress] [nvarchar](254),
	[CreatedDate] datetime,
	[Subject] [nvarchar](80) ,
	[Body] [nvarchar](max) ,
	[ToEmailAddress] [nvarchar](254),
	[CcEmailAddress] [nvarchar](254),
	[BccEmailAddress] [nvarchar](254),
	)
	BEGIN TRY
	BEGIN TRANSACTION 

INSERT INTO #TEMP
SELECT E.TemplateID,E.SenderEmailAddress,E.CreatedDate,E.[Subject],E.Body ,E.[To],E.[Cc],E.[Bcc]
FROM tblEmailTemplateDetails E 
WHERE E.SendDate IS NULL

UPDATE [dbo].[tblEmailTemplateDetails]
   SET [SendDate] = GETDATE()
	FROM #TEMP 
	INNER JOIN [tblEmailTemplateDetails] E ON E.TemplateID=#TEMP.TemplateID
	WHERE E.SendDate IS NULL

SELECT * FROM #TEMP
DROP TABLE #TEMP
COMMIT TRANSACTION 
	
END TRY
	BEGIN CATCH
				BEGIN	
				SELECT ERROR_MESSAGE();
				ROLLBACK TRANSACTION 
					
				END
	END CATCH
END

GO
USE [master]
GO
ALTER DATABASE [MailServerDB] SET  READ_WRITE 
GO
