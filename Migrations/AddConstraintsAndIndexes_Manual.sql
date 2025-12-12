-- Add Foreign Key and Indexes for Fail/Retake Tracking
-- Run this script manually in SSMS or SQL Server Management Studio

USE [SIMS];
GO

-- Step 1: Add foreign key constraint for OriginalEnrollmentId (if not exists)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Enrollments_OriginalEnrollment')
BEGIN
    ALTER TABLE [Enrollments]
    ADD CONSTRAINT [FK_Enrollments_OriginalEnrollment]
    FOREIGN KEY ([OriginalEnrollmentId]) 
    REFERENCES [Enrollments]([Id])
    ON DELETE NO ACTION;
    
    PRINT 'Foreign key FK_Enrollments_OriginalEnrollment created.';
END
ELSE
BEGIN
    PRINT 'Foreign key FK_Enrollments_OriginalEnrollment already exists.';
END
GO

-- Step 2: Create index for failed courses query (if not exists)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId_IsFailed_Status')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId_IsFailed_Status]
    ON [Enrollments] ([StudentId], [IsFailed], [Status])
    INCLUDE ([CourseId], [AverageScore]);
    
    PRINT 'Index IX_Enrollments_StudentId_IsFailed_Status created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Enrollments_StudentId_IsFailed_Status already exists.';
END
GO

-- Step 3: Create index for retaking courses query (if not exists)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Enrollments_StudentId_IsRetaking_OriginalEnrollmentId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId_IsRetaking_OriginalEnrollmentId]
    ON [Enrollments] ([StudentId], [IsRetaking], [OriginalEnrollmentId]);
    
    PRINT 'Index IX_Enrollments_StudentId_IsRetaking_OriginalEnrollmentId created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Enrollments_StudentId_IsRetaking_OriginalEnrollmentId already exists.';
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
GO

-- Step 5: Update existing data - Mark passed courses
UPDATE [Enrollments]
SET [Status] = 'Completed',
    [IsFailed] = 0
WHERE [AverageScore] IS NOT NULL
  AND [AverageScore] >= 5.0
  AND [Status] IN ('Completed', 'Active');
GO

PRINT 'Migration completed successfully!';
PRINT 'Summary:';
PRINT '- Foreign key for OriginalEnrollmentId added';
PRINT '- Indexes for performance created';
PRINT '- Existing enrollments updated with fail/pass status';
GO
