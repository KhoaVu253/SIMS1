using Microsoft.EntityFrameworkCore;

namespace SIMS.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure Student
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentCode)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Faculty
            modelBuilder.Entity<Faculty>()
                .HasIndex(f => f.FacultyCode)
                .IsUnique();

            modelBuilder.Entity<Faculty>()
                .HasOne(f => f.User)
                .WithOne(u => u.Faculty)
                .HasForeignKey<Faculty>(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Course
            modelBuilder.Entity<Course>()
                .HasIndex(c => c.CourseCode)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Faculty)
                .WithMany(f => f.Courses)
                .HasForeignKey(c => c.FacultyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.AssignedByUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin123", // In production, use hashed passwords
                    Role = "Admin",
                    IsActive = true
                }
            );

            // Seed Sample Faculty User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 2,
                    Username = "faculty001",
                    Password = "faculty123",
                    Role = "Faculty",
                    IsActive = true
                }
            );

            // Seed Sample Faculty
            modelBuilder.Entity<Faculty>().HasData(
                new Faculty
                {
                    Id = 1,
                    UserId = 2,
                    FacultyCode = "GV001",
                    FullName = "Nguyễn Văn A",
                    Email = "nguyenvana@university.edu.vn",
                    Phone = "0901234567",
                    Department = "Công nghệ thông tin",
                    IsActive = true
                }
            );

            // Seed Sample Student User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 3,
                    Username = "SV001",
                    Password = "student123",
                    Role = "Student",
                    IsActive = true
                }
            );

            // Seed Sample Student
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    Id = 1,
                    UserId = 3,
                    StudentCode = "SV001",
                    FullName = "Trần Thị B",
                    DateOfBirth = new DateTime(2003, 5, 15),
                    Email = "tranthib@student.edu.vn",
                    Phone = "0912345678",
                    Department = "Công nghệ thông tin",
                    ClassName = "CNTT2021A",
                    IsActive = true
                }
            );

            // Seed Sample Courses
            modelBuilder.Entity<Course>().HasData(
                new Course
                {
                    Id = 1,
                    CourseCode = "IT101",
                    CourseName = "Lập trình căn bản",
                    Credits = 3,
                    Department = "Công nghệ thông tin",
                    FacultyId = 1,
                    IsActive = true
                },
                new Course
                {
                    Id = 2,
                    CourseCode = "IT102",
                    CourseName = "Cấu trúc dữ liệu và giải thuật",
                    Credits = 4,
                    Department = "Công nghệ thông tin",
                    FacultyId = 1,
                    IsActive = true
                },
                new Course
                {
                    Id = 3,
                    CourseCode = "IT103",
                    CourseName = "Cơ sở dữ liệu",
                    Credits = 3,
                    Department = "Công nghệ thông tin",
                    FacultyId = 1,
                    IsActive = true
                }
            );
        }
    }
}
