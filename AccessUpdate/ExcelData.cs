using System;
using System.Linq;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;

namespace AccessUpdate
{
    public static class ExcelData
    {
        public static DataTable UploadExcelData(string ExcelFilePath, int SheetNumber, int NumberTopRows, int FirstRow, int LoginColumnNumber, int RoleColumnNumber, int FBUColumnNumber)
        {
            DataSet AccessTableSet = new DataSet("AccessTableSet");
            DataTable AccessTable = AccessTableSet.Tables.Add("AccessTable");
            DataColumn AccTableID = AccessTable.Columns.Add("Login", typeof(string));
            AccessTable.Columns.Add("Role", typeof(string));
            AccessTable.Columns.Add("Context", typeof(string));

            Excel.Application ObjExcel = new Excel.Application();
            Excel.Workbook ObjWorkBook = ObjExcel.Workbooks.Open(ExcelFilePath, 0, false, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);
            Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[SheetNumber];
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);

            int lastRow = (int)lastCell.Row;
            if (lastRow > NumberTopRows && NumberTopRows != 0)
            {
                lastRow = NumberTopRows;
            }
            FirstRow -= 1;

            for (int i = FirstRow; i < lastRow; i++)
            {
                DataRow NewRow = AccessTable.NewRow();
                NewRow["Login"] = ObjWorkSheet.Cells[i + 1, LoginColumnNumber].Text.ToString().Trim();
                NewRow["Role"] = ObjWorkSheet.Cells[i + 1, RoleColumnNumber].Text.ToString().Trim();
                NewRow["Context"] = ObjWorkSheet.Cells[i + 1, FBUColumnNumber].Text.ToString().Trim();
                if (NewRow["Login"].ToString() == "" && NewRow["Role"].ToString() == "" && NewRow["Context"].ToString() == "")
                {
                    continue;
                }
                AccessTable.Rows.Add(NewRow);
            }

            ObjWorkBook.Close(false, Type.Missing, Type.Missing);
            //ObjWorkExcel.Quit();
            GC.Collect(); // убрать за собой -- в том числе не используемые явно объекты !

            //Загружаем только уникальные строки
            var UniqueRows = AccessTable.AsEnumerable().Distinct(DataRowComparer.Default);

            return UniqueRows.CopyToDataTable();
        }

        private static void FillWorkSheet(DataGridView DataGridIOutput, Excel.Worksheet worksheet)
        {
            //header
            for (int j = 1; j < DataGridIOutput.ColumnCount + 1; j++)
            {
                worksheet.Rows[1].Columns[j] = DataGridIOutput.Columns[j - 1].HeaderText;
                //worksheet.Rows[1].Columns[j].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                worksheet.Rows[1].Columns[j].EntireRow.Font.Bold = true;
                worksheet.Rows[1].Columns[j].EntireColumn.AutoFit();
            }

            //body            
            for (int i = 1; i < DataGridIOutput.RowCount + 1; i++)
            {
                for (int j = 1; j < DataGridIOutput.ColumnCount + 1; j++)
                {
                    worksheet.Rows[i + 1].Columns[j] = DataGridIOutput.Rows[i - 1].Cells[j - 1].Value.ToString();
                }
            }
            for (int j = 1; j < DataGridIOutput.ColumnCount + 1; j++)
            {
                worksheet.Columns[j].EntireColumn.AutoFit();
            }
        }

        public static string SaveAllDataToExcel(string PathToOutput, DataGridView dataGridViewInvoicing, DataGridView dataGridViewTRM, DataGridView dataGridViewEDFBUManager, DataGridView dataGridViewED)
        {
            Excel.Application excelapp = new Excel.Application();
            Excel.Workbook workbook = excelapp.Workbooks.Add();
            Excel.Worksheet worksheet = workbook.ActiveSheet;

            foreach (Excel.Worksheet ws in workbook.Worksheets)
            {
                if (worksheet != ws)
                {
                    ws.Delete();
                }
            }

            if (dataGridViewInvoicing.RowCount > 0)
            {
                Excel.Worksheet worksheet4 = workbook.Worksheets.Add();
                worksheet4.Name = "Invoicing";
                FillWorkSheet(dataGridViewInvoicing, worksheet4);
            }
            if (dataGridViewTRM.RowCount > 0)
            {
                Excel.Worksheet worksheet3 = workbook.Worksheets.Add();
                worksheet3.Name = "TRM";
                FillWorkSheet(dataGridViewTRM, worksheet3);
            }
            if (dataGridViewEDFBUManager.RowCount > 0)
            {
                Excel.Worksheet worksheet2 = workbook.Worksheets.Add();
                worksheet2.Name = "ED_Managers";
                FillWorkSheet(dataGridViewEDFBUManager, worksheet2);
            }
            if (dataGridViewED.RowCount > 0)
            {
                Excel.Worksheet worksheet1 = workbook.Worksheets.Add();
                worksheet1.Name = "ED_Permissions";
                FillWorkSheet(dataGridViewED, worksheet1);
            }
            if (workbook.Worksheets.Count > 1)
            {
                worksheet.Delete();
            }

            string localDateStr = DateTime.Now.ToString().Replace(":", "").Replace(".", "");
            excelapp.AlertBeforeOverwriting = false;
            workbook.SaveAs(PathToOutput + "TerminatedEmployees_" + localDateStr + ".xlsx");
            excelapp.Quit();

            return "Save completed. " + PathToOutput + "\\TerminatedEmployees_" + localDateStr + ".xlsx";
        }

    }
}
