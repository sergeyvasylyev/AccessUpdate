using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.IO;
using System.Web.Helpers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace AccessUpdate
{
    class PulsarObjects
    {
    }

    public class Context
    {
        public string Id { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public int Version { get; set; }
        public bool CalcActive { get; set; }
        public string ContextTypeId { get; set; }
        public string ContextTypeName { get; set; }
        public string Name { get; set; }
        public object PeriodEndDate { get; set; }
        public object PeriodStartDate { get; set; }
        public DateTime SourceCreateDate { get; set; }
        public string SourceCreatedBy { get; set; }
        public string SourceModifiedBy { get; set; }
        public DateTime SourceModifyDate { get; set; }
    }

    public class NameEng
    {
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public string LastName { get; set; }
    }

    public class NameNative
    {
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
    }

    public class Employee
    {
        public string Id { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public int Version { get; set; }
        public bool CalcActive { get; set; }
        public string ContextTypeId { get; set; }
        public string ContextTypeName { get; set; }
        public object DismissDate { get; set; }
        public string Email { get; set; }
        public string EmployeePosition { get; set; }
        public object ExternalId { get; set; }
        public DateTime HireDate { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public NameEng NameEng { get; set; }
        public NameNative NameNative { get; set; }
        public string NamePinPosition { get; set; }
        public object PeriodEndDate { get; set; }
        public object PeriodStartDate { get; set; }
        public int Pin { get; set; }
        public object SourceCreateDate { get; set; }
        public string SourceCreatedBy { get; set; }
        public string SourceModifiedBy { get; set; }
        public DateTime SourceModifyDate { get; set; }
    }

    public class Role
    {
        public string Id { get; set; }
        public bool Active { get; set; }
        public object CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public int Version { get; set; }
        public bool CalcActive { get; set; }
        public string CorporateSystemName { get; set; }
        public string CorporateSystemPulsarName { get; set; }
        public string Description { get; set; }
        public string DescriptionPulsar { get; set; }
        public string ExternalIadId { get; set; }
        public object ExternalId { get; set; }
        public bool IncludedInGroup { get; set; }
        public bool IsTemplate { get; set; }
        public string Name { get; set; }
        public string NameWithCorporateSystemPulsarName { get; set; }
        public object PeriodEndDate { get; set; }
        public object PeriodStartDate { get; set; }
        public string PrimaryContextId { get; set; }
        public string PrimaryContextName { get; set; }
        public object SourceCreateDate { get; set; }
        public object SourceCreatedBy { get; set; }
        public string SourceModifiedBy { get; set; }
        public DateTime SourceModifyDate { get; set; }
    }

    public class RoleCorporateSystemPulsar
    {
        public string Id { get; set; }
        public bool Active { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public int Version { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public object SourceCreateDate { get; set; }
        public object SourceCreatedBy { get; set; }
        public object SourceModifiedBy { get; set; }
        public object SourceModifyDate { get; set; }
    }

    public class GetRichSODConflictsCheckResult
    {
        public object CheckIcdApprovedResult { get; set; }
        public string CheckResult { get; set; }
        public object LastChangeDate { get; set; }
        public int Status { get; set; }
        public Context Context { get; set; }
        public Employee Employee { get; set; }
        public Role Role { get; set; }
        public RoleCorporateSystemPulsar RoleCorporateSystemPulsar { get; set; }
        public List<object> RoleConflicts { get; set; }
        public List<object> SodCheckContextConflicts { get; set; }
    }

    [DataContract]
    public class RootObject
    {
        [DataMember]
        public List<GetRichSODConflictsCheckResult> GetRichSODConflictsCheckResult { get; set; }
    }
}
