﻿using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Security.Claims;
using System.Threading.Tasks;
using CodeninModel;
using CodeninModel.Quiz;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CodedenimWebApp.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            if (String.IsNullOrEmpty(authenticationType))
            {
                var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
                return userIdentity;

            }
            else
            {
                var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
                return userIdentity;
            }
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            // Add custom user claims here

        }


    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<CourseCategory> CourseCategories { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Module> Modules { get; set; }

        public DbSet<Topic> Topics { get; set; }

        public DbSet<TopicMaterialUpload> TopicMaterialUploads { get; set; }
        public DbSet<File> Files { get; set; }

        public System.Data.Entity.DbSet<CodeninModel.Tutor> Tutors { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        public System.Data.Entity.DbSet<CodeninModel.Student> Students { get; set; }

        //public override int SaveChanges()
        //{
        //    try
        //    {
        //        // Your code...
        //        // Could also be before try if you know the exception occurs in SaveChanges

        //        base.SaveChanges();
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        foreach (var eve in e.EntityValidationErrors)
        //        {
        //            Console.WriteLine(
        //                "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
        //                eve.Entry.Entity.GetType().Name, eve.Entry.State);
        //            foreach (var ve in eve.ValidationErrors)
        //            {
        //                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
        //                    ve.PropertyName, ve.ErrorMessage);
        //            }
        //        }
        //        throw;

        //    }
        //    return 0;
        //}
    }
}