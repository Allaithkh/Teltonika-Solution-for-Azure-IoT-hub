# TeltonikaSolution
The Solution is related to reading data from Teltonika device over IOT Hub and Receive the data from IOT Hub after converting raw data into information and storing it in the database

# We can receive data from Teltonika Devices using multiple ways some of them are

# 1-) TCP
    For this, we need socket programming and an open socket for communication and configure the same in the Teltonika device
# 2-) AWS IoT Core 
    we need to configure the AWS IOT connection string in the Teltonika device and do related programming stuff.
# 3-) Azure IOT Hub
    we need to configure the Azure IOT Hub Connection string in the Teltonika device and do related programming stuff.

This Solution is related to Azure IOT Hub Stuff

# All the Codec protocols are handled in this solution We only need to add a database, IOT Hub, and Storage Account Connection string and run the project it will start sending the data into the database.

The project has 3 WebJobs

# 1-) TeltonikaTelemetry 
                        This web job is responsible for receiving the Byte array (Raw Data) and storing the same with deviceId into the database table name DeviceTransmission.

# 2-) TeltonikaTelemetricTrove
                            This webjob is responsible for reading the Raw data from the database table name DeviceTransmission which is saved by TeltonikaTelemetry Webjob. After reading the data they process into convert it into information like Latitude, Longitude, Speed, Priority, and other stuff and store it in the database table named DecodedDeviceTransmission and delete the raw data from the DeviceTransmission table after processing it. It will help to keep the database lighter.

# 3-) TeltonikaDataCleansing
                           This job is responsible for keeping only the last two days processed data and deleting other days' data. It will help to keep the database lighter.

# How to run this Project

# First, download this project and change the connection strings in All Webjobs

![image](https://github.com/hassan418/TeltonikaSolution/assets/20794109/50c4187f-cb79-49b2-9249-0b2a31933691)

![image](https://github.com/hassan418/TeltonikaSolution/assets/20794109/92337a25-3c26-404d-8bc1-a70dd379e048)

# Create a Database and provide the same database connection in app settings and run this script on the database or Create a Table Manually or add a migration process in code.
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


# Now Run the Webjobs with multiple startup projects setting

![image](https://github.com/hassan418/TeltonikaSolution/assets/20794109/fa597a18-5491-4343-ad67-588f9e8adbde)

# Here is Results

![image](https://github.com/hassan418/TeltonikaSolution/assets/20794109/19b46c45-fe38-4123-883a-e9578aeae99e)


