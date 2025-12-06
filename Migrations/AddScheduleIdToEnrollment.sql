-- Migration: Add ScheduleId to Enrollment table
-- Date: 2025-12-05
-- Description: Cho phép phân công sinh viên vào lớp/lịch học cụ thể

-- Kiểm tra nếu cột chưa tồn tại thì mới thêm
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[Enrollments]') 
               AND name = 'ScheduleId')
BEGIN
    -- Thêm cột ScheduleId (nullable)
    ALTER TABLE [dbo].[Enrollments]
    ADD [ScheduleId] INT NULL;

    PRINT 'Đã thêm cột ScheduleId vào bảng Enrollments';
END
ELSE
BEGIN
    PRINT 'Cột ScheduleId đã tồn tại';
END
GO

-- Tạo Foreign Key constraint
IF NOT EXISTS (SELECT * FROM sys.foreign_keys 
               WHERE object_id = OBJECT_ID(N'[dbo].[FK_Enrollments_CourseSchedules_ScheduleId]') 
               AND parent_object_id = OBJECT_ID(N'[dbo].[Enrollments]'))
BEGIN
    ALTER TABLE [dbo].[Enrollments]
    ADD CONSTRAINT [FK_Enrollments_CourseSchedules_ScheduleId]
    FOREIGN KEY ([ScheduleId])
    REFERENCES [dbo].[CourseSchedules]([Id])
    ON DELETE NO ACTION; -- ✅ FIX: Đổi từ SET NULL sang NO ACTION để tránh cascade cycle

    PRINT 'Đã tạo Foreign Key FK_Enrollments_CourseSchedules_ScheduleId';
END
ELSE
BEGIN
    PRINT 'Foreign Key đã tồn tại';
END
GO

-- Tạo index để tăng tốc query
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE object_id = OBJECT_ID(N'[dbo].[Enrollments]') 
               AND name = N'IX_Enrollments_ScheduleId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Enrollments_ScheduleId]
    ON [dbo].[Enrollments] ([ScheduleId])
    WHERE [ScheduleId] IS NOT NULL;

    PRINT 'Đã tạo index IX_Enrollments_ScheduleId';
END
ELSE
BEGIN
    PRINT 'Index đã tồn tại';
END
GO

PRINT '';
PRINT '✅ Migration completed successfully!';
PRINT 'Enrollments table hiện có thể liên kết với CourseSchedules';
GO
