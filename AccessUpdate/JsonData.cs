using System;
using System.Linq;
using System.Data;

namespace AccessUpdate
{
    public static class JsonData
    {
        public static string GenerateJson(DataTable DataTableCurrentAccess)
        {
            string SODJson = "";
            string SODJsonBody = "";
            DataTable DataTableSODCheck = DataTableCurrentAccess.AsEnumerable().GroupBy(r => new { Col1 = r["EmployeeId"], Col2 = r["RoleId"], Col3 = r["FBUID"] })
            .Select(g => g.First())
            .CopyToDataTable();
            foreach (DataRow row in DataTableSODCheck.Rows)
            {
                if (row["EmployeeId"].ToString() != ""
                    & row["WhatToAdd"].ToString() != "Error"
                    )
                {
                    SODJsonBody = SODJsonBody + @"{""Context"":{""Id"":""" + row["FBUID"].ToString() + @"""},"
                                              + @"""Employee"":{""Id"":""" + row["EmployeeId"].ToString() + @"""},"
                                              + @"""Role"":{""Id"":"""     + row["RoleId"].ToString() + @"""}}";
                    if (DataTableSODCheck.Rows.Count != DataTableSODCheck.Rows.IndexOf(row) + 1)
                    { SODJsonBody = SODJsonBody + ","; }
                }
            }
            SODJson = SODJson + @"{""checkList"":["
                              + SODJsonBody
                              + @"],""includeIcdApprovedExceptions"":true}";

            return SODJson;
        }

        public static string GenerateJsonFBUManager(string BusinessUnit, string Employee, string Role, string TargetSystemEmployeeRoleLinks)
        {
            string SODJson = "";
            SODJson = @"{""businessUnitEmployeeRoleStrict"":"
                      + @"{""BusinessUnit"":{""Id"":""" + BusinessUnit + @"""},"
                      + @"""Employee"":{""Id"":"""      + Employee + @"""},"
                      + @"""Role"":"""                  + Role + @""","
                      + @"""TargetSystemEmployeeRoleLinks"":[]}}";
            return SODJson;
        }

        public static string GenerateJsonFBUManagerDelete(string FBUEmployeeRoleLink)
        {
            string SODJson = "";
            SODJson = @"{""businessUnitEmployeeRoleIdent"":{""Id"":""" + FBUEmployeeRoleLink + @"""}}";
            return SODJson;
        }

        public static string GenerateJsonSEProject(DataRow row)
        {
            string SODJson = "";
            string PM = row["CurrentPMID"].ToString();
            string AM = row["CurrentAMID"].ToString();

            string RoleName = row["Role"].ToString().ToLower();
            if (RoleName == "pm" || RoleName == "project manager")
            {
                PM = row["EmployeeId"].ToString();
            }            
            if (RoleName == "am" || RoleName == "account manager")
            {
                AM = row["EmployeeId"].ToString();
            }

            string ServiceDesriptionId = row["servicesDescriptionId"].ToString();
            string ServiceDesriptionString = @"""ServicesDescription"":{""Id"":""" + ServiceDesriptionId + @"""},";
            if (ServiceDesriptionId == "")
            {
                ServiceDesriptionString = "";
            }

            if (row["projectStartType"].ToString() == "1")
            {
                SODJson = @"{""seProjectStrict"":"
                          + @"{"
                          + @"""ProjectName"":"""               + row["ProjectName"] + @""","
                          + @"""ProjectStartType"":"            + row["projectStartType"] + @","
                          + @"""ProjectPaymentType"":"          + row["projectPaymentType"] + @","
                          + @"""AccountManager"":{""Id"":"""    + AM + @"""},"
                          + @"""FinancialProject"":{""Id"":"""  + row["financialProjectId"] + @"""},"
                          + @"""ProjectManager"":{""Id"":"""    + PM + @"""},"
                          + @"""Code"":"""                      + row["ProjectExist"] + @""","
                          + @"""BusinessUnit"":{""Id"":"""      + row["FBUID"] + @"""},"
                          + @"""StartDate"":"                   + FormatJsonData(row["startDate"]) + @","
                          + @"""EndDate"":"                     + FormatJsonData(row["endDate"]) + @","
                          + @"""PlannedEndDate"":"              + FormatJsonData(row["plannedEndDate"]) + @","
                          + @"""Id"":"""                        + row["ProjectID"] + @""","
                          + @"""Version"":"                     + row["ProjectVersion"] + @""
                          + @"}"
                          + @"}";
            }
            else
            {
                SODJson = @"{""seProjectStrict"":"
                          + @"{"
                          + @"""ProjectName"":"""               + row["ProjectName"] + @""","
                          + @"""Client"":{""Id"":"""            + row["clientId"] + @"""},"
                          + @"""ClientLegalEntity"":{""Id"":""" + row["clientLegalEntityId"] + @"""},"
                          + @"""CompanyLegalEntity"":{""Id"":"""+ row["companyLegalEntityId"] + @"""},"
                          + @"""ProjectStartType"":"            + row["projectStartType"] + @","
                          + @"""Industry"":{""Id"":"""          + row["industryId"] + @"""},"
                          + @"""Contract"":{""Id"":"""          + row["contractId"] + @"""},"
                          + ServiceDesriptionString
                          + @"""ProjectPaymentType"":"          + row["projectPaymentType"] + @","
                          + @"""AccountManager"":{""Id"":"""    + AM + @"""},"
                          + @"""FinancialProject"":{""Id"":"""  + row["financialProjectId"] + @"""},"
                          + @"""ProjectManager"":{""Id"":"""    + PM + @"""},"
                          + @"""Code"":"""                      + row["ProjectExist"] + @""","
                          + @"""BusinessUnit"":{""Id"":"""      + row["FBUID"] + @"""},"
                          + @"""StartDate"":"                   + FormatJsonData(row["startDate"]) + @","
                          + @"""EndDate"":"                     + FormatJsonData(row["endDate"]) + @","
                          + @"""PlannedEndDate"":"              + FormatJsonData(row["plannedEndDate"]) + @","
                          + @"""Id"":"""                        + row["ProjectID"] + @""","
                          + @"""Version"":"                     + row["ProjectVersion"] + @""
                          + @"}"
                          + @"}";
            }
            return SODJson;
        }

        public static string GenerateJsonBillProject(DataRow row)
        {
            string SODJson = "";
            string PM  = row["CurrentPMID"].ToString();
            string AM  = row["CurrentAMID"].ToString();
            string OSR = row["CurrentOSID"].ToString();

            string RoleName = row["Role"].ToString().ToLower();
            if (RoleName == "pm" || RoleName == "project manager")
            {
                PM = row["EmployeeId"].ToString();
            }
            if (RoleName == "am" || RoleName == "account manager")
            {
                AM = row["EmployeeId"].ToString();
            }
            if (RoleName == "osresponsible")
            {
                OSR = row["EmployeeId"].ToString();
            }

            SODJson = @"{""billingProjectStrict"":"
                    + @"{"                    
                    + @"""Client"":{""Id"":"""                  + row["clientId"] + @"""},"
                    + @"""CurrencyForInvoices"":{""Id"":"""     + row["CurrencyForInvoicesId"] + @"""},"
                    + @"""ClientLegalEntity"":{""Id"":"""       + row["clientLegalEntityId"] + @"""},"
                    + @"""Contract"":{""Id"":"""                + row["contractId"] + @"""},"
                    + @"""CompanyLegalEntity"":{""Id"":"""      + row["companyLegalEntityId"] + @"""},"
                    + @"""ProjectPaymentType"":"                + row["ProjectPaymentType"] + @","
                    + @"""AccountManager"":{""Id"":"""          + AM + @"""},"
                    + @"""ProjectName"":"""                     + row["ProjectName"] + @""","
                    + @"""OsResponsible"":{""Id"":"""           + OSR + @"""},"
                    + @"""VendorNumber"":"""                    + row["VendorNumber"] + @""","
                    + @"""TimeUnit"":"                          + row["TimeUnit"] + @","
                    + @"""FinancialProject"":{""Id"":"""        + row["financialProjectId"] + @"""},"
                    + @"""ProjectManager"":{""Id"":"""          + PM + @"""},"
                    + @"""Code"":"""                            + row["ProjectExist"] + @""","
                    + @"""BusinessUnit"":{""Id"":"""            + row["FBUID"] + @"""},"
                    + @"""StartDate"":"                         + FormatJsonData(row["startDate"]) + @","
                    + @"""EndDate"":"                           + FormatJsonData(row["endDate"]) + @","
                    + @"""PlannedEndDate"":"                    + FormatJsonData(row["plannedEndDate"]) + @","
                    + @"""Id"":"""                              + row["ProjectID"] + @""","
                    + @"""Version"":"                           + row["ProjectVersion"] + @""
                    + @"}"
                    + @"}";
            return SODJson;
        }

        private static string FormatJsonData(object DateToEdit)
        {
            string NewString = "";

            if (DateToEdit.ToString() == "")
            {
                NewString = "null";
            }
            else
            {
                DateTime dt = (DateTime)DateToEdit;
                DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan tsInterval = dt.Subtract(dt1970);
                Int32 iSeconds = Convert.ToInt32(tsInterval.TotalSeconds);
                Int64 iMilliseconds = Convert.ToInt64(tsInterval.TotalMilliseconds);

                NewString = @"""/Date(" + iMilliseconds + @")/""";
            }

            return NewString;
        }

    }
}
