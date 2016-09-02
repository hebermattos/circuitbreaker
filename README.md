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
    //the circuit is open. after 3 success the circuit half open, and after 15 with no errors, the circuit close
}

catch (Exception)
{
    //something went wrong. dont worry, the circuit breaker is watching. after five errors the circuit is open
}
```
