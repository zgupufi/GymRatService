using GymRatService.Common.Models;
using GymRatService.Common.Models.Workout;
using Microsoft.EntityFrameworkCore;
using System;

namespace GymRatService.DAL.Core
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ExerciseCard> Exercises { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
        public DbSet<WorkoutSet> WorkoutSets { get; set; }
    }
}
