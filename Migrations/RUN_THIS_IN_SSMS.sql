-- ============================================
-- MANUAL MIGRATION: Add Fail/Retake Tracking
-- Run this in SQL Server Management Studio (SSMS)
-- ============================================

USE [SIMS];
GO

PRINT 'Starting migration...';
GO

-- Step 1: Add new columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'IsFailed')
BEGIN
    ALTER TABLE [Enrollments] ADD [IsFailed] BIT NOT NULL DEFAULT 0;
    PRINT '✓ Added IsFailed column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'IsRetaking')
BEGIN
    ALTER TABLE [Enrollments] ADD [IsRetaking] BIT NOT NULL DEFAULT 0;
    PRINT '✓ Added IsRetaking column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'OriginalEnrollmentId')
BEGIN
    ALTER TABLE [Enrollments] ADD [OriginalEnrollmentId] INT NULL;
    PRINT '✓ Added OriginalEnrollmentId column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Enrollments]') AND name = 'RetakeCount')
BEGIN
    ALTER TABLE [Enrollments] ADD [RetakeCount] INT NOT NULL DEFAULT 0;
    PRINT '✓ Added RetakeCount column';
END
GO

-- Step 2: Add foreign key for self-referencing
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Enrollments_OriginalEnrollment')
BEGIN
    ALTER TABLE [Enrollments]
    ADD CONSTRAINT [FK_Enrollments_OriginalEnrollment]
    FOREIGN KEY ([OriginalEnrollmentId]) 
    REFERENCES [Enrollments]([Id]);
    PRINT '✓ Added FK_Enrollments_OriginalEnrollment';
END
GO

-- Step 3: Create indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId_IsFailed_Status')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId_IsFailed_Status]
    ON [Enrollments] ([StudentId], [IsFailed], [Status])
    INCLUDE ([CourseId], [AverageScore]);
    PRINT '✓ Created index IX_Enrollments_StudentId_IsFailed_Status';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId_IsRetaking_OriginalEnrollmentId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId_IsRetaking_OriginalEnrollmentId]
    ON [Enrollments] ([StudentId], [IsRetaking], [OriginalEnrollmentId]);
    PRINT '✓ Created index IX_Enrollments_StudentId_IsRetaking_OriginalEnrollmentId';
END
GO

-- Step 4: Update existing data - Mark failed courses
UPDATE [Enrollments]
SET [Status] = 'Failed',
    [IsFailed] = 1,
    [LetterGrade] = 'F'
WHERE [AverageScore] IS NOT NULL
  AND [AverageScore] < 5.0
  AND [Status] IN ('Completed', 'Active');

PRINT '✓ Marked failed courses';
GO

-- Step 5: Update existing data - Mark passed courses  
UPDATE [Enrollments]
SET [Status] = 'Completed',
    [IsFailed] = 0
WHERE [AverageScore] IS NOT NULL
  AND [AverageScore] >= 5.0
  AND [Status] IN ('Completed', 'Active');

PRINT '✓ Marked passed courses';
GO

-- Step 6: Show summary
SELECT 
    'MIGRATION COMPLETE' AS [Status],
    (SELECT COUNT(*) FROM [Enrollments] WHERE [IsFailed] = 1) AS [Failed_Courses],
    (SELECT COUNT(*) FROM [Enrollments] WHERE [Status] = 'Completed' AND [IsFailed] = 0) AS [Passed_Courses],
    (SELECT COUNT(*) FROM [Enrollments] WHERE [Status] = 'Active') AS [Active_Courses];
GO

PRINT '========================================';
PRINT 'MIGRATION SUCCESSFUL!';
PRINT 'You can now run the application.';
PRINT '========================================';
GO
