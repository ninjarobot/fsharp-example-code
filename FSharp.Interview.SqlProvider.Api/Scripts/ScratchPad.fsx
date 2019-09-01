#r @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.2.6\netstandard.dll"
#r @"C:\Users\srpet\.nuget\packages\sqlprovider\1.1.66\lib\netstandard2.0\FSharp.Data.SqlProvider.dll"
#r @"C:\Users\srpet\.nuget\packages\newtonsoft.json\12.0.2\lib\netstandard2.0\Newtonsoft.Json.dll"

#load @"..\Code\Domain.fs"
#load @"..\Code\Lifting.fs"
#load @"..\Code\OneBigFile.fs"
open System
open Microsoft.FSharp.Core
open FSharp.Data.Sql
open FSharp.Interview.SqlProvider.Api.Domain.RequestModels
open FSharp.Interview.SqlProvider.Api.Lifters.AlphaNumeric
open FSharp.Interview.SqlProvider.Api.OneBigFile.Code
