USE master;
GO
CREATE DATABASE UserProcessorDB;
GO
USE UserProcessorDB;
GO

CREATE TABLE NamespaceInfo(Id BIGINT PRIMARY KEY Identity NOT NULL);
			
CREATE TABLE UserProcessorInfo(
FullProcessorName VarChar(255) NOT NULL, 
NamespaceInfoId BIGINT NOT NULL,
TemplateId BIGINT NOT NULL, 
GeoserverName VarChar(255) NOT NULL, 
UserName VarChar(100) NOT NULL, 
ProcessorName VarChar(255) NOT NULL, 
Version VarChar(50) NOT NULL, 
CreationalTime DateTime NOT NULL, 
IsActive BIT NOT NULL, 
PRIMARY KEY CLUSTERED (UserName, ProcessorName, Version));

ALTER TABLE UserProcessorInfo ADD CONSTRAINT FK_UserProcessorInfo_NamespaceInfo
FOREIGN KEY(NamespaceInfoId) REFERENCES NamespaceInfo(Id);
