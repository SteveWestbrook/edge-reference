// Segment 1: references
const edge = require('edge');
const edgeReference = require('edge-reference');
const Complex = require('Namespace-Complex');

// Segment 2: 
const Reference = edge.func(
  // reference assembly stuff
);

const referenceFactory = edge.func(
  // Build a reference to the object, store it, and throw it into 
  // Wrapper.Instance
);

class Scratchpad {
  /*
   * @param constructorArgs An associative array containing the parameters to 
   *        be passed to the constructor with matching parameters.
   */
  constructor(referenceId, constructorArgs) {
    if (referenceId) {
      this._referenceId = referenceId;
    } else {
      this._referenceId = referenceConstructor();

      // TODO: underlying constructor
    }

    EdgeReference.register(this);
  }

  get _edgeId() {
    return this._referenceId;
  }

  set _edgeId(value) {
    this._referenceId = value;
  }

  get Property() {
    return Reference.Property(this._referenceId);
  }
  
  set Property(value) {
    Reference.Property(this._referenceId, value);
  }

  get StringProperty() {
    return Reference.StringProperty(this._referenceId);
  }

  get ComplexProperty() {
    var returnId = Reference.ComplexProperty();
    return new Complex(returnId);
  }

  set ComplexProperty(value) {
    Reference.ComplexProperty(this._referenceId, value._edgeId);
  }

  ComplexReturnFunction(simple, string, complex) {
    var arg1 = simple;
    var arg2 = string;
    var arg3 = complex ? complex._edgeId : 0;

    var result = Reference.ComplexReturnFunction(arg1, arg2, arg3);

    return new Complex(result);
  }
}
