INSERT INTO [Inventory].[Property]
           ([PropertyName]
           ,[PropertyValue]
           ,[Description]
           ,[PropertyType])
     VALUES('FtpOutgoingServer','','Outgoing ftp server hostname or IP for outputting order files to',1),
	 ('FtpOutgoingUsername','','Username for outgoing ftp server for outputting order files to',1),
	 ('FtpOutgoingPassword','','Password for outgoing ftp server for outputting order files to',1),
	 ('FtpIncomingServer'  ,''  ,'Incoming ftp server hostname or IP for accessing order lookup data',1),
	 ('FtpIncomingUsername','','Username for Incoming ftp server for accessing order lookup data',1),
	 ('FtpIncomingPassword','','Password for Incoming ftp server for accessing order lookup data',1),
	 ('OutgoingFilePath'   ,'','Outgoing File share path for outputting order files to',1)


