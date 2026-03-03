using System.Data.Entity;
using ContosoUniversity.Models;

namespace ContosoUniversity.Data
{
    public class SchoolContext : DbContext
    {
        public SchoolContext(string connectionString) : base(connectionString)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configure DateTime properties to use datetime2 for entities without the attribute
            modelBuilder.Entity<Department>()
                .Property(d => d.StartDate)
                .HasColumnType("datetime2");

            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Enrollment>().ToTable("Enrollment");
            modelBuilder.Entity<Department>().ToTable("Department");
            modelBuilder.Entity<OfficeAssignment>().ToTable("OfficeAssignment");
            modelBuilder.Entity<CourseAssignment>().ToTable("CourseAssignment");
            modelBuilder.Entity<Notification>().ToTable("Notification");

            // Configure Table-per-Hierarchy (TPH) inheritance for Person
            modelBuilder.Entity<Person>()
                .ToTable("Person")
                .Map<Student>(m => m.Requires("Discriminator").HasValue("Student"))
                .Map<Instructor>(m => m.Requires("Discriminator").HasValue("Instructor"));

            // Configure composite key for CourseAssignment
            modelBuilder.Entity<CourseAssignment>()
                .HasKey(c => new { c.CourseID, c.InstructorID });

            // Configure relationships
            modelBuilder.Entity<CourseAssignment>()
                .HasRequired(m => m.Course)
                .WithMany(t => t.CourseAssignments)
                .HasForeignKey(m => m.CourseID);

            modelBuilder.Entity<CourseAssignment>()
                .HasRequired(m => m.Instructor)
                .WithMany(t => t.CourseAssignments)
                .HasForeignKey(m => m.InstructorID);

            // Configure one-to-one relationship
            modelBuilder.Entity<Instructor>()
                .HasOptional(s => s.OfficeAssignment)
                .WithRequired(ad => ad.Instructor);
        }
    }
}
