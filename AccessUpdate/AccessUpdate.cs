using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Configuration;

namespace AccessUpdate
{
    public partial class AccessUpdate : Form
    {
        public AccessUpdate()
        {
            InitializeComponent();
        }

//////////////////////////////////////////////////////
//Form
        private void Form1_Load(object sender, EventArgs e)
        {
            this.LoadSettings();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
      
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.ExcelFilePath = this.ExcelFilePath.Text;
            Properties.Settings.Default.ExcelFilePathToSave = this.textBoxOutput.Text;
            Properties.Settings.Default.numericUpDownLoginRowNumber = this.numericUpDownLoginCollumnNumber.Value;
            Properties.Settings.Default.numericUpDownRoleRowNumber  = this.numericUpDownRoleCollumnNumber.Value;
            Properties.Settings.Default.numericUpDownFBURowNumber   = this.numericUpDownFBUCollumnNumber.Value;
            Properties.Settings.Default.textBoxCookie = this.textBoxCookie.Text;
            Properties.Settings.Default.Save();
        }

        private void LoadSettings()
        {
            this.ExcelFilePath.Text = Properties.Settings.Default.ExcelFilePath;
            this.textBoxOutput.Text = Properties.Settings.Default.ExcelFilePathToSave;
            if (this.textBoxOutput.Text == "") this.textBoxOutput.Text = "D:\\";
            this.numericUpDownLoginCollumnNumber.Value = Properties.Settings.Default.numericUpDownLoginRowNumber;
            this.numericUpDownRoleCollumnNumber.Value = Properties.Settings.Default.numericUpDownRoleRowNumber;
            this.numericUpDownFBUCollumnNumber.Value = Properties.Settings.Default.numericUpDownFBURowNumber;
            this.Text = this.ExcelFilePath.Text;
            this.numericUpDownCountPFI.Value = 50;

            if (this.numericUpDownLoginCollumnNumber.Value == 0) this.numericUpDownLoginCollumnNumber.Value = 1;
            if (this.numericUpDownRoleCollumnNumber.Value == 0) this.numericUpDownRoleCollumnNumber.Value = 2;
            if (this.numericUpDownFBUCollumnNumber.Value == 0) this.numericUpDownFBUCollumnNumber.Value = 3;
            if (this.numericUpDownSheet.Value == 0) this.numericUpDownSheet.Value = 1;
            if (this.numericUpDownFirstRow.Value == 0) this.numericUpDownFirstRow.Value = 2;

            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            try
            {
                comboBoxEDSQLConnetction.Items.Add(config.AppSettings.Settings["EDPred"].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error with config. file!" + Environment.NewLine + config.FilePath + Environment.NewLine + ex.Message);
                return;
            }
            comboBoxEDSQLConnetction.Items.Add(config.AppSettings.Settings["EDTest"].Value);
            comboBoxEDSQLConnetction.Items.Add(config.AppSettings.Settings["EDoro"].Value);
            comboBoxEDSQLConnetction.Items.Add(config.AppSettings.Settings["EDProd"].Value);
            comboBoxEDSQLConnetction.SelectedIndex = 0;

            comboBoxTRMSQLConnetction.Items.Add(config.AppSettings.Settings["TRMPred"].Value);
            comboBoxTRMSQLConnetction.Items.Add(config.AppSettings.Settings["TRMTest"].Value);
            comboBoxTRMSQLConnetction.Items.Add(config.AppSettings.Settings["TRMProd"].Value);
            comboBoxTRMSQLConnetction.SelectedIndex = 0;
            
            comboBoxInvoicingSQLConnetction.Items.Add(config.AppSettings.Settings["INVPred"].Value);
            comboBoxInvoicingSQLConnetction.Items.Add(config.AppSettings.Settings["INVTest"].Value);
            comboBoxInvoicingSQLConnetction.Items.Add(config.AppSettings.Settings["INVProd"].Value);
            comboBoxInvoicingSQLConnetction.SelectedIndex = 0;

            comboBoxAllrightsConnection.Items.Add(config.AppSettings.Settings["AlRProd"].Value);
            comboBoxAllrightsConnection.Items.Add(config.AppSettings.Settings["AlRPred"].Value);
            comboBoxAllrightsConnection.Items.Add(config.AppSettings.Settings["AlRTest"].Value);
            comboBoxAllrightsConnection.SelectedIndex = 0;

            comboBoxSystemName.Items.Add("ED");
            comboBoxSystemName.Items.Add("TRM");
            comboBoxSystemName.Items.Add("Invoicing");
            comboBoxSystemName.SelectedIndex = 0;

            comboBoxProjectType.Items.Add("SE");
            comboBoxProjectType.Items.Add("Billing");
            comboBoxProjectType.SelectedIndex = 0;

            comboBoxActionType.Items.Add("Access");
            comboBoxActionType.Items.Add("FBU Manager");
            comboBoxActionType.Items.Add("Projects");
            comboBoxActionType.SelectedIndex = 0;

            checkBoxFBUManagerOnlyActiveEmployees.Checked = true;

            radioButtonAccessTypeFBU.Checked = true;

            radioButtonClearDuplicatesED.Checked = true;

            progressBarProjects.Visible   = false;
            progressBarFBUManager.Visible = false;
            
            this.dataGridViewTerminated.Rows.Add(true, "1 Get list of terminated employees ED");
            this.dataGridViewTerminated.Rows.Add(true, "2 Get list of terminated employees TRM");
            this.dataGridViewTerminated.Rows.Add(true, "3 Get list of terminated employees Invoicing");
            this.dataGridViewTerminated.Rows.Add(true, "4 Save list(xls)");

            this.dataGridViewTerminatedToDelete.Rows.Add(true, "1 Delete FBU managers");
            this.dataGridViewTerminatedToDelete.Rows.Add(true, "2 Delete ED principals");
            this.dataGridViewTerminatedToDelete.Rows.Add(true, "3 Delete TRM principals");
            this.dataGridViewTerminatedToDelete.Rows.Add(true, "4 Delete Invoicing principals");

            numericUpDownConTimeout.Value = 300;
            
            SetAccessContext();
            UpdateWebPage();
        }

        private void comboBoxAllrightsConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateWebPage();
        }

        private void comboBoxTRMSQLConnetction_SelectedIndexChanged(object sender, EventArgs e)
        {
            SystemSettingsTRM SystemSettingsData = new SystemSettingsTRM();
            UpdateWebPage();
            int TabNum = 2;
            TabPage currentab = this.tabControlActions.TabPages[TabNum];            
            if (comboBoxTRMSQLConnetction.SelectedItem.ToString() == SystemSettingsData.DatabaseProd)
            {
                currentab.Text = "!!!_" + currentab.Text + "_!!!";
            }
            else
            {
                currentab.Text = currentab.Text.Replace("_!!!", "").Replace("!!!_", "");
            }
        }

        private void comboBoxEDSQLConnetction_SelectedIndexChanged(object sender, EventArgs e)
        {
            SystemSettingsED SystemSettingsData = new SystemSettingsED();
            if (comboBoxEDSQLConnetction.SelectedItem.ToString() == SystemSettingsData.DatabaseProd)
            {
                this.tabControlActions.TabPages[1].Text = "!!!_" + this.tabControlActions.TabPages[1].Text + "_!!!";
                this.tabControlActions.TabPages[4].Text = "!!!_" + this.tabControlActions.TabPages[4].Text + "_!!!";
                this.tabControlActions.TabPages[5].Text = "!!!_" + this.tabControlActions.TabPages[5].Text + "_!!!";
            }
            else
            {
                this.tabControlActions.TabPages[1].Text = this.tabControlActions.TabPages[1].Text.Replace("_!!!", "").Replace("!!!_", "");
                this.tabControlActions.TabPages[4].Text = this.tabControlActions.TabPages[4].Text.Replace("_!!!", "").Replace("!!!_", "");
                this.tabControlActions.TabPages[5].Text = this.tabControlActions.TabPages[5].Text.Replace("_!!!", "").Replace("!!!_", "");
            }

        }

        private void comboBoxInvoicingSQLConnetction_SelectedIndexChanged(object sender, EventArgs e)
        {
            SystemSettingsInvoicing SystemSettingsData = new SystemSettingsInvoicing();
            TabPage currentab = this.tabControlActions.TabPages[3];
            if (comboBoxInvoicingSQLConnetction.SelectedItem.ToString() == SystemSettingsData.DatabaseProd)
            {
                currentab.Text = "!!!_" + currentab.Text + "_!!!";
            }
            else
            {
                currentab.Text = currentab.Text.Replace("_!!!","").Replace("!!!_", "");
            }
        }       

        private void UpdateWebPage()
        {
            try
            {
                Uri AllrightsUri = new Uri("https://" + comboBoxAllrightsConnection.SelectedItem + ".luxoft.com/Pulsar/HtmlClient/#");                
                webBrowserAllrights.Url = AllrightsUri;                
                //webBrowserAllrights.Refresh();

                SystemSettingsTRM SystemSettingsData = new SystemSettingsTRM(comboBoxTRMSQLConnetction.SelectedItem.ToString());
                Uri TRMUri = new Uri(SystemSettingsData.SystemUrl);
                webBrowserTRM.Url = TRMUri;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void buttonGetCookie_Click(object sender, EventArgs e)
        {
            GetCookie();
        }

        private void GetCookie()
        {
            textBoxCookie.Text    = FullWebBrowserCookie.GetCookieInternal(webBrowserAllrights.Url, false);
            textBoxCookieTRM.Text = FullWebBrowserCookie.GetCookieInternal(webBrowserTRM.Url, false);
        }

        //////////////////////////////////////////////////////
        //Access context

        private void radioButtonAccessTypeFBU_CheckedChanged(object sender, EventArgs e)
        {
            SetAccessContext();
        }

        private void radioButtonAccessTypeProject_CheckedChanged(object sender, EventArgs e)
        {
            SetAccessContext();
        }

        private void radioButtonAccessTypeMBU_CheckedChanged(object sender, EventArgs e)
        {
            SetAccessContext();
        }

        private void SetAccessContext()
        {
            string ContextType = "";
            if (radioButtonAccessTypeFBU.Checked == true)
            {
                ContextType = "FBU";
            }
            if (radioButtonAccessTypeProject.Checked == true)
            {
                ContextType = "Project";
            }
            if (radioButtonAccessTypeMBU.Checked == true)
            {
                ContextType = "MBU";
            }
            labelAccessContext.Text = "Access context: " + ContextType;
        }

        ///////////////////////////////////////////////
        //Buttons
        //Excel
        private void button1_UploadFromExcel_Click(object sender, EventArgs e)
        {
            if (dataGridViewSourceData.DataSource == null)
            {
                dataGridViewSourceData.Rows.Clear();
                dataGridViewSourceData.Columns.Clear();
            }
            else
            {
                dataGridViewSourceData.DataSource = null;
            }
            
            dataGridViewSourceData.DataSource = ExcelData.UploadExcelData(ExcelFilePath.Text, (int)numericUpDownSheet.Value, (int)numericUpDownTop.Value, (int)numericUpDownFirstRow.Value
                , (int)numericUpDownLoginCollumnNumber.Value, (int)numericUpDownRoleCollumnNumber.Value, (int)numericUpDownFBUCollumnNumber.Value);

            dataGridViewSourceData.AutoResizeColumns();

            RowCountUpdate();
        }

        private void RowCountUpdate()
        {
            labelTotalExcelRows.Text = "Total rows: " + dataGridViewSourceData.RowCount;
        }

        private void button_OpenFileDialog_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "*.xls;*.xlsx";
            ofd.Filter = "Microsoft Excel (*.xls*)|*.xls*";
            ofd.Title = "Выберите документ Excel";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.ExcelFilePath.Text = ofd.FileName;
            }
            else
            { this.ExcelFilePath.Text = ""; }

            if (ofd.CheckFileExists != true)
            {
                MessageBox.Show("Файл " + ofd.FileName + " не существует!", "", MessageBoxButtons.OK);
                this.ExcelFilePath.Text = "";
            }
        }

        private void UpdateColumnColor(String GridName)
        {
            SystemSettings SystemSettingsData = new SystemSettings(GridName, GetSQLConnectionString(GridName));
            var dataGridViewTemp = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            DataGridView TempdataGridView = (DataGridView)dataGridViewTemp[0];

            foreach (DataGridViewRow row in TempdataGridView.Rows)
            {
                switch (Convert.ToString(row.Cells["WhatToAdd"].Value))
                {
                    case "Access":
                        row.Cells["WhatToAdd"].Style.ForeColor = Color.Green;
                        break;
                    case "Principal and access":
                        row.Cells["WhatToAdd"].Style.ForeColor = Color.Green;
                        break;
                    case "Exist":
                        row.Cells["WhatToAdd"].Style.ForeColor = Color.Blue;
                        break;
                    case "Project closed":
                        row.Cells["WhatToAdd"].Style.ForeColor = Color.Blue;
                        break;
                    case "Error":
                        row.Cells["WhatToAdd"].Style.ForeColor = Color.Red;
                        break;
                }
                if (Convert.ToString(row.Cells["RoleExist"].Value) == "")
                {
                    row.Cells["Role"].Style.ForeColor = Color.Red;
                }
                if (row.Cells["SODCheck"].Value.ToString() == "No conflicts")
                {
                    row.Cells["SODCheck"].Style.ForeColor = Color.Black;
                }
                else { row.Cells["SODCheck"].Style.ForeColor = Color.Red; }
                if (row.Cells["SODCheck"].Value.ToString().Contains("Approved") ==true)
                {
                    row.Cells["SODCheck"].Style.ForeColor = Color.Green;
                }

                string whatToUpdate = "FBU";
                if (this.tabControlActions.SelectedTab.Name == "Settings")
                {
                    if (Convert.ToString(row.Cells["EmployeeExist"].Value) == "")
                    {
                        row.Cells["Login"].Style.ForeColor = Color.Red;
                    }
                }
                
                if (Convert.ToString(row.Cells[whatToUpdate + "Exist"].Value) == "")
                {
                    row.Cells["Context"].Style.ForeColor = Color.Red;
                }

            }
        }

        private void buttonSourceAddRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewSourceData.ColumnCount == 0)
            {
                dataGridViewSourceData.Columns.Add("Login", "Login");
                dataGridViewSourceData.Columns.Add("Role", "Role");
                dataGridViewSourceData.Columns.Add("Context", "Context");
            }

            if (dataGridViewSourceData.DataSource == null)
            {
                dataGridViewSourceData.Rows.Add();
            }
            else
            {
                DataRow newRow = ((System.Data.DataTable)dataGridViewSourceData.DataSource).NewRow();
                ((System.Data.DataTable)dataGridViewSourceData.DataSource).Rows.Add(newRow);
            }
            RowCountUpdate();
        }

        private void buttonSourceCopyRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewSourceData.CurrentCell != null)
            {
                DataGridViewRow row = (DataGridViewRow)dataGridViewSourceData.Rows[dataGridViewSourceData.CurrentCell.RowIndex];
                DataGridViewRow clonedRow = (DataGridViewRow)row.Clone();
                for (Int32 index = 0; index < row.Cells.Count; index++)
                {
                    clonedRow.Cells[index].Value = row.Cells[index].Value;
                }
                
                if (dataGridViewSourceData.DataSource == null)
                {
                    dataGridViewSourceData.Rows.Add(clonedRow);
                }
                else
                {
                    DataRow newRow = ((System.Data.DataTable)dataGridViewSourceData.DataSource).NewRow();
                    for (Int32 index = 0; index < row.Cells.Count; index++)
                    {
                        newRow[index] = row.Cells[index].Value;
                    }
                    ((System.Data.DataTable)dataGridViewSourceData.DataSource).Rows.Add(newRow);
                }
            }
            RowCountUpdate();
        }

        private void buttonSourceDeleteRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewSourceData.CurrentCell != null)
            {
                dataGridViewSourceData.Rows.RemoveAt(dataGridViewSourceData.CurrentCell.RowIndex);
            }
            RowCountUpdate();
        }

        //Color update on event
        private void dataGridViewED_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            UpdateColumnColor("ED");
        }

        private void dataGridViewTRM_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            UpdateColumnColor("TRM");
        }

        private void dataGridViewInvoicing_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            UpdateColumnColor("Invoicing");
        }

        private void dataGridViewFBUManager_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            UpdateColumnColor("EDFBUManager");
        }

        //////////////////////////////////////////////////////
        //Form update grid
        private void buttonUpdateSourceRole_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewSourceData.Rows)
            {
                row.Cells["Role"].Value = comboBoxNewRoleName.Text;
            }
            dataGridViewSourceData.AutoResizeColumns();
        }

        private void buttonUpdateSourceLogin_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewSourceData.Rows)
            {
                row.Cells["Login"].Value = textBoxNewLogin.Text;
            }
            dataGridViewSourceData.AutoResizeColumns();
        }

//////////////////////////////////////////////////////
//Get settings
        private string GetSQLConnectionString(string SystemName)
        {            
            string DataSource = "";
            DataSource = comboBoxEDSQLConnetction.SelectedItem.ToString();

            switch (SystemName)
            {
                case "TRM":
                    DataSource = comboBoxTRMSQLConnetction.SelectedItem.ToString();
                    break;
                case "Invoicing":
                    DataSource = comboBoxInvoicingSQLConnetction.SelectedItem.ToString();
                    break;                
                case "ClearDuplicatesTRM":
                    DataSource = comboBoxTRMSQLConnetction.SelectedItem.ToString();
                    break;
                case "ClearDuplicatesInvoicing":
                    DataSource = comboBoxInvoicingSQLConnetction.SelectedItem.ToString();
                    break;
            }

            return DataSource;
        }

        //////////////////////////////////////////////////////
        //Tabs
        //By script
        //ED 
        private void buttonValidateED_Click(object sender, EventArgs e)
        {
            string SystemName = "ED";
            ValidateData(SystemName, true);
        }
        
        //TRM
        private void buttonValidateTRM_Click(object sender, EventArgs e)
        {
            string SystemName = "TRM";
            ValidateData(SystemName, true);
        }
      
        //Invoicing
        private void buttonValidateInvoicing_Click(object sender, EventArgs e)
        {            
            string SystemName = "Invoicing";
            ValidateData(SystemName, true);
        }
      
 //////////////////////////////////////////////////////
 //Common procedures
        //validation
        private void ValidateData(string SystemName, Boolean ClearResultText)
        {
            string SQLRequestData = GetReplacedSQLRequestLog("Validate", SystemName);
            ExecuteSQLScript(SQLRequestData, SystemName, true, true);

            if (ClearResultText == true)
            {
                SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));
                var textBoxResultTemp = this.Controls.Find(SystemSettingsData.textBoxResult, true);
                TextBox textBoxResult = (TextBox)textBoxResultTemp[0];
                textBoxResult.Text = "";
                textBoxResult.Visible = false;
            }
        }

        //////////////////////////////////////////////////////
        //GetFilterString
        private string GetFilterString()
        {
            string ValidateData = "";
            foreach (DataGridViewRow row in dataGridViewSourceData.Rows)
            {
                //if ((row.Index / 999) == row.Index % 999)
                if ((row.Index / 400) == row.Index % 400)
                {
                    ValidateData = ValidateData + Environment.NewLine + @"insert into #TempAccess (Login, Role, Context)
                                                    values ";
                }
                else
                if (row.Index != 0)
                {
                    ValidateData = ValidateData + ",";
                }
                ValidateData = ValidateData + "('" + row.Cells["Login"].Value + "','" + row.Cells["Role"].Value + "','" + row.Cells["Context"].Value + "')";
            }

            return ValidateData;
        }

        //////////////////////////////////////////////////////
        //compose SQL request text
        private string GetReplacedSQLRequestLog(string ActionType, string SystemName)
        {
            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));
            
            string SQLRequestData       = "";
            string ConditionValue       = "";
            string DatabaseToUse        = SystemSettingsData.DatabaseToUse;

            string ValidateData = GetFilterString();
            
            //выбираем начальный шаблон запроса
            switch (ActionType)
            {
                case "Validate":                    
                    SQLRequestData = SQLQueriesTemplates.SQLValidateRequestTemplate().Replace("#DatabaseToUse", DatabaseToUse);
                    string OnlyActiveEmployees = "";
                    if (checkBoxFBUManagerOnlyActiveEmployees.Checked == true)
                    { OnlyActiveEmployees = "and Employee.active = 1"; }
                    SQLRequestData = SQLRequestData.Replace("#OnlyActiveEmployees", OnlyActiveEmployees);
                    break;
                case "CurrentAccess":
                    SQLRequestData = SQLQueriesTemplates.SQLCurrentAccess().Replace("#DatabaseToUse", DatabaseToUse);
                    SQLRequestData = SQLRequestData.Replace("#SDRequestToUse", textBoxSDRequest.Text);
                    break;
                case "CurrentAccessToDelete":
                    SQLRequestData = SQLQueriesTemplates.SQLCurrentAccessToDelete().Replace("#DatabaseToUse", DatabaseToUse);
                    SQLRequestData = SQLRequestData.Replace("#SDRequestToUse", textBoxSDRequest.Text);
                    break;
                case "CurrentAccessSource":
                    SQLRequestData = SQLQueriesTemplates.SQLCurrentAccessPrincipal().Replace("#DatabaseToUse", DatabaseToUse);
                    if (textBoxPrincipalAccess.Text != "")
                    {
                        ConditionValue = ConditionValue + @" and Login in ('" + textBoxPrincipalAccess.Text + "')";
                    }
                    if (textBoxFBUAccess.Text != "")
                    {
                        //ConditionValue = ConditionValue + @" and BU in ('" + textBoxFBUAccess.Text + "')";
                        ConditionValue = ConditionValue + @" and BU_path like ('%" + textBoxFBUAccess.Text + "%')";
                    }
                    SQLRequestData = SQLRequestData.Replace("#Condition", ConditionValue);
                    break;
                case "CurrentFBUManagerSource":
                    SQLRequestData = SQLQueriesTemplates.SQLCurrentFBUManager().Replace("#DatabaseToUse", DatabaseToUse);
                    if (textBoxPrincipalAccess.Text != "")
                    {
                        ConditionValue = ConditionValue + @" and Login in ('" + textBoxPrincipalAccess.Text + "')";
                    }
                    if (textBoxFBUAccess.Text != "")
                    {
                        //ConditionValue = ConditionValue + @" and BusinessUnit.name in ('" + textBoxFBUAccess.Text + "')";
                        ConditionValue = ConditionValue + @" and BUFullTree.root like ('%" + textBoxFBUAccess.Text + "%')";
                    }
                    SQLRequestData = SQLRequestData.Replace("#Condition", ConditionValue);
                    if (numericUpDownTop.Value > 0)
                    {
                        SQLRequestData = SQLRequestData.Replace("#TopRows", "top "+ numericUpDownTop.Value);
                    } else
                    {
                        SQLRequestData = SQLRequestData.Replace("#TopRows", "");
                    }                    
                    
                    break;
                case "ClearDuplicatesValidate":
                    SQLRequestData = SQLQueriesTemplates.SQLClearDuplicatesValidate().Replace("#DatabaseToUse", DatabaseToUse);
                    break;
                case "ClearDuplicatesToDelete":
                    SQLRequestData = SQLQueriesTemplates.SQLClearDuplicatesToDelete().Replace("#DatabaseToUse", DatabaseToUse);
                    break;
            }
            SQLRequestData = SQLRequestData.Replace("#ValuesToInsert", ValidateData);

            //Access type context
            if (radioButtonAccessTypeFBU.Checked == true)
            {
                SQLRequestData = SQLRequestData.Replace("#BusinessUnitTableName", SystemSettingsData.BusinessUnitTableName);
                SQLRequestData = SQLRequestData.Replace("#EntityTypeName", SystemSettingsData.EntityTypeNameFBU);
            }
            if (radioButtonAccessTypeProject.Checked == true)
            {
                SQLRequestData = SQLRequestData.Replace("#BusinessUnitTableName", SystemSettingsData.ProjectTableName);
                SQLRequestData = SQLRequestData.Replace("#EntityTypeName", SystemSettingsData.EntityTypeNameProject);
                SQLRequestData = SQLRequestData.Replace("BusinessUnit.name", "BusinessUnit.code");
            }
            if (radioButtonAccessTypeMBU.Checked == true)
            {
                SQLRequestData = SQLRequestData.Replace("#BusinessUnitTableName", SystemSettingsData.MBUTableName);
                SQLRequestData = SQLRequestData.Replace("#EntityTypeName", SystemSettingsData.EntityTypeNameMBU);
            }

            //one db. Скрипты отличаются
            if (SystemSettingsData.OneDB == true)
            {
                SQLRequestData = SQLRequestData.Replace("[Authorization].[dbo]", " [auth]");
                SQLRequestData = SQLRequestData.Replace("[AuthorizationAudit].[dbo]", " [authAudit]");
                SQLRequestData = SQLRequestData.Replace("[dbo]", " [app]");
            }
            
            SQLRequestData = SQLRequestData.Replace("#ExternalID", "Principal.ExternalId");
            
            return SQLRequestData;
        }

        //////////////////////////////////////////////////////
        //execute script
        private void ExecuteSQLScript(string SQLRequest, string SystemName, Boolean Validation, Boolean FillFormGrid, Boolean? SourceData = false)
        {
            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));

            var dataGridViewTemp = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            var labelRowCountTemp = this.Controls.Find(SystemSettingsData.labelRowCount, true);
            DataGridView DataGridViewToUpdate = (DataGridView)dataGridViewTemp[0];
            Label labelRowCount = (Label)labelRowCountTemp[0];

            string SQLConnection = SystemSettingsData.SQLConnection;
            string DataSource = SystemSettingsData.DataSource;
            string InitialCatalog = SystemSettingsData.InitialCatalog;

            if (SourceData != false)
            {
                DataGridViewToUpdate = dataGridViewSourceData;
                labelRowCount = new Label();
            }

            DataSet ds = new DataSet();
            SqlConnection con = new SqlConnection("Data Source=" + SQLConnection + ";Initial Catalog=" + InitialCatalog + ";Integrated Security=True");
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = new SqlCommand(SQLRequest, con);
            da.SelectCommand.CommandTimeout = Convert.ToInt32(numericUpDownConTimeout.Value);
            try
            {
                con.Open();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(DataSource + Environment.NewLine + ex.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataSource + Environment.NewLine + ex.Message);
                return;
            }
            da.Fill(ds, "TempToInsert");
            if (FillFormGrid == true)
            {
                DataGridViewToUpdate.DataSource = ds.Tables[0];
            }
            da.Dispose();
            con.Dispose();
            ds.Dispose();

            if (Validation == true)
            {
                DataGridViewToUpdate.AutoResizeColumns();
                foreach (string ColumnToHide in SystemSettingsData.ColumnsToHide)
                {
                    DataGridViewToUpdate.Columns[ColumnToHide].Visible = false;
                }

                labelRowCount.Text = "Total rows: " + DataGridViewToUpdate.RowCount;
                UpdateColumnColor(SystemName);
            }
        }

 //////////////////////////////////////////////////////
 //By post request
        //ED add
        private void buttonEDAddByPost_Click(object sender, EventArgs e)
        {
            AddByPost("ED");
        }

        //ED delete post
        private void buttonEDDeletePost_Click(object sender, EventArgs e)
        {
            DeleteByPost("ED");
        }

        //By post request
        //TRM add
        private void buttonTRMAddByPost_Click(object sender, EventArgs e)
        {
            AddByPost("TRM");
        }

        //trm delete post
        private void buttonTRMDeleteByPost_Click(object sender, EventArgs e)
        {
            DeleteByPost("TRM");
        }

        //By post request
        //invoicing add
        private void buttonInvAddByPostRequst_Click(object sender, EventArgs e)
        {          
            AddByPost("Invoicing");
        }

        //invoicing delete post
        private void buttonInvoicingDeletePost_Click(object sender, EventArgs e)
        {
           DeleteByPost("Invoicing");
        }

        private void AddByPost(string SystemName)
        {
            if (textBoxSDRequest.Text == "")
            {
                MessageBox.Show("SD number is empty! (Source data tab)");
                return;
            }
            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));            
            var dataGridViewTemp                = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            var textBoxResultTemp               = this.Controls.Find(SystemSettingsData.textBoxResult, true);
            DataGridView DataGridViewToUpdate   = (DataGridView)dataGridViewTemp[0];
            TextBox textBoxResult               = (TextBox)textBoxResultTemp[0];
            textBoxResult.Text                  = "";

            string ResponseBody      = "";
            string StringXmlPostData = "";
            GetCookie();//from browser
            string CookieData        = GetCookieFromForm(SystemSettingsData);//from textbox
            //если пользователь запустил RunAs, то выдать права не получится, нужно об этом сообщить
            GetRunAsData(SystemSettingsData);

            string SQLRequest = GetReplacedSQLRequestLog("CurrentAccess", SystemName);
            ExecuteSQLScript(SQLRequest, SystemName, false, true);

            string RequestResult = "";

            DataTable DataTableCurrentAccess = (DataTable)DataGridViewToUpdate.DataSource;
            if (DataTableCurrentAccess != null & DataTableCurrentAccess.Rows.Count > 0)
            {
                DataTable DataTableCurrentAccessPrincipal = DataTableCurrentAccess.AsEnumerable().GroupBy(r => new { Col1 = r["PrincipalId"], Col2 = r["PrincipalName"], Col3 = r["ExternalId"] })
                .Select(g => g.First())
                .CopyToDataTable();
                foreach (DataRow rowPrincipal in DataTableCurrentAccessPrincipal.Rows)
                {
                    StringXmlPostData = XMLData.GenerateXmlPostData(DataTableCurrentAccess, rowPrincipal, numericUpDownCountPFI.Value);
                    ResponseBody = HTTPRequestsData.SendRequest(SystemSettingsData.Uri, StringXmlPostData, SystemSettingsData.ContentTypeXml, SystemSettingsData.AccessUpdateSavePrincipal, SystemSettingsData.AuthDefault, CookieData);
                    //обработка результата запроса
                    RequestResult += XMLData.GetXmlDataByName(ResponseBody, "//s:Fault", "faultstring") + @"
";
                }

                if (RequestResult != "")
                {
                    textBoxResult.Visible = true;
                    textBoxResult.Text = RequestResult;
                }

                ValidateData(SystemName, false);
            }
        }

        private void DeleteByPost(string SystemName)
        {
            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));
            
            var dataGridViewTemp                = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            var textBoxResultTemp               = this.Controls.Find(SystemSettingsData.textBoxResult, true);
            DataGridView DataGridViewToUpdate   = (DataGridView)dataGridViewTemp[0];
            TextBox textBoxResult               = (TextBox)textBoxResultTemp[0];
            textBoxResult.Text                  = "";

            string ResponseBody      = "";
            string StringXmlPostData = "";
            GetCookie();//from browser
            string CookieData        = GetCookieFromForm(SystemSettingsData);
            //если пользователь запустил RunAs, то выдать права не получится, нужно об этом сообщить
            GetRunAsData(SystemSettingsData);
            
            //доступа после проверки может не остаться, но айди и имя принципала все равно нужно,
            //поэтому возьмем данные принципала из валиационного запроса
            ValidateData(SystemName, false);
            DataTable DataTableValidateData = (DataTable)DataGridViewToUpdate.DataSource;
            DataTable DataTableValidateDataPrincipal = DataTableValidateData.AsEnumerable().GroupBy(r => new { Col1 = r["PrincipalId"], Col2 = r["PrincipalName"], Col3 = r["ExternalId"] })
                    .Select(g => g.First())
                    .CopyToDataTable();

            string SQLRequest = GetReplacedSQLRequestLog("CurrentAccessToDelete", SystemName);
            ExecuteSQLScript(SQLRequest, SystemName, false, true);

            string RequestResult = "";

            DataTable DataTableCurrentAccess = (DataTable)DataGridViewToUpdate.DataSource;
            foreach (DataRow rowPrincipal in DataTableValidateDataPrincipal.Rows)
            {
                StringXmlPostData = XMLData.GenerateXmlPostData(DataTableCurrentAccess, rowPrincipal, numericUpDownCountPFI.Value);
                ResponseBody      = HTTPRequestsData.SendRequest(SystemSettingsData.Uri, StringXmlPostData, SystemSettingsData.ContentTypeXml, SystemSettingsData.AccessUpdateSavePrincipal, SystemSettingsData.AuthDefault, CookieData);
                //обработка результата запроса
                RequestResult += XMLData.GetXmlDataByName(ResponseBody, "//s:Fault", "faultstring") + @"
";
            }
            if (RequestResult != "")
            {
                textBoxResult.Visible = true;
                textBoxResult.Text = RequestResult;
            }

            ValidateData(SystemName, false);
        }

        private string GetCookieFromForm(SystemSettings SystemSettingsData)
        {
            string CookieData = "";
            if (SystemSettingsData.textBoxCookie != "")
            {
                var TextBoxTemp = this.Controls.Find(SystemSettingsData.textBoxCookie, true);
                TextBox textBoxCookieTemp = (TextBox)TextBoxTemp[0];
                CookieData = textBoxCookieTemp.Text;
            }
            return CookieData;
        }

        private void GetRunAsData(SystemSettings SystemSettingsData)
        {
            string CookieData = GetCookieFromForm(SystemSettingsData);            
            //если пользователь запустил RunAs, то выдать права не получится, нужно об этом сообщить
            string StringXmlPostData = XMLData.GenerateXmlPostDataGetCurrentPrincipal();
            string ResponseBody = HTTPRequestsData.SendRequest(SystemSettingsData.Uri, StringXmlPostData, SystemSettingsData.ContentTypeXml, SystemSettingsData.AccessUpdateGetCurrentPrincipal, SystemSettingsData.AuthDefault, CookieData);

            string RunAsData = XMLData.GetXmlDataByName(ResponseBody, "//a:RunAs", "a:Name");
            var LabelRunAsTemp = this.Controls.Find(SystemSettingsData.LabelRunAs, true);
            Label LabelRunAs = (Label)LabelRunAsTemp[0];
            LabelRunAs.Text = "";
            if (RunAsData != "")
            {
                LabelRunAs.Text = "Run as: " + RunAsData;
                LabelRunAs.Visible = true;
                LabelRunAs.ForeColor = Color.Red;
            }
        }

//////////////////////////////////////////////////////
//SOD check
        //ED
        private void buttonEDSodCheck_Click(object sender, EventArgs e)
        {            
            SODCheckRequest("ED");
        }

        //SOD check
        //TRM
        private void buttonTRMSODCheck_Click(object sender, EventArgs e)
        {
            SODCheckRequest("TRM");
        }

        //SOD check
        //Invoicing
        private void buttonInvoicingSODCheck_Click(object sender, EventArgs e)
        {
            SODCheckRequest("Invoicing");
        }

        private void SODCheckRequest(string SystemName)
        {
            //update cookie data            
            GetCookie();

            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));
            var dataGridViewTemp = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            DataGridView DataGridViewToUpdate = (DataGridView)dataGridViewTemp[0];
            DataTable DataTableCurrentAccess = (DataTable)DataGridViewToUpdate.DataSource;

            if (DataTableCurrentAccess != null)
            {
                if (DataTableCurrentAccess.Rows.Count > 0)
                {
                    string SODJson = JsonData.GenerateJson(DataTableCurrentAccess);
                    string ResponseBody = SendSODData(DataTableCurrentAccess, SODJson);
                    UpdateFormSODData(ResponseBody, SystemName);
                }
            }
        }
   
        //Common SOD check data
        private string SendSODData(DataTable DataTableCurrentAccess, string PostData)
        {
            //SystemSettingsED SystemSettingsData = new SystemSettingsED(comboBoxEDSQLConnetction.SelectedItem.ToString());
            string DataSource = "" + comboBoxAllrightsConnection.SelectedItem;
            string Uri = "https://" + DataSource + ".luxoft.com/Pulsar/Services/ServiceFacade/JsonFacade.svc/GetRichSODConflictsCheck";
            string ContentType = "application/json;  charset=UTF-8";
            string ResponseBody = "";
            ResponseBody = HTTPRequestsData.SendRequest(Uri, PostData, ContentType, "", false, textBoxCookie.Text);

            return ResponseBody;
        }

        private void UpdateFormSODData(string ResponseBody, string SystemName)
        {
            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));
            var dataGridViewTemp = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            DataGridView DataGridViewToUpdate = (DataGridView)dataGridViewTemp[0];

            if (ResponseBody.IndexOf("{") == 0)
            {
                string jsonPath = "TempJson.json";
                File.WriteAllText(jsonPath, ResponseBody);

                DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(RootObject));
                using (FileStream fs = new FileStream(jsonPath, FileMode.OpenOrCreate))
                {
                    RootObject NewData = (RootObject)jsonFormatter.ReadObject(fs);
                    if (NewData.GetRichSODConflictsCheckResult != null)
                    {
                        foreach (GetRichSODConflictsCheckResult r in NewData.GetRichSODConflictsCheckResult)
                        {
                            for (int i = 0; i < DataGridViewToUpdate.RowCount; i++)
                            {
                                if ((DataGridViewToUpdate.Rows[i].Cells["EmployeeId"]).Value.ToString() == r.Employee.Id
                                    & (DataGridViewToUpdate.Rows[i].Cells["RoleId"]).Value.ToString() == r.Role.Id
                                    & (DataGridViewToUpdate.Rows[i].Cells["FBUID"]).Value.ToString() == r.Context.Id)
                                {
                                    if (r.CheckResult != "")
                                    {
                                        (DataGridViewToUpdate.Rows[i].Cells["SODCheck"]).Value = r.CheckResult;
                                    }
                                    else
                                    {
                                        (DataGridViewToUpdate.Rows[i].Cells["SODCheck"]).Value = "Approved " + r.CheckIcdApprovedResult;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        MessageBox.Show("Error getting SOD check data!" + Environment.NewLine + ResponseBody);
                    }
                }
                DataGridViewToUpdate.AutoResizeColumns();
                UpdateColumnColor(SystemName);
            }
            else
            {
                MessageBox.Show("Error getting SOD check data!" + Environment.NewLine + ResponseBody);
            }
        }

 //////////////////////////////////////////////////////
 //FBU managers
        //FBU managers tab
        private void buttonFBUManagerValidate_Click(object sender, EventArgs e)
        {
            ValidateDataFBU();
        }        

        //Add FBU manager
        private void buttonAddFBUManager_Click(object sender, EventArgs e)
        {
            DataTable DataTableCurrentAccess = (DataTable)dataGridViewEDFBUManager.DataSource;
            if (DataTableCurrentAccess.Rows.Count > 0)
            {
                progressBarFBUManager.Visible   = true;
                progressBarFBUManager.Minimum   = 1;
                progressBarFBUManager.Maximum   = DataTableCurrentAccess.Rows.Count;
                progressBarFBUManager.Value     = 1;
                progressBarFBUManager.Step      = 1;
            }
            SystemSettingsED SystemSettingsData = new SystemSettingsED(comboBoxEDSQLConnetction.SelectedItem.ToString());

            foreach (DataRow row in DataTableCurrentAccess.Rows)
            {
                if (row["WhatToAdd"].ToString() == "Access")
                {
                    string SODJson      = JsonData.GenerateJsonFBUManager(row["FBUId"].ToString(), row["EmployeeId"].ToString(), row["Role"].ToString(), "");
                    string ResponseBody = HTTPRequestsData.SendRequest(SystemSettingsData.UriSaveManager, SODJson, SystemSettingsData.ContentTypeJson);
                }
                progressBarFBUManager.PerformStep();
            }
            if (DataTableCurrentAccess.Rows.Count > 0)
            {
                progressBarFBUManager.Visible = false;
            }

            ValidateDataFBU();
        }

        //delete FBU manager
        private void buttonDeleteFBUManager_Click(object sender, EventArgs e)
        {
            DeleteFBUManager();
            ValidateDataFBU();
        }

        private void DeleteFBUManager()
        {
            DataTable DataTableCurrentAccess = (DataTable)dataGridViewEDFBUManager.DataSource;
            if (DataTableCurrentAccess.Rows.Count > 0)
            {
                progressBarFBUManager.Visible   = true;
                progressBarFBUManager.Minimum   = 1;
                progressBarFBUManager.Maximum   = DataTableCurrentAccess.Rows.Count;
                progressBarFBUManager.Value     = 1;
                progressBarFBUManager.Step      = 1;
            }
            SystemSettingsED SystemSettingsData = new SystemSettingsED(comboBoxEDSQLConnetction.SelectedItem.ToString());

            foreach (DataRow row in DataTableCurrentAccess.Rows)
            {
                if (row["WhatToAdd"].ToString() == "Exist")
                {
                    string SODJson      = JsonData.GenerateJsonFBUManagerDelete(row["RoleId"].ToString());
                    string ResponseBody = HTTPRequestsData.SendRequest(SystemSettingsData.UriRemoveManager, SODJson, SystemSettingsData.ContentTypeJson);
                }
                progressBarFBUManager.PerformStep();
            }
            if (DataTableCurrentAccess.Rows.Count > 0)
            {
                progressBarFBUManager.Visible = false;
            }
        }

        private void ValidateDataFBU()
        {
            string SQLRequestData = SQLQueriesTemplates.SQLValidateFBUManager();
            string ValidateData = GetFilterString();

            SQLRequestData = SQLRequestData.Replace("#ValuesToInsert", ValidateData);

            string OnlyActiveEmployees = "";
            if (checkBoxFBUManagerOnlyActiveEmployees.Checked == true)
            {
                OnlyActiveEmployees = "and Employee.active = 1";
            }
            SQLRequestData = SQLRequestData.Replace("#OnlyActiveEmployees", OnlyActiveEmployees);
            ExecuteSQLScript(SQLRequestData, "EDFBUManager", true, true);
        }

 //////////////////////////////////////////////////////
 //PM 
        private void buttonPMValidate_Click(object sender, EventArgs e)
        {
            ValidateDataPM();
        }

        private void buttonUpdateProject_Click(object sender, EventArgs e)
        {

            if (comboBoxProjectType.SelectedIndex == 0)//SE
            {
                UpdateProject("SE", "SaveSeProject");
            }
            else
            {
                UpdateProject("Bill", "SaveBillingProject");
            }

            ValidateDataPM();
        }

        private void comboBoxProjectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dataGridViewEDPM.DataSource == null)
            {
                dataGridViewEDPM.Rows.Clear();
                dataGridViewEDPM.Columns.Clear();
            }
            else
            {
                dataGridViewEDPM.DataSource = null;
            }
        }        

        private void ValidateDataPM()
        {
            string SQLRequestData = "";
            if (comboBoxProjectType.SelectedIndex == 0)//SE
            {
                SQLRequestData = SQLQueriesTemplates.SQLValidateSEProject();
            }
            else
            {
                SQLRequestData = SQLQueriesTemplates.SQLValidateBillProject();
            }
            string ValidateData = GetFilterString();

            SQLRequestData = SQLRequestData.Replace("#ValuesToInsert", ValidateData);
            string OnlyActiveEmployees = "";
            if (checkBoxFBUManagerOnlyActiveEmployees.Checked == true)
            {
                OnlyActiveEmployees = "and Employee.active = 1";
            }
            SQLRequestData = SQLRequestData.Replace("#OnlyActiveEmployees", OnlyActiveEmployees);
            ExecuteSQLScript(SQLRequestData, "EDPM", true, true);
        }

        private void UpdateProject(string ProjectType, string Method)
        {
            DataTable DataTableCurrentAccess = (DataTable)dataGridViewEDPM.DataSource;
            if (DataTableCurrentAccess == null)
            {
                return;
            }
            if (DataTableCurrentAccess.Rows.Count > 0)
            {
                progressBarProjects.Visible = true;
                progressBarProjects.Minimum = 1;
                progressBarProjects.Maximum = DataTableCurrentAccess.Rows.Count;
                progressBarProjects.Value = 1;
                progressBarProjects.Step = 1;
            }

            SystemSettingsED SystemSettingsData = new SystemSettingsED(comboBoxEDSQLConnetction.SelectedItem.ToString());

            foreach (DataRow row in DataTableCurrentAccess.Rows)
            {
                if (row["WhatToAdd"].ToString() == "Access")
                {
                    string SODJson = "";
                    if (ProjectType == "SE")
                    {
                        SODJson = JsonData.GenerateJsonSEProject(row);
                    }
                    else
                    {
                        SODJson = JsonData.GenerateJsonBillProject(row);
                    }                   
                    
                    string ResponseBody = "";
                    try
                    {
                        ResponseBody = HTTPRequestsData.SendRequest(SystemSettingsData.UriProject + Method, SODJson, SystemSettingsData.ContentTypeJson);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                }
                progressBarProjects.PerformStep();
            }
            if (DataTableCurrentAccess.Rows.Count > 0)
            {
                progressBarProjects.Visible = false;
            }
        }

//////////////////////////////////////////////////////
//Generate data
//Source tab
        private void buttonGenerateCurrentData_Click(object sender, EventArgs e)
        {
            if (textBoxPrincipalAccess.Text != "" || textBoxFBUAccess.Text != "")
            {

                if (dataGridViewSourceData.DataSource == null)
                {
                    dataGridViewSourceData.Rows.Clear();
                    dataGridViewSourceData.Columns.Clear();
                }
                else
                {
                    dataGridViewSourceData.DataSource = null;
                }

                switch (comboBoxActionType.SelectedIndex)
                {
                    case 0: //Access
                        GenerateCurrentAccess();
                        break;
                    case 1: //FBU Manager
                        GenerateCurrentFBUManager();
                        break;
                    case 2: //Projects
                        GenerateCurrentProjectsData();
                        break;
                }
            }
            RowCountUpdate();
        }

        private void GenerateCurrentAccess()
        {
            string SystemName = comboBoxSystemName.SelectedItem.ToString();
            string SQLRequestData = GetReplacedSQLRequestLog("CurrentAccessSource", SystemName);
            ExecuteSQLScript(SQLRequestData, SystemName, false, true, true);
            dataGridViewSourceData.AutoResizeColumns();
        }

        private void GenerateCurrentFBUManager()
        {
            string SystemName = comboBoxSystemName.SelectedItem.ToString();
            string SQLRequestData = GetReplacedSQLRequestLog("CurrentFBUManagerSource", SystemName);
            ExecuteSQLScript(SQLRequestData, SystemName, false, true, true);
            dataGridViewSourceData.AutoResizeColumns();
        }

        private void GenerateCurrentProjectsData()
        {
            string SQLRequestData = "";
            if (comboBoxSystemName.SelectedIndex == 0 || comboBoxSystemName.SelectedIndex == 1)
            {
                SQLRequestData = SQLQueriesTemplates.SQLCurrentSEProjectData();
                if (textBoxPrincipalAccess.Text != "")
                {
                    SQLRequestData = SQLRequestData + " and " + SQLQueriesTemplates.SQLLoginFilterSEProject();
                }
            }
            else
            {
                SQLRequestData = SQLQueriesTemplates.SQLCurrentBillProjectData();
                if (textBoxPrincipalAccess.Text != "")
                {
                    SQLRequestData = SQLRequestData + " and " + SQLQueriesTemplates.SQLLoginFilterBillProject();
                }
            }
                
            if (textBoxFBUAccess.Text != "")
            {
                SQLRequestData = SQLRequestData + @" and BusinessUnit.name in ('" + textBoxFBUAccess.Text + "')";
            }

            SQLRequestData = SQLRequestData.Replace("#LoginType", comboBoxSystemName.SelectedIndex.ToString());
            SQLRequestData = SQLRequestData.Replace("#Login", textBoxPrincipalAccess.Text);

            ExecuteSQLScript(SQLRequestData, "ED", false, true, true);

            dataGridViewSourceData.AutoResizeColumns();
        }

        private void comboBoxActionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxActionType.SelectedIndex == 2)//Projects
            {
                labelFilterType.Text = "Project:";
                comboBoxSystemName.Items.Clear();
                comboBoxSystemName.Items.Add("PM: SE");
                comboBoxSystemName.Items.Add("AM: SE");
                comboBoxSystemName.Items.Add("PM: Billing");
                comboBoxSystemName.Items.Add("AM: Billing");
                comboBoxSystemName.Items.Add("OSResponsible");
                comboBoxSystemName.SelectedIndex = 0;
            }
            else
            {
                labelFilterType.Text = "System:";
                comboBoxSystemName.Items.Clear();
                comboBoxSystemName.Items.Add("ED");
                comboBoxSystemName.Items.Add("TRM");
                comboBoxSystemName.Items.Add("Invoicing");
                comboBoxSystemName.SelectedIndex = 0;
            }
        }

////////////////////////////////////////////////////////////
//Terminated employees
        //log data
        private void buttonTerminated_Click(object sender, EventArgs e)
        {
            //clear previous data
            for (int i = 0; i < 4; i++)
            {
                DataGridViewRow Row = dataGridViewTerminated.Rows[i];
                Row.Cells["Status"].Value = "";
                
                dataGridViewED.DataSource = null;
                dataGridViewTRM.DataSource = null;
                dataGridViewInvoicing.DataSource = null;
                dataGridViewEDFBUManager.DataSource = null;

            }
            //fill data
            for (int i = 0; i < 4; i++)
            {
                StartAction(i);
            }
        }
        //delete data
        private void buttonDeleteAccess_Click(object sender, EventArgs e)
        {
            GetCookie();

            //clear previous data
            for (int i = 0; i < 4; i++)
            {
                DataGridViewRow Row = dataGridViewTerminatedToDelete.Rows[i];
                Row.Cells["StatusDelete"].Value = "";
            }
            //fill data
            for (int i = 0; i < 4; i++)
            {
                StartActionToDelete(i);
            }
        }
        //common start procedures
        private void StartAction(int RowNum)
        {
            DataGridViewRow Row = dataGridViewTerminated.Rows[RowNum];
            if ((Boolean)Row.Cells["Use"].Value == true)
            {
                switch (RowNum)
                {
                    case 0: //ListOfTerminatedEmployees ED
                        Row.Cells["Status"].Value = GetListOfTerminatedEmployees();
                        break;
                    case 1: //ListOfTerminatedEmployees TRM
                        Row.Cells["Status"].Value = GetListOfTerminatedEmployeesTRMInvoicing("TRM");
                        break;
                    case 2: //ListOfTerminatedEmployees Invoicing
                        Row.Cells["Status"].Value = GetListOfTerminatedEmployeesTRMInvoicing("Invoicing");
                        break;
                    case 3: //Save file 
                        Row.Cells["Status"].Value = ExcelData.SaveAllDataToExcel(this.textBoxOutput.Text, dataGridViewInvoicing, dataGridViewTRM, dataGridViewEDFBUManager, dataGridViewED);
                        break;
                }
             
                if (Row.Cells["Status"].Value.ToString() != "" && Row.Cells["Status"].Value != null)
                {
                    Row.Cells["Use"].Value = false;
                }
            }
        }

        private void StartActionToDelete(int RowNum)
        {
            DataGridViewRow Row = dataGridViewTerminatedToDelete.Rows[RowNum];
            if ((Boolean)Row.Cells["UseDelete"].Value == true)
            {
                switch (RowNum)
                {                    
                    case 0: //delete FBU managers
                        Row.Cells["StatusDelete"].Value = DeleteFBUManagersTerminated();
                        break;
                    case 1: //delete ED data
                        Row.Cells["StatusDelete"].Value = DeleteTerminatedPrincipals("ED");
                        break;
                    case 2: //delete TRM data
                        Row.Cells["StatusDelete"].Value = DeleteTerminatedPrincipals("TRM");
                        break;
                    case 3: //delete Invoicing data
                        Row.Cells["StatusDelete"].Value = DeleteTerminatedPrincipals("Invoicing");
                        break;
                }

                if (Row.Cells["StatusDelete"].Value.ToString() != "" && Row.Cells["StatusDelete"].Value != null)
                {
                    Row.Cells["UseDelete"].Value = false;
                }
            }
        }

        private string GetListOfTerminatedEmployees()
        {
            string SQLRequestData = SQLQueriesTemplates.SQLTerminatedEmployeesED();
            
            ExecuteSQLScript(SQLRequestData, "ED", false, true);
            
            string SQLRequestDataPermissions = "";
            string SQLRequestDataManagers = "";
            foreach (DataGridViewRow DataRow in dataGridViewED.Rows)
            {
                if ((Boolean)DataRow.Cells["Active"].Value == false)
                {
                    SQLRequestDataPermissions = SQLRequestDataPermissions + DataRow.Cells["SelectUserPermissionsByLogin"].Value.ToString() 
                                                                          + " union ";

                    if (DataRow.Cells["HasRoleOnFBU"].Value.ToString() == "YES")
                    {
                        SQLRequestDataManagers = SQLRequestDataManagers + DataRow.Cells["SelectUserRolesOnFBUByLogin"].Value.ToString()
                                                                        + " union ";
                    }
                }                
            }
            //Permissions
            if (SQLRequestDataPermissions != "")
            {
                SQLRequestDataPermissions = SQLRequestDataPermissions.Substring(0, SQLRequestDataPermissions.Length - 7);
                ExecuteSQLScript(SQLRequestDataPermissions, "ED", false, true);
            }
            //Manager access
            if (SQLRequestDataManagers != "")
            {
                SQLRequestDataManagers = SQLRequestDataManagers.Substring(0, SQLRequestDataManagers.Length - 7);
                ExecuteSQLScript(SQLRequestDataManagers, "EDFBUManager", false, true);
            }
            
            return "OK";
        }

        private string GetListOfTerminatedEmployeesTRMInvoicing(string SystemName)
        {
            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));

            var dataGridViewTemp            = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            DataGridView TempdataGridView   = (DataGridView)dataGridViewTemp[0];
            
            string DatabaseToUse  = SystemSettingsData.DatabaseToUse;
            string SQLRequestData = SQLQueriesTemplates.SQLTerminatedEmployeesTRMInvoicing().Replace("#DatabaseToUse", DatabaseToUse);
            if (SystemSettingsData.OneDB == true)
            {
                SQLRequestData = SQLRequestData.Replace("[Authorization].[dbo]", " [auth]");
            }

            ExecuteSQLScript(SQLRequestData, SystemName, false, true);
            
            SQLRequestData = "";
            foreach (DataGridViewRow DataRow in TempdataGridView.Rows)
            {
                if ((Boolean)DataRow.Cells["Active"].Value == false)
                {
                    SQLRequestData = SQLRequestData + DataRow.Cells["Select_User_Permissions_By_Login"].Value.ToString();
                    if (DataRow.Index + 1 < TempdataGridView.RowCount)
                    {
                        SQLRequestData = SQLRequestData + " union ";
                    }
                }
            }
            if (SQLRequestData != "")
            {
                ExecuteSQLScript(SQLRequestData, SystemName, false, true);
            }            
            return "OK";
        }
                
        private string DeleteFBUManagersTerminated()
        {
            if (dataGridViewEDFBUManager.RowCount > 0)
            {
                DataTable dataTableFBUManagers = new DataTable();
                dataTableFBUManagers = ((System.Data.DataTable)dataGridViewEDFBUManager.DataSource).Copy();
                dataTableFBUManagers.Columns["BU"].ColumnName = "Context";
                dataGridViewSourceData.DataSource = dataTableFBUManagers;

                checkBoxFBUManagerOnlyActiveEmployees.Checked = false;
                ValidateDataFBU();
                DeleteFBUManager();
                ValidateDataFBU();
                checkBoxFBUManagerOnlyActiveEmployees.Checked = true;
            }

            return "OK";
        }

        private string DeleteTerminatedPrincipals(string SystemName)
        {
            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));

            var dataGridViewTemp                = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            DataGridView DataGridViewToUpdate   = (DataGridView)dataGridViewTemp[0];
            string DatabaseToUse                = SystemSettingsData.DatabaseToUse;
            string CookieData = "";
            if (SystemSettingsData.textBoxCookie != "")
            { 
                var TextBoxTemp = this.Controls.Find(SystemSettingsData.textBoxCookie, true);
                TextBox textBoxCookieTemp = (TextBox)TextBoxTemp[0];
                CookieData = textBoxCookieTemp.Text;
            }

            if (DataGridViewToUpdate.RowCount > 0)
            {
                DataTable DataTableValidateData = (DataTable)DataGridViewToUpdate.DataSource;
                DataTableValidateData = DataTableValidateData.AsEnumerable().GroupBy(r => new { Col1 = r["Login"] })
                        .Select(g => g.First())
                        .CopyToDataTable();
                DataTable DataTableCurrentAccess = new DataTable();

                string Logins = "";
                for (int i = 0; i < DataTableValidateData.Rows.Count; i++)
                {
                    Logins = Logins + "'" + DataTableValidateData.Rows[i]["Login"].ToString() + "'";
                    if (i < DataTableValidateData.Rows.Count-1)
                    {   Logins = Logins + ",";  }
                }
                
                string SQLRequestData = SQLQueriesTemplates.SQLPrincipalId().Replace("#DatabaseToUse", DatabaseToUse);
                SQLRequestData = SQLRequestData.Replace("#PrincipalNames", Logins);
                if (SystemSettingsData.OneDB == true)
                {
                    SQLRequestData = SQLRequestData.Replace("[Authorization].[dbo]", " [auth]");
                }
                
                SQLRequestData = SQLRequestData.Replace("#ExternalID", "Principal.ExternalId");
                ExecuteSQLScript(SQLRequestData, SystemName, false, true);

                DataTableValidateData = (DataTable)DataGridViewToUpdate.DataSource;
                foreach (DataRow rowPrincipal in DataTableValidateData.Rows)
                {
                    //remove access
                    string StringXmlPostData = XMLData.GenerateXmlPostData(DataTableCurrentAccess, rowPrincipal, numericUpDownCountPFI.Value);
                    string ResponseBody      = HTTPRequestsData.SendRequest(SystemSettingsData.Uri, StringXmlPostData, SystemSettingsData.ContentTypeXml, SystemSettingsData.AccessUpdateSavePrincipal, SystemSettingsData.AuthDefault, CookieData);
                    //remove principal
                    StringXmlPostData       = XMLData.GenerateXmlPostDataToDelete(rowPrincipal["PrincipalId"].ToString());
                    ResponseBody            = HTTPRequestsData.SendRequest(SystemSettingsData.Uri, StringXmlPostData, SystemSettingsData.ContentTypeXml, SystemSettingsData.AccessUpdateRemovePrincipal, SystemSettingsData.AuthDefault, CookieData);                    
                }
            }

            return "OK";
        }

        private void buttonFinReportOutput_Click(object sender, EventArgs e)
        {
            DirectoryOpen_Click(this.textBoxOutput.Text);
        }

        //open directiory
        private void DirectoryOpen_Click(string PathName)
        {
            if (this.Controls.Find(PathName, true).Count() > 0)
            {
                Control pathString = this.Controls.Find(PathName, true).First();
            }
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    //pathString.Text = fbd.SelectedPath;
                    textBoxOutput.Text = fbd.SelectedPath;
                    if (textBoxOutput.Text.Substring(textBoxOutput.Text.Length - 1) != @"\")
                    {
                        textBoxOutput.Text = textBoxOutput.Text + @"\";
                    }
                }
            }
        }

/////////////////////
//Clear duplicates       

        private void buttonValidateClearDuplicates_Click(object sender, EventArgs e)
        {            
            string SystemName = "";
            if (radioButtonClearDuplicatesED.Checked == true)
            {
                SystemName = "ClearDuplicatesED";
            }
            else if (radioButtonClearDuplicatesTRM.Checked == true)
            {
                SystemName = "ClearDuplicatesTRM";
            }
            else if (radioButtonClearDuplicatesInvoicing.Checked == true)
            {
                SystemName = "ClearDuplicatesInvoicing";
            }
            string SQLRequestData = GetReplacedSQLRequestLog("ClearDuplicatesValidate", SystemName);
            ExecuteSQLScript(SQLRequestData, SystemName, true, true);
        }

        private void buttonClearDuplicates_Click(object sender, EventArgs e)
        {
            string SystemName = "";
            if (radioButtonClearDuplicatesED.Checked == true)
            {
                SystemName = "ClearDuplicatesED";
            }
            else if (radioButtonClearDuplicatesTRM.Checked == true)
            {
                SystemName = "ClearDuplicatesTRM";
            }
            else if (radioButtonClearDuplicatesInvoicing.Checked == true)
            {
                SystemName = "ClearDuplicatesInvoicing";
            }
            SystemSettings SystemSettingsData = new SystemSettings(SystemName, GetSQLConnectionString(SystemName));

            var dataGridViewTemp = this.Controls.Find(SystemSettingsData.DataGridViewToUpdate, true);
            var textBoxResultTemp = this.Controls.Find(SystemSettingsData.textBoxResult, true);
            DataGridView DataGridViewToUpdate = (DataGridView)dataGridViewTemp[0];

            string ResponseBody = "";
            string StringXmlPostData = "";
            GetCookie();//from browser
            string CookieData = GetCookieFromForm(SystemSettingsData);

            string SQLRequestData = GetReplacedSQLRequestLog("ClearDuplicatesValidate", SystemName);
            ExecuteSQLScript(SQLRequestData, SystemName, true, true);
            DataTable DataTableValidateData = (DataTable)DataGridViewToUpdate.DataSource;
            DataTable DataTableValidateDataPrincipal = DataTableValidateData.AsEnumerable().GroupBy(r => new { Col1 = r["PrincipalId"], Col2 = r["PrincipalName"], Col3 = r["ExternalId"] })
                    .Select(g => g.First())
                    .CopyToDataTable();

            string SQLRequest = GetReplacedSQLRequestLog("ClearDuplicatesToDelete", SystemName);
            ExecuteSQLScript(SQLRequest, SystemName, false, true);            

            DataTable DataTableCurrentAccess = (DataTable)DataGridViewToUpdate.DataSource;
            foreach (DataRow rowPrincipal in DataTableValidateDataPrincipal.Rows)
            {
                StringXmlPostData = XMLData.GenerateXmlPostData(DataTableCurrentAccess, rowPrincipal, numericUpDownCountPFI.Value);
                ResponseBody = HTTPRequestsData.SendRequest(SystemSettingsData.Uri, StringXmlPostData, SystemSettingsData.ContentTypeXml, SystemSettingsData.AccessUpdateSavePrincipal, SystemSettingsData.AuthDefault, CookieData);                
            }

            SQLRequestData = GetReplacedSQLRequestLog("ClearDuplicatesValidate", SystemName);
            ExecuteSQLScript(SQLRequestData, SystemName, true, true);
        }
    }

    /////////////////////
    //to do для Post
    /*
     +++    * 1. удаление 
     +++    *  1.1 запрос для удаления
     +++    *  1.2 прикрутить к интерфейсу
     +++    * 2. добавить обработку нескольких логинов в одном файле
     +++    * 3. сохранение по 50 шт.
     +++    * 4. проверка выдачи/удаления на разные роли //выдает на все юниты
     +++    * SOD check
     +++    * 5. проверка в инвосинге
     +++    * 6. проверка в трм
     +++    * 7. назначение менеджеров юнитов
     +++    * 8. назначение менеджеров проектов
     +++    * 9. назначение ос респонсибл
     +++    * 10.удаление принципала
     +++    * 11.удаление уволеных 
     +++    * 12.вынести настройки в отдельный класс     
     +++    * 13.добавить возможность выдавать доступ на различные контексты через интерфейс
     +++    * 14.добавить AllRights в настройки
     +++    * 15.вынести начальные настройки в config файл

    active для принципалов. Проверять на деактивированность

    цвета вынести в настройки    

    скрипты переписать на EntityFramework

    */
}