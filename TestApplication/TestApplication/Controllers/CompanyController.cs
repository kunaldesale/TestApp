using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Protocols;
using TestApplication;
using TestApplication.Helper;

namespace TestApplication.Controllers
{
    public class CompanyController : Controller
    {
        private TestAppDbEntities db = new TestAppDbEntities();

        // GET: Company
        public ActionResult Index()
        {
            string summaryFileName;
            if (TempData.ContainsKey("SummaryFileName"))
               summaryFileName = TempData["SummaryFileName"].ToString();
            return View(db.CompanyDetails.ToList());
        }

        // GET: Company/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CompanyDetail companyDetail = db.CompanyDetails.Find(id);
            if (companyDetail == null)
            {
                return HttpNotFound();
            }
            return View(companyDetail);
        }

        // GET: Company/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Company/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CompanyName,GSTIN,StartDate,EndDate,TurnOverAmount,Email,ContactNumber")] CompanyDetail companyDetail)
        {
            if (ModelState.IsValid)
            {
                db.CompanyDetails.Add(companyDetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(companyDetail);
        }

        // GET: Company/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CompanyDetail companyDetail = db.CompanyDetails.Find(id);
            if (companyDetail == null)
            {
                return HttpNotFound();
            }
            return View(companyDetail);
        }

        // POST: Company/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CompanyName,GSTIN,StartDate,EndDate,TurnOverAmount,Email,ContactNumber")] CompanyDetail companyDetail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(companyDetail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(companyDetail);
        }

        // GET: Company/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CompanyDetail companyDetail = db.CompanyDetails.Find(id);
            if (companyDetail == null)
            {
                return HttpNotFound();
            }
            return View(companyDetail);
        }

        // POST: Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CompanyDetail companyDetail = db.CompanyDetails.Find(id);
            db.CompanyDetails.Remove(companyDetail);
            db.SaveChanges();
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


        [HttpPost]
        public ActionResult uploadparamatercsv()
        {
            var httpPostedFile = Request.Files[0];
            var dataInSummary = false;
            IList<CompanyDetail> companyDetails = new List<CompanyDetail>();
            if (httpPostedFile != null)
            {
                DataTable dt = new DataTable();
                var fileName = Path.GetFileName(httpPostedFile.FileName);
                var filepath = Server.MapPath("~/App_Data/Uploadroutcsv");
                if (!System.IO.Directory.Exists(filepath))
                {
                    System.IO.Directory.CreateDirectory(filepath);
                }
                string path = Path.Combine(Server.MapPath("~/App_Data/Uploadroutcsv"), fileName);
                try
                {
                    httpPostedFile.SaveAs(path);
                    UploadCompany objcsv = new UploadCompany();
                    dt = objcsv.processcsv(path);
                    var companycount = 0;
                    if (dt != null)
                    {
                        companyDetails = objcsv.processbulkcsv(dt);
                        companycount = companyDetails.Count;

                        var summaryFileName = "UploadingSummary_" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + DateTime.Now.Second + ".txt";
                        var uploadfilepath = Server.MapPath("~/App_Data/UploadSummary");
                        if (!System.IO.Directory.Exists(uploadfilepath))
                        {
                            System.IO.Directory.CreateDirectory(uploadfilepath);
                        }
                        string uplpath = Path.Combine(Server.MapPath("~/App_Data/UploadSummary"), summaryFileName);

                        if (companycount != 0)
                        {
                            for (int i = 0; i < companyDetails.Count(); i++)
                            {
                                var SummaryString = "";
                                var isValidEntry = true;
                                //Validate entry
                                #region Validate Entry
                                //GstNo
                                int SrNo = 0;
                                SrNo = i + 1;
                                if (companyDetails[i].GSTIN.Length == 15)
                                {
                                    if (!Regex.Match(companyDetails[i].GSTIN, "^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$").Success)
                                    {
                                        isValidEntry = false;
                                        if(SummaryString=="")
                                        {
                                            SummaryString = "Entry for sheet line " + SrNo + " - " +" GStNo not is required format.";
                                        }
                                        else
                                        {
                                            SummaryString = SummaryString + " , " + " GStNo not is required format.";
                                        }
                                        
                                    }
                                }
                                else
                                {
                                    isValidEntry = false;
                                    if (SummaryString == "")
                                    {
                                        SummaryString = "Entry for Line " + SrNo + " - " + " GStNo length less than 15.";
                                    }
                                    else
                                    {
                                        SummaryString = SummaryString + " , " + " GStNo length less than 15.";
                                    }
                                }

                                //Email
                                bool isEmailValid = IsValidEmailAddress(companyDetails[i].Email);
                                if (isEmailValid == false)
                                {
                                    isValidEntry = false;
                                    if (SummaryString == "")
                                    {
                                        SummaryString = "Entry for Line " + SrNo + " - " + " Email is not valid.";
                                    }
                                    else
                                    {
                                        SummaryString = SummaryString + " , " + " Email is not valid.";
                                    }
                                }

                                //Mobile
                                if (!Regex.Match(companyDetails[i].ContactNumber, @"\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})").Success)
                                {
                                    isValidEntry = false;
                                    if (SummaryString == "")
                                    {
                                        SummaryString = "Entry for Line " + SrNo + " - " + " Contact number is not valid.";
                                    }
                                    else
                                    {
                                        SummaryString = SummaryString + " , " + " Contact number is not valid.";
                                    }
                                }
                                //Start date
                                if(companyDetails[i].StartDate == null)
                                {
                                    isValidEntry = false;
                                    if (SummaryString == "")
                                    {
                                        SummaryString = "Entry for Line " + SrNo + " - " + " Start date not available.";
                                    }
                                    else
                                    {
                                        SummaryString = SummaryString + " , " + " Start date not available.";
                                    }
                                }
                                //end date
                                if (companyDetails[i].EndDate == null)
                                {
                                    isValidEntry = false;
                                    if (SummaryString == "")
                                    {
                                        SummaryString = "Entry for Line " + SrNo + " - " + " End date not available.";
                                    }
                                    else
                                    {
                                        SummaryString = SummaryString + " , " + " End date not available.";
                                    }
                                }


                                //Dublicate check for email,Phone, gst
                                List<CompanyDetail> AllCompanyDtls = db.CompanyDetails.ToList();
                                var emaildub = AllCompanyDtls.Where(x => x.Email == companyDetails[i].Email).FirstOrDefault();
                                if (emaildub != null)
                                {
                                    isValidEntry = false;
                                    if (SummaryString == "")
                                    {
                                        SummaryString = "Entry for Line " + SrNo + " - " + " Email already in use.";
                                    }
                                    else
                                    {
                                        SummaryString = SummaryString + " , " + " Email already in use.";
                                    }
                                }
                                var phonedub = AllCompanyDtls.Where(x => x.ContactNumber == companyDetails[i].ContactNumber).FirstOrDefault();
                                if (phonedub != null)
                                {
                                    isValidEntry = false;
                                    if (SummaryString == "")
                                    {
                                        SummaryString = "Entry for Line " + SrNo + " - " + " Phone number already in use.";
                                    }
                                    else
                                    {
                                        SummaryString = SummaryString + " , " + " Phone number already in use.";
                                    }
                                }
                                var gstdub = AllCompanyDtls.Where(x => x.GSTIN == companyDetails[i].GSTIN).FirstOrDefault();
                                if (gstdub != null)
                                {
                                    isValidEntry = false;
                                    if (SummaryString == "")
                                    {
                                        SummaryString = "Entry for Line " + SrNo + " - " + " GST number already in use.";
                                    }
                                    else
                                    {
                                        SummaryString = SummaryString + " , " + " GST number already in use.";
                                    }
                                }
                                #endregion

                                if(isValidEntry == true)
                                {
                                    db.CompanyDetails.Add(companyDetails[i]);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    dataInSummary = true;
                                    using (StreamWriter stream = new FileInfo(uplpath).AppendText())//ur file location//.AppendText())
                                    {
                                        stream.WriteLine(SummaryString);//display textbox data in notepad
                                    }

                                }
                            }
                            if(dataInSummary == true)
                            {
                                TempData["SummaryFileName"] = summaryFileName;
                            }
                            
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }


        public FileResult GetReport(string fName)
        {
            string uplpath = Path.Combine(Server.MapPath("~/App_Data/UploadSummary"), fName);
            byte[] FileBytes = System.IO.File.ReadAllBytes(uplpath);
            return File(FileBytes, "text/plain");
        }

        private bool IsValidEmailAddress(string emailAddress)
        {
            return new System.ComponentModel.DataAnnotations
                                .EmailAddressAttribute()
                                .IsValid(emailAddress);
        }

    }
}
