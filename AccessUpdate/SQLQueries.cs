namespace AccessUpdate
{

    public static class SQLQueriesTemplates
    {
        public static string SQLValidateRequestTemplate()
        {
            return @"use #DatabaseToUse
                    if object_id('tempdb..#TempAccess') is null
                    begin
                        create table #TempAccess (Login varchar(50), Role varchar(50), Context varchar(100))
	                    #ValuesToInsert
                    end
                select distinct
                    TempAccess.Login
                    , TempAccess.Role
                    , TempAccess.Context
                    , ExistingAccess.Principal as AccessExists
                    , RoleExist.Role as RoleExist
                    , RoleExist.RoleId as RoleId
                    , EmployeeExist.Employee as EmployeeExist
                    , EmployeeExist.EmployeeId as EmployeeId
                    , PrincipalExist.Principal as PrincipalExist
                    , PrincipalExist.ExternalId
                    , FBUExist.FBU as FBUExist
                    , FBUExist.FBUId as FBUId
                    ,case
                        when RoleExist.Role is not null
                            and FBUExist.FBU is not null
                            and PrincipalExist.Principal is not null
                            and ExistingAccess.Principal is null
                            then 'Access'
                        when RoleExist.Role is not null
                            and FBUExist.FBU is not null
                            and PrincipalExist.Principal is null
                            then 'Principal and access'
                        when ExistingAccess.Principal is not null
                            then 'Exist'
                        when RoleExist.Role is null
                            or FBUExist.FBU is null
                            or PrincipalExist.Principal is null
                            then 'Error'
		                else ''
                      end as WhatToAdd
                    , PrincipalExist.Principal as PrincipalName
                    , PrincipalExist.PrincipalId as PrincipalId
                    ,'' as SODCheck
                from #TempAccess as TempAccess
                left join
                    (
                    SELECT distinct
                         Principal.name as Principal
                        , BusinessRole.name as Role
                        , Permission.comment
                        , EntityType.name as [Entity type]
                        , BusinessUnit.name as FBU
                      FROM[Authorization].[dbo].[Permission] as Permission
                      join[Authorization].[dbo].[Principal] as Principal on Principal.id = Permission.principalId
                      join[Authorization].[dbo].[BusinessRole] as BusinessRole on BusinessRole.id = Permission.roleId
                      left join[Authorization].[dbo].[PermissionFilterItem] as PermissionFilterItem on PermissionFilterItem.permissionId = Permission.id
                      left join[Authorization].[dbo].[PermissionFilterEntity] as PermissionFilterEntity on PermissionFilterItem.entityId = PermissionFilterEntity.id
                      left join[Authorization].[dbo].[EntityType] As EntityType on EntityType.id = PermissionFilterEntity.entityTypeId
                      left join [dbo].#BusinessUnitTableName on BusinessUnit.id = PermissionFilterEntity.entityId
                      where
                        Principal.name in (select Login from #TempAccess as TempAccess)
		                and BusinessUnit.name in (select Context from #TempAccess as TempAccess)
		                and BusinessRole.name in (select Role from #TempAccess as TempAccess)
	                ) as ExistingAccess
	                on 		ExistingAccess.Principal = TempAccess.Login
		                and ExistingAccess.Role = TempAccess.Role
		                and ExistingAccess.FBU = TempAccess.Context
	                --FBU
	                left join 
		                (
			                SELECT BusinessUnit.name as FBU, BusinessUnit.id as FBUID
			                FROM [dbo].#BusinessUnitTableName
			                WHERE BusinessUnit.name in (select Context from #TempAccess as TempAccess)
		                ) as FBUExist
		                on FBUExist.FBU = TempAccess.Context
	                --Principal
	                left join 
		                (
			                SELECT Principal.name as Principal, Principal.id as PrincipalId
                            ,#ExternalID as ExternalId
			                FROM [Authorization].[dbo].[Principal]
			                WHERE Principal.name in (select Login from #TempAccess as TempAccess)
		                ) as PrincipalExist
		                on PrincipalExist.Principal = TempAccess.Login
	                --Employee
	                left join 
		                (
			                SELECT distinct Employee.Login as Employee, Principal.name as Principal, Principal.id as PrincipalId, Employee.id as EmployeeId
			                FROM [dbo].[Employee]
			                LEFT JOIN [Authorization].[dbo].[Principal] on Employee.login = Principal.name
			                WHERE Employee.login in (select Login from #TempAccess as TempAccess)
                            #OnlyActiveEmployees
		                ) as EmployeeExist
		                on EmployeeExist.Employee = TempAccess.Login
	                --Role
	                left join
		                (
			                SELECT BusinessRole.name as Role, BusinessRole.id as RoleId
			                FROM [Authorization].[dbo].[BusinessRole]
			                WHERE BusinessRole.Name in (select Role from #TempAccess as TempAccess)
		                ) as RoleExist
		                on RoleExist.Role = TempAccess.Role
    where
	    TempAccess.Login != '' ";
        }
        
        public static string SQLUpdateWithLogRequestTemplate()
        {
            return @"DECLARE
		@sqlquery nvarchar(max)
	SELECT		
		@sqlquery = '#SQLUpdateRequest'

    EXEC(@sqlquery)	

BEGIN TRY
	BEGIN TRAN
	---------------------------------------------------------
	-- Declare variables
	DECLARE
		@modifyDate	datetime,
		@modifiedBy	nvarchar(255),
		@approvedBy nvarchar(1000)='#ApprovedByToUse',
		@jiraIssue nvarchar(1000)='',
		@sdIssue nvarchar(1000)='#SDRequestToUse',
		@Rev int		
	SELECT
		@modifyDate = GETDATE(),
		@modifiedBy = SUSER_NAME()

	---------------------------------------------------------
	--write audit script
	  --INSERT INTO [ED_WORK_CONFIDENTIAL].[dbo].[ExecutedScripts_Log]
        --INSERT INTO [ED_WORK].[dbo].[ExecutedScripts_Log]
        INSERT INTO #DatabaseToLogInsert.[dbo].[ExecutedScripts_Log]
           ([script]
           ,[executionDate]
           ,[executedBy]
           ,[approvedBy]
           ,[jiraIssue]
           ,[sdIssue])
	output inserted.*
     VALUES
           (@sqlquery
           ,@modifyDate
           ,@modifiedBy
           ,@approvedBy
           ,@jiraIssue
           ,@sdIssue
		   );

	--ROLLBACK TRAN
	--PRINT 'ROLLBACK TRAN'
	COMMIT TRAN
	PRINT 'COMMIT TRAN'
	PRINT 'Operation successful complete'
END TRY
BEGIN CATCH
	ROLLBACK TRAN
	PRINT 'ROLLBACK TRAN'
	PRINT 'Error occurred in processing query: ' + ERROR_MESSAGE()
	PRINT 'Row with error: ' + CAST(ERROR_LINE() AS varchar(30))
END CATCH";
        }

        public static string SQLCurrentAccess()
        {
            return @"use #DatabaseToUse
                    if object_id('tempdb..#TempAccess') is null
                    begin
                        create table #TempAccess (Login varchar(50), Role varchar(50), Context varchar(100))
	                    #ValuesToInsert
                    end
select	 AccessToAdd.PrincipalId
		,AccessToAdd.PrincipalName
        ,AccessToAdd.ExternalId
        ,AccessToAdd.PrincipalActive
		,AccessToAdd.PermissionId
		,AccessToAdd.PermissionStartDate
		,AccessToAdd.PermissionEndDate
		,AccessToAdd.PermissionRoleId
        ,AccessToAdd.PermissionComment
		,AccessToAdd.PermissionFilterItemId
		,AccessToAdd.EntityTypeId
		,AccessToAdd.SecurityEntityId
        ,AccessToAdd.WhatToAdd
from (
                select                    
					isnull(PrincipalExist.PrincipalId,'00000000-0000-0000-0000-000000000000') as PrincipalId
					,isnull(PrincipalExist.Principal,TempAccess.Login) as PrincipalName
                    ,PrincipalExist.ExternalId
                    ,isnull(PrincipalExist.PrincipalActive,0) as PrincipalActive
					,isnull(ExistingAccess.PermissionId,'00000000-0000-0000-0000-000000000000') as PermissionId
					,isnull(ExistingAccess.periodstartDate,getdate()) as PermissionStartDate
					,ExistingAccess.periodendDate as PermissionEndDate
					,RoleExist.RoleId as PermissionRoleId
                    ,'#SDRequestToUse' as PermissionComment
					,isnull(ExistingAccess.PermissionFilterItemId,'00000000-0000-0000-0000-000000000000') as PermissionFilterItemId
					--,isnull(ExistingPHE.EntityTypeId,'263D2C60-7BCE-45D6-A0AF-A0830152353E') as EntityTypeId
                    ,isnull(ExistingPHE.EntityTypeId,EntityType.id) as EntityTypeId
					--,isnull(ExistingPHE.PFEntityId,'00000000-0000-0000-0000-000000000000') as SecurityEntityId
                    ,isnull(ExistingPHE.PFEntityId,FBUExist.FBUID) as SecurityEntityId
                    ,case
                        when RoleExist.Role is not null
                            and FBUExist.FBU is not null
                            and PrincipalExist.Principal is not null
                            and ExistingAccess.Principal is null
                            then 'Access'
                        when RoleExist.Role is not null
                            and FBUExist.FBU is not null
                            and PrincipalExist.Principal is null
                            then 'Principal and access'
                        when ExistingAccess.Principal is not null
                            then 'Exist'
                        when RoleExist.Role is null
                            or FBUExist.FBU is null
                            or PrincipalExist.Principal is null
                            then 'Error'
		                else ''
                      end as WhatToAdd
                from #TempAccess as TempAccess
                left join
                    (
                    SELECT distinct
                         Principal.name as Principal
                        ,BusinessRole.name as Role
						,BusinessRole.id as RoleId
                        ,Permission.comment
                        ,EntityType.name as [Entity type]
                        ,BusinessUnit.name as FBU
						,Permission.id as PermissionId
						,Permission.periodstartDate
						,Permission.periodendDate
						,PermissionFilterItem.id as PermissionFilterItemId
                      FROM [Authorization].[dbo].[Permission] as Permission
                      join [Authorization].[dbo].[Principal] as Principal on Principal.id = Permission.principalId
                      join [Authorization].[dbo].[BusinessRole] as BusinessRole on BusinessRole.id = Permission.roleId
                      left join [Authorization].[dbo].[PermissionFilterItem] as PermissionFilterItem on PermissionFilterItem.permissionId = Permission.id
                      left join [Authorization].[dbo].[PermissionFilterEntity] as PermissionFilterEntity on PermissionFilterItem.entityId = PermissionFilterEntity.id
                      left join [Authorization].[dbo].[EntityType] As EntityType on EntityType.id = PermissionFilterEntity.entityTypeId
                      left join [dbo].#BusinessUnitTableName on BusinessUnit.id = PermissionFilterEntity.entityId
                      where
                        Principal.name in (select Login from #TempAccess as TempAccess)
		                and BusinessUnit.name in (select Context from #TempAccess as TempAccess)
		                and BusinessRole.name in (select Role from #TempAccess as TempAccess)
	                ) as ExistingAccess
	                on 		ExistingAccess.Principal = TempAccess.Login
		                and ExistingAccess.Role = TempAccess.Role
		                and ExistingAccess.FBU = TempAccess.Context
	                --FBU
	                left join 
		                (
			                SELECT BusinessUnit.name as FBU, BusinessUnit.id as FBUID
			                FROM  [dbo].#BusinessUnitTableName
			                WHERE BusinessUnit.name in (select Context from #TempAccess as TempAccess)
		                ) as FBUExist
		                on FBUExist.FBU = TempAccess.Context
	                --Principal
	                left join 
		                (
			                SELECT Principal.name as Principal, Principal.id as PrincipalId, Principal.active as PrincipalActive
                            ,#ExternalID as ExternalId
			                FROM  [Authorization].[dbo].[Principal]
			                WHERE Principal.name in (select Login from #TempAccess as TempAccess)
		                ) as PrincipalExist
		                on PrincipalExist.Principal = TempAccess.Login
	                --Employee
	                left join 
		                (
			                SELECT distinct Employee.Login as Employee, Principal.name as Principal, Principal.id as PrincipalId
			                FROM  [dbo].[Employee]
			                LEFT JOIN  [Authorization].[dbo].[Principal] on Employee.login = Principal.name
			                WHERE Employee.login in (select Login from #TempAccess as TempAccess)
		                ) as EmployeeExist
		                on EmployeeExist.Employee = TempAccess.Login
	                --Role
	                left join
		                (
			                SELECT BusinessRole.name as Role, BusinessRole.id as RoleId
			                FROM  [Authorization].[dbo].[BusinessRole]
			                WHERE BusinessRole.Name in (select Role from #TempAccess as TempAccess)
		                ) as RoleExist
		                on RoleExist.Role = TempAccess.Role
					----PFE
					left join
						(
						SELECT distinct
							BusinessUnit.name as FBU		
							,PermissionFilterEntity.id as PFEId
							,PermissionFilterEntity.EntityId as PFEntityId
							,PermissionFilterEntity.EntityTypeId as EntityTypeId
						  FROM 
						  [Authorization].[dbo].[PermissionFilterEntity] as PermissionFilterEntity
						  join [Authorization].[dbo].[EntityType] As EntityType on EntityType.id = PermissionFilterEntity.entityTypeId
						  join [dbo].#BusinessUnitTableName on BusinessUnit.id = PermissionFilterEntity.entityId
						  where		
							BusinessUnit.name in (select Context from #TempAccess as TempAccess)
						) as ExistingPHE
						on ExistingPHE.FBU = TempAccess.Context
                    --EntityType для новых пермиссий возьмем из контекста доступа
                    left join [Authorization].[dbo].[EntityType] As EntityType on EntityType.name = '#EntityTypeName'
			
) as AccessToAdd
 where
	AccessToAdd.WhatToAdd in ('Access', 'Principal and access')
    and SecurityEntityId != '00000000-0000-0000-0000-000000000000'

	union all

SELECT Principal.id as PrincipalId
      ,Principal.name as PrincipalName
      ,#ExternalID as ExternalId
      ,Principal.active as PrincipalActive
	  ,Permission.id as PermissionId
	  ,Permission.periodstartDate as PermissionStartDate
	  ,Permission.periodendDate as PermissionEndDate
	  ,Permission.roleId as PermissionRoleId
      ,Permission.Comment as PermissionComment
	  ,PermissionFilterItem.id as PermissionFilterItemId
	  ,PermissionFilterEntity.entityTypeId as EntityTypeId
	  ,PermissionFilterEntity.entityId as SecurityEntityId
	  ,'Exist' as WhatToAdd
  FROM [Authorization].[dbo].[Principal]
  join [Authorization].[dbo].[Permission] on Permission.Principalid = Principal.id
  left join [Authorization].[dbo].[PermissionFilterItem] on PermissionFilterItem.permissionId = Permission.id
  left join [Authorization].[dbo].[PermissionFilterEntity] on PermissionFilterItem.entityId = PermissionFilterEntity.id
  
 WHERE Principal.name in (select Login from #TempAccess as TempAccess)

 drop table #TempAccess";
        }

        public static string SQLCurrentAccessToDelete()
        {
            return @"use #DatabaseToUse
                    if object_id('tempdb..#TempAccess') is null
                    begin
                        create table #TempAccess (Login varchar(50), Role varchar(50), Context varchar(100))
	                    #ValuesToInsert
                    end

SELECT Principal.id as PrincipalId
      ,Principal.name as PrincipalName
      ,#ExternalID as ExternalId
	  ,Permission.id as PermissionId
	  ,Permission.periodstartDate as PermissionStartDate
	  ,Permission.periodendDate as PermissionEndDate
	  ,Permission.roleId as PermissionRoleId
      ,Permission.Comment as PermissionComment
	  ,PermissionFilterItem.id as PermissionFilterItemId
	  ,PermissionFilterEntity.entityTypeId as EntityTypeId
	  ,PermissionFilterEntity.entityId as SecurityEntityId
	  ,'Exist' as WhatToAdd
  FROM [Authorization].[dbo].[Principal]
  join [Authorization].[dbo].[Permission] on Permission.Principalid = Principal.id
  left join [Authorization].[dbo].[PermissionFilterItem] on PermissionFilterItem.permissionId = Permission.id
  left join [Authorization].[dbo].[PermissionFilterEntity] on PermissionFilterItem.entityId = PermissionFilterEntity.id
  --FBU
  left join 
      (
          SELECT BusinessUnit.name as FBU, BusinessUnit.id as FBUID
          FROM  [dbo].#BusinessUnitTableName
      ) as FBUExist
      on FBUExist.FBUID = PermissionFilterEntity.entityId
  --Role
  left join
      (
          SELECT BusinessRole.name as Role, BusinessRole.id as RoleId
          FROM  [Authorization].[dbo].[BusinessRole]
      ) as RoleExist
      on RoleExist.RoleId = Permission.roleId

    left join #TempAccess as TempAccess2 on	 Principal.name = TempAccess2.Login
											and RoleExist.Role = TempAccess2.role
											and FBUExist.FBU   = TempAccess2.Context
  
 WHERE 
    Principal.name in (select Login from #TempAccess as TempAccess)
    and TempAccess2.Login is null

 order by
	Principal.name
	,Permission.id

 drop table #TempAccess";
        }

        public static string SQLCurrentAccessPrincipal()
        {
            return @"use #DatabaseToUse

SELECT
	 LOGIN as Login
	,role as Role
	,BU as Context
FROM [Authorization].[dbo].[PermissionsAll]
WHERE
    BU is not null
	#Condition";
        }

        public static string SQLValidateFBUManager()
        {
            return @"use EnterpriseDirectories

create table #TempAccess (Login varchar(50), Role varchar(50), Context varchar(100))
#ValuesToInsert

select distinct
	TempAccess.login
	--,TempAccess.Role    
    ,case when lower(TempAccess.Role) in ('manager','1') then '1'
          when lower(TempAccess.Role) in ('manager delegated','managerdelegated','2') then '2'
     end as Role
	,TempAccess.Context
    ,FBUManagerExist.role as AccessExists
	,FBUExist.FBU as FBUExist
	,FBUExist.FBUID as FBUId
	,EmployeeExist.EmployeeLogin as EmployeeExist
	,EmployeeExist.EmployeeID as EmployeeId
	,case when FBUExist.FBU is null then 'Error'
		  when EmployeeExist.EmployeeLogin is null then 'Error'
          when lower(TempAccess.Role) not in ('1','2','manager','manager delegated','managerdelegated') then 'Error'
          when FBUManagerExist.role is not null then 'Exist'
	 else 'Access'
	 end as WhatToAdd
    ,case when lower(TempAccess.Role) not in ('1','2','manager','manager delegated','managerdelegated') then ''
	 else TempAccess.Role
	 end as RoleExist
	,'' as PrincipalName
	,'' as PrincipalId
    ,'' as SODCheck
    ,'' as ExternalId
    ,FBUManagerExist.BusinessUnitEmployeeRoleId as RoleId
  from #TempAccess as TempAccess
  --FBU Manager link exists
  left join (select distinct
				 BusinessUnit.name as FBU
				,BusinessUnit.Id as FBUID
                --,BusinessUnit.ComingBusinessUnitId as FBUID
				,Employee.login as EmployeeLogin
				,Employee.Id as EmployeeID
				,BusinessUnitEmployeeRole.role
                ,BusinessUnitEmployeeRole.id as BusinessUnitEmployeeRoleId
			  from [app].[BusinessUnitEmployeeRole]
			  join [app].[BusinessUnit] on BusinessUnitEmployeeRole.businessUnitId = businessUnit.Id
              --join [app].BusinessUnitVersion as BusinessUnit on BusinessUnitEmployeeRole.businessUnitId = businessUnit.ComingBusinessUnitId
			  join [app].[Employee] on Employee.id = BusinessUnitEmployeeRole.employeeId
			  join #TempAccess as TempAccess on TempAccess.Context = BusinessUnit.name
											and TempAccess.Login = Employee.login
			  where
				BusinessUnitEmployeeRole.active = 1) as FBUManagerExist on TempAccess.Context = FBUManagerExist.FBU
																  and TempAccess.Login = FBUManagerExist.EmployeeLogin
																  and (TempAccess.Role = cast(FBUManagerExist.role as varchar(1))
                                                                    or lower(TempAccess.Role) = case when lower(FBUManagerExist.role) = '1' then 'manager'
                                                                                              when lower(FBUManagerExist.role) = '2' then 'managerdelegated' end
                                                                    or lower(TempAccess.Role) = case when lower(FBUManagerExist.role) = '2' then 'manager delegated' end
                                                                       )
	--FBU exist
	left join (select distinct
				 BusinessUnit.name as FBU
				,BusinessUnit.Id as FBUID
              from [app].[BusinessUnit]
               -- ,BusinessUnit.ComingBusinessUnitId as FBUID
              --from [app].BusinessUnitVersion as BusinessUnit
			  where 
				BusinessUnit.name in (select Context from #TempAccess)
				) as FBUExist on TempAccess.Context = FBUExist.FBU
	--Employee exists
	left join (select
				Employee.login as EmployeeLogin
				,Employee.Id as EmployeeID
			  from [app].[Employee]
			  where
				Employee.login in (select login from #TempAccess as TempAccess)
                #OnlyActiveEmployees
			  ) as EmployeeExist on TempAccess.Login = EmployeeExist.EmployeeLogin

drop table #TempAccess";
        }

        public static string SQLCurrentFBUManager()
        {
            return @"use EnterpriseDirectories

select 
#TopRows
	 Employee.login as Login
	,cast(BusinessUnitEmployeeRole.role as nvarchar(20)) as Role
    ,BusinessUnit.name as Context
from [app].[BusinessUnitEmployeeRole]
	join [app].[BusinessUnit] on BusinessUnitEmployeeRole.businessUnitId = businessUnit.Id
    join [app].[BUFullTree] on BusinessUnitEmployeeRole.businessUnitId = BUFullTree.Id
	join [app].[Employee] on Employee.id = BusinessUnitEmployeeRole.employeeId
where
    BusinessUnitEmployeeRole.active = 1
    and BusinessUnitEmployeeRole.role in (1,2)
    and BusinessUnit.active = 1
    #Condition";
        }

        public static string SQLValidateSEProject()
        {
            return @"use EnterpriseDirectories

create table #TempAccess (Login varchar(50), Role varchar(50), Context varchar(100))
#ValuesToInsert

select distinct
	 TempAccess.Login
	,TempAccess.Role
	,TempAccess.Context
    ,ProjectExist.FBUID as FBUID
    ,ProjectExist.FBU as FBU    
    ,ProjectExist.FBU as FBUExist
	,ProjectExist.Project as ProjectExist
    ,ProjectExist.ProjectStatus
	,ProjectExist.ProjectID
    ,ProjectExist.ProjectName
    ,ProjectExist.ProjectVersion
    ,ProjectExist.plannedEndDate
    ,ProjectExist.endDate
    ,ProjectExist.startDate
    ,ProjectExist.clientId
    ,ProjectExist.clientLegalEntityId
    ,ProjectExist.contractId
    ,ProjectExist.financialProjectId
    ,ProjectExist.companyLegalEntityId
	,ProjectExist.industryId
	,ProjectExist.servicesDescriptionId
	,ProjectExist.projectPaymentType
	,ProjectExist.projectStartType
    ,ProjectExist.CurrencyForInvoicesId
	,ProjectExist.VendorNumber
    ,ProjectExist.TimeUnit
	,EmployeeExist.Employee as EmployeeExist
	,EmployeeExist.EmployeeId as EmployeeId
	,AccessExist.Project as AccessExist
	,case when EmployeeExist.Employee is null then 'Error'
		  when ProjectExist.Project is null then 'Error'
          when lower(TempAccess.Role) not in ('pm','project manager','am','account manager') then 'Error'
          when ProjectExist.ProjectStatus = 'Closed' then 'Project closed'
		  when AccessExist.Project is not null then 'Exist'          
	else 'Access'
	end as WhatToAdd
	,ProjectExist.PMID as CurrentPMID
	,ProjectExist.AMID as CurrentAMID
	,ProjectExist.PMLogin as CurrentPMLogin
	,ProjectExist.AMLogin as CurrentAMLogin
	,ProjectExist.osResponsibleId as CurrentOSID
	,ProjectExist.OSResponsibleLogin as CurrentOSLogin
	,EmployeeExist.EmployeeId as PrincipalId
	,EmployeeExist.Employee as PrincipalName
	,TempAccess.Role as RoleId
	,case when lower(TempAccess.Role) in ('pm','project manager','am','account manager') then TempAccess.Role
     else '' end as RoleExist
    ,'' as SODCheck
    ,'' as ExternalId
from #TempAccess as TempAccess
left join	(select 
			   Project.id as ProjectID
			  ,Project.code as Project			  
			  ,ProjectM.projectManagerId as PMID
			  ,ProjectM.accountManagerId as AMID
			  ,PM.Login as PMLogin
			  ,AM.Login as AMLogin
		  from [app].[Project]
		  join [app].[SeProject] as ProjectM on Project.id = ProjectM.id
		  join [app].[BusinessUnit] on Project.businessUnitId = businessUnit.Id
		  join [app].[Employee] as PM on PM.id = ProjectM.projectManagerId
		  join [app].[Employee] as AM on AM.id = ProjectM.accountManagerId
		  where
			Project.code in (select Context from #TempAccess)
			and (PM.Login in (select Login from #TempAccess where lower(Role) in ('pm','project manager'))
			  or AM.Login in (select Login from #TempAccess where lower(Role) in ('am','account manager'))
			  )
		  ) as AccessExist on AccessExist.Project = TempAccess.Context
						  and (AccessExist.PMLogin = TempAccess.Login	
						    or AccessExist.AMLogin = TempAccess.Login)
--Project exist
left join (select 
			   Project.id as ProjectID
			  ,Project.code as Project
              ,ProjectM.projectName as ProjectName
			  ,Project.version as ProjectVersion
			  ,Project.plannedEndDate
			  ,Project.endDate
			  ,Project.startDate
			  ,ProjectM.clientId
			  ,ProjectM.clientLegalEntityId
			  ,ProjectM.contractId
			  ,ProjectM.financialProjectId
			  ,ProjectM.companyLegalEntityId
			  ,ProjectM.industryId
			  ,ProjectM.servicesDescriptionId
			  ,ProjectM.projectPaymentType
			  ,ProjectM.projectStartType
              ,'' as CurrencyForInvoicesId
	          ,'' as VendorNumber
              ,'' as osResponsibleId
	          ,'' as OSResponsibleLogin
              ,'' as TimeUnit
              ,case when Project.active = 1 then 'Active' else 'Closed' end as ProjectStatus
              ,Project.businessUnitId as FBUID
			  ,businessUnit.name as FBU
			  ,ProjectM.projectManagerId as PMID
			  ,ProjectM.accountManagerId as AMID
			  ,PM.Login as PMLogin
			  ,AM.Login as AMLogin
		  from [app].[Project]
		  join [app].[SeProject] as ProjectM on Project.id = ProjectM.id
		  join [app].[Employee] as PM on PM.id = ProjectM.projectManagerId
		  join [app].[Employee] as AM on AM.id = ProjectM.accountManagerId
          join [app].[BusinessUnit] on Project.businessUnitId = businessUnit.Id
		  ) as ProjectExist on ProjectExist.Project = TempAccess.Context
--Employee exist
left join	(select 
				 Employee.Id as EmployeeId
				,Employee.Login as Employee
		  from [app].[Employee]
		  where
			Employee.Login in (select Login from #TempAccess)			
            #OnlyActiveEmployees
			  ) as EmployeeExist on EmployeeExist.Employee = TempAccess.Login

drop table #TempAccess";
        }

        public static string SQLValidateBillProject()
        {
            return @"use EnterpriseDirectories

create table #TempAccess (Login varchar(50), Role varchar(50), Context varchar(100))
#ValuesToInsert

select distinct
	 TempAccess.Login
	,TempAccess.Role
	,TempAccess.Context
    ,ProjectExist.FBUID as FBUID
    ,ProjectExist.FBU as FBU    
    ,ProjectExist.FBU as FBUExist
	,ProjectExist.Project as ProjectExist
    ,ProjectExist.ProjectStatus
	,ProjectExist.ProjectID
    ,ProjectExist.ProjectName
    ,ProjectExist.ProjectVersion
    ,ProjectExist.plannedEndDate
    ,ProjectExist.endDate
    ,ProjectExist.startDate
    ,ProjectExist.clientId
    ,ProjectExist.clientLegalEntityId
    ,ProjectExist.contractId
    ,ProjectExist.financialProjectId
    ,ProjectExist.companyLegalEntityId
	,ProjectExist.industryId
	,ProjectExist.servicesDescriptionId
	,ProjectExist.projectPaymentType
	,ProjectExist.projectStartType
    ,ProjectExist.CurrencyForInvoicesId
	,ProjectExist.VendorNumber
    ,ProjectExist.TimeUnit
	,EmployeeExist.Employee as EmployeeExist
	,EmployeeExist.EmployeeId as EmployeeId
	,AccessExist.Project as AccessExist
	,case when EmployeeExist.Employee is null then 'Error'
		  when ProjectExist.Project is null then 'Error'
          when lower(TempAccess.Role) not in ('pm','project manager','am','account manager','osresponsible') then 'Error'
          when ProjectExist.ProjectStatus = 'Closed' then 'Project closed'
		  when AccessExist.Project is not null then 'Exist'          
	else 'Access'
	end as WhatToAdd
	,ProjectExist.PMID as CurrentPMID
	,ProjectExist.AMID as CurrentAMID
	,ProjectExist.PMLogin as CurrentPMLogin
	,ProjectExist.AMLogin as CurrentAMLogin
	,ProjectExist.osResponsibleId as CurrentOSID
	,ProjectExist.OSResponsibleLogin as CurrentOSLogin
	,EmployeeExist.EmployeeId as PrincipalId
	,EmployeeExist.Employee as PrincipalName
	,TempAccess.Role as RoleId
	,case when lower(TempAccess.Role) in ('pm','project manager','am','account manager','osresponsible') then TempAccess.Role
     else '' end as RoleExist
    ,'' as SODCheck
    ,'' as ExternalId
from #TempAccess as TempAccess
left join	(select 
			   Project.id as ProjectID
			  ,Project.code as Project			  
			  ,ProjectM.projectManagerId as PMID
			  ,ProjectM.accountManagerId as AMID
			  ,PM.Login as PMLogin
			  ,AM.Login as AMLogin
			  ,ProjectM.osResponsibleId
			  ,OSResponsible.Login as OSResponsibleLogin
		  from [app].[Project]
          join [app].[BillingProject] as ProjectM on Project.id = ProjectM.id
		  join [app].[BusinessUnit] on Project.businessUnitId = businessUnit.Id
		  join [app].[Employee] as PM on PM.id = ProjectM.projectManagerId
		  join [app].[Employee] as AM on AM.id = ProjectM.accountManagerId
		  join [app].[Employee] as OSResponsible on OSResponsible.id = ProjectM.osResponsibleId
		  where
			Project.code in (select Context from #TempAccess)
			and (PM.Login in (select Login from #TempAccess where lower(Role) in ('pm','project manager'))
			  or AM.Login in (select Login from #TempAccess where lower(Role) in ('am','account manager'))
			  or OSResponsible.Login in (select Login from #TempAccess where lower(Role) = 'OSResponsible')
			  )
		  ) as AccessExist on AccessExist.Project = TempAccess.Context
						  and (AccessExist.PMLogin = TempAccess.Login	
						    or AccessExist.AMLogin = TempAccess.Login
						    or AccessExist.OSResponsibleLogin = TempAccess.Login)
--Project exist
left join (select 
			   Project.id as ProjectID
			  ,Project.code as Project
              ,ProjectM.projectName as ProjectName
			  ,Project.version as ProjectVersion
			  ,Project.plannedEndDate
			  ,Project.endDate
			  ,Project.startDate
			  ,ProjectM.clientId
			  ,ProjectM.clientLegalEntityId
			  ,ProjectM.contractId
			  ,ProjectM.financialProjectId
			  ,ProjectM.companyLegalEntityId			  
			  ,ProjectM.projectPaymentType
              ,'' as industryId
			  ,'' as servicesDescriptionId
			  ,'' as projectStartType
              ,ProjectM.CurrencyForInvoicesId
			  ,ProjectM.VendorNumber
              ,ProjectM.TimeUnit
              ,case when Project.active = 1 then 'Active' else 'Closed' end as ProjectStatus
              ,Project.businessUnitId as FBUID
			  ,businessUnit.name as FBU
			  ,ProjectM.projectManagerId as PMID
			  ,ProjectM.accountManagerId as AMID
			  ,PM.Login as PMLogin
			  ,AM.Login as AMLogin
			  ,ProjectM.osResponsibleId
			  ,OSResponsible.Login as OSResponsibleLogin
		  from [app].[Project]
		  join [app].[BillingProject] as ProjectM on Project.id = ProjectM.id
		  join [app].[Employee] as PM on PM.id = ProjectM.projectManagerId
		  join [app].[Employee] as AM on AM.id = ProjectM.accountManagerId
          join [app].[Employee] as OSResponsible on OSResponsible.id = ProjectM.osResponsibleId
          join [app].[BusinessUnit] on Project.businessUnitId = businessUnit.Id
		  ) as ProjectExist on ProjectExist.Project = TempAccess.Context
--Employee exist
left join	(select 
				 Employee.Id as EmployeeId
				,Employee.Login as Employee
		  from [app].[Employee]
		  where
			Employee.Login in (select Login from #TempAccess)			
            #OnlyActiveEmployees
			  ) as EmployeeExist on EmployeeExist.Employee = TempAccess.Login

drop table #TempAccess";
        }

        public static string SQLCurrentSEProjectData()
        {
            return @"use EnterpriseDirectories

declare @LoginType int = #LoginType;
declare @Login varchar(50) = '#Login';

select
	case when @LoginType = 0 then PM.login
		 when @LoginType = 1 then AM.login
	end as Login
    ,case when @LoginType = 0 then 'pm'
		 when @LoginType = 1 then 'am'
	end as Role
	,BusinessUnit.name as FBU
	,Project.code as Project
from [app].[Project]
join [app].[SEProject] as ProjectM on ProjectM.id = Project.id
join [app].[BusinessUnit] on Project.businessUnitId = businessUnit.Id  
join [app].[Employee] as PM on PM.id = ProjectM.projectManagerId
join [app].[Employee] as AM on AM.id = ProjectM.accountManagerId
where
	Project.active = 1
    ";
        }

        public static string SQLCurrentBillProjectData()
        {
            return @"use EnterpriseDirectories

declare @LoginType int = #LoginType;
declare @Login varchar(50) = '#Login';

select
	case when @LoginType = 2 then PM.login
		 when @LoginType = 3 then AM.login
		 when @LoginType = 4 then OSResponsible.login
	end as Login
    ,case when @LoginType = 2 then 'PM'
		 when @LoginType = 3 then 'AM'
		 when @LoginType = 4 then 'OSResponsible'
	end as Role
	,BusinessUnit.name as FBU
	,Project.code as Project
from [app].[Project]
join [app].[BillingProject] as ProjectM on ProjectM.id = Project.id
join [app].[BusinessUnit] on Project.businessUnitId = businessUnit.Id  
join [app].[Employee] as PM on PM.id = ProjectM.projectManagerId
join [app].[Employee] as AM on AM.id = ProjectM.accountManagerId
join [app].[Employee] as OSResponsible on OSResponsible.id = ProjectM.osResponsibleId
where
	Project.active = 1
	";
        }

        public static string SQLLoginFilterBillProject()
        {
            return @"case when @LoginType = 2 then PM.login
		 when @LoginType = 3 then AM.login
		 when @LoginType = 4 then OSResponsible.login
	end = @Login";
        }

        public static string SQLLoginFilterSEProject()
        {
            return @"case when @LoginType = 0 then PM.login
		 when @LoginType = 1 then AM.login
	end = @Login";
        }
       
        public static string SQLTerminatedEmployeesED()
        {
            return @"SELECT 
       [WhatDelete]
      ,[FullName]
      ,[Login]
      ,[Email]
      ,[Active]
      ,[DismissDate]
      ,[HasRoleOnFBU]
      ,[IsPA]
      ,[SelectEmployeeByLogin]
      ,[SelectUserPermissionsByLogin]
      ,[SelectUserRolesOnFBUByLogin]
  FROM [EnterpriseDirectories].[auth].[TerminatedUsers]";
        }

        public static string SQLTerminatedEmployeesTRMInvoicing()
        {
            return @"use #DatabaseToUse
SELECT [Fullname]
      ,[Login]
      ,[Email]
      ,[active]
      ,[Dismiss_Date]
      ,[What_Delete]
      ,[Select_Employee_By_Login]
      ,[Select_User_Permissions_By_Login]
  FROM [Authorization].[dbo].[TerminatedUsers]
";
        }

        public static string SQLPrincipalId()
        {
            return @"use #DatabaseToUse
SELECT [id] as PrincipalId
      ,[active]
      ,[name] as PrincipalName
      ,#ExternalID as ExternalId
FROM [Authorization].[dbo].[Principal]
where
	name in (#PrincipalNames)";
        }

        public static string SQLClearDuplicatesValidate()
        {
            return @"use #DatabaseToUse

SELECT Principal.id as PrincipalId
      ,Principal.name as PrincipalName
      ,Principal.name as Login      
      ,RoleExist.RoleId as RoleExist
      ,RoleExist.Role      
      ,#ExternalID as ExternalId
	  ,Permission.id as PermissionId
	  ,Permission.periodstartDate as PermissionStartDate
	  ,Permission.periodendDate as PermissionEndDate
	  ,Permission.roleId as PermissionRoleId
      ,Permission.Comment as PermissionComment
	  ,PermissionFilterItem.id as PermissionFilterItemId
	  ,PermissionFilterEntity.entityTypeId as EntityTypeId
	  ,PermissionFilterEntity.entityId as SecurityEntityId
      ,FBUExist.FBUID as FBUID
      ,FBUExist.FBU as Context
      ,FBUExist.FBUID as FBUExist
	  ,'Exist' as WhatToAdd
      ,'' as EmployeeId
      ,Permission.roleId as RoleId      
      ,'' as SODCheck
      ,'' as EmployeeExist
  FROM auth.[Principal]
  join auth.[Permission] on Permission.Principalid = Principal.id
  left join auth.[PermissionFilterItem] on PermissionFilterItem.permissionId = Permission.id
  left join auth.[PermissionFilterEntity] on PermissionFilterItem.entityId = PermissionFilterEntity.id
  --FBU
  left join 
      (
          SELECT BusinessUnit.name as FBU, BusinessUnit.id as FBUID
          FROM  [app].#BusinessUnitTableName
      ) as FBUExist
      on FBUExist.FBUID = PermissionFilterEntity.entityId
  --Role
  left join
      (
          SELECT BusinessRole.name as Role, BusinessRole.id as RoleId
          FROM  auth.[BusinessRole]
      ) as RoleExist
      on RoleExist.RoleId = Permission.roleId
    
 WHERE 
    PermissionFilterItem.id in (
SELECT PermissionFilterItemId--,PermissionID,PrincipalId
		FROM (
		SELECT distinct
		pr.id AS PrincipalId,
			  pr.name AS PrincipalName,
			  p.id AS PermissionID,
			  r.id AS RoleId,
			  r.name AS RoleName,
			  i.id AS PermissionFilterItemId,
			  e.id AS PermissionFilterEntityId,
			  e.entityId AS pfe_entityId,
			  ROW_NUMBER() OVER(PARTITION BY pr.id,r.id,e.entityid ORDER BY i.createdate ASC) AS RowNum
		FROM	auth.Permission AS p 
			INNER JOIN
				auth.BusinessRole AS r ON r.id = p.roleId
			INNER JOIN
				auth.Principal AS pr ON pr.id = p.principalId 
			INNER JOIN
				.auth.PermissionFilterItem AS i ON i.permissionId = p.id 
			INNER JOIN
				auth.PermissionFilterEntity AS e ON e.id = i.entityId 
			
		where p.status = 2 
            AND (p.[periodendDate] is null or cast(getdate() as date) <= p.[periodendDate] )
            --Есть дубликаты
			AND EXISTS
			(SELECT *
				FROM         
					auth.Permission AS p1 
				INNER JOIN
					auth.BusinessRole AS r1 ON r1.id = p1.roleId  AND r1.id = r.id
				INNER JOIN
					auth.Principal AS pr1 ON pr1.id = p1.principalId AND pr1.id=pr.id  
				INNER JOIN
					auth.PermissionFilterItem AS i1 ON i1.permissionId = p1.id  AND i1.id<>i.id
				INNER JOIN
					auth.PermissionFilterEntity AS e1 ON e1.id = i1.entityId AND e1.entityId=e.entityId 
				WHERE p1.status = 2 
                                and (p1.[periodendDate] is null or cast(getdate() as date) <= p1.[periodendDate] )
                               
			)
			--Не заполнен другой тип контекста
			AND NOT EXISTS
			(SELECT *
				FROM         
					auth.Permission AS p2 
				INNER JOIN
					auth.PermissionFilterItem AS i2 ON i2.permissionId = p2.id  
				INNER JOIN
					auth.PermissionFilterEntity AS e2 ON e2.id = i2.entityId 

			WHERE p2.id = p.id AND i2.id<>i.id AND e2.entityTypeId<>e.entityTypeId 
                                

			))T
			WHERE RowNum > 1
)

 order by
	Principal.name
	,Permission.id";
        }

        public static string SQLClearDuplicatesToDelete()
        {
            return @"use #DatabaseToUse

SELECT PermissionFilterItemId
	  ,PermissionID
	  ,PrincipalId
into #TempPermissions
		FROM (
		SELECT distinct
		pr.id AS PrincipalId,
			  pr.name AS PrincipalName,
			  p.id AS PermissionID,
			  r.id AS RoleId,
			  r.name AS RoleName,
			  i.id AS PermissionFilterItemId,
			  e.id AS PermissionFilterEntityId,
			  e.entityId AS pfe_entityId,
			  ROW_NUMBER() OVER(PARTITION BY pr.id,r.id,e.entityid ORDER BY i.createdate ASC) AS RowNum
		FROM	auth.Permission AS p 
			INNER JOIN
				auth.BusinessRole AS r ON r.id = p.roleId
			INNER JOIN
				auth.Principal AS pr ON pr.id = p.principalId 
			INNER JOIN
				auth.PermissionFilterItem AS i ON i.permissionId = p.id 
			INNER JOIN
				auth.PermissionFilterEntity AS e ON e.id = i.entityId 
			
		where p.status = 2 
            AND (p.[periodendDate] is null or cast(getdate() as date) <= p.[periodendDate] )
            --Есть дубликаты
			AND EXISTS
			(SELECT *
				FROM         
					auth.Permission AS p1 
				INNER JOIN
					auth.BusinessRole AS r1 ON r1.id = p1.roleId  AND r1.id = r.id
				INNER JOIN
					auth.Principal AS pr1 ON pr1.id = p1.principalId AND pr1.id=pr.id  
				INNER JOIN
					auth.PermissionFilterItem AS i1 ON i1.permissionId = p1.id  AND i1.id<>i.id
				INNER JOIN
					auth.PermissionFilterEntity AS e1 ON e1.id = i1.entityId AND e1.entityId=e.entityId 
				WHERE p1.status = 2 
                                and (p1.[periodendDate] is null or cast(getdate() as date) <= p1.[periodendDate] )
                               
			)
			--Не заполнен другой тип контекста
			AND NOT EXISTS
			(SELECT *
				FROM         
					auth.Permission AS p2 
				INNER JOIN
					auth.PermissionFilterItem AS i2 ON i2.permissionId = p2.id  
				INNER JOIN
					auth.PermissionFilterEntity AS e2 ON e2.id = i2.entityId 

			WHERE p2.id = p.id AND i2.id<>i.id AND e2.entityTypeId<>e.entityTypeId 
                                

			))T
			WHERE RowNum > 1


SELECT Principal.id as PrincipalId
      ,Principal.name as PrincipalName
      ,#ExternalID as ExternalId
	  ,Permission.id as PermissionId
	  ,Permission.periodstartDate as PermissionStartDate
	  ,Permission.periodendDate as PermissionEndDate
	  ,Permission.roleId as PermissionRoleId
      ,Permission.Comment as PermissionComment
	  ,PermissionFilterItem.id as PermissionFilterItemId
	  ,PermissionFilterEntity.entityTypeId as EntityTypeId
	  ,PermissionFilterEntity.entityId as SecurityEntityId
	  ,'Exist' as WhatToAdd
      ,'' as EmployeeId
      ,Permission.roleId as RoleId
      ,'' as FBUID
      ,'' as RoleExist
      ,'' as Role
      ,'' as SODCheck
      ,'' as EmployeeExist
      ,'' as Login
      ,'' as Context
      ,'' as FBUExist
  FROM auth.[Principal]
  join auth.[Permission] on Permission.Principalid = Principal.id
  left join auth.[PermissionFilterItem] on PermissionFilterItem.permissionId = Permission.id
  left join auth.[PermissionFilterEntity] on PermissionFilterItem.entityId = PermissionFilterEntity.id
  --FBU
  left join 
      (
          SELECT BusinessUnit.name as FBU, BusinessUnit.id as FBUID
          FROM  [app].#BusinessUnitTableName
      ) as FBUExist
      on FBUExist.FBUID = PermissionFilterEntity.entityId
  --Role
  left join
      (
          SELECT BusinessRole.name as Role, BusinessRole.id as RoleId
          FROM  auth.[BusinessRole]
      ) as RoleExist
      on RoleExist.RoleId = Permission.roleId
    
 WHERE 
    (
        PermissionFilterItem.id not in (SELECT PermissionFilterItemId from #TempPermissions)
		or PermissionFilterItem.id is null
	)			
    and Principal.id in (SELECT PrincipalId from #TempPermissions)

 order by
	Principal.name
	,Permission.id

drop table #TempPermissions
";
        }
    }
}

