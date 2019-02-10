using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;

namespace AccessUpdate
{
    public static class XMLData
    {
        //public string GenerateXmlPostData(DataTable DataTableCurrentAccess, string PrincipalId, string PrincipalName, decimal NumberOfPFI)
        public static string GenerateXmlPostData(DataTable DataTableCurrentAccess, DataRow rowPrincipal, decimal NumberOfPFI)
        {
            DataTable DataTableCurrentAccessPermissions = DataTableCurrentAccess.Clone();
            DataTableCurrentAccessPermissions.Clear();

            if (DataTableCurrentAccess.Rows.Count > 0)
            {
                DataTableCurrentAccessPermissions = DataTableCurrentAccess.AsEnumerable().GroupBy(r => new {
                    Col1 = r["PrincipalId"],
                    Col2 = r["PrincipalName"],
                    Col3 = r["PermissionId"],
                    Col4 = r["PermissionStartDate"],
                    Col5 = r["PermissionEndDate"],
                    Col6 = r["PermissionRoleId"],
                    Col7 = r["PermissionComment"]
                })
                .Select(g => g.First())
                .CopyToDataTable();
            }

            string PrincipalId    = rowPrincipal["PrincipalId"].ToString();
            string PrincipalName  = rowPrincipal["PrincipalName"].ToString();
            string PrincipalExtId = rowPrincipal["ExternalId"].ToString();

            string pathToXml = "TestAccessXML.xml";
            XmlTextWriter textWritter = new XmlTextWriter(pathToXml, Encoding.UTF8);
            textWritter.WriteStartDocument();

            textWritter.WriteStartElement("s:Envelope");
            textWritter.WriteAttributeString("xmlns:s", "http://schemas.xmlsoap.org/soap/envelope/");
            textWritter.WriteStartElement("s:Body");
            textWritter.WriteStartElement("SavePrincipal");
            textWritter.WriteAttributeString("xmlns", "http://tempuri.org/");
            textWritter.WriteStartElement("principalStrict");
            textWritter.WriteAttributeString("xmlns:d4p1", "Auth");
            textWritter.WriteAttributeString("xmlns:i", "http://www.w3.org/2001/XMLSchema-instance");            
            
            textWritter.WriteStartElement("d4p1:Active");
            textWritter.WriteString("true");
            textWritter.WriteEndElement();//d4p1:Active
            textWritter.WriteStartElement("d4p1:ExternalId");
            if (PrincipalExtId == "")
            {
                textWritter.WriteAttributeString("i:nil", "true");
            }
            else
            {
                textWritter.WriteString(PrincipalExtId);
            }
            textWritter.WriteEndElement();//d4p1:ExternalId
            
            textWritter.WriteStartElement("d4p1:Id");
            textWritter.WriteString(PrincipalId);
            textWritter.WriteEndElement();//d4p1:Id
            textWritter.WriteStartElement("d4p1:Name");
            textWritter.WriteString(PrincipalName);
            textWritter.WriteEndElement();//d4p1:Name
            textWritter.WriteStartElement("d4p1:Permissions");
            if (DataTableCurrentAccess.Rows.Count > 0)
            {
                foreach (DataRow rowPermissions in DataTableCurrentAccessPermissions.Rows)
                {
                    if ((PrincipalId == rowPermissions["PrincipalId"].ToString()) & (PrincipalName == rowPermissions["PrincipalName"].ToString()))
                    {
                        textWritter.WriteStartElement("d4p1:PermissionStrictDTO");
                        textWritter.WriteStartElement("d4p1:Comment");
                        string PermissionComment = rowPermissions["PermissionComment"].ToString();
                        if (PermissionComment == "") { PermissionComment = " "; }
                        textWritter.WriteString(PermissionComment);
                        textWritter.WriteEndElement();//d4p1: Comment
                        textWritter.WriteStartElement("d4p1:FilterItems");

                        int PFIperString = 0;

                        foreach (DataRow rowItems in DataTableCurrentAccess.Rows)
                        {
                            if ((rowPermissions["PermissionId"].ToString() == rowItems["PermissionId"].ToString())
                                & (rowPermissions["PermissionRoleId"].ToString() == rowItems["PermissionRoleId"].ToString())
                                & (PrincipalName == rowItems["PrincipalName"].ToString()))
                            {
                                //только для новых пермиссий делим по numericUpDownCountPFI.Value шт. в строку
                                if (rowPermissions["PermissionId"].ToString() == "00000000-0000-0000-0000-000000000000")
                                {
                                    if ((PFIperString % NumberOfPFI == 0) & (PFIperString != 0))
                                    {
                                        textWritter.WriteEndElement();//d4p1: FilterItems
                                        textWritter.WriteStartElement("d4p1:Id");
                                        textWritter.WriteString(rowPermissions["PermissionId"].ToString());
                                        textWritter.WriteEndElement();//d4p1: Id
                                        textWritter.WriteStartElement("d4p1:Period");
                                        textWritter.WriteStartElement("EndDate");
                                        textWritter.WriteAttributeString("i:nil", "true");
                                        textWritter.WriteAttributeString("xmlns", "");
                                        textWritter.WriteString(rowPermissions["PermissionEndDate"].ToString());
                                        textWritter.WriteEndElement();//EndDate
                                        textWritter.WriteStartElement("StartDate");
                                        textWritter.WriteAttributeString("xmlns", "");
                                        textWritter.WriteString(((System.DateTime)(rowPermissions["PermissionStartDate"])).ToString("yyyy-MM-ddTHH:mm:ss"));
                                        textWritter.WriteEndElement();//StartDate
                                        textWritter.WriteEndElement();//d4p1: Period
                                        textWritter.WriteStartElement("d4p1:Role");
                                        textWritter.WriteStartElement("d4p1:Id");
                                        textWritter.WriteString(rowPermissions["PermissionRoleId"].ToString());
                                        textWritter.WriteEndElement();//d4p1: Id //Role
                                        textWritter.WriteEndElement();//d4p1: Role
                                        textWritter.WriteEndElement();//d4p1: PermissionStrictDTO

                                        textWritter.WriteStartElement("d4p1:PermissionStrictDTO");
                                        textWritter.WriteStartElement("d4p1:Comment");
                                        PermissionComment = rowPermissions["PermissionComment"].ToString();
                                        if (PermissionComment == "") { PermissionComment = " "; }
                                        textWritter.WriteString(PermissionComment);
                                        textWritter.WriteEndElement();//d4p1: Comment
                                        textWritter.WriteStartElement("d4p1:FilterItems");
                                    }
                                }
                                if (rowItems["EntityTypeId"].ToString() != "")
                                {
                                    textWritter.WriteStartElement("d4p1:PermissionFilterItemStrictDTO");
                                    textWritter.WriteStartElement("d4p1:EntityType");
                                    textWritter.WriteStartElement("d4p1:Id");
                                    textWritter.WriteString(rowItems["EntityTypeId"].ToString());
                                    textWritter.WriteEndElement();//d4p1:Id
                                    textWritter.WriteEndElement();//d4p1:EntityType
                                    textWritter.WriteStartElement("d4p1:Id");
                                    textWritter.WriteString(rowItems["PermissionFilterItemId"].ToString());
                                    textWritter.WriteEndElement();//d4p1:Id //entity
                                    textWritter.WriteStartElement("d4p1:SecurityEntity");
                                    textWritter.WriteStartElement("d4p1:Id");
                                    textWritter.WriteString(rowItems["SecurityEntityId"].ToString());
                                    textWritter.WriteEndElement();//d4p1:Id
                                    textWritter.WriteEndElement();//d4p1:SecurityEntity
                                    textWritter.WriteEndElement();//d4p1: PermissionFilterItemStrictDTO

                                    PFIperString++;
                                }                                
                            }
                        }//rowItems
                        textWritter.WriteEndElement();//d4p1: FilterItems
                        textWritter.WriteStartElement("d4p1:Id");
                        textWritter.WriteString(rowPermissions["PermissionId"].ToString());
                        textWritter.WriteEndElement();//d4p1: Id
                        textWritter.WriteStartElement("d4p1:Period");
                        textWritter.WriteStartElement("EndDate");
                        textWritter.WriteAttributeString("i:nil", "true");
                        textWritter.WriteAttributeString("xmlns", "");
                        textWritter.WriteString(rowPermissions["PermissionEndDate"].ToString());
                        textWritter.WriteEndElement();//EndDate
                        textWritter.WriteStartElement("StartDate");
                        textWritter.WriteAttributeString("xmlns", "");
                        textWritter.WriteString(((System.DateTime)(rowPermissions["PermissionStartDate"])).ToString("yyyy-MM-ddTHH:mm:ss"));
                        textWritter.WriteEndElement();//StartDate
                        textWritter.WriteEndElement();//d4p1: Period
                        textWritter.WriteStartElement("d4p1:Role");
                        textWritter.WriteStartElement("d4p1:Id");
                        textWritter.WriteString(rowPermissions["PermissionRoleId"].ToString());
                        textWritter.WriteEndElement();//d4p1: Id //Role
                        textWritter.WriteEndElement();//d4p1: Role
                        textWritter.WriteEndElement();//d4p1: PermissionStrictDTO
                    }
                }//rowPermissions
            }
            textWritter.WriteEndElement();//d4p1:Permissions
            textWritter.WriteEndElement();//principalStrict
            textWritter.WriteEndElement();//SavePrincipal
            textWritter.WriteEndElement();//s:Body
            textWritter.WriteEndElement();//s:Envelope

            textWritter.Close();

            var XmlText = System.IO.File.ReadAllLines(pathToXml);
            return XmlText[0];
        }

        public static string GenerateXmlPostDataToDelete(string PrincipalId)
        {
            string pathToXml = "TestAccessXML.xml";
            XmlTextWriter textWritter = new XmlTextWriter(pathToXml, Encoding.UTF8);
            textWritter.WriteStartDocument();

            textWritter.WriteStartElement("s:Envelope");
            textWritter.WriteAttributeString("xmlns:s", "http://schemas.xmlsoap.org/soap/envelope/");
            textWritter.WriteStartElement("s:Body");
            textWritter.WriteStartElement("RemovePrincipal");
            textWritter.WriteAttributeString("xmlns", "http://tempuri.org/");
            textWritter.WriteStartElement("principalIdent");
            textWritter.WriteAttributeString("xmlns:d4p1", "Auth");
            textWritter.WriteAttributeString("xmlns:i", "http://www.w3.org/2001/XMLSchema-instance");
            textWritter.WriteStartElement("d4p1:Id");
            textWritter.WriteString(PrincipalId);
            textWritter.WriteEndElement();//d4p1:Id
            textWritter.WriteEndElement();//principalIdent
            textWritter.WriteEndElement();//RemovePrincipal
            textWritter.WriteEndElement();//s:Body
            textWritter.WriteEndElement();//s:Envelope

            textWritter.Close();

            var XmlText = System.IO.File.ReadAllLines(pathToXml);
            return XmlText[0];
        }

        public static string GenerateXmlPostDataGetCurrentPrincipal()
        {
            string pathToXml = "TestAccessXML.xml";
            XmlTextWriter textWritter = new XmlTextWriter(pathToXml, Encoding.UTF8);
            textWritter.WriteStartDocument();

            textWritter.WriteStartElement("s:Envelope");
            textWritter.WriteAttributeString("xmlns:s", "http://schemas.xmlsoap.org/soap/envelope/");
            textWritter.WriteStartElement("s:Body");
            textWritter.WriteStartElement("GetCurrentPrincipal");
            textWritter.WriteAttributeString("xmlns", "http://tempuri.org/");
            textWritter.WriteEndElement();//GetCurrentPrincipal
            textWritter.WriteEndElement();//s:Body
            textWritter.WriteEndElement();//s:Envelope

            textWritter.Close();

            var XmlText = System.IO.File.ReadAllLines(pathToXml);
            return XmlText[0];
        }

        public static string GetXmlDataByName(string XMLStringToFind, string TagNameWhereToFind, string TagNameWhatToFind)
        {
            string ResultData = "";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(XMLStringToFind);
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            
            manager.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
            manager.AddNamespace("tempuri", "http://tempuri.org/");
            manager.AddNamespace("a", "Auth");
            manager.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");
            manager.AddNamespace("l", "en-US");

            XmlNodeList xnList = xmlDoc.SelectNodes(TagNameWhereToFind, manager);
            foreach (XmlNode childrenNode in xnList)
            {
                if (childrenNode.FirstChild != null)
                {
                    ResultData = (childrenNode[TagNameWhatToFind]).InnerText;
                }
            }

            return ResultData;
        }

        public static string GetXmlDataAll(string XMLStringToFind, string TagNameWhereToFind, string TagNameWhatToFind)
        {
            string ResultData = "";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(XMLStringToFind);
            //xmlDoc.LoadXml(XMLStringToFind.Substring(XMLStringToFind.IndexOf(Environment.NewLine)));
            XmlNamespaceManager manager = new XmlNamespaceManager(xmlDoc.NameTable);
            
            manager.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");            

            XmlNodeList xnList = xmlDoc.SelectNodes(TagNameWhereToFind, manager);
            foreach (XmlNode childrenNode in xnList)
            {
                if (childrenNode.FirstChild != null)
                {
                    ResultData = (childrenNode[TagNameWhatToFind]).InnerText;
                }
            }

            return ResultData;
        }

    }
}
