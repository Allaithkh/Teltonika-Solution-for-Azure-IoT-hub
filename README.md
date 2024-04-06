# TeltonikaSolution
The Solution is related to reading data from Teltonika device over IOT Hub and Receive the data from IOT Hub after converting raw data into information and store it in database

# We can recieve data Teltonika Devices using multiple ways some of them are

# 1-) TCP
    For this we need socket programming and open socket for communication and configure the same in teltonika device
# 2-) AWS IoT Core 
    we need to configure AWS IOT Connnection string in teltonika device and do related programming stuff.
# 3-) Azure IOT Hub
    we need to configure Azure IOT Hub Connnection string in teltonika device and do related programming stuff.

This Solution is related to Azure IOT Hub Stuff

# All the Codec protocols are handled in this solution We only need to add database, IOT Hub and Storage Account Connection string and run the project it will start sending database.

Project have 3 WebJobs

# 1-) TeltonikaTelemetry 
                        This webjob is responsible for receiving the Byte array (Raw Data) and store the same with deviceId into the database table name DeviceTransmission.

# 2-) TeltonikaTelemetricTrove
                            This webjob is responsible for reading the Raw data from the database table name DeviceTransmission which is saved by TeltonikaTelemetry Webjob. After reading the data they process into convert into information like Latitude, Longtitude, Speed , Priority and other stuff and store into the database table name DecodedDeviceTransmission and delete the raw data from the DeviceTransmission table after processing it. It will help to keep database lighter.

# 3-) TeltonikaDataCleansing
                           This webjob is responsible for keeping only last two days processed data and delete other days data. It will help to keep database lighter.


# How to run this Project

# First download this project and change the connection strings in All Webjobs
![image](https://github.com/hassan418/TeltonikaSolution/assets/20794109/50c4187f-cb79-49b2-9249-0b2a31933691)

![image](https://github.com/hassan418/TeltonikaSolution/assets/20794109/92337a25-3c26-404d-8bc1-a70dd379e048)

# Create Database and provided same database connectionstring in appsettings and run this script on database Or Create Table Manually or add migration process in code.
                SET ANSI_NULLS ON
		GO
		SET QUOTED_IDENTIFIER ON
	        GO
		CREATE TABLE [dbo].[DecodedDeviceTransmissions](
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[DeviceIMEI] [nvarchar](max) NOT NULL,
			[Priority] [nvarchar](max) NOT NULL,
			[Created] [datetime2](7) NOT NULL,
			[MessageCreated] [datetime2](7) NOT NULL,
			[Longitude] [real] NOT NULL,
			[Latitude] [real] NOT NULL,
			[Altitude] [int] NOT NULL,
			[Angle] [int] NOT NULL,
			[Satellites] [tinyint] NOT NULL,
			[Speed] [int] NOT NULL,
 		CONSTRAINT [PK_DecodedDeviceTransmissions] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

		GO
		SET ANSI_NULLS ON
		GO
		SET QUOTED_IDENTIFIER ON
		GO
		CREATE TABLE [dbo].[DeviceTransmissions](
			[Id] [bigint] IDENTITY(1,1) NOT NULL,
			[DeviceIMEI] [nvarchar](max) NOT NULL,
			[DeviceMessage] [varbinary](max) NOT NULL,
			[IsProcessed] [bit] NOT NULL,
 		CONSTRAINT [PK_DeviceTransmissions] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
		GO


# Now Run the Webjobs with multiple startup projection setting

![image](https://github.com/hassan418/TeltonikaSolution/assets/20794109/fa597a18-5491-4343-ad67-588f9e8adbde)

# Here is Results

