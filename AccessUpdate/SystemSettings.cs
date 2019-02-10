using System;
using System.Collections.Generic;
using System.Configuration;

namespace AccessUpdate
{
    public abstract class SystemSettingsAbstract
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

        public SystemSettingsAbstract()
        {
            ContentTypeJson                 = "application/json;  charset=UTF-8";
            ContentTypeXml                  = "text/xml; charset=utf-8";
            AccessUpdateSavePrincipal       = "SavePrincipal";
            AccessUpdateRemovePrincipal     = "RemovePrincipal";
            AccessUpdateGetCurrentPrincipal = "GetCurrentPrincipal";
            ColumnsToHide           = new List<string> { "PrincipalId", "PrincipalName", "EmployeeId", "RoleId", "FBUID", "ExternalId" };
            ExtendedContext         = false;
            ProjectTableName        = "BusinessUnit";
            MBUTableName            = "BusinessUnit";
            EntityTypeNameFBU       = "BusinessUnit";
            EntityTypeNameMBU       = "ManagementUnit";
            EntityTypeNameProject   = "SeProject";
            ExternalID              = true;
            OneDB                   = true;
        }
    }

    public class SystemSettingsED : SystemSettingsAbstract
    {
        public string UriProject;
        public string UriSaveManager;
        public string UriRemoveManager;
        public SystemSettingsED(string SQLConnection_v)
        {
            SystemName              = "ED";
            //SQL part
            SQLConnection           = SQLConnection_v;
            DatabaseToUse           = "EnterpriseDirectories";
            BusinessUnitTableName   = "BusinessUnit";
            MBUTableName            = "ManagementUnit as BusinessUnit";
            //BusinessUnitTableName = "Location as BusinessUnit";
            DatabaseProd            = ConfigurationManager.AppSettings.Get("EDProd");
            //Post part
            DataSource              = SQLConnection.Replace(ConfigurationManager.AppSettings.Get("EDPred"), ConfigurationManager.AppSettings.Get("EDPredApp"));
            DataSource              = DataSource.Replace(ConfigurationManager.AppSettings.Get("EDTest"), ConfigurationManager.AppSettings.Get("EDTestApp"));
            DataSource              = DataSource.Replace(ConfigurationManager.AppSettings.Get("EDoro"), ConfigurationManager.AppSettings.Get("EDoroApp"));
            DataSource              = DataSource.Replace(DatabaseProd, ConfigurationManager.AppSettings.Get("EDProdApp"));
            InitialCatalog          = "EnterpriseDirectories";
            Uri                     = "https://" + DataSource + ".luxoft.com/enterprisedirectories/services/configurationservice/authfacade.svc";
            UriProject              = "https://" + DataSource + ".luxoft.com/" + InitialCatalog + "/Services/EDService/NewJsonFacade.svc/";
            UriRemoveManager        = "https://" + DataSource + ".luxoft.com/" + InitialCatalog + "/Services/EDService/NewJsonFacade.svc/RemoveBusinessUnitEmployeeRole";
            UriSaveManager          = "https://" + DataSource + ".luxoft.com/" + InitialCatalog + "/Services/EDService/NewJsonFacade.svc/SaveBusinessUnitEmployeeRole";
            SystemUrl               = "https://" + DataSource + ".luxoft.com";
            AuthDefault             = true;
            textBoxCookie           = "";
        }
        //only prod with other settings
        public SystemSettingsED()
        {
            SystemName              = "ED";
            //SQL part
            DatabaseProd            = ConfigurationManager.AppSettings.Get("EDProd");
            SQLConnection           = DatabaseProd;
            DatabaseToUse           = "EnterpriseDirectories";
            BusinessUnitTableName   = "BusinessUnit";
            MBUTableName            = "ManagementUnit as BusinessUnit";
            //BusinessUnitTableName = "Location as BusinessUnit";
            //Post part
            DataSource              = SQLConnection.Replace(DatabaseProd, ConfigurationManager.AppSettings.Get("EDProdApp"));
            InitialCatalog          = "EnterpriseDirectories";
            Uri                     = "https://" + DataSource + ".luxoft.com/enterprisedirectories/services/configurationservice/authfacade.svc";
            UriProject              = "https://" + DataSource + ".luxoft.com/" + InitialCatalog + "/Services/EDService/NewJsonFacade.svc/";
            UriRemoveManager        = "https://" + DataSource + ".luxoft.com/" + InitialCatalog + "/Services/EDService/NewJsonFacade.svc/RemoveBusinessUnitEmployeeRole";
            UriSaveManager          = "https://" + DataSource + ".luxoft.com/" + InitialCatalog + "/Services/EDService/NewJsonFacade.svc/SaveBusinessUnitEmployeeRole";
            SystemUrl               = "https://" + DataSource + ".luxoft.com";
            AuthDefault             = true;
            textBoxCookie           = "";
        }
    }

    public class SystemSettingsTRM : SystemSettingsAbstract
    {
        public SystemSettingsTRM(string SQLConnection_v)
        {
            SystemName              = "TRM";
            //SQL part
            SQLConnection           = SQLConnection_v;
            DatabaseToUse           = "TRMSys";
            BusinessUnitTableName   = "[FinancialBusinessUnit] as BusinessUnit";
            //если контекст не фбю, то нужно указать другое имя таблицы контекста
            //но для проектов нужна связка 2х таблиц
            ProjectTableName        = "[SEProject] join [dbo].[Project] as BusinessUnit on SEProject.id = BusinessUnit.id";
            EntityTypeNameFBU       = "FinancialBusinessUnit";
            DatabaseProd            = ConfigurationManager.AppSettings.Get("TRMProd");
            //Post part
            DataSource              = SQLConnection.Replace(ConfigurationManager.AppSettings.Get("TRMPred"), ConfigurationManager.AppSettings.Get("TRMPredApp"));
            DataSource              = DataSource.Replace(ConfigurationManager.AppSettings.Get("TRMTest"), ConfigurationManager.AppSettings.Get("TRMTestApp"));
            DataSource              = DataSource.Replace(DatabaseProd, ConfigurationManager.AppSettings.Get("TRMProdApp"));
            InitialCatalog          = "TRMSys";
            Uri                     = "https://" + DataSource + ".luxoft.com/TRMSys/services/ConfigurationFacade/AuthorizationXmlFacade.svc";
            SystemUrl               = "https://" + DataSource + ".luxoft.com";
            AuthDefault             = false;
            textBoxCookie           = "textBoxCookie" + SystemName;
        }
        //only prod with other settings
        public SystemSettingsTRM()
        {
            SystemName              = "TRM";
            //SQL part
            DatabaseProd            = ConfigurationManager.AppSettings.Get("TRMProd");
            SQLConnection           = DatabaseProd;
            DatabaseToUse           = "TRMSys";
            BusinessUnitTableName   = "[FinancialBusinessUnit] as BusinessUnit";
            ProjectTableName        = "[SEProject] join [dbo].[Project] as BusinessUnit on SEProject.id = BusinessUnit.id";
            EntityTypeNameFBU       = "FinancialBusinessUnit";
            
            //Post part
            DataSource              = SQLConnection.Replace(DatabaseProd, ConfigurationManager.AppSettings.Get("TRMProdApp"));
            InitialCatalog          = "TRMSys";
            Uri                     = "https://" + DataSource + ".luxoft.com/TRMSys/services/ConfigurationFacade/AuthorizationXmlFacade.svc";
            SystemUrl               = "https://" + DataSource + ".luxoft.com";
            AuthDefault             = false;
            textBoxCookie           = "textBoxCookie" + SystemName;
        }
    }

    public class SystemSettingsInvoicing : SystemSettingsAbstract
    {
        public SystemSettingsInvoicing(string SQLConnection_v)
        {
            SystemName              = "Invoicing";
            //SQL part
            SQLConnection           = SQLConnection_v;
            DatabaseToUse           = "invoicing";
            BusinessUnitTableName   = "BusinessUnit";
            DatabaseProd            = ConfigurationManager.AppSettings.Get("INVProd");
            //Post part
            DataSource              = SQLConnection.Replace(ConfigurationManager.AppSettings.Get("INVPred"), ConfigurationManager.AppSettings.Get("INVPredApp"));
            DataSource              = DataSource.Replace(ConfigurationManager.AppSettings.Get("INVTest"), ConfigurationManager.AppSettings.Get("INVTestApp"));
            DataSource              = DataSource.Replace(DatabaseProd, ConfigurationManager.AppSettings.Get("INVProdApp"));
            InitialCatalog          = "Invoicing";
            Uri                     = "https://" + DataSource + ".luxoft.com/invoicing/services/authservice/authfacade.svc";
            SystemUrl               = "https://" + DataSource + ".luxoft.com";
            AuthDefault             = true;
            textBoxCookie           = "";
        }
        //only prod with other settings
        public SystemSettingsInvoicing()
        {
            SystemName              = "Invoicing";
            //SQL part
            DatabaseProd            = ConfigurationManager.AppSettings.Get("INVProd");
            SQLConnection           = DatabaseProd;
            DatabaseToUse           = "invoicing";
            BusinessUnitTableName   = "BusinessUnit";
            //Post part
            DataSource              = SQLConnection.Replace(DatabaseProd, ConfigurationManager.AppSettings.Get("INVProdApp"));
            InitialCatalog          = "Invoicing";
            Uri                     = "https://" + DataSource + ".luxoft.com/invoicing/services/authservice/authfacade.svc";
            SystemUrl               = "https://" + DataSource + ".luxoft.com";
            AuthDefault             = true;
            textBoxCookie           = "";
        }
    }

    public class SystemSettingsEDFBUManager : SystemSettingsED
    {
        public SystemSettingsEDFBUManager(string SQLConnection_v) : base(SQLConnection_v)
        {
            SystemName              = "EDFBUManager";
            DataGridViewToUpdate    = "dataGridViewFBUManager";
            labelRowCount           = "labelRowCountFBUManager";

            ColumnsToHide.Add("RoleExist");
        }
    }

    public class SystemSettingsEDPM : SystemSettingsED
    {
        public SystemSettingsEDPM(string SQLConnection_v) : base(SQLConnection_v)
        {
            SystemName              = "EDPM";
            DataGridViewToUpdate    = "dataGridViewPM";
            labelRowCount           = "labelPMRowCount";
                        
            ColumnsToHide.Add("CurrentPMID");
            ColumnsToHide.Add("CurrentAMID");
            ColumnsToHide.Add("CurrentPMLogin");
            ColumnsToHide.Add("CurrentAMLogin");
            ColumnsToHide.Add("ProjectID");
            ColumnsToHide.Add("RoleExist");
            ColumnsToHide.Add("FBU");
            ColumnsToHide.Add("FBUExist");
            ColumnsToHide.Add("ProjectName");
            ColumnsToHide.Add("projectStartType");
            ColumnsToHide.Add("projectPaymentType");
            ColumnsToHide.Add("financialProjectId");
            ColumnsToHide.Add("startDate");
            ColumnsToHide.Add("endDate");
            ColumnsToHide.Add("plannedEndDate");
            ColumnsToHide.Add("ProjectID");
            ColumnsToHide.Add("ProjectVersion");
            ColumnsToHide.Add("clientId");
            ColumnsToHide.Add("clientLegalEntityId");
            ColumnsToHide.Add("companyLegalEntityId");
            ColumnsToHide.Add("industryId");
            ColumnsToHide.Add("contractId");
            ColumnsToHide.Add("servicesDescriptionId");
            ColumnsToHide.Add("CurrencyForInvoicesId");
            ColumnsToHide.Add("VendorNumber");
            ColumnsToHide.Add("CurrentOSID");
            ColumnsToHide.Add("CurrentOSLogin");
            ColumnsToHide.Add("TimeUnit");
        }
    }

    public class SystemSettingsClearDuplicatesED : SystemSettingsED
    {
        public SystemSettingsClearDuplicatesED(string SQLConnection_v) : base(SQLConnection_v)
        {
            SystemName = "ClearDuplicates";
            DataGridViewToUpdate = "dataGridViewClearDuplicates";
            labelRowCount = "labelRowCountClearDuplicates";
            
            ColumnsToHide.Add("RoleExist");                                    
            ColumnsToHide.Add("SODCheck");
            ColumnsToHide.Add("EmployeeExist");
            ColumnsToHide.Add("FBUExist");            
            ColumnsToHide.Add("PermissionRoleId");
            ColumnsToHide.Add("PrincipalId");
            ColumnsToHide.Add("EmployeeId");
            ColumnsToHide.Add("RoleId");
            ColumnsToHide.Add("PermissionId");
            ColumnsToHide.Add("EntityTypeId");
            ColumnsToHide.Add("SecurityEntityId");
            
        }
    }

    public class SystemSettingsClearDuplicatesTRM : SystemSettingsTRM
    {
        public SystemSettingsClearDuplicatesTRM(string SQLConnection_v) : base(SQLConnection_v)
        {
            SystemName = "ClearDuplicates";
            DataGridViewToUpdate = "dataGridViewClearDuplicates";
            labelRowCount = "labelRowCountClearDuplicates";

            ColumnsToHide.Add("RoleExist");
            ColumnsToHide.Add("SODCheck");
            ColumnsToHide.Add("EmployeeExist");
            ColumnsToHide.Add("FBUExist");
            ColumnsToHide.Add("PermissionRoleId");
            ColumnsToHide.Add("PrincipalId");
            ColumnsToHide.Add("EmployeeId");
            ColumnsToHide.Add("RoleId");
            ColumnsToHide.Add("PermissionId");
            ColumnsToHide.Add("EntityTypeId");
            ColumnsToHide.Add("SecurityEntityId");

        }
    }

    public class SystemSettingsClearDuplicatesInvoicing : SystemSettingsInvoicing
    {
        public SystemSettingsClearDuplicatesInvoicing(string SQLConnection_v) : base(SQLConnection_v)
        {
            SystemName = "ClearDuplicates";
            DataGridViewToUpdate = "dataGridViewClearDuplicates";
            labelRowCount = "labelRowCountClearDuplicates";

            ColumnsToHide.Add("RoleExist");
            ColumnsToHide.Add("SODCheck");
            ColumnsToHide.Add("EmployeeExist");
            ColumnsToHide.Add("FBUExist");
            ColumnsToHide.Add("PermissionRoleId");
            ColumnsToHide.Add("PrincipalId");
            ColumnsToHide.Add("EmployeeId");
            ColumnsToHide.Add("RoleId");
            ColumnsToHide.Add("PermissionId");
            ColumnsToHide.Add("EntityTypeId");
            ColumnsToHide.Add("SecurityEntityId");

        }
    }

    public class SystemSettings : SystemSettingsAbstract
    {
        public SystemSettings(string SystemName, string SQLConnection_v)
        {
            SystemSettingsAbstract SystemSettingsData;
            SystemSettingsData = new SystemSettingsED(SQLConnection_v);

            switch (SystemName)
            {
                case "TRM":
                    SystemSettingsData = new SystemSettingsTRM(SQLConnection_v);
                    break;
                case "Invoicing":
                    SystemSettingsData = new SystemSettingsInvoicing(SQLConnection_v);
                    break;
                case "EDFBUManager":
                    SystemSettingsData = new SystemSettingsEDFBUManager(SQLConnection_v);
                    break;
                case "EDPM":
                    SystemSettingsData = new SystemSettingsEDPM(SQLConnection_v);
                    break;
                case "ClearDuplicatesED":
                    SystemSettingsData = new SystemSettingsClearDuplicatesED(SQLConnection_v);
                    break;
                case "ClearDuplicatesTRM":
                    SystemSettingsData = new SystemSettingsClearDuplicatesTRM(SQLConnection_v);
                    break;
                case "ClearDuplicatesInvoicing":
                    SystemSettingsData = new SystemSettingsClearDuplicatesInvoicing(SQLConnection_v);
                    break;
            }

            SystemName = SystemSettingsData.SystemName;

            SQLConnection                   = SystemSettingsData.SQLConnection;
            DatabaseToUse                   = SystemSettingsData.DatabaseToUse;
            BusinessUnitTableName           = SystemSettingsData.BusinessUnitTableName;
            ProjectTableName                = SystemSettingsData.ProjectTableName;
            MBUTableName                    = SystemSettingsData.MBUTableName;
            EntityTypeNameFBU               = SystemSettingsData.EntityTypeNameFBU;
            EntityTypeNameMBU               = SystemSettingsData.EntityTypeNameMBU;
            EntityTypeNameProject           = SystemSettingsData.EntityTypeNameProject;

            DatabaseProd                    = SystemSettingsData.DatabaseProd;
            OneDB                           = SystemSettingsData.OneDB;
            ExternalID                      = SystemSettingsData.ExternalID;
            ExtendedContext                 = SystemSettingsData.ExtendedContext;

            DataGridViewToUpdate            = "dataGridView" + SystemName;
            textBoxResult                   = "textBoxResult" + SystemName;
            labelRowCount                   = "label" + SystemName + "RowCount";
            LabelRunAs                      = "labelRunAs" + SystemName;
            ColumnsToHide                   = SystemSettingsData.ColumnsToHide;

            DataSource                      = SystemSettingsData.DataSource;
            Uri                             = SystemSettingsData.Uri;
            SystemUrl                       = SystemSettingsData.SystemUrl;
            InitialCatalog                  = SystemSettingsData.InitialCatalog;
            ContentTypeJson                 = SystemSettingsData.ContentTypeJson;
            ContentTypeXml                  = SystemSettingsData.ContentTypeXml;
            AuthDefault                     = SystemSettingsData.AuthDefault;
            textBoxCookie                   = SystemSettingsData.textBoxCookie;
            AccessUpdateSavePrincipal       = SystemSettingsData.AccessUpdateSavePrincipal;
            AccessUpdateRemovePrincipal     = SystemSettingsData.AccessUpdateRemovePrincipal;
            AccessUpdateGetCurrentPrincipal = SystemSettingsData.AccessUpdateGetCurrentPrincipal;

        }
    }
}
