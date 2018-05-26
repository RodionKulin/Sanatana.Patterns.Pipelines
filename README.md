# Sanatana.Patterns.Pipelines
Pipeline pattern implementation.

# Benefits
-Structured.
Structure complex tasks often involving validation, permission checks, multiple requests to external services into set of separate steps.

-Testable.
Pipeline can be tested on each step individually, as long as steps are just public methods.

-Reusable.
Predefined pipeline can be edited to replace or remove steps from initial implementation and create altered pipeline.

-Predictable results.
Pipeline does not end in semi completed state. Optional roll back steps can compensate already completed steps after exception occurrence. Useful for systems that does not share common transaction, like between file system, database and search index.



# Usage
-Pipeline defines context type, that will be shared among steps. Context contains input data class and output data class.

```csharp
public class InsertInputData
{
	public string Title { get; set; }
	public string Text { get; set; }
}

public class InsertOutputData
{
	public int Id { get; set; }
	public string ErrorMessage { get; set; }
}

public class InsertPostPipeline : Pipeline<InsertInputData, InsertOutputData>
{
	//Pipeline steps definition
}
```


-Pipeline defines a list of steps that will be executed in a sequence.
Each step is a method returning true or false depending if the step was completed successfully.
```csharp
public Task<bool> Validate(PipelineContext<InsertInputData, InsertOutputData> context)
{
	if (string.IsNullOrEmpty(context.Input.Title) == true)
	{
		context.Output.ErrorMessage = "Title can not be empty"; 
		return Task.FromResult(false);
	}

	return Task.FromResult(true);
}
```


-Each step is registered in the pipeline before execution. Usually on pipeline initialization.
```csharp
public InsertPostPipeline()
{
	Register(Validate);
	Register(InsertToDatabase);
	Register(InsertToSearchIndex);
}
```

-Each step can have optional compensation implemented.
```csharp
public InsertPostPipeline()
{
    //Register other steps
    Register(InsertToDatabase, InsertToDatabaseRollback);
}   
  
public Task<bool> InsertToDatabase(PipelineContext<InsertInputData, InsertOutputData> context)
{
    //Insert to database
    return Task.FromResult(true);
}

public Task<bool> InsertToDatabaseRollback(PipelineContext<InsertInputData, InsertOutputData> context)
{
    //Delete from database
    return Task.FromResult(true);
}
```


# Complete example
Define pipeline
```csharp
public class InsertInputData
{
	public string Title { get; set; }
	public string Text { get; set; }
}

public class InsertOutputData
{
	public int Id { get; set; }
	public string ErrorMessage { get; set; }
}

public class InsertPostPipeline : Pipeline<InsertInputData, InsertOutputData>
{
    public InsertPostPipeline()
    {
        Register(Validate);
        Register(InsertToDatabase, InsertToDatabaseRollback);
        Register(InsertToSearchIndex);
    }

    public Task<bool> Validate(PipelineContext<InsertInputData, InsertOutputData> context)
    {
        if (string.IsNullOrEmpty(context.Input.Title) == true)
        {
            context.Output.ErrorMessage = "Title can not be empty"; 
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task<bool> InsertToDatabase(PipelineContext<InsertInputData, InsertOutputData> context)
    {
        //Insert to database
        context.Output.Id = //return id assigned by database
        return Task.FromResult(true);
    }

    public Task<bool> InsertToDatabaseRollback(PipelineContext<InsertInputData, InsertOutputData> context)
    {
        //Delete from database
        return Task.FromResult(true);
    }

    public Task<bool> InsertToSearchIndex(PipelineContext<InsertInputData, InsertOutputData> context)
    {
        //Insert to search index
        return Task.FromResult(true);
    }
}
```

Then execute it
```csharp
var input = new InsertInputData();
var output = new InsertOutputData ();
var insertPipeline = new InsertPostPipeline();
insertPipeline.Execute(input, output)
```


# Workflow
## Interrupt execution
-If not caught exception is thrown during step execution it will interrupt execution of further steps and invoke pipeline roll back steps.

-Step can also interrupt following execution by returning boolean false as result. Useful for validation steps that does not usually throw exceptions.


## RollBack
-Useful for applying compensation in multiple services like database, search index, file system that does not share common transaction scope.

-Rollback steps are optional. It is also legit to only define roll forward steps if your pipeline can not be interrupted by external service dysfunction and you don't need to compensate semi completed pipeline steps.

-Once interrupted pipeline will execute default defined rollback steps. It will sequentially execute rollback step for each previous completed steps. 

-Rollback can not be interrupted by exception in one the rollback steps by default. 
Exceptions will be accumulated in context property and should be handled after pipeline finished. 



# Edit predefined pipeline 
## Edit pipeline steps
-Register new pipeline steps.
New step can be added to the end of pipeline,inserted to specific index or inserted after or before specific existing step (need to provide existing step method to match it by reference). Use pipeline methods:

Register
RegisterAt
RegisterBefore
RegisterAfter

-Steps can be removed or replaced by different steps. To match a step to remove need to supply it's index in the array or an original method itself. Later is recommended as a more reliable solution. Use pipeline methods:

Remove
Replace

## Edit pipeline workflow
Pipeline workflow can be altered by overriding 2 methods.
-Execute. Controls roll forward execution, exception catching and calling roll back after first step that does not complete successfully.

-Rollback. Controls execution of roll back steps.

```csharp
public class InsertPostPipeline : Pipeline<InsertInputData, InsertOutputData>
{
	public override Task<InsertOutputData> Execute(InsertInputData inputModel, InsertOutputData outputModel)
	{
		return base.Execute(inputModel, outputModel);
	}

	public override Task RollBack(PipelineContext<InsertInputData, InsertOutputData> context)
	{
		return base.RollBack(context);
	}
}
```

## Override RollBack method to handle exceptions
```csharp
public class InsertPostPipeline : Pipeline<InsertInputData, InsertOutputData>
{    
  public override async Task RollBack(
	  PipelineContext<InsertInputData, InsertOutputData> context)
  {
    await base.RollBack(context);

    if (context.Exceptions != null)
    {
      //log or rethrow exceptions here
    }
    if (context.Output == null)
    {
      context.Output.ErrorMessage = ContentEditResult.Error(ContentsMessages.Common_ProcessingError);
    }

    return;
  }
}
```

