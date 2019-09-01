namespace FSharp.Interview.SqlProvider.Api.Domain

module RequestModels =
    open Newtonsoft.Json
    (*
        Note:

        In order for Swagger to display the model correctly,
        the model needs to be a 'CSharpy' type 
        example: type UpdateSalaryRequest() =

        If swagger isn't needed, then the 'FSharpy' type works just fine
        example: type UpdateSalaryRequest = {..} (see commented out below)

        
        //    //[<CLSCompliant(true)>]
        //      //type UpdateSalaryRequest = {
        //      //    [<JsonProperty("id", Required = Required.Always)>] Id: int
        //      //    [<JsonProperty("salary", Required = Required.Always)>] Salary: float }
    
    *)
    type UpdateSalaryRequest() =

        [<JsonProperty("id", Required = Required.Always)>]
        member val Id: int = 0 with get, set

        [<JsonProperty("salary", Required = Required.Always)>]
        member val Salary: int = 0 with get, set


    type AddEmployeeRequest() =

        [<JsonProperty("firstName", Required = Required.Always)>]
        member val FirstName: string = null with get, set

        [<JsonProperty("lastName", Required = Required.Always)>]
        member val LastName: string = null with get, set

        [<JsonProperty("salary", Required = Required.Always)>]
        member val Salary: int = 0 with get,set

        [<JsonProperty("mi", Required = Required.AllowNull)>]
        member val MiddleInitial: string = null with get, set

  
    type AddDependentRequest() =

        [<JsonProperty("employeeId", Required = Required.Always)>]
        member val EmployeeId: int = 0 with get, set

        [<JsonProperty("name", Required = Required.Always)>]
        member val Name: string = null with get, set

        [<JsonProperty("dependentTypeId", Required = Required.Always)>]
        member val DependentTypeId: byte = 0uy with get, set

module ResponseModels =
    open System
    type StandardResponse = { Status: string; StatusCode: int; Message: string; Result: Object }

module DomainTypes = 

    //common
    type Id = Id of int 

    //dependent
    type EmployeeId = EmployeeId of int
    type DependentType = Spouse of byte | Child of byte
    type DependentTypeId = DependentTypeId of byte
    type Name = Name of string
    type Dependent = Id * EmployeeId * DependentType * Name
    type NumberOfDependents = NumberOfDependents of int

    //employee
    type FirstName = FirstName of string
    type LastName = LastName of string
    type MiddleInitial = MiddleInitial of string option
    type AnnualSalary = AnnualSalary of float
    type PaycheckGross = PaycheckGross of float
    type DeductionBenefits = DeductionBenefits of float
    type Discount = Discount of float
    type PaycheckNet = PaycheckNet of float
    type EmployeeName = FirstName * LastName * MiddleInitial
    type Paycheck = PaycheckGross * DeductionBenefits * Discount * PaycheckNet
    type Employee = Id *  EmployeeName * AnnualSalary * Paycheck 
    type EmployeeAndDependents = Employee * Dependent list

    //new paycheck types
    type NewSalary = NewSalary of float
    type CalculatePaycheckData = LastName * NewSalary * NumberOfDependents

    //exception/failure types
    type FriendlyMessage = FriendlyMessage of string
    type ExceptionMessage = ExceptionMessage of string
    type NotFound  = string
    type SqlException = FriendlyMessage * ExceptionMessage
    type DbFailure =  SqlException of SqlException | NotFound of NotFound

    //validation
    type InvalidFirstName = string
    type InvalidLastName = string
    type InvalidMiddleInitialName= string
    type InvalidSalary = string
    type InvalidName = string
    type InvalidDependentTypeId = string
    type AlreadyHasSpouseFailure = string
    type MaxNumOfDependentsFailure = string

    type OperationFailure = 
        | InvalidFirstName of InvalidFirstName 
        | InvalidLastName of InvalidLastName 
        | InvalidMiddleInitialName of InvalidMiddleInitialName 
        | InvalidSalary of InvalidSalary 
        | InvalidName of InvalidName
        | InvalidDependentTypeId of InvalidDependentTypeId 
        | AlreadyHasSpouseFailure of AlreadyHasSpouseFailure
        | MaxNumOfDependentsFailure of MaxNumOfDependentsFailure
        | DbFailure of DbFailure

    //update salary types
    type UpdateSalaryData = Id * NewSalary
    type EmployeeAndDependentsAndNewSalary = EmployeeAndDependents * NewSalary

    //add employee types
    type AddEmployeeData = EmployeeName * AnnualSalary

    //add dependent types
    type AddDependentData = EmployeeId * DependentTypeId * Name
    type Dependents = Dependents of Dependent list
    type DependentsAndAddDependentData = Dependents * AddDependentData

    //for paging
    type Skip = Skip of int
    type Take = Take of int
    type Paging = Skip  * Take 

module DomainConstants =

    let minSalary = 120000.00
    let maxSalary = 180000.00
    let maxEmployeeFirstNameChars = 20
    let maxDependentNameChars = 20
    let maxEmployeeLastNameChars = 50
    let maxNumOfDependents = 10
    let employeeCostForBenefitsPerYear = 5000.
    let dependentCostForBenefitsPerYear = 1000.
    let paychecksPerYear = 24.

module SqlProviderConnection =
    open System

    type JobInterviewConnection() = 

        //static field should be defined first
        static let mutable connString = null

        member private __.checkConnString cs =
                match cs with
                | s when String.IsNullOrWhiteSpace(connString) -> s
                | _ -> failwith "The connection string has already been defined"

        member __.SetConnString (value: string) = 
            connString <- __.checkConnString(value)

        static member GetConnString
            with get() = connString
   





   
