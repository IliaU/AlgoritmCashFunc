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
INSERT INTO `aks`.`CashFunc_Operation` (`Id`, `OpFullName`, `OperationName`, `KoefDebitor`, `KoefCreditor`) VALUES (1, 'OperationProhod', 'Приходный ордер', 1, 0);



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




Drop Table `aks`.`cashfunc_local_Kassa`;
#
CREATE TABLE `cashfunc_local_kassa` (
  `Id` int NOT NULL,
  `HostName` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL,
  `Organization` varchar(500) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `StructPodr` varchar(500) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  `OKPO` varchar(100) CHARACTER SET utf8 COLLATE utf8_bin NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UIX_local_kassa_HostName` (`HostName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
#
#INSERT INTO `aks`.`cashfunc_local_Kassa` (`Id`, `HostName`, `Organization`, `StructPodr`, `OKUD`, `OKPO`, `KassaName`) VALUES (1, 'LocalPokupatel', 'Разовый покупатель', 0, 1, 0);





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
  `IsDraft` bit(1) NOT NULL DEFAULT b'1',
  `IsProcessed` bit(1) NOT NULL DEFAULT b'0',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci; 

	
		

