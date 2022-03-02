#	Drop Table `aks`.`CashFunc_Operation`;
#
CREATE TABLE `cashfunc_operation` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OpFullName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `OperationName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `KoefDebitor` int NOT NULL DEFAULT '1',
  `KoefCreditor` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uix_CashFunc_Operaion_OpFullName` (`OpFullName`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
# 
INSERT INTO `aks`.`CashFunc_Operation` (`Id`, `OpFullName`, `OperationName`, `KoefDebitor`, `KoefCreditor`) VALUES (1, 'OperationPrihod', 'Приходный ордер', 1, 0);
INSERT INTO `aks`.`CashFunc_Operation` (`Id`, `OpFullName`, `OperationName`, `KoefDebitor`, `KoefCreditor`) VALUES (2, 'OperationRashod', 'Расходный ордер', -1, 0);


#Drop Table `aks`.`cashfunc_Operation_Prihod`;
#
CREATE TABLE `aks`.`cashfunc_Operation_Prihod` (
  `Id` int NOT NULL,
  `OKUD` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
#

#Drop Table `aks`.`cashfunc_Operation_Rashod`;
#
CREATE TABLE `aks`.`cashfunc_Operation_Rashod` (
  `Id` int NOT NULL,
  `OKUD` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
#


#Drop Table `aks`.`CashFunc_Local`;
#
CREATE TABLE `aks`.`cashfunc_local` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `LocFullName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `LocalName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `IsSeller` bit(1) NOT NULL DEFAULT b'0',
  `IsСustomer` bit(1) NOT NULL DEFAULT b'0',
  `IsDivision` bit(1) NOT NULL DEFAULT b'0',
  `IsDraft` bit(1) NOT NULL DEFAULT b'1',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci; 
#
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (1, 'LocalPokupatel', 'Разовый покупатель', 0, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (2, 'LocalPaidInReasons', 'Возврат от подотчётного лица', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (3, 'LocalPaidInReasons', 'Оплата от покупателя (наличные)', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (4, 'LocalPaidInReasons', 'Оплата от покупателя (кредитные карты)', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (5, 'LocalPaidInReasons', 'Аванс от покупателя (наличные)', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (6, 'LocalPaidInReasons', 'Возврат денежных средств на размен', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (7, 'LocalPaidInReasons', 'Прочий приход', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (8, 'LocalPaidRashReasons', 'Выдача подотчетному лицу', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (9, 'LocalPaidRashReasons', 'Возврат покупателю', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (10, 'LocalPaidRashReasons', 'Выдача денежных средств на размен', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (11, 'LocalPaidRashReasons', 'Инкассация', 1, 1, 0);
INSERT INTO `aks`.`cashfunc_local` (`Id`, `LocFullName`, `LocalName`, `IsSeller`, `IsСustomer`, `IsDivision`) VALUES (12, 'LocalPaidRashReasons', 'Прочий расход', 1, 1, 0);




#Drop Table `aks`.`cashfunc_local_Kassa`;
#
CREATE TABLE `aks`.`cashfunc_local_kassa` (
  `Id` int NOT NULL,
  `HostName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `Organization` varchar(500) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `StructPodr` varchar(500) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `OKPO` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `LastDocNumPrih` int not null default 0,
  `LastDocNumRash` int not null default 0,
  `LastDocNumKasBook` int not null default 0,
  `LastDocNumActVozv` int not null default 0,
  `LastDocNumReportKas` int not null default 0,
  `LastDocNumScetKkm` int not null default 0,
  `LastDocNumVerifNal` int not null default 0,
  `LastDocNumInvent` int not null default 0,
  `INN` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `ZavodKKM` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `RegKKM` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `GlavBuhFio` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KkmName` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `DolRukOrg` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `RukFio` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `ZavDivisionFio` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UIX_local_kassa_HostName` (`HostName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
#

#Drop Table `aks`.`cashfunc_local_PaidInReasons`;
#
CREATE TABLE `aks`.`cashfunc_local_PaidInReasons` (
  `Id` int NOT NULL,
  `Osnovanie` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `DebetNomerSchet` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KredikKorSchet` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UIX_local_PaidInReasons` (`Osnovanie`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
#
INSERT INTO `aks`.`cashfunc_local_PaidInReasons` (`Id`, `Osnovanie`, `DebetNomerSchet`, `KredikKorSchet`) VALUES (2, '101', '50.01', '71.01');
INSERT INTO `aks`.`cashfunc_local_PaidInReasons` (`Id`, `Osnovanie`, `DebetNomerSchet`, `KredikKorSchet`) VALUES (3, '102', '50.01', '62.P');
INSERT INTO `aks`.`cashfunc_local_PaidInReasons` (`Id`, `Osnovanie`, `DebetNomerSchet`, `KredikKorSchet`) VALUES (4, '103', '57.03', '62.01');
INSERT INTO `aks`.`cashfunc_local_PaidInReasons` (`Id`, `Osnovanie`, `DebetNomerSchet`, `KredikKorSchet`) VALUES (5, '104', '50.01', '62.02');
INSERT INTO `aks`.`cashfunc_local_PaidInReasons` (`Id`, `Osnovanie`, `DebetNomerSchet`, `KredikKorSchet`) VALUES (6, '105', '50.01', '73.04');
INSERT INTO `aks`.`cashfunc_local_PaidInReasons` (`Id`, `Osnovanie`, `DebetNomerSchet`, `KredikKorSchet`) VALUES (7, '106', '50.01', '91.01');


#Drop Table `aks`.`cashfunc_local_PaidRashReasons`;
#
CREATE TABLE `aks`.`cashfunc_local_PaidRashReasons` (
  `Id` int NOT NULL,
  `Osnovanie` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KreditNomerSchet` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `DebetKorSchet` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UIX_local_PaidRashReasons` (`Osnovanie`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
#
INSERT INTO `aks`.`cashfunc_local_PaidRashReasons` (`Id`, `Osnovanie`, `KreditNomerSchet`, `DebetKorSchet`) VALUES (8, '201', '50.01', '71.01');
INSERT INTO `aks`.`cashfunc_local_PaidRashReasons` (`Id`, `Osnovanie`, `KreditNomerSchet`, `DebetKorSchet`) VALUES (9, '203', '50.01', '62.01');
INSERT INTO `aks`.`cashfunc_local_PaidRashReasons` (`Id`, `Osnovanie`, `KreditNomerSchet`, `DebetKorSchet`) VALUES (10, '204', '50.01', '73.04');
INSERT INTO `aks`.`cashfunc_local_PaidRashReasons` (`Id`, `Osnovanie`, `KreditNomerSchet`, `DebetKorSchet`) VALUES (11, '205', '50.01', '57.01');
INSERT INTO `aks`.`cashfunc_local_PaidRashReasons` (`Id`, `Osnovanie`, `KreditNomerSchet`, `DebetKorSchet`) VALUES (12, '206', '50.01', '91.02');




#Drop Table `aks`.`CashFunc_Document`;
#
CREATE TABLE `aks`.`CashFunc_Document` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `DocFullName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `UreDate` Date NOT NULL,
  `CteateDate` DateTime NOT NULL,
  `ModifyDate` DateTime NOT NULL,
  `ModifyUser` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `OperationId` int NOT NULL,
  `LocalDebitorId` int NOT NULL,
  `LocalCreditorId` int NOT NULL,
  `DocNum` int NOT NULL,
  `IsDraft` bit(1) NOT NULL DEFAULT b'1',
  `IsProcessed` bit(1) NOT NULL DEFAULT b'0',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci; 


#Drop Table `aks`.`cashfunc_document_Prihod`;
#
CREATE TABLE `aks`.`cashfunc_document_Prihod` (
  `Id` int NOT NULL,
  `DebetNomerSchet` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KreditKodDivision` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KredikKorSchet` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KredikKodAnalUch` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `Summa` decimal(16,4) DEFAULT NULL,
  `SummaStr` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KodNazn` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `Osnovanie` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `Id_PaidInReasons` int NULL,
  `VtomChisle` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `NDS` decimal(5,2) DEFAULT NULL,
  `Prilozenie` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `GlavBuh` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
#

#Drop Table `aks`.`cashfunc_document_Rashod`;
#
CREATE TABLE `aks`.`cashfunc_document_Rashod` (
  `Id` int NOT NULL,
  `DebetKodDivision` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `DebetKorSchet` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `DebetKodAnalUch` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KreditNomerSchet` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `Summa` decimal(16,4) DEFAULT NULL,
  `SummaStr` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `KodNazn` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `PoDoc` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `Osnovanie` varchar(50) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `Id_PaidRashReasons` int NULL,
  `Prilozenie` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `DolRukFio` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `RukFio` varchar(200) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `GlavBuh` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
#	