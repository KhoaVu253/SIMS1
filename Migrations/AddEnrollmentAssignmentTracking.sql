-- Migration: Add Assignment Tracking to Enrollment
-- Date: 2024
-- Purpose: Track admin assignments and add semester/year fields

-- Step 1: Add new columns to Enrollments table
ALTER TABLE Enrollments
ADD AssignedByUserId INT NULL,
    AssignedDate DATETIME NULL,
    Notes NVARCHAR(500) NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active';

-- Step 2: Add foreign key constraint
ALTER TABLE Enrollments
ADD CONSTRAINT FK_Enrollments_AssignedByUser
FOREIGN KEY (AssignedByUserId) REFERENCES Users(Id);

-- Step 3: Update existing enrollments (if any)
UPDATE Enrollments
SET Status = 'Active'
WHERE Status IS NULL OR Status = '';

-- Step 4: Set default semester and academic year for existing records
UPDATE Enrollments
SET Semester = 'HK1',
    AcademicYear = '2024-2025'
WHERE Semester IS NULL OR Semester = '';

-- Step 5: Create index for better performance
CREATE INDEX IX_Enrollments_Semester_AcademicYear 
ON Enrollments(Semester, AcademicYear);

CREATE INDEX IX_Enrollments_Status 
ON Enrollments(Status);

CREATE INDEX IX_Enrollments_AssignedByUserId 
ON Enrollments(AssignedByUserId);

-- Verification queries
SELECT COUNT(*) AS TotalEnrollments FROM Enrollments;
SELECT Status, COUNT(*) AS Count FROM Enrollments GROUP BY Status;
SELECT Semester, AcademicYear, COUNT(*) AS Count FROM Enrollments GROUP BY Semester, AcademicYear;

PRINT 'Migration completed successfully!';
