# circuitbreaker
simple net circuitbreaker

# how to

``` c#
var cb = new CircuitBreaker(
                maxErrors: 5, 
                maxSuccess: 3, 
                circuitReset: TimeSpan.FromSeconds(15)
                );

try
{
    cb.Execute(() => { Foo(); });
}
catch (OpenCircuitException)
{
    //the circuit is open. make a sandwich and wait
}

catch (Exception)
{
    //something went wrong. dont worry, the circuit breaker is watching
}
```
