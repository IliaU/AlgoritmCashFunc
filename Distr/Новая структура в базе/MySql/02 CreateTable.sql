#	Drop Table `aks`.`CashFunc_Operation`;
#
CREATE TABLE `aks`.`CashFunc_Operation` (
  `Operation` int NOT NULL AUTO_INCREMENT,
  `DocFullName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `OperationName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`Operation`),
  UNIQUE KEY `uix_CashFunc_Operaion_DocFullName` (`DocFullName`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci; 
# 
INSERT INTO `aks`.`CashFunc_Operation` (`Operation`, `DocFullName`, `OperationName`) VALUES (1, 'DocumentPrihod', 'Ïðèõîäíûé îðäåð');



#Drop Table `aks`.`CashFunc_Local`;
#
CREATE TABLE `aks`.`cashfunc_local` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `LocFullName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `LocalName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `IsSeller` bit(1) NOT NULL DEFAULT b'0',
  `IsÑustomer` bit(1) NOT NULL DEFAULT b'0',
  `IsDivision` bit(1) NOT NULL DEFAULT b'0',
  `IsDraft` bit(1) NOT NULL DEFAULT b'1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci; 
#
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsÑustomer`, `IsDivision`) VALUES (1, 'LocalPokupatel', 'Ðàçîâûé ïîêóïàòåëü', 0, 1, 0);




	
		

CREATE TABLE [dbo].[Document](
	[Document] [int] IDENTITY(1,1) NOT NULL,
	[Operation] [int] NOT NULL,
	[DocNumber] [int] NOT NULL,
	[FromLocal] [smallint] NOT NULL,
	[ToLocal] [smallint] NOT NULL DEFAULT ((0)),
	[UreDate] [datetime] NOT NULL DEFAULT (getdate()),
	[OldUreDate] [datetime] NULL,
	[IsDraft] [bit] NOT NULL DEFAULT ((0)),
	[IsEdited] [bit] NOT NULL DEFAULT ((0)),
	[isCheck] [bit] NOT NULL DEFAULT ((0)),
	[Deleted] [bit] NOT NULL DEFAULT ((0)),
	[LoockOperator] [varchar](30) NULL,
	[EnterDate] [datetime] NOT NULL DEFAULT (getdate()),
	[ModifyDate] [datetime] NOT NULL DEFAULT (getdate()),
	[Operator] [varchar](30) NOT NULL DEFAULT (suser_sname()),
	CONSTRAINT [UIX_Document] PRIMARY KEY CLUSTERED 
	(
		[Document] DESC,[UreDate] DESC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON S_Month([UreDate])
) On S_Month([UreDate])
CREATE NONCLUSTERED INDEX [IX_UreDate] ON [dbo].[Document] 
([UreDate] DESC)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) On S_Month([UreDate])
GO

Create Table DocumentInfo
(
	[Document] [int] NOT NULL,
	[UreDate] [datetime] NOT NULL default(getDate()),
	[InfoGroup] [varchar](30) NOT NULL,
	[InfoName] [varchar](30) NOT NULL,
	[Bool1] [bit] NULL,
	[Int1] [int] NULL,
	[Int2] [int] NULL,
	[Var1] [varchar](30) NULL,
	[Dat1] [datetime] NULL,
	[Txt1] [text] NULL,
	[Mon1] [money] NULL,
	[Mon2] [money] NULL,
	[ModifyDate] [datetime] NOT NULL DEFAULT (getdate()),
	CONSTRAINT [UIX_DocumentInfo] PRIMARY KEY CLUSTERED 
	(
		[Document] DESC, [UreDate] DESC, [InfoGroup], [InfoName]
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON S_Month([UreDate])
)  ON S_Month([UreDate])
GO


Create Table Journal
(
        [Journal] [bigint] IDENTITY(1,1) NOT NULL,
	[Document] [int] NOT NULL,
	[RowNumber] [int] NOT NULL default(0),
	[Iteration] [tinyint] NOT NULL default(0),
	[Product] [int] NOT NULL,
	[Qty] [money] NOT NULL,
	[UreDate] [datetime] NOT NULL default(getDate()),
	[ModifyDate] [datetime] NOT NULL DEFAULT (getdate()),
	CONSTRAINT [UIX_Journal] PRIMARY KEY CLUSTERED 
	(
		[Journal] DESC, [UreDate] DESC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON S_Month([UreDate])
)  ON S_Month([UreDate])


Create Table JournalInfo
(
	[Document] [int] NOT NULL,
	[UreDate] [datetime] NOT NULL default(getDate()),
	[Journal] [bigint] NOT NULL,
	[RowNumber] [int] NOT NULL default(0),
	[Iteration] [tinyint] NOT NULL default(0),
	[InfoGroup] [varchar](30) NOT NULL,
	[InfoName] [varchar](30) NOT NULL,
	[Bool1] [bit] NULL,
	[Int1] [int] NULL,
	[Var1] [varchar](30) NULL,
	[Var2] [varchar](30) NULL,
	[Dat1] [datetime] NULL,
	[Dat2] [datetime] NULL,
	[Txt1] [text] NULL,
	[ModifyDate] [datetime] NOT NULL DEFAULT (getdate()),
	CONSTRAINT [UIX_JournalInfo] PRIMARY KEY CLUSTERED 
	(
		[Document] DESC, [UreDate] DESC, [Journal], [RowNumber], [Iteration], [InfoGroup], [InfoName]
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON S_Month([UreDate])
)  ON S_Month([UreDate])
GO


Create Table JournalQR
(
	[JournalQR] [bigint] IDENTITY(1,1) NOT NULL,
	[Document] [int] NOT NULL,
    [Journal] [bigint] NOT NULL,
	[QR] [varchar](400) NOT NULL,
	[UreDate] [datetime] NOT NULL default(getDate()),
	[ModifyDate] [datetime] NOT NULL DEFAULT (getdate()),
	CONSTRAINT [UIX_JournalQR] PRIMARY KEY CLUSTERED 
	(
		[JournalQR] DESC, [UreDate] DESC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON S_Month([UreDate])
)  ON S_Month([UreDate])

Create index [Document_UreDate_Journal] on JournalQR
([Document], [UreDate], [Journal]) ON S_Month([UreDate])