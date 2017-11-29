# SQL Logger
Implementation of Logging in SQL Server Database for Dot Net Core Applications.

This repository contains Implementation of ASP.NET Core Logger Provider in SQL Server Database. It enables you to log the information in SQL Database table. For general ASP.NET Core logger implementation, visit [here](https://github.com/aspnet/Logging "ASP.NET Core Logging").

### Installation 


**Install** the Daenet.Common.Logging.Sql [NuGet Package](https://www.nuget.org/packages/Daenet.Common.Logging.Sql) in your application.

### Configuration

**Following code block**, shows how to add SqlServerLogger provider to the loggerFactory in Startup class:

```C#
  public IConfigurationRoot Configuration;
  var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
  Configuration = builder.Build();

  ILoggerFactory loggerFactory = new LoggerFactory().AddSqlServerLogger(Configuration.GetSection("SqlLogging"));
  ILogger logger = loggerFactory.CreateLogger<SqlServerLoggerTests>();
  ```

In the ***appsettings.json***, the `SqlProviderSettings` part needs to be added.

```JSON
{
  "Logging": {
    "IncludeScopes": false,
    "Debug": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "SqlProvider": {
      "LogLevel": {
        "Default": "Error"
      }
    },
    "SqlProviderSettings": {
      "ConnectionString": "SQL database connection string",
      "TableName": "Name of the table",
      "CreateTblIfNotExist": false,
      "IncludeExceptionStackTrace": false,
      "IgnoreLoggingErrors": false
    }
  }
}

```

***LogLevel*** configuration are done on global and logger level see [Introduction to Logging in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging?tabs=aspnetcore2x)

***IncludeScope*** flag is used when the logging is to be done inside one particular scope. IncludeScope when set to true, and initialized inside code with *beginScope("scopeStatement")* adds scope id to every logged statement which is added to database under column "Scope".
Following example illustrates using scope in logging when *IncludeScopes* flag is set to true.

```C#
 using (m_Logger.BeginScope($"Scope Begins : {Guid.NewGuid()}"))
 {
   //Here log something
 }
```
This will add "Scope begins : *new hexadecimal guid*" for every object instance of the class where scope is began.

***SqlProviderSettings*** section of the configuration contains SQL Logger specific configurations.


*ConnectionString* is ADO.NET connection string for SQL authentication.

*TableName* is name of the table where logger should log.

*CreateTblIfNotExist* flag when set to true, gives the logger ability to create the table of table name that is provided in configuration in case it is not already available in database.
This flag is useful while testing the logger in development environment.

*IncludeExceptionStackTrace* flag is currently not implemented and complete exception is logged in table regardless of this flag.

*IgnoreLoggingErrors* flag is to decide if the exceptions of logger should be logged somewhere and ignored or should it be thrown further. When set to false, the errors occurring inside SQL logger are logged in another logger. are  At the moment this configuration is not implemented completely and when set to false, the logger at the moment logs these errors in debug console.

### Database Configuration

To log the error in SQL database, A table must be present in database with predefined column format. Following columns are filled by logger - 

* Id     -   Unique id of each entry, automatically incremented
* EventId  - Event Id of each log. 
* Type - Log level
* Scope - Scope information if scoping is enabled and used
* Message - Log message
* Exception - Complete exception in case of error
* TimeStamp - Timestamp of log
* CategoryName - Namespace from where the log is logged


Following query will create a new table in SQL Database with above parameters, and should be used as a reference query for default format.

```SQL
CREATE TABLE [dbo].[YourTableName](
       [Id] [bigint] IDENTITY(1,1) NOT NULL,
       [EventId] [int] NULL,
       [Type] [nvarchar](15) NOT NULL,
       [Scope] [nvarchar](255) NULL,
       [Message] [nvarchar](max) NOT NULL,
       [Exception] [nvarchar](max) NULL,
       [TimeStamp] [datetime] NOT NULL,
       [CategoryName] [nvarchar] (max) NULL,
CONSTRAINT [PK_YourTableName] PRIMARY KEY CLUSTERED 
(
       [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
```

### Custom SQL Logging Format

It is also possible to use your own custom logging format by providing loggingFormatter
