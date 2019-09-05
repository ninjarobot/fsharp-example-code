namespace FSharp.Interview.SqlProvider.Api.Controllers

open Microsoft.AspNetCore.Mvc
open FSharp.Interview.SqlProvider.Api.Domain.ResponseModels
open FSharp.Interview.SqlProvider.Api.Domain.RequestModels
open FSharp.Interview.SqlProvider.Api.Domain.DomainTypes
open FSharp.Interview.SqlProvider.Api.Lifting.AlphaNumeric
open FSharp.Interview.SqlProvider.Api.Business.EmployeeResponses

[<ApiController>]
[<Route("api/employee")>]
type EmployeeController ()  =
    inherit ControllerBase()

    [<HttpGet("{id}")>]
    member __.GetEmployee(id:int) =
        let response = id |> getEmployeeResponse
        ActionResult<StandardResponse>(__.StatusCode(response.StatusCode, response))

    [<HttpGet("all/salaryinfo")>]
    member __.GetSalaryInfo() =
        let response = (Skip 0, Take 10)  |> getSalaryInfoResponse //todo: get skip, take from query string
        ActionResult<StandardResponse>(__.StatusCode(response.StatusCode, response))

    [<HttpPut("update-salary")>]
    member __.UpdateSalary([<FromBody>] updateSalaryRequest : UpdateSalaryRequest) =
        async {
            let id = Id updateSalaryRequest.Id
            let ns = NewSalary (updateSalaryRequest.Salary |> float)
            let! response = UpdateSalaryData (id, ns) |> getUpdateSalaryResponse
            return ActionResult<StandardResponse>(__.StatusCode(response.StatusCode, response))
        } |> Async.StartAsTask

    [<HttpPost("add")>]
    member __.AddEmployee([<FromBody>] addEmployeeRequest : AddEmployeeRequest) =
        let fn = addEmployeeRequest.FirstName
        let ln =  addEmployeeRequest.LastName
        let mi = addEmployeeRequest.MiddleInitial |> toOption
        let ansal =  (addEmployeeRequest.Salary |> float)
        let response = (EmployeeName(FirstName fn, LastName ln, MiddleInitial mi ), AnnualSalary ansal) |> getAddEmployeeResponse
        ActionResult<StandardResponse>(__.StatusCode(response.StatusCode, response))

    [<HttpPost("dependents/add")>]
    member __.AddDependent([<FromBody>] addDependentRequest : AddDependentRequest) =
        let empId = addDependentRequest.EmployeeId
        let n =  addDependentRequest.Name
        let depTypeId = addDependentRequest.DependentTypeId
        let response = (EmployeeId empId, DependentTypeId depTypeId, Name n) |> getAddDependentResponse
        ActionResult<StandardResponse>(__.StatusCode(response.StatusCode, response))

    [<HttpDelete("dependent/remove/{dependentId}")>]
    member __.DeleteDependent(dependentId:int) =
        let response = dependentId |> getDeleteDependentResponse
        ActionResult<StandardResponse>(__.StatusCode(response.StatusCode, response))

    [<HttpDelete("remove/{id}")>]
    member __.DeleteEmployee(id:int) =
        let response = id |> getDeleteEmployeeResponse
        ActionResult<StandardResponse>(__.StatusCode(response.StatusCode, response))



