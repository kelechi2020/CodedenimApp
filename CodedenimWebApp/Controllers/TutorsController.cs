﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CodedenimWebApp.Models;
using CodedenimWebApp.ViewModels;
using CodeninModel;

namespace CodedenimWebApp.Controllers
{
    public class TutorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tutors
        //public async Task<ActionResult> Index()
        //{
        //    return View(await db.Tutors.ToListAsync());
        //}



        public ActionResult Index(string id, int? courseId)
        {
            var viewModel = new TutorsIndexVm();

            viewModel.Tutors = db.Tutors
                .Include(i => i.Courses.Select(c => c.CourseCategory))
                .OrderBy(i => i.LastName);

            if (id != null)
            {
                ViewBag.TutorId = id;
                viewModel.Courses = viewModel.Tutors.Single(i => i.TutorId == id).Courses;
            }

            if (courseId != null)
            {
                ViewBag.CourseId = courseId.Value;
                // Lazy loading
                //viewModel.Enrollments = viewModel.Courses.Where(
                //    x => x.CourseID == courseID).Single().Enrollments;
                // Explicit loading
                var selectedCourse = viewModel.Courses.Where(x => x.CourseId == courseId).Single();
                db.Entry(selectedCourse).Collection(x => x.Enrollments).Load();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    db.Entry(enrollment).Reference(x => x.Student).Load();
                }

                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }


        // GET: Tutors/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor =   db.Tutors.Include(s => s.Files).SingleOrDefault(s => s.TutorId == id);
           // Tutor tutor = await db.Tutors.FindAsync(id);
            if (tutor == null)
            {
                return HttpNotFound();
            }
            return View(tutor);
        }

        // GET: Tutors/Create
        public ActionResult Create()
        {
            var tutor = new Tutor();
            tutor.Courses = new List<Course>();
            PopulateAssignedCourseData(tutor);
            return View();
        }

        //method to populate the assigned course in the the create view
        private void PopulateAssignedCourseData(Tutor tutor)
        {
            var allCourses = db.Courses;
            var instructorCourses = new HashSet<int>(tutor.Courses.Select(c => c.CourseId));
            var viewModel = new List<AssignedCourses>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourses
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    Assigned = instructorCourses.Contains(course.CourseId)
                });
            }
            ViewBag.Courses = viewModel;
        }


        // POST: Tutors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Tutor tutor, string[] selectedCourses, HttpPostedFileBase upload)
        {
            if (selectedCourses != null)
            {
                tutor.Courses = new List<Course>();
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = db.Courses.Find(int.Parse(course));
                    tutor.Courses.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    var photo = new FilePath
                    {
                        FileName = System.IO.Path.GetFileName(upload.FileName),
                        FileType = FileType.Photo
                    };
                    tutor.FilePaths = new List<FilePath>();
                    tutor.FilePaths.Add(photo);
                }
                db.Tutors.Add(tutor);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(tutor);
        }

        // GET: Tutors/Edit/5
        public async Task<ActionResult> Edit(string id)
        {   
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.FindAsync(id);
            if (tutor == null)
            {
                return HttpNotFound();
            }
            return View(tutor);
        }

        // POST: Tutors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Tutor tutor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tutor).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tutor);
        }

        // GET: Tutors/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor tutor = await db.Tutors.FindAsync(id);
            if (tutor == null)
            {
                return HttpNotFound();
            }
            return View(tutor);
        }

        // POST: Tutors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Tutor tutor = await db.Tutors.FindAsync(id);
            db.Tutors.Remove(tutor);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
