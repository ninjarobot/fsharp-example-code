namespace FSharp.Interview.SqlProvider.Api.Business

module SqlProviderContext =
    open FSharp.Data.Sql
    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open FSharp.Interview.SqlProvider.Api.Domain.SqlProviderConnection
   
    type DependentDBType = spouse = 1uy | child = 2uy

    let [<Literal>] dbVendor  = Common.DatabaseProviderTypes.MSSQLSERVER

    (* 
        This is the SHAPE (Schema) connection string that SQL Provider uses on build to generate internal schema during build. 
        This will generatea compile time error if code doesn't match schema. 
        The RUNTIME sql connection string  (JobInterviewConnection.GetConnString) is pulled from the appsettings.json file
        The advantage of this approach is that if a release pipeline is set up then (assuming the schema db is somewhere accessible e.g. Azure)
        as long as the code matches then all OK. The developer can have his own db with data.
        Schema changes are still tricky, though. A schema change (if done locally) would require the developer to comment out the 'schema' conn string,
        uncomment to point to local db, make schema changes, then modify "schema" db with changes, then commit.
        Another way would be to update the "schema" db first (this might break build of other team members in real time depending on schema change), adjust code
        then commit.
        Conclusion: Using a Type Provider gives you the compile and strong typing goodness, but has it's downside is CICD piplelines, and when working on a team
        where communication and coordination is a must
    *)
    let [<Literal>] connectionString = @"Server=(localdb)\mssqllocaldb;Database=JobInterview;Trusted_Connection=True;MultipleActiveResultSets=true;Connection Timeout=30"
    let [<Literal>] tableNames = "Employee, Dependent, DependentType" //comma delimited string of only tables to include.If omiitted then all tables by default
    let [<Literal>] indivAmount = 200 // number of dropdown items to show when using individual?
    let [<Literal>] useOptTypes  = true // use F# OptionType for nulls
    let [<Literal>] caseSensitivityChange  = Common.CaseSensitivityChange.ORIGINAL // ORIGNAL - leaves table name alone when loading (default). TOUPPER and TOLOWER do just that

    type Sql = 
        SqlDataProvider<
            dbVendor, 
            connectionString, 
            IndividualsAmount = indivAmount,
            UseOptionTypes = useOptTypes,
            TableNames = tableNames,
            CaseSensitivityChange = caseSensitivityChange >


    let context = Sql.GetDataContext JobInterviewConnection.GetConnString
    let employees = context.Dbo.Employee
    let dependents = context.Dbo.Dependent

    let fetchAsync fm query = 
        async {
            let! execQry = query |> Seq.executeQueryAsync |> Async.Catch
            let result =
                match execQry with
                | Choice1Of2 a -> Ok (a)
                | Choice2Of2 ex -> Error(SqlException (FriendlyMessage fm, ExceptionMessage ex.Message))
            return result
         } |> Async.RunSynchronously

    let fetchOneAsync nfm fm query  = 
        async {
            let! execQry = query |> Seq.tryHeadAsync |> Async.Catch
            let result =
                match execQry with
                | Choice1Of2 a -> 
                    match a with
                    | Some(a) -> Ok (a)
                    | None  -> Error(NotFound (nfm))
                | Choice2Of2 ex -> Error(SqlException (FriendlyMessage fm, ExceptionMessage ex.Message))
            return result
         } |> Async.RunSynchronously

    let queryForEmployeeById empId  = 
         query {
             for employee in employees do
                 where (employee.Id = empId)
                 select (employee)
         }

    let queryForDependentById depId  = 
        query {
            for dependent in dependents do
                where (dependent.Id = depId)
                select (dependent)
        }

    let queryForDependentsByEmployeeId empId  = 
        query {
            for dependent in dependents do
                where (dependent.EmployeeId = empId)
                select (dependent)
        }

    let queryAllEmployees (pg: Paging) = 
        let (skp, tk) = pg
        query {
            for employee in employees do
                sortBy (employee.LastName)
                skip (match skp with Skip f -> f) 
                take (match tk with Take f -> f) 
                select (employee)
        }

    let deletelAllItemsAsynch = Seq.``delete all items from single table``

    let toEmployee (dbRecord: Sql.dataContext.``dbo.EmployeeEntity``) = 
        let id = Id dbRecord.Id
        let name : EmployeeName = (FirstName dbRecord.FirstName, LastName dbRecord.LastName, MiddleInitial dbRecord.MiddleInitial)
        let asal : AnnualSalary = AnnualSalary (dbRecord.AnnualSalary |> float)
        let pc = Paycheck (PaycheckGross (dbRecord.PaycheckGross |> float), 
                    DeductionBenefits (dbRecord.DeductionBenefits |> float), 
                    Discount (dbRecord.Discount |> float), 
                    PaycheckNet (dbRecord.PaycheckNet |> float) )
        Employee (id, name, asal, pc )

    let toEmployees (dbRecords: seq<Sql.dataContext.``dbo.EmployeeEntity``>) = 
        dbRecords |> Seq.map(fun empEnt -> empEnt |>  toEmployee)

    let toDependent (dbRecord: Sql.dataContext.``dbo.DependentEntity``) = 
        let dt =
            match dbRecord.DependentTypeId with 
            | sp when dbRecord.DependentTypeId = byte DependentDBType.spouse -> Spouse sp
            | ch when dbRecord.DependentTypeId = byte DependentDBType.child -> Child ch
            | _ -> failwith "The value of DependentTypeId from the database does not match one of the enum DependentDBType values"
        (Id dbRecord.Id, EmployeeId dbRecord.EmployeeId, dt, Name dbRecord.Name)

module PaycheckCalculation = 
    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open FSharp.Interview.SqlProvider.Api.Domain.DomainConstants

    let private (| GetsDiscount | DoesNotGetDiscount |) (ln:string) =
        let c = ln |> List.ofSeq |> List.tryHead |> Option.defaultValue '_'
        match c with
        | c when c = 'A' || c = 'a' -> GetsDiscount
        | _ -> DoesNotGetDiscount

    let private discountToApply = function
        | GetsDiscount -> 0.15 //15%
        | DoesNotGetDiscount -> 0.
 
    //paycheck calculations
    let private perPaycheck amount = amount / paychecksPerYear
    let private calcPayCheckGross annualSalary = annualSalary |>  perPaycheck
   
    //benefit calculations
    let private depCostYear  = (*) dependentCostForBenefitsPerYear 
    let private netCostYear = (+) employeeCostForBenefitsPerYear 
    let private calcDeductionBenefitsPerPaycheck noDeps = 
        float noDeps 
        |> depCostYear 
        |> netCostYear 
        |> perPaycheck
 
    //discount calculations
    let private calcAmount totalBenPerYear  =  (*) totalBenPerYear
    let private calcDiscountPerPayCheck ln deductPerPc =  ln |> discountToApply|> calcAmount deductPerPc

    let calculatePaycheck (newPaycheckData: CalculatePaycheckData) : Paycheck = 
        let (LastName ln, NewSalary ns, NumberOfDependents noDeps) = newPaycheckData
        let pcGross = ns |> calcPayCheckGross
        let dedPerPc = noDeps |> calcDeductionBenefitsPerPaycheck
        let discount = dedPerPc |> calcDiscountPerPayCheck ln
        let pcNet = pcGross - (dedPerPc - discount)
        (PaycheckGross pcGross, DeductionBenefits dedPerPc, Discount discount, PaycheckNet pcNet)

module GetEmployee =

    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open SqlProviderContext

    let fetchEmployee id  =
        let nfm = sprintf "Employee not found for provided Id: %d" id
        let fm = sprintf "There was an SQL error fetching employee for provided Id: %d" id
        let result =  id |> queryForEmployeeById |> fetchOneAsync nfm fm
        match result with
        | Ok (emp) -> Ok(emp |> toEmployee)
        | Error(dbex) -> Error(dbex)

    let fetchDependendentsForEmployee (emp: Employee) =
        let (id, _, _, _) = emp
        let id = match id with Id id -> id
        let fm = sprintf "There was an SQL error fetching dependents for provided Id: %d" id
        let result =  id |> queryForDependentsByEmployeeId |> fetchAsync fm
        match result with
        | Ok (ds) -> 
            let deps: Dependent list = 
                ds 
                |> Seq.map(fun i -> toDependent i) 
                |> Seq.sortBy(fun (_,_,tp,nm) -> tp, nm) 
                |> Seq.toList
            Ok(EmployeeAndDependents (emp, deps))
        | Error(dbex) -> Error(dbex)
    
    let getEmployee id  =
        Ok(id) 
        |> Result.bind fetchEmployee 
        |> Result.bind fetchDependendentsForEmployee

module GetSalaryInfo =

    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open SqlProviderContext

    let fetchEmployees (pg: Paging) = 
        let foo = pg |> queryAllEmployees |> fetchAsync "There was an error fetching employees"
        match foo with
        | Ok(emps) -> Ok(emps |> toEmployees)
        | Error(dbex) -> Error(dbex)

    let getSalaryInfo (pg: Paging)  = Ok(pg)  |> Result.bind fetchEmployees
 
module InsertEmployee =

    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open FSharp.Interview.SqlProvider.Api.Domain.DomainConstants
    open FSharp.Interview.SqlProvider.Api.Lifting.AlphaNumeric
    open SqlProviderContext
    open PaycheckCalculation

    let validateNewEmployeeFirstName (aed: AddEmployeeData)  =
        let ((FirstName fn, _, _), _) = aed
        let message = sprintf "First name can not be null or empty" 
        fn |> isNotNullOrWhiteSpaceFn aed (InvalidFirstName message)

    let validateNewEmployeeFirstNameLength (aed: AddEmployeeData)  =
        let ((FirstName fn, _, _), _) = aed 
        let message = sprintf "First name can not exceed %i characters" maxEmployeeFirstNameChars
        fn |> isLengthLessThanEqualFn aed (InvalidFirstName message) maxEmployeeFirstNameChars

    let validateNewEmployeeLastName (aed: AddEmployeeData)  =
        let ((_, LastName ln, _), _) = aed
        let message = sprintf "Last name can not be null or empty" 
        ln |> isNotNullOrWhiteSpaceFn aed (InvalidLastName message)

    let validateNewEmployeeLastNameLength (aed: AddEmployeeData)  =
        let ((_, LastName ln, _), _) = aed 
        let message = sprintf "Last name can not exceed %i characters" maxEmployeeLastNameChars
        ln |> isLengthLessThanEqualFn aed (InvalidFirstName message) maxEmployeeLastNameChars

    let validateNewEmployeeMiddleInitial (aed: AddEmployeeData)  =
        let ((_, _, MiddleInitial mi), _) = aed
        let mi = mi |> Option.defaultValue "" 
        match (mi |> isNotZeroLength) with //only check if provided at least one character
        | Ok() -> mi |> isLengthLessThanEqualFn aed (InvalidMiddleInitialName "Middle initial can only be one character in length") 1
        | _ -> Ok(aed)

    let validateNewEmployeeSalary (aed: AddEmployeeData)  =
        let (_, AnnualSalary ansal) = aed
        let message = sprintf "Salary must be between %.2f and %.2f" minSalary maxSalary
        ansal |> inRangeFn aed (InvalidSalary message) minSalary maxSalary

    let toNewEmployee (aed: AddEmployeeData)  =
        let (en , AnnualSalary ansal) = aed
        let (_, LastName ln,_) = en
        let newPaycheck = (LastName ln, NewSalary ansal, NumberOfDependents 0) |> calculatePaycheck
        Ok( Employee (Id 0, EmployeeName en, AnnualSalary ansal, Paycheck newPaycheck) )

    let insertEmployeeAsync (emp: Employee) =
        async {
            let (_, (FirstName fn, LastName ln, MiddleInitial mi), AnnualSalary ansal, pc) = emp
            let (PaycheckGross pg, DeductionBenefits dben, Discount dis, PaycheckNet pn) = pc
            let mi = match mi with Some(s) -> s  |>  toUpper | _-> None
            let row = employees.Create()
            row.FirstName <- fn.Trim()
            row.LastName <- ln.Trim()
            row.MiddleInitial <-  mi
            row.AnnualSalary <- (ansal |> decimal)
            row.PaycheckGross <-  (pg |> decimal)
            row.DeductionBenefits <-  (dben |> decimal)
            row.Discount <- (dis |> decimal)
            row.PaycheckNet <- (pn |> decimal)
            let! submitAsynch = context.SubmitUpdatesAsync() |> Async.Catch
            let result =
                match submitAsynch with
                | Choice1Of2 _ -> Ok(row |> toEmployee)
                | Choice2Of2 ex -> 
                    context.ClearUpdates |> ignore
                    Error(DbFailure(SqlException(FriendlyMessage "There was a serious error inserting employee. Please contact system administrator.", ExceptionMessage ex.Message)))
            return result
        } |> Async.RunSynchronously

    let addEmployee (newEmp: AddEmployeeData) = 
        Ok(newEmp)
        |> Result.bind validateNewEmployeeFirstName
        |> Result.bind validateNewEmployeeFirstNameLength
        |> Result.bind validateNewEmployeeLastName
        |> Result.bind validateNewEmployeeLastNameLength
        |> Result.bind validateNewEmployeeMiddleInitial
        |> Result.bind validateNewEmployeeSalary
        |> Result.bind toNewEmployee
        |> Result.bind insertEmployeeAsync

module UpdateSalary = 

    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open FSharp.Interview.SqlProvider.Api.Domain.DomainConstants
    open FSharp.Interview.SqlProvider.Api.Lifting.AlphaNumeric
    open SqlProviderContext
    open PaycheckCalculation

    let validateUpdatedSalary (usd: UpdateSalaryData)  =
        let (_, ns) = usd
        let ns = match ns with NewSalary f -> f
        let message = sprintf "Salary must be between %.2f and %.2f" minSalary maxSalary
        let inr = ns |> inRange minSalary maxSalary
        match inr with
        |Ok() -> Ok(usd)
        |Error() -> Error (InvalidSalary message)

    let getEmployeeInfo (usd: UpdateSalaryData) =
        let(Id id, ns) = usd
        let  emp = (id |> GetEmployee.getEmployee) 
        match emp with
        | Ok(empAndDeps) -> Ok(EmployeeAndDependentsAndNewSalary (empAndDeps, ns))
        | Error(dbex) -> Error(DbFailure dbex)

    let calculateNewPaycheck (edasal: EmployeeAndDependentsAndNewSalary) =
        let ((emp , deps), ns) = edasal
        let ns = match ns with NewSalary f -> f
        let (id , (fn, ln, mi), _ , _) = emp
        let newPaycheck = (ln,  NewSalary ns, NumberOfDependents deps.Length) |> calculatePaycheck 
        let emp: Employee = (id, (fn, ln, mi), AnnualSalary ns, newPaycheck)
        Ok(emp)
     
    let updateSalaryAsync (emp: Employee) =
        async {
            let (id, _, ansal, pc) = emp
            let (pcg, ded, dis, pcn) = pc
            let id = match id with Id i -> i
            let fm = sprintf "There was a serious error updating salary for employee: %i" id
            id 
            |> queryForEmployeeById
            |> Seq.iter( fun e ->
                e.AnnualSalary <- (match ansal with AnnualSalary f -> f) |> decimal
                e.PaycheckGross <-  (match pcg with PaycheckGross f -> f) |> decimal
                e.DeductionBenefits <-  (match ded with DeductionBenefits f -> f) |> decimal
                e.Discount <-  (match dis with Discount f -> f) |> decimal
                e.PaycheckNet <-  (match pcn with PaycheckNet f -> f) |> decimal
            )
            let! submitAsynch = context.SubmitUpdatesAsync() |> Async.Catch
            let result =
                match submitAsynch with
                | Choice1Of2 a -> Ok(emp)
                | Choice2Of2 ex ->
                    context.ClearUpdates |> ignore
                    Error(DbFailure (SqlException (FriendlyMessage fm, ExceptionMessage ex.Message)))
            return result
    
        } |> Async.RunSynchronously
    
    let updateSalary (usd: UpdateSalaryData) = 
        Ok (usd) 
        |> Result.bind validateUpdatedSalary
        |> Result.bind getEmployeeInfo
        |> Result.bind calculateNewPaycheck
        |> Result.bind updateSalaryAsync

module AddDependent =

    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open FSharp.Interview.SqlProvider.Api.Domain.DomainConstants
    open FSharp.Interview.SqlProvider.Api.Lifting.AlphaNumeric
    open SqlProviderContext
   
    let validateAddDependentDependentTypeId (adda: AddDependentData)  =
        let (EmployeeId empId, DependentTypeId depTypeId, Name n) = adda
        let message = sprintf "The DependentTypeId is not valid. Enter 1 for 'Spouse' and 2 for 'Child'"
        match depTypeId with 
        | sp when sp = byte DependentDBType.spouse -> Ok(adda)
        | ch when ch = byte DependentDBType.child -> Ok(adda)
        | _ -> Error(InvalidDependentTypeId message)
  
    let validateDependentName (adda: AddDependentData)  =
        let (EmployeeId empId, DependentTypeId depTypeId, Name n) = adda
        let message = sprintf "Name can not be null or empty" 
        n |> isNotNullOrWhiteSpaceFn adda (InvalidName message)
  
    let validateDependentNameLength (adda: AddDependentData)  =
        let (EmployeeId empId, DependentTypeId depTypeId, Name n) = adda 
        let message = sprintf "Name can not exceed %i characters" maxDependentNameChars
        n |> isLengthLessThanEqualFn adda (InvalidName message) maxDependentNameChars

    let fetchDependentsByEmployeeId id  =
        let fm = sprintf "There was an SQL error fetching dependents for employeeId: %d" id
        let result =  id |> queryForDependentsByEmployeeId |> fetchAsync fm
        match result with
        | Ok (emp) -> Ok(emp |> Seq.toList |> List.map(fun i -> toDependent i))
        | Error(dbex) -> Error(dbex)
  
    let getExistingDependents (adda: AddDependentData)  =
        let (EmployeeId empId, DependentTypeId depTypeId, Name n) = adda 
        let deps = empId |> fetchDependentsByEmployeeId
        match deps with
        | Ok(ds) -> Ok(DependentsAndAddDependentData (Dependents ds, adda))
        | Error(fail) -> Error(DbFailure fail)
  
    let valNumExisitngDependents (daad: DependentsAndAddDependentData) =
        let (Dependents deps, adda) = daad
        let (EmployeeId empId, DependentTypeId newDepTypeId, Name n) = adda
        let message = sprintf "Employee already has maximum number of %i allowed dependents." maxNumOfDependents
        //need to subtract one because total after adding new dep has then be maxNumOfDependents or less
        let rmax = (maxNumOfDependents - 1) 
        deps.Length |> inRangeFn daad (MaxNumOfDependentsFailure message) 0 rmax
  
    let valOnlyOneSpouse (daad: DependentsAndAddDependentData)  =
        let (Dependents deps, adda) = daad
        let (EmployeeId empId, DependentTypeId newDepTypeId, Name n) = adda
        let hasSpouse = 
            deps 
            |> List.map(fun (_, _, dt, _) -> dt) 
            |> List.contains(Spouse (byte DependentDBType.spouse))
        match newDepTypeId with
        | d when d = (byte DependentDBType.spouse) && hasSpouse -> 
            let message =  "Polygamy is not allowed!! Employee already has a spouse as a dependent."
            Error(AlreadyHasSpouseFailure message)
        | _ -> Ok(adda)
  
    let addDependentAsync (adda: AddDependentData) =
        async {
            let (EmployeeId empId, DependentTypeId depType, Name n) = adda
            let row = dependents.Create()
            row.EmployeeId <- empId
            row.DependentTypeId <- depType
            row.Name <-  n.Trim()
            let! submitAsynch = context.SubmitUpdatesAsync() |> Async.Catch
            let result =
                match submitAsynch with
                | Choice1Of2 _ -> Ok(row |> toDependent)
                | Choice2Of2 ex -> 
                    context.ClearUpdates |> ignore
                    Error(DbFailure(SqlException(FriendlyMessage "There was a serious error inserting dependent. Please contact system administrator.", ExceptionMessage ex.Message)))
            return result
        } |> Async.RunSynchronously

    let queryAnnualSalaryForEmp empId  = 
           query {
               for employee in employees do
                   where (employee.Id = empId)
                   select (employee.AnnualSalary)
           }

    let getUpdateSalaryData (dep: Dependent)= 
        let (_, EmployeeId empId, _,_) = dep
        let nfm = sprintf "Employee not found for provided Id: %d" empId
        let fm = sprintf "There was an SQL error fetching annaual salary for provided Id: %d" empId
        let result =  empId |> queryAnnualSalaryForEmp |> fetchOneAsync nfm fm
        match (result) with
        | Ok(asal) ->  Ok(Id empId, NewSalary (asal |> float))
        | Error(fail) -> Error(DbFailure fail)
  
    let updateSalary (usd: UpdateSalaryData) = 
        usd |> UpdateSalary.updateSalary

    let getUpdatedEmployee (emp: Employee) =
        let (Id id, _, _,_) = emp
        match (id |> GetEmployee.getEmployee) with
        | Ok(emp) ->  Ok(emp)
        | Error(fail) -> Error(DbFailure fail)

    let addDependent (adda: AddDependentData) = 
        Ok(adda) 
        |> Result.bind validateAddDependentDependentTypeId
        |> Result.bind validateDependentName
        |> Result.bind validateDependentNameLength
        |> Result.bind getExistingDependents
        |> Result.bind valNumExisitngDependents
        |> Result.bind valOnlyOneSpouse
        |> Result.bind addDependentAsync
        |> Result.bind getUpdateSalaryData
        |> Result.bind updateSalary
        |> Result.bind getUpdatedEmployee

module DeleteDependent =
    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open SqlProviderContext

    let deleteDependentAsync (dependentId: int) =
        async {
            let foundDependent = query {
                for p in context.Dbo.Dependent do
                where (p.Id = dependentId)
                select (Some p)
                exactlyOneOrDefault
            }
            match foundDependent with
            | Some (dep) ->
                let row = context.Dbo.Dependent.Create()
                row.Id <- dependentId
                row.Delete()
                let! submitAsynch = context.SubmitUpdatesAsync() |> Async.Catch //|> Async.RunSynchronously 
                let result = 
                    match submitAsynch with
                    | Choice1Of2 _ -> Ok(dep.EmployeeId)
                    | Choice2Of2 ex -> 
                        context.ClearUpdates |> ignore
                        Error(DbFailure(SqlException(FriendlyMessage "There was a serious error deleting dependent. Please contact system administrator.", ExceptionMessage ex.Message)))
                return result
        
            | None -> return Error(DbFailure(NotFound(sprintf "Dependent not found for provided Id: %d" dependentId)))
        } |> Async.RunSynchronously

    let queryAnnualSalaryForEmp empId  = 
              query {
                  for employee in employees do
                      where (employee.Id = empId)
                      select (employee.AnnualSalary)
              }

    let getUpdateSalaryData empId = 
        let nfm = sprintf "Employee not found for provided Id: %d" empId
        let fm = sprintf "There was an SQL error fetching annaual salary for provided Id: %d" empId
        let result =  empId |> queryAnnualSalaryForEmp |> fetchOneAsync nfm fm
        match (result) with
        | Ok(asal) ->  Ok(Id empId, NewSalary (asal |> float))
        | Error(fail) -> Error(DbFailure fail)


    let updateSalary (usd: UpdateSalaryData) = 
        usd |> UpdateSalary.updateSalary

    let getUpdatedEmployee (emp: Employee) =
        let (Id id, _, _,_) = emp
        match (id |> GetEmployee.getEmployee) with
        | Ok(emp) ->  Ok(emp)
        | Error(fail) -> Error(DbFailure fail)

    let deleteDependent dependentId = 
        Ok(dependentId) 
        |> Result.bind deleteDependentAsync
        |> Result.bind getUpdateSalaryData
        |> Result.bind updateSalary
        |> Result.bind getUpdatedEmployee

module DeleteEmployee =

    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open SqlProviderContext

    let deleteDependentsForEmployee employeeId =
        async {
            let foundDependent = query {
                for p in context.Dbo.Dependent do
                where (p.EmployeeId = employeeId)
            } 
            let! submitAsynch = foundDependent |> deletelAllItemsAsynch |> Async.Catch
            let result =
                match submitAsynch with
                | Choice1Of2 _ -> Ok(employeeId)
                | Choice2Of2 ex ->
                    context.ClearUpdates |> ignore
                    Error(DbFailure(SqlException(FriendlyMessage "There was a serious error deleting dependents for employee. Please contact system administrator.", ExceptionMessage ex.Message)))
            return result
        } |> Async.RunSynchronously

    let deleteEmployeeAsync id =
        async {
            let foundEmployee = query {
                for p in context.Dbo.Employee do
                where (p.Id = id)
                select (Some p)
                exactlyOneOrDefault
            }
            match foundEmployee with
            | Some (_) ->
                let row = context.Dbo.Employee.Create()
                row.Id <- id
                row.Delete()
                let! submitAsynch = context.SubmitUpdatesAsync() |> Async.Catch
                let result = 
                    match submitAsynch with
                    | Choice1Of2 _ -> Ok()
                    | Choice2Of2 ex -> 
                        context.ClearUpdates |> ignore
                        Error(DbFailure(SqlException(FriendlyMessage "There was a serious error deleting employee. Please contact system administrator.", ExceptionMessage ex.Message)))
                return result
    
            | None -> return Error(DbFailure(NotFound(sprintf "Employee not found for provided Id: %d" id)))
        } |> Async.RunSynchronously

    let deleteEmployee id = 
        Ok(id) 
        |> Result.bind deleteDependentsForEmployee
        |> Result.bind deleteEmployeeAsync

module CreateResponse = 
    open System
    open FSharp.Interview.SqlProvider.Api.Domain.ResponseModels
    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open FSharp.Interview.SqlProvider.Api.Lifting.AlphaNumeric
  
    let badRequestResponse s : StandardResponse = {Status= "Bad Request"; StatusCode= 400; Message= s; Result= null}
    let serverErrorResponse s : StandardResponse = {Status= "Internal Server Error"; StatusCode= 500; Message= s; Result= null}
    let okResponse s (o: Object) : StandardResponse = {Status= "Ok"; StatusCode= 200; Message= s; Result= o}

    let dbFailure = function
    | NotFound (s) -> badRequestResponse s
    | SqlException (r)  ->
        let (FriendlyMessage message, ExceptionMessage _) = r
        serverErrorResponse message

    let operationFailureResponse = function
    | InvalidFirstName s -> s |> badRequestResponse
    | InvalidLastName s -> s |> badRequestResponse
    | InvalidMiddleInitialName s -> s |> badRequestResponse
    | InvalidSalary s -> s |> badRequestResponse
    | InvalidName s -> s |> badRequestResponse
    | InvalidDependentTypeId s -> s |> badRequestResponse
    | AlreadyHasSpouseFailure s -> s |> badRequestResponse
    | MaxNumOfDependentsFailure s -> s |> badRequestResponse
    | DbFailure (dbex) -> dbex |> dbFailure

    type DependentResult = { Id: int; EmployeeId: int; Type: string; Name: string }

    type EmployeeResult = {
        Id: int  
        FirstName: string
        LastName: string
        MiddleInitial: string
        AnnualSalary: float
        PaycheckGross: float
        Deduction: float
        Discount: float
        PaycheckNet: float
        Dependents: DependentResult list}

    type SalaryInfoResult = {
        Id: int  
        Name: string
        AnnualSalary: float
        PaycheckGross: float
        Deduction: float
        Discount: float
        PaycheckNet: float }
  
    let (| Spouse | Child |) dt =
        match dt with
        | Spouse (_) -> Spouse
        | Child (_) -> Child
    
    let getDepType = function
        | Spouse -> "Spouse"
        | Child -> "Child"
    
    let toDependentResult (dep: Dependent): DependentResult  =
        let (id, empId, depType, nm) = dep
        {
            Id = match id with Id i -> i
            EmployeeId = match empId with EmployeeId i -> i
            Type = depType |> getDepType
            Name = match nm with Name s -> s.Trim()
        }
    
    let toEmployeeResult  (ead:EmployeeAndDependents): EmployeeResult =
        let (emp, deps) = ead
        let (id, empName, ansal, pc) = emp
        let (fn, ln, mi) = empName
        let (pcg, ded, dis, pcn) = pc
        {
            Id = match id with Id i -> i
            FirstName = match fn with FirstName s -> s.Trim()
            LastName = match ln with LastName s -> s.Trim()
            MiddleInitial = match mi with MiddleInitial (Some s) -> s | _ -> null
            AnnualSalary = match ansal with AnnualSalary f -> (f |> floatToTwoPlaces)
            PaycheckGross = match pcg with PaycheckGross f -> (f |> floatToTwoPlaces)
            Deduction = match ded with DeductionBenefits f -> (f |> floatToTwoPlaces)
            Discount = match dis with Discount f -> (f |> floatToTwoPlaces)
            PaycheckNet = match pcn with PaycheckNet f -> (f |> floatToTwoPlaces)
            Dependents = deps |> List.map(fun i -> toDependentResult  i)
        }

    let toSalaryInfoResult (emp : Employee) = 
        let (id, (fn, ln, mi), ansal, (pcg, ded, dis, pcn)) = emp
        let fn = match fn with FirstName s -> s.Trim()
        let ln = match ln with LastName s -> s.Trim()
        let name = match mi with MiddleInitial (Some s) -> sprintf "%s, %s %s." ln fn (s.ToUpper()) | _ -> sprintf "%s, %s" ln fn
        {
            Id = match id with Id i -> i 
            Name = name
            AnnualSalary = match ansal with AnnualSalary f -> (f |> floatToTwoPlaces)
            PaycheckGross = match pcg with PaycheckGross f -> (f |> floatToTwoPlaces)
            Deduction = match ded with DeductionBenefits f -> (f |> floatToTwoPlaces)
            Discount = match dis with Discount f -> (f |> floatToTwoPlaces)
            PaycheckNet = match pcn with PaycheckNet f -> (f |> floatToTwoPlaces)
        }

module EmployeeResponses = 
    open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
    open GetEmployee
    open GetSalaryInfo
    open InsertEmployee
    open AddDependent 
    open DeleteDependent
    open DeleteEmployee
    open CreateResponse

    let getEmployeeResponse id  = 
        let result = id |> getEmployee 
        match result with
        | Ok (ead) -> ead |> toEmployeeResult |> okResponse "Get employee operation successful"
        | Error(dbex) -> dbex |> dbFailure

    let getSalaryInfoResponse (pg: Paging) = 
        let result =  pg |> getSalaryInfo
        match result with
        | Ok (emps) -> 
            emps |> Seq.map(fun emp -> emp |> toSalaryInfoResult) 
            |> okResponse "Get salary information successful"
        | Error(dbex) -> dbex |> dbFailure

    let getUpdateSalaryResponse (usd: UpdateSalaryData) = 
        match (usd |> updateSalary) with
        | Ok (emps) -> emps |> toSalaryInfoResult |> okResponse "Update salary operation successful"
        | Error(dbex) -> dbex |> operationFailureResponse

    let getAddEmployeeResponse (aed: AddEmployeeData) = 
        match (aed |> addEmployee ) with
        | Ok (emps) -> emps |> toSalaryInfoResult |> okResponse "Add employee operation successful"
        | Error(dbex) -> dbex |> operationFailureResponse

    let getAddDependentResponse (adda: AddDependentData) = 
        match (adda |> addDependent) with
        |Ok (ead) -> ead |> toEmployeeResult  |> okResponse "Add dependent operation successful"
        |Error(dbex) -> dbex |> operationFailureResponse

    let getDeleteDependentResponse depId = 
        match (depId |> deleteDependent) with
        |Ok (ead) -> ead |> toEmployeeResult  |> okResponse "Remove dependent operation successful"
        |Error(dbex) -> dbex |> operationFailureResponse

    let getDeleteEmployeeResponse id = 
        match (id |> deleteEmployee) with
        |Ok (_) -> null |> okResponse "Remove employee operation successful"
        |Error(dbex) -> dbex |> operationFailureResponse
