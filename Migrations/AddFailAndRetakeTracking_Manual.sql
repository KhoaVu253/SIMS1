-- Migration: Add Fail and Retake Tracking to Enrollments
-- Date: 2024-12-06
-- Description: Thêm các cột để tracking môn trượt và học lại

USE [SIMS];
GO

-- Step 1: Thêm các cột mới
ALTER TABLE [Enrollments] 
ADD [IsFailed] BIT NOT NULL DEFAULT 0,
    [IsRetaking] BIT NOT NULL DEFAULT 0,
    [OriginalEnrollmentId] INT NULL,
    [RetakeCount] INT NOT NULL DEFAULT 0;
GO

-- Step 2: Thêm foreign key constraint cho OriginalEnrollmentId (self-referencing)
ALTER TABLE [Enrollments]
ADD CONSTRAINT [FK_Enrollments_OriginalEnrollment]
FOREIGN KEY ([OriginalEnrollmentId]) 
REFERENCES [Enrollments]([Id])
ON DELETE NO ACTION;
GO

-- Step 3: Tạo index để query nhanh
CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId_IsFailed_Status]
ON [Enrollments] ([StudentId], [IsFailed], [Status])
INCLUDE ([CourseId], [AverageScore]);
GO

CREATE NONCLUSTERED INDEX [IX_Enrollments_StudentId_IsRetaking_OriginalEnrollmentId]
ON [Enrollments] ([StudentId], [IsRetaking], [OriginalEnrollmentId]);
GO

-- Step 4: Cập nhật dữ liệu hiện có - Đánh dấu các môn trượt
UPDATE [Enrollments]
SET [Status] = 'Failed',
    [IsFailed] = 1
WHERE [AverageScore] IS NOT NULL
  AND [AverageScore] < 5.0
  AND [Status] IN ('Completed', 'Active');
GO

-- Step 5: Cập nhật dữ liệu hiện có - Đánh dấu các môn đạt
UPDATE [Enrollments]
SET [Status] = 'Completed',
    [IsFailed] = 0
WHERE [AverageScore] IS NOT NULL
  AND [AverageScore] >= 5.0
  AND [Status] IN ('Completed', 'Active');
GO

-- Step 6: Cập nhật LetterGrade cho các môn trượt
UPDATE [Enrollments]
SET [LetterGrade] = 'F'
WHERE [IsFailed] = 1
  AND ([LetterGrade] IS NULL OR [LetterGrade] != 'F');
GO

PRINT 'Migration completed: AddFailAndRetakeTracking';
GO
