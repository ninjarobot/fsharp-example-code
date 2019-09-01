namespace FSharp.Interview.SqlProvider.Api.Lifting

module AlphaNumeric = 
    open System
    open Microsoft.FSharp.Core
    
    /// <summary>Safely checks whether a string is null or whitespace.</summary>
    /// <param name="okf">The Function to execute Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let isNullOrWhiteSpaceFn okf errorf input  =
        match String.IsNullOrWhiteSpace(input) with 
        | true  -> Ok(okf)
        | _  -> Error(errorf)

     /// <summary>Safely checks whether a string is null or whitespace.</summary>
    /// <param name="okf">The Function to execute Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let isNullOrWhiteSpace input = isNullOrWhiteSpaceFn () () input 

    /// <summary>Safely checks whether a string is not null or not whitespace.</summary>
    /// <param name="okf">The Function to execute ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let isNotNullOrWhiteSpaceFn okf errorf input  =
        match String.IsNullOrWhiteSpace(input) with 
        | false  -> Ok(okf)
        | _  -> Error(errorf)

    /// <summary>Safely checks whether a string is not null or not whitespace.</summary>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let isNotNullOrWhiteSpace input = isNotNullOrWhiteSpaceFn () () input 

    /// <summary>Safely checks whether a string is not null or empty.</summary>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="okf">The Function to execute Ok</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let isNotNullOrEmptyFn okf errorf input =
        match String.IsNullOrEmpty(input) with 
        | false  -> Ok(okf)
        | _  -> Error(errorf)
    
    /// <summary>Safely checks whether a string is not null or empty.</summary>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let isNotNullOrEmpty input = isNotNullOrEmptyFn () () input
    
    /// <summary>Safely checks whether a string is null or empty.</summary>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="okf">The Function to execute Ok</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let isNullOrEmptyFn okf errorf input =
        match String.IsNullOrEmpty(input) with 
        | true  -> Ok(okf)
        | _  -> Error(errorf)
    
    /// <summary>Safely checks whether a string is null or empty.</summary>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let isNullOrEmpty input = isNullOrEmptyFn () () input 

    /// <summary>Returns a Result on whether a string is than or equal to given length.</summary>
    /// <param name="okf">The Function to execute Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="length">The maximum length of the string</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let isLengthLessThanEqualFn okf errorf length input =
        match isNotNullOrEmpty input with 
        | Ok() when (String.length input <= length)  -> Ok(okf)
        | _  -> Error(errorf)

    /// <summary>Returns a Result on whether a string is greater than or equal to given length.</summary>
    /// <param name="length">The maximum length of the string</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let isLengthLessThanEqual length input = isLengthLessThanEqualFn () () length input

    /// <summary>Returns a Result on whether a string is zero length.</summary>
    /// <param name="okf">The Function to execute Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let isZeroLengthFn okf errorf input =
        let nullCheck = match input with null -> true | _ -> false
        match (nullCheck) with 
        | false when (String.length input = 0) -> Ok(okf)
        | _  -> Error(errorf)

    /// <summary>Returns a Result on whether a string is zero length.</summary>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let isZeroLength input = isZeroLengthFn () () input

    /// <summary>Returns a Result on whether a string is not zero length.</summary>
    /// <param name="okf">The Function to execute Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let isNotZeroLengthFn okf errorf input =
        let nullCheck = match input with null -> true | _ -> false
        match (nullCheck) with 
        | false when (String.length input > 0) -> Ok(okf)
        | _  -> Error(errorf)

    /// <summary>Returns a Result on whether a string is not zero length.</summary>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let isNotZeroLength input = isNotZeroLengthFn () () input

    /// <summary>Safely lifts a string to option type</summary>
    /// <param name="input">The string to convert</param>
    /// <returns>
    ///     Some (input)
    ///     None 
    /// </returns>
    let toOption input =
        match isNotNullOrWhiteSpace input with 
        | Ok()  -> Some(input)
        | _  -> None
    
    /// <summary>Safely converts a string to upper case.</summary>
    /// <param name="input">The string to convert</param>
    /// <returns>
    ///     Some (input)
    ///     None 
    /// </returns>
    let toUpper input =
        match isNotNullOrWhiteSpace input with 
        | Ok()  -> Some(input.ToUpper())
        | _  -> None

    /// <summary>Safely converts a string to lower case.</summary>
    /// <param name="input">The string to convert</param>
    /// <returns>
    ///     Some (input)
    ///     None 
    /// </returns>
    let toLower input =
        match isNotNullOrWhiteSpace input with 
        | Ok()  -> Some(input.ToLower())
        | _  -> None

    /// <summary>Safely trims a string</summary>
    /// <param name="input">The string to convert</param>
    /// <returns>
    ///     Some (input)
    ///     None 
    /// </returns>
    let trim input =
        match isNotNullOrWhiteSpace input with 
        | Ok()  -> Some(input.Trim())
        | _  -> None
    
    /// <summary>Safely reverses a string.</summary>
    /// <param name="input">The string to reverse</param>
    /// <returns>
    ///     Some (input)
    ///     None 
    /// </returns>
    let reverseString input =
        let reverse s = 
            s 
            |> Array.ofSeq 
            |> Array.rev 
            |> Array.map string //convert character elements to string
            |> Array.reduce (+)
        match isNotNullOrWhiteSpace input with 
        | Ok()  -> Some(reverse input)
        | _  -> None

    /// <summary>Checks if numeric value is greater than zero</summary>
    /// <param name="okf">The Function to execute on Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let inline isGreaterThanZeroFn okf errorf input =
        match input > LanguagePrimitives.GenericZero<_> with
        | true -> Ok(okf)
        | false -> Error(errorf)

    /// <summary>Checks if numeric value is greater than zero</summary>
    /// <param name="input">The numeric value to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let inline isGreaterThanZero input = isGreaterThanZeroFn  () () input

    /// <summary>Checks if numeric value is in a range of given values</summary>
    /// <param name="okf">The Function to execute on Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="min">The minumum value in range</param>
    /// <param name="max">The max value in range</param>
    /// <param name="i">The numeric value to check</param>
    /// <returns>
    ///     Ok('a)
    ///     Error('b) 
    /// </returns>
    let inline inRangeFn okf errorf min max i =
        let ar = [min..max] |> Seq.contains i
        match ar with 
        | true -> Ok(okf)
        | false -> Error(errorf)

    /// <summary>Checks if numeric value is in a range of given values</summary>
    /// <param name="min">The minumum value in range</param>
    /// <param name="max">The max value in range</param>
    /// <param name="i">The numeric value to check</param>
    /// <returns>
    ///     Ok(unit)
    ///     Error(unit) 
    /// </returns>
    let inline inRange min max i = inRangeFn  () () min max i

    /// <summary>Safely checks a string for only non numeric characters.
    /// Must contain at least one A-Z (not case sensitive) characters</summary>
    /// <param name="okf">The Function to execute on Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok ('a)
    ///     Error ('b) 
    /// </returns>
    let containsOnlyLettersFn okf errorf input = 
        match (trim input) with
        | Some(s) -> 
            let res = s |> Seq.forall (fun ch -> System.Char.IsLetter(ch) || ch = ' ') 
            match res with
            | true -> Ok(okf)
            | false -> Error(errorf)
        | None -> Error(errorf)

    /// <summary>Safely checks a string for only non numeric characters.
    /// Must contain at least one A-Z (not case sensitive) characters</summary>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok (unit)
    ///     Error (unit) 
    /// </returns>
    let containsOnlyLetters input = containsOnlyLettersFn () () input
    
    /// <summary>Safely checks a string for only numeric characters.
    /// Must contain at least one 0-9 characters</summary>
    /// <param name="okf">The Function to execute on Ok</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok ('a)
    ///     Error ('b) 
    /// </returns>
    let containsOnlyNumbersFn okf errorf input = 
        match (trim input) with
        | Some(s) -> 
            let res = s |> Seq.forall (fun ch -> System.Char.IsLetter(ch) || ch = ' ') 
            match res with
            | true -> Ok(okf)
            | false -> Error(errorf)
        | None -> Error(errorf)

    /// <summary>Safely checks a string for only numeric characters.
    /// Must contain at least one 0-9 characters</summary>
    /// <param name="input">The string to check</param>
    /// <returns>
    ///     Ok (unit)
    ///     Error (unit) 
    /// </returns>
    let containsOnlyNumbers input = containsOnlyNumbersFn () () input

    /// <summary>Rounds float to two decimal places</summary>
    /// <param name="f">The float to round</param>
    /// <returns>Float rounded to two places</returns>
    let floatToTwoPlaces f = Operators.round (f * 100.) / 100.
    
    /// <summary>Rounds decimal to two decimal places</summary>
    /// <param name="f">The decimal to round</param>
    /// <returns>Deciaml rounded to two places</returns>
    let decimalToTwoPlaces d = Operators.round (d * 100m) / 100m

module Parsing =
    open System
    open Microsoft.FSharp.Core

    /// <summary>Attempts to parse string to boolean.
    /// Will also parse string "0" to false, or "1" to true.</summary>
    /// <param name="s">The string to parse</param>
    /// <param name="errorf">The Function to execute on error</param>
    /// <returns>
    ///     Ok (s) where s can be parsed to boolean.
    ///     Error (errorf) if s can not be parsed
    /// </returns>
    let boolTryParse errorf s =
        let q = match s with
                | "0" -> "false"
                | "1" -> "true"
                | _ -> s
        match (bool.TryParse q) with
        | true, bl -> Ok (bl)
        | false, _ -> Error(errorf)

    /// <summary>Attempts to parse given string into given primitive type</summary>
    /// <param name="s">The string to parse</param>
    /// <returns>
    ///     Ok (s) where s is the desired type
    ///     Error (errorf) if s can not be parsed
    /// </returns>
    let private tryParseFn tryParseFunc errorf (s : string) =
        match (tryParseFunc s) with
        | true, x -> Ok (x)
        | false, _ -> Error(errorf)

    /// <summary>Attempts to parse given string into Int32</summary>
    /// <param name="s">The string to parse</param>
    /// <returns>
    ///     Ok (s) where s can be converted to type
    ///     Error (errorf) if s can not be parsed
    /// </returns>
    let intTryParse errorf s = tryParseFn Int32.TryParse errorf s

    /// <summary>Attempts to parse given string into DateTime</summary>
    /// <param name="s">The string to parse</param>
    /// <returns>
    ///     Ok (s) where s can be converted to type
    ///     Error (errorf) if s can not be parsed
    /// </returns>
    let datetimeTryParse errorf s = tryParseFn DateTime.TryParse errorf s
    
    /// <summary>Attempts to parse given string into Decimal</summary>
    /// <param name="s">The string to parse</param>
    /// <returns>
    ///     Ok (s) where s can be converted to type
    ///     Error (errorf) if s can not be parsed
    /// </returns>
    let decimalTryParse errorf s= tryParseFn Decimal.TryParse errorf s

    /// <summary>Attempts to parse given string into Double (there is no Float.TryParse)</summary>
    /// <param name="s">The string to parse</param>
    /// <returns>
    ///     Ok (s) where s can be converted to type
    ///     Error (errorf) if s can not be parsed
    /// </returns>
    let floatTryParse errorf s = tryParseFn Double.TryParse errorf s 

    /// <summary>Attempts to parse given string into Double</summary>
    /// <param name="s">The string to parse</param>
    /// <returns>
    ///     Ok (s) where s can be converted to type
    ///     Error (errorf) if s can not be parsed
    /// </returns>
    let doubleTryParse errorf s = floatTryParse errorf s