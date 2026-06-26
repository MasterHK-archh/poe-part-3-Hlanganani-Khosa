CREATE TABLE [dbo].[Tasks] 
( 
    [TaskID] INT IDENTITY(1,1) PRIMARY KEY, 
    [Title] VARCHAR(100) NOT NULL, 
    [Description] VARCHAR(255) NULL,
    [DueDate] VARCHAR(50) NULL,
    [ReminderDate] DATETIME NULL,
    [Status] VARCHAR(20) NULL,
    [CreatedDate] DATETIME DEFAULT GETDATE()
); 

CREATE TABLE [dbo].[ActivityLog]
(
    [LogID] INT IDENTITY(1,1) PRIMARY KEY,
    [Username] VARCHAR(100) NOT NULL,
    [Action] VARCHAR(255) NOT NULL,
    [Details] VARCHAR(500) NULL,
    [ActionType] VARCHAR(50) NULL,
    [Timestamp] DATETIME DEFAULT GETDATE()
);

CREATE INDEX IX_Tasks_Status ON [dbo].[Tasks]([Status]);
CREATE INDEX IX_Tasks_DueDate ON [dbo].[Tasks]([DueDate]);
CREATE INDEX IX_ActivityLog_Username ON [dbo].[ActivityLog]([Username]);
CREATE INDEX IX_ActivityLog_Timestamp ON [dbo].[ActivityLog]([Timestamp]);