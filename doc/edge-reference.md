<a name="EdgeReference"></a>

## EdgeReference
A base class for proxies to .NET objects.  Contains some useful tools to 
make references match one another.

**Kind**: global class  

* [EdgeReference](#EdgeReference)
    * _instance_
        * [._referenceId](#EdgeReference+_referenceId)
        * [._referenceId](#EdgeReference+_referenceId)
        * [.referenceEquals(compareTo)](#EdgeReference+referenceEquals) ⇒ <code>Boolean</code>
    * _static_
        * [.register(toRegister)](#EdgeReference.register)
        * [.callbackOrReturn(callTarget, params, [conversionType], [callback])](#EdgeReference.callbackOrReturn)

<a name="EdgeReference+_referenceId"></a>

### edgeReference._referenceId
Gets the reference ID for this instance.  This value is used to retrieve 
the underlying .NET object that corresponds to this instance.

**Kind**: instance property of <code>[EdgeReference](#EdgeReference)</code>  
<a name="EdgeReference+_referenceId"></a>

### edgeReference._referenceId
Assigns a new ID to this reference.  Note that this is equivalent to 
changing the object represented by this instance.

**Kind**: instance property of <code>[EdgeReference](#EdgeReference)</code>  
<a name="EdgeReference+referenceEquals"></a>

### edgeReference.referenceEquals(compareTo) ⇒ <code>Boolean</code>
Compares the .NET proxy provided against this instance.

**Kind**: instance method of <code>[EdgeReference](#EdgeReference)</code>  
**Returns**: <code>Boolean</code> - If the parameter provided is non-null and has the same 
        _referenceId property as this instance, returns true.  

| Param | Type | Description |
| --- | --- | --- |
| compareTo | <code>[EdgeReference](#EdgeReference)</code> | A .NET proxy.  If it represents the same         underlying .NET object, this function will return true. |

<a name="EdgeReference.register"></a>

### EdgeReference.register(toRegister)
Begins tracking a reference in order to allow tracking of when it is 
garbage collected.  This function is intended for internal use and does 
not generally need to be called by consumers.

**Kind**: static method of <code>[EdgeReference](#EdgeReference)</code>  

| Param | Type | Description |
| --- | --- | --- |
| toRegister | <code>[EdgeReference](#EdgeReference)</code> | The object to be tracked. |

<a name="EdgeReference.callbackOrReturn"></a>

### EdgeReference.callbackOrReturn(callTarget, params, [conversionType], [callback])
Calls the specified function and either returns its converted value or 
calls the specified callback with the converted value included.

**Kind**: static method of <code>[EdgeReference](#EdgeReference)</code>  

| Param | Type | Description |
| --- | --- | --- |
| callTarget | <code>function</code> | The function to be called.  Takes one         argument and an optional callback. |
| params | <code>Object</code> | An argument to be given to the callTarget. |
| [conversionType] | <code>function</code> | An optional type.  Takes a numeric         value and wraps it in a proxy. |
| [callback] | <code>function</code> | A callback to be triggered when the call         target has completed.  If not supplied, the operation will run         synchronously. |

