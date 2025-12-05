-- SIMS Database Backup Script
-- Generated: December 2025
-- Database: SIMS

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SIMS')
BEGIN
    CREATE DATABASE SIMS;
    PRINT 'Database SIMS created successfully.';
END
ELSE
BEGIN
    PRINT 'Database SIMS already exists.';
END
GO

USE SIMS;
GO

-- =============================================
-- Sample Data Insert Script
-- =============================================

-- Insert Admin User
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Password, Role, IsActive)
    VALUES ('admin', 'admin123', 'Admin', 1);
    PRINT 'Admin user created.';
END

-- Insert Sample Departments Data
-- You can use this to populate departments dropdown

-- Insert Sample Faculty
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'faculty001')
BEGIN
    DECLARE @FacultyUserId INT;
    
    INSERT INTO Users (Username, Password, Role, IsActive)
    VALUES ('faculty001', 'faculty123', 'Faculty', 1);
    
    SET @FacultyUserId = SCOPE_IDENTITY();
    
    INSERT INTO Faculties (UserId, FacultyCode, FullName, Email, Phone, Department, IsActive)
    VALUES (@FacultyUserId, 'GV001', N'Nguyễn Văn A', 'nguyenvana@university.edu.vn', '0901234567', N'Công nghệ thông tin', 1);
    
    PRINT 'Faculty user created.';
END

-- Insert Sample Student
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'SV001')
BEGIN
    DECLARE @StudentUserId INT;
    
    INSERT INTO Users (Username, Password, Role, IsActive)
    VALUES ('SV001', 'student123', 'Student', 1);
    
    SET @StudentUserId = SCOPE_IDENTITY();
    
    INSERT INTO Students (UserId, StudentCode, FullName, DateOfBirth, Email, Phone, Department, ClassName, IsActive)
    VALUES (@StudentUserId, 'SV001', N'Trần Thị B', '2003-05-15', 'tranthib@student.edu.vn', '0912345678', N'Công nghệ thông tin', 'CNTT2021A', 1);
    
    PRINT 'Student user created.';
END

-- Insert Sample Courses
IF NOT EXISTS (SELECT 1 FROM Courses WHERE CourseCode = 'IT101')
BEGIN
    DECLARE @FacultyId INT = (SELECT Id FROM Faculties WHERE FacultyCode = 'GV001');
    
    INSERT INTO Courses (CourseCode, CourseName, Credits, Department, FacultyId, IsActive)
    VALUES 
        ('IT101', N'Lập trình căn bản', 3, N'Công nghệ thông tin', @FacultyId, 1),
        ('IT102', N'Cấu trúc dữ liệu và giải thuật', 4, N'Công nghệ thông tin', @FacultyId, 1),
        ('IT103', N'Cơ sở dữ liệu', 3, N'Công nghệ thông tin', @FacultyId, 1),
        ('IT104', N'Lập trình Web', 3, N'Công nghệ thông tin', @FacultyId, 1),
        ('IT105', N'Mạng máy tính', 3, N'Công nghệ thông tin', @FacultyId, 1);
    
    PRINT 'Sample courses created.';
END

-- =============================================
-- Useful Queries
-- =============================================

-- View all users with roles
SELECT 
    u.Id,
    u.Username,
    u.Role,
    u.IsActive,
    CASE 
        WHEN u.Role = 'Student' THEN s.FullName
        WHEN u.Role = 'Faculty' THEN f.FullName
        ELSE 'Admin'
    END AS FullName
FROM Users u
LEFT JOIN Students s ON u.Id = s.UserId
LEFT JOIN Faculties f ON u.Id = f.UserId
ORDER BY u.Role, u.Username;

-- View enrollment statistics
SELECT 
    c.CourseCode,
    c.CourseName,
    COUNT(e.Id) AS TotalEnrollments,
    f.FullName AS FacultyName
FROM Courses c
LEFT JOIN Enrollments e ON c.Id = e.CourseId
LEFT JOIN Faculties f ON c.FacultyId = f.Id
WHERE c.IsActive = 1
GROUP BY c.CourseCode, c.CourseName, f.FullName
ORDER BY TotalEnrollments DESC;

-- View student grades summary
SELECT 
    s.StudentCode,
    s.FullName,
    c.CourseCode,
    c.CourseName,
    g.MidtermScore,
    g.FinalScore,
    g.TotalScore,
    g.LetterGrade
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
INNER JOIN Courses c ON e.CourseId = c.Id
LEFT JOIN Grades g ON e.Id = g.EnrollmentId
ORDER BY s.StudentCode, c.CourseCode;

-- Calculate GPA for all students
SELECT 
    s.StudentCode,
    s.FullName,
    COUNT(g.Id) AS TotalCourses,
    CAST(AVG(g.TotalScore) AS DECIMAL(4,2)) AS GPA
FROM Students s
INNER JOIN Enrollments e ON s.Id = e.StudentId
LEFT JOIN Grades g ON e.Id = g.EnrollmentId
WHERE g.TotalScore IS NOT NULL
GROUP BY s.StudentCode, s.FullName
ORDER BY GPA DESC;

-- =============================================
-- Maintenance Queries
-- =============================================

-- Reset all passwords to default
/*
UPDATE Users SET Password = '123456';
PRINT 'All passwords reset to default.';
*/

-- Delete all enrollments and grades (USE WITH CAUTION!)
/*
DELETE FROM Grades;
DELETE FROM Enrollments;
PRINT 'All enrollments and grades deleted.';
*/

-- Backup important data
/*
SELECT * INTO Users_Backup FROM Users;
SELECT * INTO Students_Backup FROM Students;
SELECT * INTO Faculties_Backup FROM Faculties;
SELECT * INTO Courses_Backup FROM Courses;
SELECT * INTO Enrollments_Backup FROM Enrollments;
SELECT * INTO Grades_Backup FROM Grades;
PRINT 'Backup completed.';
*/

-- =============================================
-- Performance Optimization
-- =============================================

-- Create additional indexes for better performance
CREATE NONCLUSTERED INDEX IX_Enrollments_Semester_Year 
ON Enrollments(Semester, AcademicYear);

CREATE NONCLUSTERED INDEX IX_Grades_TotalScore 
ON Grades(TotalScore) WHERE TotalScore IS NOT NULL;

PRINT 'Additional indexes created.';

GO

PRINT '===========================================';
PRINT 'SIMS Database Setup Complete!';
PRINT 'You can now run the application.';
PRINT '===========================================';
