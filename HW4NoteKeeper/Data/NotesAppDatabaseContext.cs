using HW4NoteKeeperEx2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HW4NoteKeeperEx2.Data
{
    /// <summary>
    /// Database context for the application.
    /// </summary>
    public class NotesAppDatabaseContext : DbContext
    {
        /// <summary>
        /// Represents the note table in the database.
        /// </summary>
        public DbSet<Note> Note { get; set; }

        /// <summary>
        /// Represents the tag table in the database.
        /// </summary>
        public DbSet<Tag> Tag { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NotesAppDatabaseContext"/> class. Ensure the database is created.
        /// </summary>
        /// <param name="options">Entity database options</param>
        public NotesAppDatabaseContext(DbContextOptions<NotesAppDatabaseContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        /// <summary>
        /// On model creating check
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Adding one to many relationship between Note and Tag
            modelBuilder.Entity<Note>().ToTable("Note")
                .HasMany(t => t.Tags)
                .WithOne(n => n.Note)
                .HasForeignKey(n => n.NoteId);

            // Add summary contraints
            modelBuilder.Entity<Note>()
            .Property(n => n.Summary)
            .IsRequired()
            .HasMaxLength(60)
            .HasAnnotation("CheckConstraint", "LEN(Summary) > 0");                      // Cannot see an effect in the database for this code. Attempt is to not allow empty entries for summary into the database.

            // Add details contraints
            modelBuilder.Entity<Note>()
                .Property(n => n.Details)
                .IsRequired()
                .HasMaxLength(1024)
                .HasAnnotation("CheckConstraint", "LEN(Details) > 0");                   // Cannot see an effect in the database for this code. Attempt is to not allow empty entries for details into the database.

            // Add name contraints for tag
            modelBuilder.Entity<Tag>()
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(30);
        }

        /// <summary>
        /// Configures the database context options, includes: logging.
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }
}
