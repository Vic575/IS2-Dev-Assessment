# Data Exporter

The **Data Exporter** app is a small RESTful API implemented in .NET 6. It manages insurance policies and any notes the brokers might have added to the policies. It also provides a way to query and map the data to a format an external system might require for importing.

# Tasks

1. The **GetPolicy** method of the **PoliciesController** has already been implemented, but both itself and the **ReadPolicyAsync** function it calls from the service have some logic errors. Find and fix the logic errors and suggest any other improvements you would make to those methods, if any.

## Response : end points fixed, improvments included only returning JSON data rather than extraneous info 
    "id": 28,
    "exception": null,
    "status": 5,
    "isCanceled": false,
    "isCompleted": true,
    "isCompletedSuccessfully": true,
    "creationOptions": 0,
    "asyncState": null,
    "isFaulted": false
 

2. Implement the **GetPolicies** endpoint that should return all existing policies.

##  Response : completed, again removing the extraneous info shown above


3. Implement the **PostPolicies** endpoint. It should create a new policy based on the data of the DTO it receives in the body of the call and return a read DTO, if successful. 

## --- Response : completed with IDImpotent element, needs prettifying rather than returning an error if duplicate post made - working on this currently**


4. The **Note** entity has been created, but it's not yet configured in the **ExporterDbContext**. Add the missing configuration, considering there is a one-to-many relationship between the **Policy** and the **Note** entities, and seed the database with a few notes.

## --- Response : notes added, not included for add new policy function. Did include notes for the **GetPolicy** call and **Export** calls**


5. Implement the **Export** endpoint. The call receives two parameters from the query string, the **startDate** and the **endDate**. The method needs to retrieve all policies that have a start date between those two dates, and all of their notes. The data should then be mapped to the **ExportDto** class and returned.

## --- Response : Completed

## Remarks

- The tasks can be completed in any order.
- Any third party library can be used to implement some of the functionality required.
- To test the API, any tool like cURL or Postman can be used and the scripts should be included in the submission.

## Response I also have added a test class for the APIs to provide for regression testing
Added some rudimentary logic to ensure only valid data could be added to a new policy
There is no AUTH involved in this, but I expect that is to keep things simple for a tech task so will ignore
_I have also ignored the .NET 6 being beyond EOL (Nov 24) as I would want to check the wider estate can manage with this being upgraded before upgrading
