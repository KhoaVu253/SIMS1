-- Migration: Add FacultyId to CourseSchedules
-- Step 1: Add column as nullable first
ALTER TABLE [CourseSchedules] ADD [FacultyId] int NULL;

-- Step 2: Update existing records
-- Set FacultyId from Course.FacultyId if exists, otherwise set to first active faculty
UPDATE cs
SET cs.FacultyId = COALESCE(
    c.FacultyId, 
    (SELECT TOP 1 Id FROM Faculties WHERE IsActive = 1 ORDER BY Id)
)
FROM [CourseSchedules] cs
INNER JOIN [Courses] c ON cs.CourseId = c.Id;

-- Step 3: Make column NOT NULL
ALTER TABLE [CourseSchedules] ALTER COLUMN [FacultyId] int NOT NULL;

-- Step 4: Add foreign key constraint
ALTER TABLE [CourseSchedules] 
ADD CONSTRAINT [FK_CourseSchedules_Faculties_FacultyId] 
FOREIGN KEY ([FacultyId]) REFERENCES [Faculties] ([Id]) ON DELETE NO ACTION;

-- Step 5: Create index
CREATE INDEX [IX_CourseSchedules_FacultyId] ON [CourseSchedules] ([FacultyId]);

GO
