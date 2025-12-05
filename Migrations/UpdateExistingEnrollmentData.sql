-- Update existing Enrollments with default values
-- Run this after migration completes

USE SIMS_DB; -- Change to your database name
GO

-- Update Status for existing records
UPDATE Enrollments
SET Status = 'Active'
WHERE Status = '' OR Status IS NULL;
GO

-- Update Semester and AcademicYear for existing records
UPDATE Enrollments
SET Semester = 'HK1',
    AcademicYear = '2024-2025'
WHERE Semester = '' OR Semester IS NULL;
GO

-- Verify updates
SELECT 
    COUNT(*) as TotalEnrollments,
    SUM(CASE WHEN Status = 'Active' THEN 1 ELSE 0 END) as ActiveCount,
    SUM(CASE WHEN Semester = 'HK1' THEN 1 ELSE 0 END) as HK1Count
FROM Enrollments;
GO

PRINT 'Data update completed successfully!';
