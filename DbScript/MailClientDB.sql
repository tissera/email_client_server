USE [MailClientDB]
GO
/****** Object:  User [EClient]    Script Date: 4/14/2019 4:52:33 PM ******/
CREATE USER [EClient] FOR LOGIN [EClient] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [EClient]
GO
/****** Object:  Table [dbo].[tblAlias]    Script Date: 4/14/2019 4:52:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAlias](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[EmailAddress] [nvarchar](254) NOT NULL,
 CONSTRAINT [PK_tblAlias] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblEmailTemplateDetails]    Script Date: 4/14/2019 4:52:35 PM ******/
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
	[To] [nvarchar](254) NULL,
	[Cc] [nvarchar](254) NULL,
	[Bcc] [nvarchar](254) NULL,
 CONSTRAINT [PK_tblEmailTemplate] PRIMARY KEY CLUSTERED 
(
	[TemplateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tblSentEmailDetails]    Script Date: 4/14/2019 4:52:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblSentEmailDetails](
	[EmailID] [int] IDENTITY(1,1) NOT NULL,
	[Date] [datetime] NOT NULL,
	[SenderEmailAddress] [nvarchar](254) NOT NULL,
	[IsSent] [bit] NOT NULL,
	[Subject] [nvarchar](80) NULL,
	[Body] [nvarchar](max) NULL,
	[ToEmailAddress] [nvarchar](254) NULL,
	[CcEmailAddress] [nvarchar](254) NULL,
	[BccEmailAddress] [nvarchar](254) NULL,
	[IsDelete] [bit] NULL,
 CONSTRAINT [PK_tblSentEmailDetails] PRIMARY KEY CLUSTERED 
(
	[EmailID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  StoredProcedure [dbo].[Sp_InsertEmail]    Script Date: 4/14/2019 4:52:35 PM ******/
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
	--@SenderEmailAddress AS NVARCHAR(254),
	--@CreatedDate AS DATETIME,
	--@Subject AS NVARCHAR(80),
	--@Body AS NVARCHAR(MAX),
	@xmlReceiver AS XML,
	--@xmlAttachment AS XML,
	@Message AS NVARCHAR(100) OUT
	---@xmlxAttachment AS XML
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
BEGIN TRY
	BEGIN TRANSACTION 
    -- Insert statements for procedure here
	DECLARE @TemplateId AS int
	
	--INSERT INTO [dbo].[tblEmailTemplateDetails]([SenderEmailAddress],[CreatedDate],[Subject],[Body])
	--VALUES (@SenderEmailAddress,@CreatedDate, @Subject,@Body)
				
	--SELECT @TemplateId = TemplateID FROM [dbo].[tblEmailTemplateDetails] WHERE [SenderEmailAddress]=@SenderEmailAddress AND [CreatedDate] =@CreatedDate AND [Subject]=@Subject
	set @TemplateId=1
	IF @TemplateId <>0
	BEGIN
		-- multiple use xml
		INSERT INTO [dbo].[tblEmailTemplateDetails]
    SELECT
	mail.value('(From/text())[1]','NVARCHAR(254)') AS SenderEmailAddress , 
	mail.value('(Date/text())[1]','DATETIME') AS CreatedDate,
	mail.value('(Subject/text())[1]','NVARCHAR(80)') AS [Subject],
	mail.value('(Body/text())[1]','NVARCHAR(MAX)') AS Body,
	mail.value('(To/text())[1]','NVARCHAR(254)') AS [To],
	mail.value('(CC/text())[1]','NVARCHAR(254)') AS Cc,
	mail.value('(Bcc/text())[1]','NVARCHAR(254)') AS Bcc
    FROM
    @xmlReceiver.nodes('/root/mail')AS TEMPTABLE(mail)

	--INSERT INTO [dbo].[tblReceiverDetails]
 --   SELECT
	--@TemplateId as TemplateID,
 --   receiver.value('(type/text())[1]','NVARCHAR(5)') AS [Type] , 
 --   receiver.value('(emailAddress/text())[1]','NVARCHAR(254)') AS EmailAddress,
	--NULL
 --   FROM
 --   @xmlReceiver.nodes('/root/receiver')AS TEMPTABLE(receiver)

		-- multiple attachment xml
		--INSERT INTO [dbo].[tblAttachmentDetails]([TemplateID],[FileType],[FileName],[FileSize])
		--VALUES (@TemplateId,@FileType,@FileName,@FileSize)

	--INSERT INTO [dbo].[tblAttachmentDetails]
	--SELECT
	--@TemplateId as TemplateID,
 --   attachments.value('(fileType/text())[1]','VARCHAR(10)') AS [FileType] , 
 --   attachments.value('(fileName/text())[1]','VARCHAR(200)') AS [FileName],
	--10
 --   FROM
 --   @xmlAttachment.nodes('/root/attachments')AS TEMPTABLE(attachments)
	
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
				--INSERT INTO @tbl_ErrorDetails Values(CONVERT(varchar(20),@inirow),ERROR_MESSAGE()) 
				
				END
	END CATCH
END


GO
/****** Object:  StoredProcedure [dbo].[Sp_InsertUpdateSentEmail]    Script Date: 4/14/2019 4:52:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_InsertUpdateSentEmail] 
	-- Add the parameters for the stored procedure here
	@EmailID INT = null,
	@SenderEmailAddress AS NVARCHAR(254),
	@IsSent bit,
	@Subject AS NVARCHAR(80),
	@Body AS NVARCHAR(MAX),
	@ToEmailAddress AS NVARCHAR(254),
	@CcEmailAddress AS NVARCHAR(254),
	@BccEmailAddress AS NVARCHAR(254),
	@Message AS NVARCHAR(100) OUT
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--- Add email address to tblAlias
	IF((SELECT COUNT(*)FROM tblAlias WHERE EmailAddress=@ToEmailAddress) = 0 AND @ToEmailAddress <> '')
	BEGIN
	INSERT INTO [dbo].[tblAlias]([EmailAddress])  VALUES  (@ToEmailAddress)
	END
	IF((SELECT COUNT(*)FROM tblAlias WHERE EmailAddress=@CcEmailAddress) = 0 AND @CcEmailAddress <> '')
	BEGIN
	INSERT INTO [dbo].[tblAlias]([EmailAddress])  VALUES  (@CcEmailAddress)
	END
	IF((SELECT COUNT(*)FROM tblAlias WHERE EmailAddress=@BccEmailAddress) = 0 AND @BccEmailAddress <> '')
	BEGIN
	INSERT INTO [dbo].[tblAlias]([EmailAddress])  VALUES  (@BccEmailAddress)
	END
	---
	IF(@EmailID IS NULL)
	BEGIN
	INSERT INTO [dbo].[tblSentEmailDetails]
           ([Date],[SenderEmailAddress],[IsSent],[Subject],[Body],[ToEmailAddress],[CcEmailAddress],[BccEmailAddress])
     VALUES(GETDATE(),@SenderEmailAddress,@IsSent,@Subject,@Body,@ToEmailAddress,@CcEmailAddress,@BccEmailAddress)

	END
	ELSE
	BEGIN
	UPDATE [dbo].[tblSentEmailDetails]
   SET [Date] = GETDATE()
      ,[SenderEmailAddress] = @SenderEmailAddress
      ,[IsSent] = @IsSent
      ,[Subject] = @Subject
      ,[Body] = @Body
      ,[ToEmailAddress] = @ToEmailAddress
      ,[CcEmailAddress] = @CcEmailAddress
      ,[BccEmailAddress] =@BccEmailAddress
 WHERE [EmailID]=@EmailID

	END
	SET @Message= 'Success'
END

GO
/****** Object:  StoredProcedure [dbo].[Sp_SelectAllSentEmail]    Script Date: 4/14/2019 4:52:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_SelectAllSentEmail] 
	-- Add the parameters for the stored procedure here
	@SenderEmailAddress AS NVARCHAR(254),
	@IsSent bit,
	@IsDeleted bit= null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	if (@IsDeleted is null)
	begin
	SELECT [EmailID],[Date],[SenderEmailAddress],[IsSent],[Subject],[Body],[ToEmailAddress],[CcEmailAddress],[BccEmailAddress]
	FROM [dbo].[tblSentEmailDetails]
	WHERE [IsSent]=@IsSent AND [SenderEmailAddress]=@SenderEmailAddress AND IsDelete is null
	end
	else
	begin
	SELECT [EmailID],[Date],[SenderEmailAddress],[IsSent],[Subject],[Body],[ToEmailAddress],[CcEmailAddress],[BccEmailAddress]
	FROM [dbo].[tblSentEmailDetails]
	WHERE [SenderEmailAddress]=@SenderEmailAddress AND IsDelete =1
	end
END


GO
/****** Object:  StoredProcedure [dbo].[Sp_SelectEmail]    Script Date: 4/14/2019 4:52:35 PM ******/
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
	@EmailAddress AS nvarchar(254)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


SELECT *  FROM [dbo].[tblEmailTemplateDetails] 
  WHERE [To] LIKE '%'+@EmailAddress+'%' OR [Cc] LIKE '%'+@EmailAddress+'%'OR [Bcc] LIKE '%'+@EmailAddress+'%'



END


GO
/****** Object:  StoredProcedure [dbo].[Sp_SelectIdWiseSentEmail]    Script Date: 4/14/2019 4:52:35 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Sp_SelectIdWiseSentEmail] 
	-- Add the parameters for the stored procedure here
	@SenderEmailAddress AS NVARCHAR(254),
	@EmailID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT [EmailID],[Date],[SenderEmailAddress],[IsSent],[Subject],[Body],[ToEmailAddress],[CcEmailAddress],[BccEmailAddress]
	FROM [dbo].[tblSentEmailDetails]
	WHERE [EmailID]=@EmailID
END


GO
