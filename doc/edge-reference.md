## Classes

<dl>
<dt><a href="#EdgeReference">EdgeReference</a></dt>
<dd><p>A base class for proxies to .NET objects.  Contains some useful tools to 
make references match one another.</p>
</dd>
</dl>

## Members

<dl>
<dt><a href="#Unregister">Unregister</a></dt>
<dd><p>Removes a reference in .NET code to an object that is no longer used 
by JavaScript code.  If no JS references remain, the object is reclaimed.</p>
</dd>
</dl>

## Functions

<dl>
<dt><a href="#collectorFactory">collectorFactory(referenceId)</a></dt>
<dd><p>Generates and returns a callback that will clean up .NET references to a 
garbage-collected JavaScript object.</p>
</dd>
<dt><a href="#referenceCollected">referenceCollected(id)</a></dt>
<dd><p>Called when a reference is reclaimed by the garbage collector.</p>
</dd>
</dl>

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
the underlying .NET object that corresponds to this instance.  Two proxies
with matching _referenceId values represent the same .NET object instance.

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
Begins tracking a reference in order to allow cleanup when it is 
garbage collected.  This function is intended for internal use and does 
not generally need to be called by consumers.  Misuse will lead to memory
leaks.

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

<a name="Unregister"></a>

## Unregister
Removes a reference in .NET code to an object that is no longer used 
by JavaScript code.  If no JS references remain, the object is reclaimed.

**Kind**: global variable  

| Param | Type | Description |
| --- | --- | --- |
| input | <code>long</code> | The ID to be removed from the reference collection. |

<a name="collectorFactory"></a>

## collectorFactory(referenceId)
Generates and returns a callback that will clean up .NET references to a 
garbage-collected JavaScript object.

**Kind**: global function  

| Param | Type | Description |
| --- | --- | --- |
| referenceId | <code>Number</code> | The id of the object to be reclaimed when the  returned callback is executed. |

<a name="referenceCollected"></a>

## referenceCollected(id)
Called when a reference is reclaimed by the garbage collector.

**Kind**: global function  

| Param | Type | Description |
| --- | --- | --- |
| id | <code>Number</code> | The id of the object to be reclaimed. |

