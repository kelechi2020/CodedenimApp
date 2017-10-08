﻿using CodedenimWebApp.Models;
using CodedenimWebApp.ViewModels;
using CodeninModel;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using PagedList;
using PayStack.Net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using CodedenimWebApp.Migrations;

namespace CodedenimWebApp.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Students
        public async Task<ActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var students = from s in _db.Students
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                               || s.FirstName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:  // Name ascending 
                    students = students.OrderBy(s => s.LastName);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(students.ToPagedList(pageNumber, pageSize));

            // return View(await db.Students.ToListAsync());
        }

        public ActionResult DashBoard()
        {
            var userId = User.Identity.GetUserId();
            var email = _db.Students.Where(x => x.StudentId.Equals(userId)).Select(x => x.Email).FirstOrDefault();
           // var studentType = _db.Students.Where(x => x.StudentId.Equals(userId)).Select(x => x.AccountType).FirstOrDefault();
            var paymentRecord = _db.ProfessionalPayments.FirstOrDefault(x => x.UserId.Equals(userId) && x.IsPayed.Equals(true));

            if (User.IsInRole(RoleName.Corper))
            {
                var corperCourses = _db.AssignCourseCategories.Include(x => x.CourseCategory)
                                                               .Include(x => x.Courses)
                                                               .Where(x => x.CourseCategory.StudentType.Equals(RoleName.Corper))
                                                               .ToList();
                return RedirectToAction("Content", "Courses", corperCourses);


            }
            if (User.IsInRole(RoleName.UnderGraduate))
            {
                if (paymentRecord == null)
                {
                    return RedirectToAction("ListCourses", "Courses");
                }
                
                var corperCourses = _db.AssignCourseCategories.Include(x => x.CourseCategory)
                    .Include(x => x.Courses)
                    .Where(x => x.CourseCategory.StudentType.Equals(RoleName.UnderGraduate))
                    .ToList();
                return RedirectToAction("Content", "Courses", corperCourses);
            }
            if (User.IsInRole(RoleName.Student))
            {

            }

            //if (reference != null)
            //{
            //    var testOrLiveSecret = ConfigurationManager.AppSettings["PayStackSecret"];
            //    var api = new PayStackApi(testOrLiveSecret);
            //    // Verifying a transaction
            //    var verifyResponse = api.Transactions.Verify(reference); // auto or supplied when initializing;
            //    if (verifyResponse.Status)
            //    {
            //        /* 
            //           You can save the details from the json object returned above so that the authorization code 
            //           can be used for charging subsequent transactions

            //           // var authCode = verifyResponse.Data.Authorization.AuthorizationCode
            //           // Save 'authCode' for future charges!

            //       */
            //        //var customfieldArray = verifyResponse.Data.Metadata.CustomFields.A

            //        var convertedValues = new List<SelectableEnumItem>();
            //        var valuepair = verifyResponse.Data.Metadata.Where(x => x.Key.Contains("custom")).Select(s => s.Value);

            //        foreach (var item in valuepair)
            //        {
            //            convertedValues = ((JArray)item).Select(x => new SelectableEnumItem
            //            {
            //                key = (string)x["display_name"],
            //                value = (string)x["value"]
            //            }).ToList();
            //        }
            //        //var studentid = _db.Users.Find(id);
            //        var professionalPayment = new ProfessionalPayment()
            //        {
            //            //FeeCategoryId = Convert.ToInt32(verifyResponse.Data.Metadata.CustomFields[3].Value),
            //            ProfessionalWorkerId = convertedValues.Where(x => x.key.Equals("professionalworkerid")).Select(s => s.value).FirstOrDefault(),
            //            PaymentDateTime = DateTime.Now,
            //            Amount = Convert.ToDecimal(convertedValues.Where(x => x.key.Equals("amount")).Select(s => s.value).FirstOrDefault()),
            //            IsPayed = true,
            //            //StudentId = "HAS-201",
            //            AmountPaid = PaymentTypesController.KoboToNaira.ConvertKoboToNaira(verifyResponse.Data.Amount),

            //        };
            //        db.ProfessionalPayments.Add(professionalPayment);
            //        db.SaveChangesAsync();
            //    }
            //    return RedirectToAction("ListCourses", "Courses");
            //}
            //ViewBag.Profile = db.Students.Select(x => x.FileLocation);
            //var courses = db.Courses.ToList();
            ////return RedirectToAction("ListCourses", "Courses");

            ViewBag.UserCourseName = _db.ProfessionalPayments.Where(x => x.IsPayed == true && x.Email.Equals(email))
                .Select(x => x.CoursePayedFor).FirstOrDefault();
            return View(_db.Courses.ToList());
        }

        /// <summary>
        /// the list of courses that a corper has enrolled
        /// </summary>
        /// <returns></returns>
        public ActionResult CorperCourses(int? courseId)
        {
            var userId = User.Identity.GetUserId();
            //   var course = db.db.ProfessionalPayments.Where(x => x.Email.Equals(vUserCourseName)).Select(x => x.CoursePayedFor).FirstOrDefault(),.Where(x => x.Email.Equals(email)).Select(x => x.CoursePayedFor).FirstOrDefault(),
            var email = _db.Students.Where(x => x.StudentId.Equals(userId)).Select(x => x.Email).SingleOrDefault();
            var studentId = _db.Students.Where(x => x.StudentId.Equals(userId)).Select(x => x.StudentId).FirstOrDefault();
            var callUpNumber =_db.Students.Where(x => x.StudentId.Equals(userId)).Select(x => x.CallUpNo).FirstOrDefault();
             
            if ((courseId != null) && (callUpNumber != null))
            {

                var corperEnrolleCourses = new CodeninModel.CorperEnrolledCourses
                {
                    StudentId = studentId,
                    CorperCallUpNumber = callUpNumber,
                    CourseId = (int)courseId,
                };

                _db.CorperEnrolledCourses.Add(corperEnrolleCourses);
                _db.SaveChanges();

            }
            else if (callUpNumber == null)
            {
                return RedirectToAction("Edit","Students");
            }
           
            ViewBag.UserCourseName = _db.CorperEnrolledCourses.Where(x => x.StudentId.Equals(userId)).Select(x => x.CourseId);
          //  ViewBag.CourseId = db.Courses.Where(x => x.CourseName.Equals(UserCourseName)).Select(x => x.CourseId);
            //var userCourseDetail = new List<UserCourseDetail>
            //{
            //   UserCourseName = db.ProfessionalPayments.Where(x =>  x.Email.Equals(email)).Select(x => x.CoursePayedFor).FirstOrDefault(),
            //   CourseId = db.Courses.Where(x => x.CourseName.Equals(UserCourseName)).Select(x => x.CourseId )
            //};

            return View();

        }

        public ActionResult CorperDashboard()
        {
            var userId = User.Identity.GetUserId();
            var email = _db.Students.Where(x => x.StudentId.Equals(userId)).Select(x => x.Email).SingleOrDefault();
            ViewBag.CorperCourse = _db.ProfessionalPayments.Where(x => x.Email.Equals(email))
                .Select(x => x.CoursePayedFor).FirstOrDefault();
          
            return View(_db.Courses.ToList());
        }

        /// <summary>
        /// CorperDashboard1 is the First Dashboard 
        /// that a corper see when they login
        /// can view Courses based on Categories,
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> CorperDashboard1()
        {
            var categories = await  _db.CourseCategories.ToListAsync();

            var corperDashboard1 = new CorperDashboard1Vm
            {
                CourseCategories = categories,
            };
            return View(corperDashboard1);
        }

       

        // GET: Students/Details/5

        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                id = User.Identity.GetUserId();
            }
            Student student = await _db.Students.FindAsync(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        public async Task<ActionResult> CreateCorper()
        {
            return View();
        }

        public async Task<ActionResult> CreateUnderGrad()
        {
            return View();
        }

        public ActionResult RegularStudent()
        {
            return View();
        }

        // GET: Students/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(Student student)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        db.Students.Add(student);
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }

        //    return View(student);

        //}


        [Authorize]
        public ActionResult AssignedCourseView()
        {
            var student = new Student();
            student.Enrollments = new List<Enrollment>();
            PopulateAssignedCourseData(student);
            return View();
        }





        /// <summary>
        /// Method to map the assigned courses to the student
        /// </summary>
        /// <param name="student"></param>
        private void PopulateAssignedCourseData(Student student)
        {
            var allCourses = _db.Courses;
            var studentCourses = new HashSet<int>(student.Enrollments.Select(c => c.CourseID));
            var viewModel = new List<AssignedCourses>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourses
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    Assigned = studentCourses.Contains(course.CourseId)
                });
            }
            ViewBag.Courses = viewModel;
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student student, HttpPostedFileBase upload)
        {
            try
            {
                string _FileName = String.Empty;
                if (upload.ContentLength > 0)
                {
                    _FileName = Path.GetFileName(upload.FileName);
                    string path = HostingEnvironment.MapPath("~/ProfilePics/") + _FileName;
                    student.FileLocation = path;
                    var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/ProfilePics/"));
                    if (directory.Exists == false)
                    {
                        directory.Create();
                    }
                    upload.SaveAs(path);
                }
                student.FileLocation = _FileName;
                _db.Students.Add(student);
                _db.SaveChanges();
                return RedirectToAction("Index");

            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View();
        }

        // GET: Students/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                id = User.Identity.GetUserId();
            }
            Student student = await _db.Students.FindAsync(id);
            if (student != null)
            {
                var model = new StudentVm
                {
                    StudentId = student.StudentId,
                    AccountType = student.AccountType,
                    CountryOfBirth = student.CountryOfBirth,
                    DateOfBirth = student.DateOfBirth,
                    Discpline = student.Discpline,
                    Passport = student.Passport,
                    Email = student.Email,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    MiddleName = student.MiddleName,
                    TownOfBirth = student.TownOfBirth,
                    Institution = student.Institution,
                    PhoneNumber = student.PhoneNumber,
                    MatricNo = student.MatricNo,
                    CallUpNo = student.CallUpNo

                };
                return View(model);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(StudentVm student)
        {
            if (ModelState.IsValid)
            {
                var model = await _db.Students.FindAsync(student.StudentId);

                if (model != null)
                {
                    model.StudentId = student.StudentId;
                    model.AccountType = student.AccountType;
                    model.CountryOfBirth = student.CountryOfBirth;
                    model.DateOfBirth = student.DateOfBirth;
                    model.Discpline = student.Discpline;
                    model.Passport = student.Passport;
                    model.Email = student.Email;
                    model.FirstName = student.FirstName;
                    model.LastName = student.LastName;
                    model.MiddleName = student.MiddleName;
                    model.Institution = student.Institution;
                    model.EnrollmentDate = DateTime.Now;
                    model.PhoneNumber = student.PhoneNumber;
                    model.StateOfService = student.StateOfService.ToString();
                    model.StateOfOrigin = student.StateOfOrigin.ToString();
                    model.Batch = student.Batch.ToString();
                    model.Title = student.Title.ToString();
                    model.TownOfBirth = student.TownOfBirth;
                    model.Gender = student.Gender.ToString();
                    model.MatricNo = student.MatricNo;
                    model.CallUpNo = student.CallUpNo;

                    _db.Entry(model).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Details");
                }
            }
            return View(student);

        }

        [AllowAnonymous]
        public async Task<ActionResult> RenderImage(string studentId)
        {
            Student student = await _db.Students.FindAsync(studentId);

            byte[] photoBack = student.Passport;

            return File(photoBack, "image/png");
        }


        // GET: Students/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = await _db.Students.FindAsync(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Student student = await _db.Students.FindAsync(id);
            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class ProfessionalPayment
    {
        public int ProfessionalPaymentId { get; set; }
        public string UserId { get; set; }
        public string ProfessionalWorkerId { get; set; }
        public DateTime PaymentDateTime { get; set; }
        public decimal Amount { get; set; }
        public bool IsPayed { get; set; }
        public object AmountPaid { get; set; }
        public string Email { get; set; }
        public string CoursePayedFor { get; set; }
        public string PaymentDate { get; set; }
        public string PayStackCustomerId { get; set; }
    }

    public class SelectableEnumItem
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}
