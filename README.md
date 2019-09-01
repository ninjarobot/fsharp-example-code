# fsharp-example-code
Personal F# Code I've developed.
#
<h1>FSharp.Interview.SqlProvider.Api</h1>

<h3>Calculate employee's paycheck after deductions for employee benefits. Show paycheck gross, Deduction amount, Discount amount (if any), paycheck net </h3>

Business rules:

1. Employee is paid twice a month so recieves 24 paychecks/year
2. Employee deduction: $5000/year (not optional)
3. Each dependent for employee costs $1000/year
4. Employee can have 0 dependents and a maximum  of 10 dependents
5. Employee can only have one spouse as a dependent
6. If the employee's last name begins with the letter "A", then employee recieves a <strong>15% discount</strong> off the total benefits cost for himself and dependents
7. Employee's salary must be between $120,000 and $180,000 per year
8. Employee's  first name is required and can only contain 20 characters
9. Employee's  last name is required and can only contain 50 characters
10. Employee's  middle initial is optional, but if provided and can only contain 1 character
11. Dependents name is required and can only have 20 characters.

<h2>Local set up</h2>

**Requirement: .NET Core 2.2 SDK**

If you plan on running locally you will need to create a SQL Server db. The project is already configured to point to the (localdb) instance, but you can point it anywhere, but will need to modify the run time "JobInterviewConnectionString" property in appsettings.json and the shape connection string in  line 25 of Business.fs

1. Create a sql database named **"JobInterview"**
2. Run the ..\Sql\Create-Tables-Data-JobInterview.sql against the **"JobInterview"** database. This created the tables and add some dummy data.

<h2>Running locally</h2>

1. Open the  FSharp.Interview.SqlProvider.Api.sln in VS 2017 or VS 2019 (I haven't tried in VS Code)
2. Set the FSharp.Interview.SqlProvider.Api project as start up
3. Hit F5 to start debugging, or open a command prompt in the folder where the FSharp.Interview.SqlProvider.Api.jsproj is and typing 'dotnet run'
4. Open **https://localhost:[your port]/swagger/index.html** to see the Swagger definitions and to try stuff out for yourself. I prefer Postman, but that is just me..

<h2>Notes</h2>

A couple of years ago I had to do this project in C# as part of the application process to a major US company. Since then I have learned F# on my own time and thought it would be a great learning experience to rewrite the original C# project in F#. It was! A couple of interesting tidbits:

1. The C# version had approx, 1600 lines of code and 18 files (those seperate code files just for interface add upp lol!)
2. The F# version has approx 800 lines of code and only 3 files!

Anyway, I've learned F# on my own... 

So... if you have any suggestions on how things could be done better

Or..... if you know any positions open for an F# developer (please mom - don't make have to go to my C# job)  ....

**PLEASE!** Let me know or make a pull request!! :)

Steve Peterson

srpeterson@outlook.com
