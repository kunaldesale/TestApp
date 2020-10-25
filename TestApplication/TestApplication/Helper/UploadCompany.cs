using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace TestApplication.Helper
{
    public class UploadCompany
    {
        CompanyDetail companyDetail = new CompanyDetail();
        public DataTable processcsv(string fileName)
        {
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
            System.Data.DataRow row;
            var fixcol = 8;

            // work out where we should split on comma, but not in a sentence
            Regex r = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            //Set the filename in to our stream
            StreamReader sr = new StreamReader(fileName);
            line = sr.ReadLine();
            strArray = r.Split(line);
            var count = strArray.Count();
            if (fixcol == count)
            {
                //For each item in the new split array, dynamically builds our Data columns. Save us having to worry about it.
                Array.ForEach(strArray, s => dt.Columns.Add(new DataColumn()));

                //Read each line in the CVS file until it’s empty
                while ((line = sr.ReadLine()) != null)
                {
                    row = dt.NewRow();

                    //add our current value to our data row
                    row.ItemArray = r.Split(line);
                    dt.Rows.Add(row);
                }
                sr.Dispose();
                return dt;
            }
            dt = null;
            return dt;
        }

        #region processbulkcsv
        /// <summary>
        /// Process on bulk csv data.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>userlist</returns>

        public IList<CompanyDetail> processbulkcsv(DataTable dt)
        {
            IList<CompanyDetail> complist = new List<CompanyDetail>();
            int usercount = 0;
            int ucnt = 0;
            usercount = dt.Rows.Count;
            for (var i = 0; i < usercount; i++)
            {
                CompanyDetail objcmpny = companyDetailList(usercount, i, dt, ucnt);

                if (objcmpny != null)
                {
                    complist.Add(objcmpny);
                }
                else
                {
                    complist = null;
                }
            }
            return complist;
        }
        #endregion 


        #region processonuserlist
        public CompanyDetail companyDetailList(int usercount, int i, DataTable dt, int ucnt)
        {
            companyDetail = new CompanyDetail();

            List<string> deptlist = new List<string>();
            List<string> rolelist = new List<string>();

            var srno = Convert.ToString(dt.Rows[i].ItemArray[0]);
            var companyname = Convert.ToString(dt.Rows[i].ItemArray[1]);
            var gstin = Convert.ToString(dt.Rows[i].ItemArray[2]);
            var startdate = Convert.ToString(dt.Rows[i].ItemArray[3]);
            var enddate = Convert.ToString(dt.Rows[i].ItemArray[4]);
            var turnoveramount = Convert.ToString(dt.Rows[i].ItemArray[5]);
            var contactemail = Convert.ToString(dt.Rows[i].ItemArray[6]);
            var contactnumber = Convert.ToString(dt.Rows[i].ItemArray[7]);

            if (companyname.Length > 0){
                string objpc = companyname;// companyname.Remove(0, 1);
                //objpc = objpc.Remove(objpc.Length - 1);
                companyDetail.CompanyName = objpc;
            }else{
                companyDetail.CompanyName = null;
            }

            if (gstin.Length > 0)
            {
                string objpc = gstin;//.Remove(0, 1);
                //objpc = objpc.Remove(objpc.Length - 1);
                companyDetail.GSTIN = objpc;
            }
            else
            {
                companyDetail.GSTIN = null;
            }

            if (startdate.Length > 0)
            {
                string objpc = startdate;//.Remove(0, 1);
                //objpc = objpc.Remove(objpc.Length - 1);
                companyDetail.StartDate = Convert.ToDateTime(objpc);
            }
            else
            {
                companyDetail.StartDate = null;
            }

            if (enddate.Length > 0)
            {
                string objpc = enddate;//.Remove(0, 1);
                //objpc = objpc.Remove(objpc.Length - 1);
                companyDetail.EndDate = Convert.ToDateTime(objpc);
            }
            else
            {
                companyDetail.EndDate = null;
            }

            if (turnoveramount.Length > 0)
            {
                string objpc = turnoveramount.Remove(0, 1);
                objpc = objpc.Remove(objpc.Length - 1);
                companyDetail.TurnOverAmount = Convert.ToDouble(objpc);
            }
            else
            {
                companyDetail.TurnOverAmount = null;
            }

            if (contactemail.Length > 0)
            {
                string objpc = contactemail;//.Remove(0, 1);
                //objpc = objpc.Remove(objpc.Length - 1);
                companyDetail.Email = objpc;
            }
            else
            {
                companyDetail.Email = null;
            }

            if (contactnumber.Length > 0)
            {
                string objpc = contactnumber;//.Remove(0, 1);
                //objpc = objpc.Remove(objpc.Length - 1);
                companyDetail.ContactNumber = objpc;
            }
            else
            {
                companyDetail.ContactNumber = null;
            }
            return companyDetail;
        }
        #endregion processonuserlist


    }
}