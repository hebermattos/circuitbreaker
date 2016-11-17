# circuit breaker pattern
http://martinfowler.com/bliki/CircuitBreaker.html

# requirements
Framework 4.6

# nuget
Install-Package NetCircuitBreaker

# how to
``` c#
var cb = new CircuitBreaker(
                maxErrors: 5, 
                maxSuccess: 3, 
                circuitReset: TimeSpan.FromSeconds(15)
                );

cb.StatusChanged += (e) => { Console.Write(e.Status + " - " + e.Date + " - " + e.Reason); };
				
try
{
    cb.Execute(() => { Foo(); });
}
catch (OpenCircuitException)
{
    //the circuit is open. after 15 seconds the circuit is half open, and after three consecutive success, the circuit is close 
	//error while 'half open' open the circuit again
}
catch (Exception)
{
    //something went wrong. after five consecutive errors the circuit is open
}
```
