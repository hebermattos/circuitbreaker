# circuit breaker pattern
http://martinfowler.com/bliki/CircuitBreaker.html


# how to

Install-Package NetCircuitBreaker

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
    //the circuit is open. after 15 seconds the circuit half open, and after three consecutive success, the circuit close
	//on error while half open the circuit open again
}
catch (Exception)
{
    //something went wrong. after five errors the circuit is open
}
```
