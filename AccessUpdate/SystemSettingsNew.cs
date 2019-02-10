using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessUpdate
{
    public class SystemSettingsNew
    {
        public string SystemName;
        //SQL part
        public string SQLConnection;
        public string DatabaseToUse;
        public string BusinessUnitTableName;
        public string ProjectTableName;
        public string MBUTableName;
        public string DatabaseProd;
        public bool OneDB;
        public bool ExternalID;
        public bool ExtendedContext;
        public string EntityTypeNameFBU;
        public string EntityTypeNameMBU;
        public string EntityTypeNameProject;
        //Form part
        public string DataGridViewToUpdate;
        public string labelRowCount;
        public string textBoxResult;
        public List<string> ColumnsToHide;
        //Post part
        public string DataSource;
        public string Uri;
        public string SystemUrl;
        public string InitialCatalog;
        public string ContentTypeJson;
        public string ContentTypeXml;
        public Boolean AuthDefault;
        public string textBoxCookie;
        public string AccessUpdateSavePrincipal;
        public string AccessUpdateRemovePrincipal;
        public string AccessUpdateGetCurrentPrincipal;
        public string LabelRunAs;
        /*
        public SystemSettingsAbstract()
        {
            ContentTypeJson = "application/json;  charset=UTF-8";
            ContentTypeXml = "text/xml; charset=utf-8";
            AccessUpdateSavePrincipal = "SavePrincipal";
            AccessUpdateRemovePrincipal = "RemovePrincipal";
            AccessUpdateGetCurrentPrincipal = "GetCurrentPrincipal";
            ColumnsToHide = new List<string> { "PrincipalId", "PrincipalName", "EmployeeId", "RoleId", "FBUID", "ExternalId" };
            ExtendedContext = false;
            ProjectTableName = "BusinessUnit";
            MBUTableName = "BusinessUnit";
            EntityTypeNameFBU = "BusinessUnit";
            EntityTypeNameMBU = "ManagementUnit";
            EntityTypeNameProject = "SeProject";
            ExternalID = true;
            OneDB = true;
        }
        */
    }

}
